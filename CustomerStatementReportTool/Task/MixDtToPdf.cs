using System;
using System.Data;
using System.Windows.Forms;
using Stimulsoft.Report;

//合拼导出生成PDF
namespace CustomerStatementReportTool.Task
{
    public class MixDtToPdf
    {
        /// <summary>
        /// 合并生成PDF代码 Mix (重)
        /// </summary>
        /// <param name="genid">生成类型=>0:自定义批量导出 1:按季度 2:按年度</param>
        /// <param name="exportaddress">导出地址</param>
        /// <param name="customerk3Dt">K3客户DT</param>
        /// <param name="fincalresultdt">对账单结果集</param>
        /// <param name="confirmresultdt">签收确定单结果集</param>
        /// <param name="salesOutresultdt">销售出库清单结果集</param>
        public bool ExportDtToMixPdf(int genid,String exportaddress, DataTable customerk3Dt, DataTable fincalresultdt, DataTable confirmresultdt, DataTable salesOutresultdt)
        {
            var result = true;
            var pdffilename = "";
            var pdfFileAddress = "";
            var stiname = "";
            var mrtfilename = "";
            var date = DateTime.Now.ToString("yyyy-MM-dd");

            var dt = new DataTable();
            //收集最终的导出结果
            var stiFinalreport = new StiReport();
            var stireport = new StiReport();

            try
            {
                //循环customerk3Dt,并以1.对账单 2.签收确定单 3.销售出库清单 的执行顺序进行导出
                //其中‘对账单’‘签收确定单’以客户编码(rows[1])为获取数据条件; ‘销售出库清单’以客户ID(rows[0])为获取数据条件;
                foreach (DataRow rows in customerk3Dt.Rows)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        switch (i)
                        {
                            //对账单-以客户编码(rows[1])为获取数据条件
                            case 0:
                                stiname = "CustomerStatement";
                                dt = GetFincalreportdt(Convert.ToString(rows[1]), fincalresultdt).Copy();
                                mrtfilename = "BatchCustomerStatementReport.mrt";
                                break;
                            //签收确定单-以客户编码(rows[1])为获取数据条件
                            case 1:
                                stiname = "CustomerStatement";
                                dt = GetConfirmCustomerReportDt(0, "0", Convert.ToString(rows[1]), confirmresultdt).Copy();
                                mrtfilename = "BatchConfirmReport.mrt";
                                break;
                            //销售出库清单-以客户ID(rows[0])为获取数据条件
                            case 2:
                                stiname = "SalesOutList";
                                dt = GetSalesOutdt(Convert.ToInt32(rows[0]), salesOutresultdt).Copy();
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
                switch (genid)
                {
                    case 0:
                        pdffilename = "合拼输出记录_自定义批量导出_(" + date + ").pdf";
                        break;
                    case 1:
                        pdffilename = "合拼输出记录_按季度_(" + date + ").pdf";
                        break;
                    case 2:
                        pdffilename = "合拼输出记录_按年度_(" + date + ").pdf";
                        break;
                }

                pdfFileAddress = exportaddress + "\\" + pdffilename;
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
        /// 生成STI对象(合拼输出前期准备)
        /// </summary>
        /// <param name="stiname">STI结果集名称</param>
        /// <param name="dt">数据源</param>
        /// <param name="mrtfilename">MRT文件名称</param>
        /// <returns></returns>
        private StiReport GetReport(String stiname, DataTable dt, String mrtfilename)
        {
            var stiReport = new StiReport();
            var reportPath = Application.StartupPath + "/Report/" + mrtfilename;
            stiReport.Load(reportPath);
            stiReport.RegData(stiname, dt);
            return stiReport;
        }

        /// <summary>
        /// 按不同情况对‘签收确定单’进行数据整理
        /// </summary>
        /// <param name="typeid">类型:0=>以‘客户编码’为条件进行获取(合拼时使用) 1=>以‘客户编码’及‘月份’为条件进行获取(拆分时使用)</param>
        /// <param name="month">月份</param>
        /// <param name="customercode">客户编码</param>
        /// <param name="sourcedt">数据源</param>
        /// <returns></returns>
        public DataTable GetConfirmCustomerReportDt(int typeid, string month, string customercode, DataTable sourcedt)
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


        /// <summary>
        /// 根据客户编码整合相关记录集-‘对账单’生成STI报表时使用
        /// </summary>
        /// <param name="customercode"></param>
        /// <param name="sourcedt"></param>
        /// <returns></returns>
        public DataTable GetFincalreportdt(string customercode, DataTable sourcedt)
        {
            var resultdt = sourcedt.Clone();

            var dtlrows = sourcedt.Select("customercode='" + customercode + "'");

            for (var i = 0; i < dtlrows.Length; i++)
            {
                var newrow = resultdt.NewRow();
                newrow[0] = Convert.ToString(dtlrows[i][0]);   //开始日期
                newrow[1] = Convert.ToString(dtlrows[i][1]);   //结束日期
                newrow[2] = Convert.ToString(dtlrows[i][2]);   //往来单位名称
                newrow[3] = Convert.ToString(dtlrows[i][3]);   //单据日期-用于排序
                newrow[4] = Convert.ToString(dtlrows[i][4]);   //摘要
                newrow[5] = Convert.ToString(dtlrows[i][5]);   //本期应收(当“摘要”为“期初余额”时,"本期应收"项为空显示)=>以千位符进行分隔                     
                newrow[6] = Convert.ToString(dtlrows[i][6]);   //本期实收(当“摘要”为“期初余额”时,"本期实收"项为空显示)=>以千位符进行分隔          
                newrow[7] = Convert.ToString(dtlrows[i][7]);   //期末余额=>以千位符进行分隔    
                newrow[8] = Convert.ToString(dtlrows[i][8]);   //记录结束日期备注
                newrow[9] = Convert.ToString(dtlrows[i][9]);   //单据编号
                newrow[10] = Math.Round(Convert.ToDecimal(dtlrows[i][10]), 2); //记录最后一行‘期末余额’
                newrow[11] = Convert.ToString(dtlrows[i][11]); //月份
                newrow[12] = Convert.ToString(dtlrows[i][12]); //单据日期-用于显示
                newrow[13] = Convert.ToString(dtlrows[i][13]); //对相同客户的区分显示(当要针对相同客户打印多次时)
                newrow[14] = Convert.ToString(dtlrows[i][14]); //STI排序ID;注:以A开始,若ID值小于10 即加0,如:A01
                newrow[15] = Convert.ToInt32(dtlrows[i][15]);  //用于STI报表明细行排序
                newrow[16] = Convert.ToString(dtlrows[i][16]); //客户开票名称-二级客户对账单.核算项目名称使用
                newrow[17] = Convert.ToString(dtlrows[i][17]); //客户编码
                newrow[18] = Convert.ToString(dtlrows[i][18]); //change date:20221216 记录‘总期末余额’（以千位符进行分隔,在STI报表显示）
                resultdt.Rows.Add(newrow);
            }

            return resultdt;
        }

        /// <summary>
        /// 根据客户ID整合相关记录集-‘销售发货清单’生成STI报表时使用
        /// </summary>
        /// <param name="fcustid">客户ID</param>
        /// <param name="sourcedt">结果集DT</param>
        /// <returns></returns>
        public DataTable GetSalesOutdt(int fcustid, DataTable sourcedt)
        {
            var resultdt = sourcedt.Clone();

            var dtlrows = sourcedt.Select("FCUSTID='" + fcustid + "'");

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
                newrow[21] = Convert.ToString(dtlrows[i][21]);                 //作用:对相同客户的区分显示(当要针对相同客户打印多次时)
                resultdt.Rows.Add(newrow);
            }
            return resultdt;
        }
    }
}
