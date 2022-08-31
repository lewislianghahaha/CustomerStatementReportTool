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
        /// 根据各参数运算结果-供STI报表使用
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
                        newrow[3] = Convert.ToString(dtlrows[i][1]);  //单据日期
                        newrow[4] = Convert.ToString(dtlrows[i][2]);  //摘要
                        newrow[5] = Convert.ToDecimal(dtlrows[i][4]); //本期应收
                        newrow[6] = Convert.ToInt32(dtlrows[i][5]);   //本期收款
                        newrow[7] = Convert.ToDecimal(dtlrows[i][3]); //期末余额
                        newrow[8] = remark1;                          //记录结束日期备注
                        result.Rows.Add(newrow);
                    }
                }
            }
            catch (Exception)
            {
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

            //根据customername,查询明细记录
            var dtlrows = sourcedt.Select("往来单位名称='"+customername+"'");

            for (var i = 0; i < dtlrows.Length; i++)
            {
                //var a = balancetemp;

                //若检测到‘摘要’为‘期初余额’ 即将其值保存至balancetemp内
                if (Convert.ToString(dtlrows[i][2]) == "期初余额")
                {
                    balancetemp = Convert.ToDecimal(dtlrows[i][3]);
                }
                //反之，先插入，然后根据公式计算出‘期末余额’后,将其值赋值至balancetemp内
                else
                {
                    var newrow = tempdt.NewRow();
                    newrow[0] = Convert.ToString(dtlrows[i][0]); //往来单位名称
                    newrow[1] = Convert.ToString(dtlrows[i][1]); //单据日期
                    newrow[2] = Convert.ToString(dtlrows[i][2]); //摘要
                    newrow[3] = balancetemp + Convert.ToDecimal(dtlrows[i][4])- Convert.ToDecimal(dtlrows[i][5])- Convert.ToDecimal(dtlrows[i][6]); //期末余额
                    newrow[4] = Convert.ToDecimal(dtlrows[i][4]); //本期应收
                    newrow[5] = Convert.ToInt32(dtlrows[i][5]); //本期实收
                    newrow[6] = Convert.ToDecimal(dtlrows[i][6]); //原币本期冲销额

                    tempdt.Rows.Add(newrow);
                    //将当前行计算出来的‘期末余额’赋值给balancetemp,给下一行使用
                    balancetemp += Convert.ToDecimal(dtlrows[i][4]) - Convert.ToDecimal(dtlrows[i][5]) - Convert.ToDecimal(dtlrows[i][6]);
                }
            }

            return tempdt;
        }

    }
}
