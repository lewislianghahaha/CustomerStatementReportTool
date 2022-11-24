﻿using System;
using System.Data;
using System.Windows.Forms;
using CustomerStatementReportTool.DB;
using Stimulsoft.Report;

namespace CustomerStatementReportTool.Task
{
    //运算
    public class Generate
    {
        SearchDt searchDt=new SearchDt();
        TempDtList tempDt=new TempDtList();

        #region 参数
        //记录出现异常的提示
        public string Errormessage = string.Empty;
        #endregion

        /// <summary>
        /// 根据各参数运算结果-供STI报表使用(纵向)
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="customerlist">客户列表信息</param>
        /// <returns></returns>
        public DataTable GenerateFincal(string sdt, string edt, string customerlist)
        {
            //输出结果集
            var result = tempDt.MakeExportDtTemp();
            //获取从SQL查询语句运行的记录集
            var sqldt = tempDt.GetSearchTempDt();
            //中间表-递归运算时使用
            var tempdt = tempDt.GetSearchTempDt();
            //获取收录‘客户’记录集
            var custdt = tempDt.GetCustomerNameListTempdt();
            //记录结束日期备注
            var remark1 = Convert.ToDateTime(edt).Year+"年"+Convert.ToDateTime(edt).Month+"月";

            try
            {
                //通过条件获取SQL并插入至sqldt临时表内
                sqldt = searchDt.SearchFinialRecord(sdt, edt, customerlist).Copy();

                //change date:20221015 当sqldt没有记录时才出异常提法
                if (sqldt.Rows.Count == 0) throw new Exception("没有明细记录");

                //从sqldt获取‘客户名称’信息至临时表(取唯一值)
                foreach (DataRow rows in sqldt.Rows)
                {
                    var newrow = custdt.NewRow();
                    if (custdt.Rows.Count == 0)
                    {
                        newrow[0] = Convert.ToString(rows[0]);
                    }
                    else
                    {
                        //判断若custdt.select()=0 才插入记录;
                        var dtlrows = custdt.Select("客户名称='" + rows[0] + "'").Length;
                        if (dtlrows > 0) continue;
                        newrow[0] = Convert.ToString(rows[0]);
                    }
                    custdt.Rows.Add(newrow);
                }

                var a1 = custdt.Copy();

                //当发现sqldt不只有一行时,才执行
                if(sqldt.Rows.Count >1)
                {
                    //循环处理(重) 期末余额=期未余额+本期应收-本期实收-本期冲销额 
                    foreach (DataRow row in custdt.Rows)
                    {
                        tempdt.Merge(GenerateReportDtlTemp(Convert.ToString(row[0]), sqldt, tempdt));
                    }
                }

                var a = tempdt.Copy();

                //处理数据并整理后将数据插入至result内
                foreach (DataRow custrow in custdt.Rows)
                {
                    //若在检测sqldt到只有1行,即插入至结果临时表
                    var sdtlrows = sqldt.Select("往来单位名称='" + Convert.ToString(custrow[0]) + "'");
                    if (sdtlrows.Length == 1)
                    {
                        result.Merge(GetResultDt(result,sdt,edt, Convert.ToString(custrow[0]),"",Convert.ToString(sdtlrows[0][2]),
                                                0,0, Convert.ToDecimal(sdtlrows[0][3]),remark1,null, 
                                                Convert.ToDecimal(sdtlrows[0][3]),null));
                    }
                    //当为多行时,才获取tempdt计算的数据
                    else
                    {
                        //根据customername,查询明细记录
                        var dtlrows = tempdt.Select("往来单位名称='" + Convert.ToString(custrow[0]) + "'");

                        for (var i = 0; i < dtlrows.Length; i++)
                        {
                            result.Merge(GetResultDt(result, sdt, edt, Convert.ToString(dtlrows[i][0]), Convert.ToString(dtlrows[i][1]),
                                                Convert.ToString(dtlrows[i][2]), Convert.ToDecimal(dtlrows[i][4]), Convert.ToDecimal(dtlrows[i][5]),
                                                Convert.ToDecimal(dtlrows[i][3]), remark1, Convert.ToString(dtlrows[i][7]), Convert.ToDecimal(dtlrows[i][8]),
                                                Convert.ToString(dtlrows[i][9])));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                result.Rows.Clear();
            }

            return result;
        }

        /// <summary>
        /// 整合最终结果
        /// </summary>
        /// <param name="tempdt"></param>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="customerName">往来单位名称</param>
        /// <param name="dt">单据日期-用于排序</param>
        /// <param name="remark">摘要</param>
        /// <param name="yibalance">本期应收</param>
        /// <param name="xibalance">本期实收</param>
        /// <param name="endbalance">期末余额</param>
        /// <param name="fbillno">单据编号</param>
        /// <param name="lastEndBalance">记录最后一行‘期末余额’</param>
        /// <param name="month">月份</param>
        /// <param name="remark1">记录结束日期备注</param>
        /// <returns></returns>
        private DataTable GetResultDt(DataTable tempdt, string sdt,string edt,string customerName, string dt, string remark
                                      ,decimal yibalance, decimal xibalance, decimal endbalance,string remark1,string fbillno
                                      ,decimal lastEndBalance, string month)
        {
            var newrow = tempdt.NewRow();
            newrow[0] = sdt;                               //开始日期
            newrow[1] = edt;                               //结束日期
            newrow[2] = customerName;                      //往来单位名称
            newrow[3] = dt;                                //单据日期-用于排序
            newrow[4] = remark;                            //摘要
            newrow[5] = remark == "期初余额" ? "" : Convert.ToString(Decimal.Round(yibalance,2)); //本期应收(当“摘要”为“期初余额”时,"本期应收"项为空显示)                 
            newrow[6] = remark == "期初余额" ? "" : Convert.ToString(Decimal.Round(xibalance,2)); //本期实收(当“摘要”为“期初余额”时,"本期实收"项为空显示)      
            newrow[7] = Convert.ToString(Decimal.Round(endbalance, 2));      //期末余额
            newrow[8] = remark1;                           //记录结束日期备注
            newrow[9] = fbillno;                           //单据编号
            newrow[10] = Decimal.Round(lastEndBalance,2);  //记录最后一行‘期末余额’
            newrow[11] = month;                            //月份
            newrow[12] = remark == "本期合计" ? "" : dt;    //单据日期-用于显示
            tempdt.Rows.Add(newrow);
            return tempdt;
        }

        /// <summary>
        /// 循环处理‘期末余额’ 
        /// 期末余额=期未余额+本期应收-本期实收-本期冲销额 
        /// </summary>
        /// <param name="customername">客户名称</param>
        /// <param name="sourcedt">SQL数据源</param>
        /// <param name="tempdt">临时表</param>
        /// <returns></returns>
        private DataTable GenerateReportDtlTemp(string customername,DataTable sourcedt,DataTable tempdt)
        {
            //'期末余额'中转值
            decimal balancetemp=0;
            //'本期应收'中转值
            decimal yitemp = 0;
            //‘本期实收’中转值
            decimal xitemp = 0;
            //‘月份’中转值
            var monthtemp = string.Empty;
            //‘单据日期’中转值
            var dt = string.Empty;

            //根据customername,查询明细记录
            var dtlrows = sourcedt.Select("往来单位名称='"+customername+"'");

            for (var i = 0; i < dtlrows.Length; i++)
            {
                //若检测到‘摘要’为‘期初余额’ 即将其值保存至balancetemp内
                //change date:20221013 当检测到‘摘要’为‘期初余额’时,也要收录至tempdt内
                if (Convert.ToString(dtlrows[i][2]) == "期初余额")
                {
                    balancetemp = Convert.ToDecimal(dtlrows[i][3]);
                    yitemp = Convert.ToDecimal(dtlrows[i][4]);
                    xitemp = Convert.ToDecimal(dtlrows[i][5]);

                    tempdt.Merge(InsertRecordToTempdt(tempdt, Convert.ToString(dtlrows[i][0]),
                        Convert.ToString(dtlrows[i][1]),
                        Convert.ToString(dtlrows[i][2]), balancetemp, Convert.ToDecimal(dtlrows[i][4]),
                        Convert.ToDecimal(dtlrows[i][5]),
                        Convert.ToDecimal(dtlrows[i][6]), Convert.ToString(dtlrows[i][7]), 0, Convert.ToString(dtlrows[i][8])));
                }
                //反之，先插入，然后根据公式计算出‘期末余额’后,将其值赋值至balancetemp内
                //change date:20221013 当发现月份与monthtemp不同时,插入“本期合计”行至临时表
                else
                {
                    //monthtemp为空 或 相同时执行
                    if (monthtemp == "" || monthtemp == Convert.ToString(dtlrows[i][8]))
                    {
                        tempdt.Merge(InsertRecordToTempdt(tempdt, Convert.ToString(dtlrows[i][0]), Convert.ToString(dtlrows[i][1]),
                        Convert.ToString(dtlrows[i][2]), balancetemp + Convert.ToDecimal(dtlrows[i][4]) - Convert.ToDecimal(dtlrows[i][5]) - Convert.ToDecimal(dtlrows[i][6])
                        , Convert.ToDecimal(dtlrows[i][4]), Convert.ToDecimal(dtlrows[i][5]),
                        Convert.ToDecimal(dtlrows[i][6]), Convert.ToString(dtlrows[i][7]), 0, Convert.ToString(dtlrows[i][8])));

                        //将当前行计算出来的‘期末余额’赋值给balancetemp,给下一行使用
                        balancetemp += Convert.ToDecimal(dtlrows[i][4]) - Convert.ToDecimal(dtlrows[i][5]) - Convert.ToDecimal(dtlrows[i][6]);

                        //记录本期应收 及 本期实收的累加值(新增‘本期合计’行时用到)
                        yitemp += Convert.ToDecimal(dtlrows[i][4]);
                        xitemp += Convert.ToDecimal(dtlrows[i][5]);
                        //记录日期-‘本期合计’行使用
                        dt = Convert.ToString(dtlrows[i][1]);
                        //当monthtemp为空时,将当前‘月份’赋值至monthtemp内
                        if(monthtemp=="")
                            monthtemp = Convert.ToString(dtlrows[i][8]);
                    }
                    //不相同时执行
                    else
                    {
                        //检测是否到不同月份时,1)插入上个月的'本期合计'相关记录 2)将当前行的值插入 3)将monthtemp赋值为当前行的月份 (注:插入后,将yitemp 及 xitemp赋值为当行前对应的值)
                        tempdt.Merge(InsertRecordToTempdt(tempdt, Convert.ToString(dtlrows[i][0]), dt,"本期合计", balancetemp, yitemp, xitemp,0, "", 0, monthtemp));

                        //将对当前行赋值
                        tempdt.Merge(InsertRecordToTempdt(tempdt, Convert.ToString(dtlrows[i][0]), Convert.ToString(dtlrows[i][1]),
                        Convert.ToString(dtlrows[i][2]), balancetemp + Convert.ToDecimal(dtlrows[i][4]) - Convert.ToDecimal(dtlrows[i][5]) - Convert.ToDecimal(dtlrows[i][6])
                        , Convert.ToDecimal(dtlrows[i][4]), Convert.ToDecimal(dtlrows[i][5]),
                        Convert.ToDecimal(dtlrows[i][6]), Convert.ToString(dtlrows[i][7]), 0, Convert.ToString(dtlrows[i][8])));

                        //1)将yitemp 及 xitemp清空 2) 重新对balancetemp 进行赋值 3)将monthtemp赋值为当前行的月份;将DT赋值为当前行'单据日期'
                        yitemp = Convert.ToDecimal(dtlrows[i][4]);
                        xitemp = Convert.ToDecimal(dtlrows[i][5]);
                        balancetemp += Convert.ToDecimal(dtlrows[i][4]) - Convert.ToDecimal(dtlrows[i][5]) - Convert.ToDecimal(dtlrows[i][6]);
                        monthtemp = Convert.ToString(dtlrows[i][8]);
                        dt = Convert.ToString(dtlrows[i][1]);
                    }
                }
            }
            //跳出循环后,插入最后一行值
            tempdt.Merge(InsertRecordToTempdt(tempdt, customername, dt, "本期合计", balancetemp, yitemp, xitemp, 0, "", balancetemp, monthtemp));
            return tempdt;
        }

        /// <summary>
        /// 将记录插入临时表
        /// </summary>
        /// <param name="tempdt">输出临时表</param>
        /// <param name="customerName">往来单位名称</param>
        /// <param name="dt">单据日期</param>
        /// <param name="remark">摘要</param>
        /// <param name="endbalance">期末余额</param>
        /// <param name="yibalance">本期应收</param>
        /// <param name="xibalance">本期实收</param>
        /// <param name="yuabalance">原币本期冲销额</param>
        /// <param name="fbillno">单据编号</param>
        /// <param name="lastEndBalance">记录最后一行‘期末余额</param>
        /// <param name="month">月份(用于STI报表排序)</param>
        /// <returns></returns>
        private DataTable InsertRecordToTempdt(DataTable tempdt,string customerName,string dt,string remark,decimal endbalance
                                               ,decimal yibalance,decimal xibalance,decimal yuabalance,string fbillno,decimal lastEndBalance,string month)
        {
            var newrow = tempdt.NewRow();
            newrow[0] = customerName;           //往来单位名称
            newrow[1] = dt;                     //单据日期
            newrow[2] = remark;                 //摘要
            newrow[3] = endbalance;             //期末余额
            newrow[4] = yibalance;              //本期应收
            newrow[5] = xibalance;              //本期实收
            newrow[6] = yuabalance;             //原币本期冲销额
            newrow[7] = fbillno;                //单据编号
            newrow[8] = lastEndBalance;         //判断,若不是最后一行时,LastEndBalance为0,反之,将balancetemp赋值给[8]
            newrow[9] = month;                  //月份(用于STI报表排序)
            tempdt.Rows.Add(newrow);
            return tempdt; 
        }


        /// <summary>
        /// 根据各参数运算结果-供STI报表"工业对账单生成"使用(横向)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public DataTable GenerateProduct(string sdt, string edt, string customerlist)
        {
            //输出结果集
            var resultdt = tempDt.MakeProductExportDtTemp();

            try
            {
                //根据相关条件获取‘应收单’列表记录
                var recorddt = searchDt.SearchProductCustomer(sdt,edt,customerlist).Copy();
                //循环recorddt,并获取相关值并插入至resultdt内
                foreach (DataRow rows in recorddt.Rows)
                {
                    var newrow = resultdt.NewRow();
                    newrow[0] = Convert.ToString(rows[1]);    //单据编号
                    newrow[1] = Convert.ToString(rows[2]);    //业务日期
                    newrow[2] = Convert.ToString(rows[3]);    //物料名称
                    newrow[3] = Convert.ToString(rows[4]);    //品牌
                    newrow[4] = Convert.ToString(rows[5]);    //规格型号
                    newrow[5] = Math.Round(Convert.ToDecimal(rows[6]),4);   //实发数量
                    newrow[6] = Math.Round(Convert.ToDecimal(rows[7]),4);   //计价数量
                    newrow[7] = Math.Round(Convert.ToDecimal(rows[8]),4);   //含税单价
                    newrow[8] = Math.Round(Convert.ToDecimal(rows[9]),4);   //价税合计
                    newrow[9] = Convert.ToInt32(rows[0]);     //客户ID
                    newrow[10] = sdt;   //开始日期
                    newrow[11] = edt;   //结束日期
                    newrow[12] = Convert.ToString(rows[10]);   //客户名称
                    resultdt.Rows.Add(newrow);
                }
            }
            catch (Exception)
            {
                resultdt.Rows.Clear();
            }
            return resultdt;
        }

        /// <summary>
        /// 根据各参数运算结果-供STI报表"销售发货清单"使用(纵向)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public DataTable GenerateSalesOutList(string sdt, string edt, string customerlist)
        {
            //输出结果集
            var resultdt = tempDt.MakeSalesOutListDtTemp();

            try
            {
                //根据相关条件获取‘应收单’列表记录
                var recorddt = searchDt.SearchSalesOutList(sdt, edt, customerlist);
                foreach (DataRow rows in recorddt.Rows)
                {
                    var newrow = resultdt.NewRow();
                    newrow[0] = Convert.ToInt32(rows[0]);    //客户ID
                    newrow[1] = Convert.ToInt32(rows[1]);    //应收单ID
                    newrow[2] = Convert.ToString(rows[3]);   //终端客户
                    newrow[3] = Convert.ToString(rows[4]);   //收货单位
                    newrow[4] = Convert.ToString(rows[5]);   //收货单位1
                    newrow[5] = Convert.ToString(rows[6]);   //二级客户
                    newrow[6] = Convert.ToString(rows[7]);   //三级客户
                    newrow[7] = Convert.ToString(rows[8]);   //摘要
                    newrow[8] = Convert.ToString(rows[9]);   //销售订单号
                    newrow[9] = Convert.ToString(rows[10]);  //日期
                    newrow[10] = Convert.ToString(rows[11]); //U订货单号
                    newrow[11] = Convert.ToString(rows[12]); //单据编号
                    newrow[12] = Convert.ToString(rows[13]); //托运货场地址
                    newrow[13] = Convert.ToString(rows[14]); //产品名称
                    newrow[14] = Convert.ToString(rows[15]); //规格
                    newrow[15] = Convert.ToInt32(rows[16]);  //实发罐数
                    newrow[16] = Math.Round(Convert.ToDecimal(rows[17]),2); //单价
                    newrow[17] = Convert.ToDouble(rows[18]);  //合同金额
                    newrow[18] = Convert.ToString(rows[19]);  //备注
                    newrow[19] = Convert.ToString(rows[20]);  //促销备注
                    newrow[20] = Convert.ToString(rows[21]);  //开票人
                    resultdt.Rows.Add(newrow);
                }
            }
            catch (Exception)
            {
                resultdt.Rows.Clear();
            }
            return resultdt;
        }

        /// <summary>
        /// '自定义批量导出'-运算执行
        /// 执行顺序:1)对账单  2)销售发货清单 
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="exportaddress">输出地址</param>
        /// <param name="customerlist">客户列表信息</param>
        /// <param name="duiprintpagenum">对账单打印次数</param>
        /// <param name="salesoutprintpagenum">销售发货清单打印次数</param>
        /// <returns></returns>
        public DataTable GenerateBatchexport(string sdt, string edt, string exportaddress, string customerlist,int duiprintpagenum, int salesoutprintpagenum)
        {
            //定义生成PDF返回是否成功标记
            var resultbool = true;
            //定义STI文件名称
            var stifilename = string.Empty;
            //定义执行结束时间
            var endtime = DateTime.Now;

            //定义‘对账单’收集SQL返回记录临时表
            var fincalK3Record = tempDt.GetSearchTempDt();
            //定义‘销售出库清单’收集SQL返回记录临时表
            var salesoutK3Record = tempDt.MakeSalesOutListDtTemp();

            //‘对账单’输出结果集
            var fincalresultdt = tempDt.BatchMakeExportDtTemp();
            //'销售出库清单'输出结果集
            var salesOutresultdt = tempDt.BatchMakeSalesOutListDtTemp();

            //记录‘自定义批量导出’返回结果-(即记录各个客户执行历史记录)
            var resultdt = tempDt.BatchGenerateResultDt();

            try
            {
                //根据customerlist获取K3客户记录
                var customerk3Dt = searchDt.GetSearchCustomerList(customerlist).Copy();

                //获取‘对账单’SQL记录(必须‘对账单打印次数’大于0时才会执行)
                if (duiprintpagenum > 0)
                {
                    fincalK3Record = searchDt.SearchFinialRecord(sdt, edt, customerlist).Copy();
                }

                //获取‘销售出库清单’SQL记录(必须‘销售出库清单打印次数’大于0时才会执行)
                if (salesoutprintpagenum > 0)
                {
                    salesoutK3Record = searchDt.SearchSalesOutList(sdt, edt, customerlist).Copy();
                }

                //循环customerK3Dt - 分别收集‘对账单’及‘销售发货清单’结果集
                foreach (DataRow rows in customerk3Dt.Rows)
                {
                    //针对每个客户中在‘对账单’‘销售发货清单’中的返回结果;每循环一个客户将重新创建该Dt对象
                    var tempdt = new DataTable();
                    //记录开始执行时间
                    var stime = DateTime.Now.ToLocalTime();

                    //循环执行顺序:(0)对账单->(1)销售发货清单,分别收集这两种单据类型的执行结果
                    for (var i = 0; i < 2; i++)
                    {
                        switch (i)
                        {
                            //‘对账单’使用,匹配条件:客户名称
                            case 0:
                                tempdt = GenerateFincalDtRecord(fincalresultdt,fincalK3Record,Convert.ToString(rows[2]),duiprintpagenum,sdt,edt).Copy();
                                endtime = DateTime.Now.ToLocalTime();
                                //若tempdt返回行数为0,即不插入
                                if(tempdt.Rows.Count == 0) continue;
                                 fincalresultdt.Merge(tempdt);
                                break;
                            //'销售出库清单'使用,匹配条件:Fcustid 
                            case 1:
                                tempdt = GenerateSalesoutlistDtRecord(salesOutresultdt, salesoutK3Record,salesoutprintpagenum).Copy();
                                endtime = DateTime.Now.ToLocalTime();
                                //若tempdt返回行数为0,即不插入
                                if (tempdt.Rows.Count == 0) continue;
                                    salesOutresultdt.Merge(tempdt);
                                break;
                        }
                        //执行插入历史记录临时表
                        resultdt.Merge(InsertHistoryToDt(tempdt,resultdt,Convert.ToString(rows[1]),Convert.ToString(rows[2]), stime, endtime, i));
                        var a1 = tempdt;
                    }
                }

                var b = fincalresultdt.Copy();
                var c = salesOutresultdt.Copy();

                //循环执行顺序:(0)对账单->(1)销售发货清单
                //注:‘对账单’批量一次性生成PDF  ‘销售出库清单’是根据‘客户ID’循环生成PDF,并且文件名为‘客户名称’+'生成日期'
                for (var i = 0; i < 2; i++)
                {
                    stifilename = i == 0 ? "BatchCustomerStatementReport.mrt" : "BatchSalesOutListReport.mrt";
                    var dt = i == 0 ? fincalresultdt.Copy() : salesOutresultdt.Copy();
                    resultbool = ExportDtToPdf(i, exportaddress, stifilename, customerk3Dt, dt);
                }
                //检测若resultbool最后的结果为false,即跳出异常
                if(!resultbool) throw new Exception("导出PDF产生异常");
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                resultdt.Rows.Clear();
            }

            return resultdt;
        }

        /// <summary>
        /// 将计算的DT输出至指定位置的PDF
        /// 注:‘对账单’批量一次性生成PDF  并且文件名为‘对账单’+'生成日期'
        ///    ‘销售出库清单’是根据‘客户ID’循环生成PDF,并且文件名为‘客户名称’+'生成日期'
        /// </summary>
        /// <param name="typeid">类型ID 0:'对账单' 1:'销售出库清单'</param>
        /// <param name="address">输出地址</param>
        /// <param name="filename">STI文件名</param>
        /// <param name="custdt">客户DT</param>
        /// <param name="resultdt">运行结果集DT</param>
        /// <returns></returns>
        private bool ExportDtToPdf(int typeid,string address,string filename,DataTable custdt,DataTable resultdt)
        {
            var result = true;
            var stiReport = new StiReport();
            var filepath = Application.StartupPath + "/Report/" + filename;

            try
            {
                //执行‘对账单’输出设置  STI文件里的DB名称:CustomerStatement
                if (typeid == 0)
                {
                    var pdfFileAddress = address + "\\" + "对账单_" + DateTime.Now.Date + ".pdf";

                    stiReport.Load(filepath);
                    stiReport.RegData("CustomerStatement", resultdt);
                    stiReport.Render(false);  //重点-没有这个生成的文件会提示“文件已损坏”
                    stiReport.ExportDocument(StiExportFormat.Pdf, pdfFileAddress); //生成指定格式文件
                }
                //执行‘销售发货清单’输出 STI文件里的DB名称:SalesOutList
                //根据custdt循环输出;输出文件名为‘客户名称’+'生成日期'
                else
                {
                    foreach (DataRow rows in custdt.Rows)
                    {
                        var pdfFileAddress = address + "\\" + Convert.ToString(rows[2]) +"_" + DateTime.Now.Date + ".pdf";

                        //根据Convert.ToInt32(rows[0]) 在resultdt查找,并最后整合记录至dt内
                        var dt = Getreportdt(Convert.ToInt32(rows[0]),resultdt).Copy();

                        stiReport.Load(filepath);
                        stiReport.RegData("SalesOutList", dt);
                        stiReport.Render(false);  //重点-没有这个生成的文件会提示“文件已损坏”
                        stiReport.ExportDocument(StiExportFormat.Pdf, pdfFileAddress); //生成指定格式文件
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 根据客户ID整合相关记录集-‘销售发货清单’使用
        /// </summary>
        /// <param name="fcustid">客户ID</param>
        /// <param name="sourcedt">结果集DT</param>
        /// <returns></returns>
        private DataTable Getreportdt(int fcustid,DataTable sourcedt)
        {
            var resultdt = sourcedt.Clone();
            var dtlrows = sourcedt.Select("FCUSTID='"+fcustid+"'");

            for (var i = 0; i < dtlrows.Length; i++)
            {
                var newrow = resultdt.NewRow();
                newrow[0] = Convert.ToInt32(dtlrows[i][0]);                    //客户ID
                newrow[1] = Convert.ToInt32(dtlrows[i][1]);                    //应收单ID
                newrow[2] = Convert.ToString(dtlrows[i][2]);                   //终端客户
                newrow[3] = Convert.ToString(dtlrows[i][3]);                   //收货单位
                newrow[4] = Convert.ToString(dtlrows[i][4]);                   //收货单位1
                newrow[5] = Convert.ToString(dtlrows[i][5]);                   //二级客户
                newrow[6] = Convert.ToString(dtlrows[i][6]);                   //三级客户
                newrow[7] = Convert.ToString(dtlrows[i][7]);                   //摘要
                newrow[8] = Convert.ToString(dtlrows[i][8]);                   //销售订单号
                newrow[9] = Convert.ToString(dtlrows[i][9]);                   //日期
                newrow[10] = Convert.ToString(dtlrows[i][10]);                 //U订货单号
                newrow[11] = Convert.ToString(dtlrows[i][11]);                 //单据编号
                newrow[12] = Convert.ToString(dtlrows[i][12]);                 //托运货场地址
                newrow[13] = Convert.ToString(dtlrows[i][13]);                 //产品名称
                newrow[14] = Convert.ToString(dtlrows[i][14]);                 //规格
                newrow[15] = Convert.ToInt32(dtlrows[i][15]);                  //实发罐数
                newrow[16] = Math.Round(Convert.ToDecimal(dtlrows[i][16]), 2); //单价
                newrow[17] = Convert.ToDouble(dtlrows[i][17]);                 //合同金额
                newrow[18] = Convert.ToString(dtlrows[i][18]);                 //备注
                newrow[19] = Convert.ToString(dtlrows[i][19]);                 //促销备注
                newrow[20] = Convert.ToString(dtlrows[i][20]);                 //开票人
                newrow[21] = Convert.ToInt32(dtlrows[i][21]);                  //作用:对相同客户的区分显示(当要针对相同客户打印多次时)
                resultdt.Rows.Add(newrow);
            }
            return resultdt;
        }

        /// <summary>
        /// '对账单'所表所需数据生成
        /// 运算核心:针对同一个客户,只运算一次将结果保存至tempdt内,然后通过printpaagenum ('对账单'打印次数) 进行循环插入至resultdt
        /// </summary>
        /// <param name="resultdt">结果集临时表（作用:STI对账单报表使用）</param>
        /// <param name="k3Record">K3获取数据结果集</param>
        /// <param name="customername">客户名称</param>
        /// <param name="printpagenum">'对账单'打印次数</param>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <returns></returns>
        private DataTable GenerateFincalDtRecord(DataTable resultdt,DataTable k3Record,string customername,int printpagenum,string sdt,string edt)
        {
            try
            {
                //记录结束日期备注
                var remark1 = Convert.ToDateTime(edt).Year + "年" + Convert.ToDateTime(edt).Month + "月";
                //若printpagenum-打印数量为0,即跳出异常
                if(printpagenum == 0) throw new Exception("因'对账单'打印次数为0,故不能生成文件");
                //若k3Record 为空,即跳出异常
                if(k3Record.Rows.Count == 0) throw new Exception("没有K3对应的返回记录,故不能生成文件");
                //中间表-递归运算时使用(与K3Record的表结构一致)
                var tempdt = k3Record.Clone();

                //当发现k3Record不只有一行时才执行;tempdt为运算后的记录集(重)
                if (k3Record.Rows.Count > 1)
                {
                    //期末余额=期未余额+本期应收-本期实收-本期冲销额 
                    tempdt.Merge(GenerateReportDtlTemp(customername,k3Record,tempdt));
                }

                var a = tempdt.Copy();

                //循环printpagenum(对账单打印次数)，并将记录插入至resultdt内
                //处理数据并整理后将数据插入至resultdt内
                //若在检测k3Record到只有1行,即插入至结果临时表
                for (var i = 0; i < printpagenum; i++)
                {
                    var sdtlows = k3Record.Select("往来单位名称='" + customername + "'");
                    if (sdtlows.Length == 1)
                    {
                        resultdt.Merge(GetBatchResultDt(resultdt, sdt, edt, customername, "", Convert.ToString(sdtlows[0][2]),
                                                0, 0, Convert.ToDecimal(sdtlows[0][3]), remark1, null,
                                                Convert.ToDecimal(sdtlows[0][3]), null,i));
                    }
                    else
                    {
                        //根据customername,查询明细记录
                        var dtlrows = tempdt.Select("往来单位名称='" + customername + "'");
                        for (var j = 0; j < dtlrows.Length; j++)
                        {
                            resultdt.Merge(GetBatchResultDt(resultdt, sdt, edt, Convert.ToString(dtlrows[j][0]), Convert.ToString(dtlrows[j][1]),
                                                Convert.ToString(dtlrows[j][2]), Convert.ToDecimal(dtlrows[j][4]), Convert.ToDecimal(dtlrows[j][5]),
                                                Convert.ToDecimal(dtlrows[j][3]), remark1, Convert.ToString(dtlrows[j][7]), Convert.ToDecimal(dtlrows[j][8]),
                                                Convert.ToString(dtlrows[j][9]),i));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //收集异常信息
                Errormessage = ex.Message;
                resultdt.Rows.Clear();
            }

            return resultdt;
        }

        /// <summary>
        /// 对账单-‘自定义批量导出’功能使用
        /// </summary>
        /// <param name="tempdt">输出临时表</param>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="customerName">往来单位名称</param>
        /// <param name="dt">单据日期-用于排序</param>
        /// <param name="remark">摘要</param>
        /// <param name="yibalance">本期应收</param>
        /// <param name="xibalance">本期实收</param>
        /// <param name="endbalance">期末余额</param>
        /// <param name="fbillno">单据编号</param>
        /// <param name="lastEndBalance">月份</param>
        /// <param name="month">月份</param>
        /// <param name="remark1">记录结束日期备注</param>
        /// <param name="fRowId">对相同客户的区分显示(当要针对相同客户打印多次时)</param>
        /// <returns></returns>
        private DataTable GetBatchResultDt(DataTable tempdt, string sdt, string edt, string customerName, string dt, string remark
                              , decimal yibalance, decimal xibalance, decimal endbalance, string remark1, string fbillno
                              , decimal lastEndBalance, string month,int fRowId)
        {
            var newrow = tempdt.NewRow();
            newrow[0] = sdt;                               //开始日期
            newrow[1] = edt;                               //结束日期
            newrow[2] = customerName;                      //往来单位名称
            newrow[3] = dt;                                //单据日期-用于排序
            newrow[4] = remark;                            //摘要
            newrow[5] = remark == "期初余额" ? "" : Convert.ToString(Decimal.Round(yibalance, 2)); //本期应收(当“摘要”为“期初余额”时,"本期应收"项为空显示)                 
            newrow[6] = remark == "期初余额" ? "" : Convert.ToString(Decimal.Round(xibalance, 2)); //本期实收(当“摘要”为“期初余额”时,"本期实收"项为空显示)      
            newrow[7] = Convert.ToString(Decimal.Round(endbalance, 2));      //期末余额
            newrow[8] = remark1;                           //记录结束日期备注
            newrow[9] = fbillno;                           //单据编号
            newrow[10] = Decimal.Round(lastEndBalance, 2);  //记录最后一行‘期末余额’
            newrow[11] = month;                            //月份
            newrow[12] = remark == "本期合计" ? "" : dt;    //单据日期-用于显示
            newrow[13] = fRowId;                           //对相同客户的区分显示(当要针对相同客户打印多次时)
            tempdt.Rows.Add(newrow);
            return tempdt;
        }


        /// <summary>
        /// '销售发货清单'报表所需数据生成
        /// </summary>
        /// <param name="resultdt">结果集临时表（作用:STI'销售出库清单'报表使用）</param>
        /// <param name="salesoutK3Record">K3获取数据结果集</param>
        /// <param name="salesoutprintpagenum">'销售出库清单'打印次数</param>
        /// <returns></returns>
        private DataTable GenerateSalesoutlistDtRecord(DataTable resultdt, DataTable salesoutK3Record, int salesoutprintpagenum)
        {
            try
            {
                //若'销售出库清单'打印次数为0,即跳出异常
                if(salesoutprintpagenum == 0) throw new Exception("因'销售出库清单'打印次数为0,故不能生成文件");
                //根据salesoutK3Record循环(若为0,即插入错误信息并continue)
                if(salesoutK3Record.Rows.Count == 0) throw new Exception("没有K3对应的返回记录,故不能生成文件");

                //根据salesoutprintpagenum循环插入记录至resultdt
                for (var i = 0; i < salesoutprintpagenum; i++)
                {
                    foreach (DataRow rows in salesoutK3Record.Rows)
                    {
                        var newrow = resultdt.NewRow();
                        newrow[0] = Convert.ToInt32(rows[0]);    //客户ID
                        newrow[1] = Convert.ToInt32(rows[1]);    //应收单ID
                        newrow[2] = Convert.ToString(rows[3]);   //终端客户
                        newrow[3] = Convert.ToString(rows[4]);   //收货单位
                        newrow[4] = Convert.ToString(rows[5]);   //收货单位1
                        newrow[5] = Convert.ToString(rows[6]);   //二级客户
                        newrow[6] = Convert.ToString(rows[7]);   //三级客户
                        newrow[7] = Convert.ToString(rows[8]);   //摘要
                        newrow[8] = Convert.ToString(rows[9]);   //销售订单号
                        newrow[9] = Convert.ToString(rows[10]);  //日期
                        newrow[10] = Convert.ToString(rows[11]); //U订货单号
                        newrow[11] = Convert.ToString(rows[12]); //单据编号
                        newrow[12] = Convert.ToString(rows[13]); //托运货场地址
                        newrow[13] = Convert.ToString(rows[14]); //产品名称
                        newrow[14] = Convert.ToString(rows[15]); //规格
                        newrow[15] = Convert.ToInt32(rows[16]);  //实发罐数
                        newrow[16] = Math.Round(Convert.ToDecimal(rows[17]), 2); //单价
                        newrow[17] = Convert.ToDouble(rows[18]);  //合同金额
                        newrow[18] = Convert.ToString(rows[19]);  //备注
                        newrow[19] = Convert.ToString(rows[20]);  //促销备注
                        newrow[20] = Convert.ToString(rows[21]);  //开票人
                        newrow[21] = i;                           //作用:对相同客户的区分显示(当要针对相同客户打印多次时)
                        resultdt.Rows.Add(newrow);
                    }
                }

            }
            catch (Exception ex)
            {
                Errormessage = ex.Message;
                resultdt.Rows.Clear();
            }
            return resultdt;
        }



        /// <summary>
        /// 将每个客户的执行结果插入至操作日志记录表内
        /// </summary>
        /// <param name="tempdt">中间结果临时表;主要用于收集‘对账单’及‘销售发货清单’记录集</param>
        /// <param name="resultdt">返回DT</param>
        /// <param name="customercode">客户编码</param>
        /// <param name="customername">客户名称</param>
        /// <param name="stime">开始执行时间</param>
        /// <param name="endtime">结束执行时间</param>
        /// <param name="ordertypeid">单据类型;0:对账单  1:销售发货清单</param>
        /// <returns></returns>
        private DataTable InsertHistoryToDt(DataTable tempdt,DataTable resultdt, string customercode,string customername,DateTime stime,DateTime endtime, int ordertypeid)
        {
            var result = tempdt.Rows.Count == 0 ? "执行异常,原因:" + Errormessage : "执行成功,请到导出地址进行查阅.";

            var newrow = resultdt.NewRow();
            newrow[0] = customercode;                     //客户编码
            newrow[1] = customername;                     //客户名称
            newrow[2] = stime;                            //开始执行时间
            newrow[3] = endtime;                          //结束执行时间
            newrow[4] = result;                           //执行结果
            newrow[5] = Convert.ToString(ordertypeid);    //导出单据类型-(0)'对帐单' (1)‘销售出库清单’
            newrow[6] = !result.Contains("成功") ? 1 : 0; //是否成功标记(0:是 1:否)
            resultdt.Rows.Add(newrow);
            return resultdt;
        }

    }
}
