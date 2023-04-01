using System;
using System.Data;
using System.Windows.Forms;
using CustomerStatementReportTool.DB;
using Stimulsoft.Report;

//(按‘季度’导出 或 按‘年份’导出)使用
namespace CustomerStatementReportTool.Task
{
    public class QuartYearOutGenerate
    {
        SearchDt searchDt=new SearchDt();
        TempDtList tempDt=new TempDtList();
        Generate generate=new Generate();
        MixDtToPdf mixDtToPdf=new MixDtToPdf();

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
                    mixDtToPdf.ExportDtToMixPdf(1,exportaddress, customerk3Dt, fincalresultdt.Clone(), confirmresultdt, salesOutresultdt);
                }
                //按‘年度’导出使用-主要使用单据->‘签收确定单’
                //有合拼 及 拆分输出
                else
                {
                    //合拼输出
                    if (GlobalClasscs.RmMessage.IsuseYearMixExport)
                    {
                        mixDtToPdf.ExportDtToMixPdf(2,exportaddress, customerk3Dt, fincalresultdt.Clone(), confirmresultdt, salesOutresultdt.Clone());
                    }
                    //拆分输出
                    else
                    {
                        ExportDtToSplitPdf(exportaddress, confirmresultdt, customerk3Dt);
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalClasscs.RmMessage.Errormesage = ex.Message;
            }
        }

        /// <summary>
        /// 拆分生成PDF代码（注:按照‘客户编码’&&‘月份’为条件进行拆分-按‘年份’导出功能使用） Split
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
                        
                        var pdfFileAddress = exportaddress + "\\" + $"客户(" + Convert.ToString(rows[2]) +$")_签收确定单_第{i}月份记录.pdf";
                        var dt = mixDtToPdf.GetConfirmCustomerReportDt(1,Convert.ToString(i),Convert.ToString(rows[1]), confirmresultdt).Copy();

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
    }
}
