using System;
using System.Data;

namespace CustomerStatementReportTool.DB
{
    //临时表
    public class TempDtList
    {
        /// <summary>
        /// 保存最终运算结果
        /// </summary>
        /// <returns></returns>
        public DataTable MakeExportDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 9; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //开始日期
                        dc.ColumnName = "SDT";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 1: //结束日期
                        dc.ColumnName = "EDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //往来单位名称
                        dc.ColumnName = "CustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //单据日期
                        dc.ColumnName = "FDATE";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 4: //摘要
                        dc.ColumnName = "Remark";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //本期应收
                        dc.ColumnName = "ReceiveQTY";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 6: //本期收款
                        dc.ColumnName = "ReceiveCurrentQty";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 7: //期末余额
                        dc.ColumnName = "EndBalance";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 8: //记录结束日期备注
                        dc.ColumnName = "REMARK1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取从SQL查询语句运行的记录集
        /// </summary>
        /// <returns></returns>
        public DataTable GetSearchTempDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 7; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0:
                        dc.ColumnName = "往来单位名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1:
                        dc.ColumnName = "单据日期";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2:
                        dc.ColumnName = "摘要";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3:
                        dc.ColumnName = "期末余额";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 4:
                        dc.ColumnName = "本期应收";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 5:
                        dc.ColumnName = "本期实收";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 6:
                        dc.ColumnName = "原币本期冲销额";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取‘客户’记录集
        /// </summary>
        /// <returns></returns>
        public DataTable GetSearchCustomerListTempDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 2; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户编码
                        dc.ColumnName = "客户编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取‘客户名称’临时表
        /// </summary>
        /// <returns></returns>
        public DataTable GetCustomerNameListTempdt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 1; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }


    }
}
