namespace CustomerStatementReportTool
{
    public class GlobalClasscs
    {
        public struct RestMessage
        {
            public string Errormesage; //记录运行时的异常信息,并传输到前面进行显示
        }

        public static RestMessage RmMessage;
    }
}
