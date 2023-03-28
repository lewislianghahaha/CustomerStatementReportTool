using System;
using System.Data;
using System.Windows.Forms;
using CustomerStatementReportTool.DB;
using Stimulsoft.Report;

//合拼输出 及(按‘季度’导出 或 按‘年份’导出)使用
namespace CustomerStatementReportTool.Task
{
    public class MixGenerate
    {
        SearchDt searchDt=new SearchDt();
        Generate generate = new Generate();
        TempDtList tempDt=new TempDtList();

        /// <summary>
        /// 合拼生成-按‘季度’导出 或 按‘年份’导出使用
        /// </summary>
        /// <param name="genid">运算类别:0=>按‘季度’导出使用 1=>按‘年度’导出使用</param>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="exportaddress">输出地址</param>
        /// <param name="customerlist">客户列表信息</param>
        /// <param name="custdtlist">接收前端客户DT</param>
        public void GenerateMixExport(int genid,string sdt, string edt, string exportaddress, string customerlist,DataTable custdtlist)
        {
            try
            {
                //定义循环值,fsortid使用
                var id = 0;
                //定义STI使用的排序ID; --STI表头排序ID;注:以A开始,若ID值小于10 即加0,如:A01
                var fsortid = "A00";
                //'销售发货清单'打印次数(默认:1)
                var salesoutprintpagenum = 1;
                //'签收确认单'打印次数(默认:1)
                var confirmprintpagenum = 1;

                ////////////////////////////SQL数据源定义////////////////////////////////////////
                //定义‘签收确定单’收集SQL语句返回记录集临时表
                var fincalK3Record = tempDt.GetSearchTempDt();
                //定义‘销售出库清单’收集SQL返回记录集临时表
                var salesoutK3Record = tempDt.MakeSalesOutListDtTemp();

                ////////////////////////////结果集定义/////////////////////////////////////////
                //'销售出库清单'输出结果集
                var salesOutresultdt = tempDt.BatchMakeSalesOutListDtTemp();
                //‘签收确认单’中转使用
                var fincalresultdt = tempDt.BatchMakeExportDtTemp();
                //'签收确认单'输出结果集
                var confirmresultdt = tempDt.BatchMakeConfirmDtTemp();

                //接收从前端导入的客户DT,用于在不改变导入顺序的前提下获取对应的FCUSTID,并整合成customerk3Dt
                var customerk3Dt = generate.GetSearchCustomerList(custdtlist).Copy();

                //获取‘销售出库清单’所需SQL数据源记录-(按'季度‘功能导出才需要)
                if (genid == 0)
                {
                    salesoutK3Record = searchDt.SearchSalesOutList(sdt, edt, customerlist).Copy();
                }

                //获取‘签收确定单’所需SQL数据源记录-('季度'‘年度’功能导出也需要)
                fincalK3Record = searchDt.SearchFinialRecord(sdt, edt, customerlist).Copy();

                //循环customerK3Dt - 分别收集‘对账单’及‘销售发货清单’结果集
                foreach (DataRow rows in customerk3Dt.Rows)
                {
                    /////////////////////////////////////////////执行'销售出库清单'////////////////////////////////////////////////////
                    
                    //'销售出库清单'使用,匹配条件:Fcustid 
                    var tempdt1 = salesOutresultdt.Clone();

                    tempdt1 = generate.GenerateSalesoutlistDtRecord(tempdt1, salesoutK3Record, salesoutprintpagenum, Convert.ToInt32(rows[0]),
                                                            sdt, edt, Convert.ToString(rows[2])).Copy();

                    //若tempdt1返回行数不为0,才插入
                    if (tempdt1.Rows.Count > 0) { salesOutresultdt.Merge(tempdt1); }

                    /////////////////////////////////////////////执行'签收确认单'////////////////////////////////////////////////////
                    
                    //‘签收确认单’使用,匹配条件:客户编码
                    var tempdt2 = fincalresultdt.Clone();

                    tempdt2 = generate.GenerateFincalDtRecord(tempdt2, fincalK3Record, Convert.ToString(rows[2]), confirmprintpagenum, sdt, edt
                                                    , fsortid, Convert.ToString(rows[1]), Convert.ToString(rows[3])).Copy();

                    //若tempdt2返回行数不为0,才插入
                    if (tempdt2.Rows.Count > 0) { fincalresultdt.Merge(tempdt2); }

                    //每循环一次将fsortid自增1
                    id++;
                    //根据ID判断设置fsortid值
                    fsortid = id < 10 ? "A" + "0" + id : "A" + id;
                }

                //当fincalresultdt1行数大于0时,得出相关结果并赋值到confirmresultdt内,供‘签收确认单’打印模板使用
                if (fincalresultdt.Rows.Count > 0)
                {
                    confirmresultdt = generate.GetConfirmReportDt(confirmresultdt, fincalresultdt, customerk3Dt).Copy();
                }

                //按‘季度’导出使用-主要使用单据->1.‘签收确定单’ 2.‘销售发货清单’
                //合拼输出
                if (genid == 0)
                {
                    ExportDtToMixPdf(exportaddress, customerk3Dt, fincalresultdt.Clone(), confirmresultdt, salesOutresultdt);
                }
                //按‘年度’导出使用-主要使用单据->‘签收确定单’
                //有合拼 及 拆分输出
                else
                {
                    //拆分输出
                    if (!GlobalClasscs.RmMessage.IsuseYearMixExport)
                    {
                        ExportDtToSplitPdf(exportaddress, confirmresultdt, customerk3Dt);
                    }
                    //合拼输出
                    else
                    {
                        ExportDtToMixPdf(exportaddress,customerk3Dt, fincalresultdt.Clone(), confirmresultdt,salesOutresultdt.Clone());
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalClasscs.RmMessage.Errormesage = ex.Message;
            }
        }

        /// <summary>
        /// 生成STI对象(合拼输出前期准备)
        /// </summary>
        /// <param name="stiname">STI结果集名称</param>
        /// <param name="dt">数据源</param>
        /// <param name="mrtfilename">MRT文件名称</param>
        /// <returns></returns>
        private StiReport GetReport(String stiname,DataTable dt,String mrtfilename)
        {
            var stiReport = new StiReport();
            var reportPath = Application.StartupPath + "/Report/" + mrtfilename;
            stiReport.Load(reportPath);
            stiReport.RegData(stiname,dt);
            return stiReport;
        }

        /// <summary>
        /// 合并生成PDF代码 Mix (重)
        /// </summary>
        /// <param name="exportaddress">导出地址</param>
        /// <param name="customerk3Dt">K3客户DT</param>
        /// <param name="fincalresultdt">对账单结果集</param>
        /// <param name="confirmresultdt">签收确定单结果集</param>
        /// <param name="salesOutresultdt">销售出库清单结果集</param>
        public bool ExportDtToMixPdf(String exportaddress,DataTable customerk3Dt, DataTable fincalresultdt, DataTable confirmresultdt,DataTable salesOutresultdt)
        {
            var result = true;

            var pdfFileAddress = "";
            var stiname = "";
            var mrtfilename = "";
            var date = DateTime.Now.ToString("yyyy-MM-dd");

            var dt = new DataTable();
            //收集最终的导出结果
            var stiFinalreport = new StiReport();

            try
            {
                //循环customerk3Dt,并以1.对账单 2.签收确定单 3.销售出库清单 的执行顺序进行导出
                //其中‘对账单’‘签收确定单’以客户编码(rows[1])为获取数据条件; ‘销售出库清单’以客户ID(rows[0])为获取数据条件;
                foreach (DataRow rows in customerk3Dt.Rows)
                {
                    var stireport = new StiReport();

                    for (var i = 0; i < 3; i++)
                    {
                        switch (i)
                        {
                            //对账单-以客户编码(rows[1])为获取数据条件
                            case 0:
                                stiname = "CustomerStatement";
                                dt = generate.GetSecondcustomerreportdt(Convert.ToString(rows[1]), fincalresultdt).Copy();
                                mrtfilename = "BatchCustomerStatementReport.mrt";
                                break;
                            //签收确定单-以客户编码(rows[1])为获取数据条件
                            case 1:
                                stiname = "CustomerStatement";
                                dt = GetConfirmCustomerReportDt(0,"0",Convert.ToString(rows[1]), confirmresultdt).Copy();
                                mrtfilename = "BatchConfirmReport.mrt";
                                break;
                            //销售出库清单-以客户ID(rows[0])为获取数据条件
                            case 2:
                                stiname = "SalesOutList";
                                dt = generate.Getreportdt(Convert.ToInt32(rows[0]), salesOutresultdt).Copy();
                                mrtfilename = "BatchSalesOutListReport.mrt";
                                break;    
                        }
                        //将整理后的STIREPORT添加至stireport内
                        if (dt.Rows.Count > 0)
                        {
                            //渲染报表添加
                            stireport.SubReports.Add(GetReport(stiname, dt, mrtfilename));
                        }
                    }
                    //渲染报表添加;循环完一个客户后,将stireport添加至stiFinalreport内
                    stiFinalreport.SubReports.Add(stireport);
                }
                //输出
                pdfFileAddress = exportaddress + "\\" + "合拼输出记录_(" + date + ").pdf";
                stiFinalreport.Render(false);
                stiFinalreport.ExportDocument(StiExportFormat.Pdf, pdfFileAddress);
            }
            catch (Exception ex)
            {
                GlobalClasscs.RmMessage.Printerrmessge = ex.Message;
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 拆分生成PDF代码（注:按照‘客户编码’&&‘月份’为条件进行拆分-按‘年份’导出使用） Split
        /// </summary>
        /// <param name="exportaddress">导出地址</param>
        /// <param name="confirmresultdt">签收确定单结果集</param>
        /// <param name="customerk3Dt">K3客户DT</param>
        private bool ExportDtToSplitPdf(String exportaddress, DataTable confirmresultdt,DataTable customerk3Dt)
        {
            var result = true;
            var stiReport = new StiReport();

            try
            {
                var filepath = Application.StartupPath + "/Report/BatchConfirmReport.mrt";

                //以‘客户编码’ ‘月份’作为循环条件
                foreach (DataRow rows in customerk3Dt.Rows)
                {
                    for (var i = 1; i < 13; i++)
                    {
                        if(confirmresultdt.Select("customercode='"+ Convert.ToString(rows[1])+"' and Month='"+Convert.ToString(i)+"'").Length==0)continue;
                        
                        var pdfFileAddress = exportaddress + "\\" + $"签收确定单_客户("+ Convert.ToString(rows[2]) +$")_第{i}月份记录.pdf";
                        var dt = GetConfirmCustomerReportDt(1,Convert.ToString(i),Convert.ToString(rows[1]), confirmresultdt).Copy();
                        if (dt.Rows.Count > 0)
                        {
                            stiReport.Load(filepath);
                            stiReport.RegData("CustomerStatement", dt);
                            stiReport.Render(false);
                            stiReport.ExportDocument(StiExportFormat.Pdf, pdfFileAddress);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalClasscs.RmMessage.Printerrmessge = ex.Message;
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 按不同情况对‘签收确定单’进行数据整理
        /// </summary>
        /// <param name="typeid">类型:0=>以‘客户编码’为条件进行获取(合拼时使用) 1=>以‘客户编码’及‘月份’为条件进行获取(拆分时使用)</param>
        /// <param name="month">月份</param>
        /// <param name="customercode">客户编码</param>
        /// <param name="sourcedt">数据源</param>
        /// <returns></returns>
        private DataTable GetConfirmCustomerReportDt(int typeid,string month,string customercode, DataTable sourcedt)
        {
            DataRow[] dtlrows = null;

            var resultdt = sourcedt.Clone();
            dtlrows = typeid == 0 ? sourcedt.Select("customercode='" + customercode + "'") : sourcedt.Select("customercode='" + customercode + "' and Month='" + month + "'");

            for (var i = 0; i < dtlrows.Length; i++)
            {
                var newrow = resultdt.NewRow();
                newrow[0] = Convert.ToString(dtlrows[i][0]); //开始日期
                newrow[1] = Convert.ToString(dtlrows[i][1]); //结束日期
                newrow[2] = Convert.ToString(dtlrows[i][2]); //往来单位名称
                newrow[3] = Convert.ToString(dtlrows[i][3]); //单据日期
                newrow[4] = Convert.ToString(dtlrows[i][4]); //摘要
                newrow[5] = Convert.ToString(dtlrows[i][5]); //本期应收
                newrow[6] = Convert.ToString(dtlrows[i][6]); //本期收款
                newrow[7] = Convert.ToString(dtlrows[i][7]); //期末余额
                newrow[8] = Convert.ToString(dtlrows[i][8]); //记录结束日期备注
                newrow[9] = Convert.ToString(dtlrows[i][9]); //单据编号
                newrow[10] = Math.Round(Convert.ToDecimal(dtlrows[i][10]), 2); //记录最后一行‘期末余额’
                newrow[11] = Convert.ToString(dtlrows[i][11]); //月份
                newrow[12] = Convert.ToString(dtlrows[i][12]); //单据日期-用于显示
                newrow[13] = Convert.ToString(dtlrows[i][13]); //对相同客户的区分显示(作用:针对相同客户打印多次时)
                newrow[14] = Convert.ToString(dtlrows[i][14]); //STI排序ID;注:以A开始,若ID值小于10 即加0,如:A01
                newrow[15] = Convert.ToString(dtlrows[i][15]); //用于STI报表明细行排序
                newrow[16] = Convert.ToString(dtlrows[i][16]); //客户开票名称-二级客户对账单.核算项目名称使用
                newrow[17] = Convert.ToString(dtlrows[i][17]); //客户编码
                newrow[18] = Convert.ToString(dtlrows[i][18]); //记录‘总期末余额’（以千位符进行分隔,在STI报表显示）
                newrow[19] = Convert.ToString(dtlrows[i][19]); //记录'签收日期'
                resultdt.Rows.Add(newrow);
            }
            return resultdt;
        }

    }
}
