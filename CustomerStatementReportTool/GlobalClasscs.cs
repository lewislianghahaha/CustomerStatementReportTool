namespace CustomerStatementReportTool
{
    public class GlobalClasscs
    {
        public struct RestMessage
        {
            public string Errormesage;      //记录运行时的异常信息,并传输到前面进行显示
            public string Printerrmessge;   //记录输出PDF时产生的异常
            public int Isusesecondcustomer; //记录是否调用二级客户对账单模板(0:是 1:否)
            public bool IsuseMixExport;     //记录是否合拼打印-‘自定义打印’功能使用
            public bool IsuseYearMixExport; //记录是否合拼打印-按‘年份’导出功能使用
            public int YearValue;           //记录所选择的‘年度’-按‘年份’导出功能使用
        }

        public static RestMessage RmMessage;
    }
}
