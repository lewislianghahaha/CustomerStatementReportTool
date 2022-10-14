using System;
using System.Data;
using CustomerStatementReportTool.DB;

namespace CustomerStatementReportTool.Task
{
    //运算
    public class Generate
    {
        SearchDt searchDt=new SearchDt();
        TempDtList tempDt=new TempDtList();

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

                //若发现sqldt只有一行摘要为“期初余额”，即不能继续进行处理
                if (sqldt.Rows.Count == 1 && Convert.ToString(sqldt.Rows[0][2]) == "期初余额") throw new Exception("没有明细记录");

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

                //循环处理(重) 期末余额=期未余额+本期应收-本期实收-本期冲销额 
                foreach (DataRow row in custdt.Rows)
                {
                    tempdt.Merge(GenerateReportDtlTemp(Convert.ToString(row[0]),sqldt,tempdt));
                }

                var a = tempdt.Copy();

                //处理数据并整理后将数据插入至result内
                foreach (DataRow custrow in custdt.Rows)
                {
                    //根据customername,查询明细记录
                    var dtlrows = tempdt.Select("往来单位名称='" + Convert.ToString(custrow[0]) + "'");

                    for (var i = 0; i < dtlrows.Length; i++)
                    {
                        var newrow = result.NewRow();
                        newrow[0] = sdt;                              //开始日期
                        newrow[1] = edt;                              //结束日期
                        newrow[2] = Convert.ToString(dtlrows[i][0]);  //往来单位名称
                        newrow[3] = Convert.ToString(dtlrows[i][1]); //单据日期-用于排序
                        newrow[4] = Convert.ToString(dtlrows[i][2]);  //摘要
                        newrow[5] = Convert.ToDecimal(dtlrows[i][4]); //本期应收
                        newrow[6] = Convert.ToDecimal(dtlrows[i][5]); //本期收款
                        newrow[7] = Convert.ToDecimal(dtlrows[i][3]); //期末余额
                        newrow[8] = remark1;                          //记录结束日期备注
                        newrow[9] = Convert.ToString(dtlrows[i][7]);  //单据编号
                        newrow[10] = Convert.ToDecimal(dtlrows[i][8]); //记录最后一行‘期末余额’
                        newrow[11] = Convert.ToString(dtlrows[i][9]);  //月份
                        newrow[12] = Convert.ToString(dtlrows[i][2]) == "本期合计" ? "" : Convert.ToString(dtlrows[i][1]); ;//单据日期-用于显示
                        result.Rows.Add(newrow);
                    }
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                result.Columns.Clear();
            }

            return result;
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
                resultdt.Columns.Clear();
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
                resultdt.Columns.Clear();
            }
            return resultdt;
        }

    }
}
