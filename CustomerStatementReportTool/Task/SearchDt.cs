using System;
using System.Data;
using System.Data.SqlClient;
using CustomerStatementReportTool.DB;
using Stimulsoft.Controls.Win.DotNetBar.Controls;

namespace CustomerStatementReportTool.Task
{
    //查询
    public class SearchDt
    {
        ConDb conDb=new ConDb();
        SqlList sqlList=new SqlList();

        /// <summary>
        /// 根据SQL语句查询得出对应的DT
        /// </summary>
        /// <param name="sqlscript"></param>
        /// <returns></returns>
        private DataTable UseSqlSearchIntoDt(string sqlscript)
        {
            var resultdt=new DataTable();

            try
            {
                var sqlDataAdapter = new SqlDataAdapter(sqlscript,conDb.GetK3CloudConn());
                sqlDataAdapter.Fill(resultdt);
            }
            catch (Exception)
            {
                resultdt.Rows.Clear();
                resultdt.Columns.Clear();
            }

            return resultdt;
        }

        /// <summary>
        /// 获取客户记录列表
        /// </summary>
        /// <returns></returns>
        public DataTable SearchCustomerList()
        {
            var dt = UseSqlSearchIntoDt(sqlList.GetCustomerList());
            return dt;
        }

        /// <summary>
        /// 查询SQL获取财务数据
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="customerlist">客户列表</param>
        /// <returns></returns>
        public DataTable SearchFinialRecord(string sdt,string edt,string customerlist)
        {
            var dt = UseSqlSearchIntoDt(sqlList.GetFinialRecord(sdt, edt, customerlist));
            return dt;
        }

    }
}
