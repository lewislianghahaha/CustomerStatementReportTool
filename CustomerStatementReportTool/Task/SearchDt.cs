﻿using System;
using System.Data;
using System.Data.SqlClient;
using CustomerStatementReportTool.DB;

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
        /// 根据不同条件查询客户列表信息 
        /// </summary>
        /// <param name="typeid">-1:全查找记录 0:按‘客户编码’查找 1:按'客户名称'查找</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DataTable SearchCustomTypeList(int typeid, string value)
        {
            var dt = UseSqlSearchIntoDt(sqlList.SearchCustomerList(typeid, value));
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

        /// <summary>
        /// 根据客户记录查找相关应收单记录(销售单位为‘工业涂料事业部’)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public DataTable SearchProductCustomer(string sdt, string edt, string customerlist)
        {
            var dt = UseSqlSearchIntoDt(sqlList.SearchProductCustomer(sdt, edt, customerlist));
            return dt;
        }

        /// <summary>
        /// 销售发货清单查询
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public DataTable SearchSalesOutList(string sdt, string edt, string customerlist)
        {
            var dt = UseSqlSearchIntoDt(sqlList.SearchSalesOutList(sdt, edt, customerlist));
            return dt;
        }

        /// <summary>
        /// 获取客户列表信息-'自定义批量导出'功能使用
        /// </summary>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public DataTable GetSearchCustomerList(string customerlist)
        {
            var dt = UseSqlSearchIntoDt(sqlList.GetSearchCustomerList(customerlist));
            return dt;
        }

    }
}
