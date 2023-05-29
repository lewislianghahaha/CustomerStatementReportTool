namespace CustomerStatementReportTool
{
    public class GlobalClasscs
    {
        public struct RestMessage
        {
            public string Errormesage;      //记录运行时的异常信息,并传输到前面进行显示
            public string Printerrmessge;   //记录输出PDF时产生的异常

            public bool Isusesecondcustomer; //记录是否调用二级客户对账单模板-‘自定义打印’功能使用
            public bool IsuseMixExport;     //记录是否合拼打印-‘自定义打印’功能使用
            public bool IsuseSplitdui;      //记录是否按‘客户’进行拆分导出对账单-‘自定义打印’使用
            public bool IsuseNormual;       //记录‘常规导出’-‘自定义打印’使用-(常规导出 指 ‘对账单’合拼导出 ‘销售出库清单’拆分导出 ‘签收确认单’合拼导出)

            public bool IsuseYearMixExport; //记录是否合拼打印-按‘年份’导出功能使用
            public int YearValue;           //记录所选择的‘年度’-按‘年份’导出功能使用

            public bool IsuseQuartMixExport; //记录是否合拼导出-按‘季度’导出功能使用

        }

        public static RestMessage RmMessage;
    }
}
