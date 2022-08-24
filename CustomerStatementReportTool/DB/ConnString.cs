using System.Configuration;

//获取连接字符串
namespace CustomerStatementReportTool.DB
{
    public class ConnString
    {
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            //读取App.Config配置文件中的Connstring节点
            var pubs = ConfigurationManager.ConnectionStrings["ConnString"];
            var result = pubs.ConnectionString;
            return result;
        }
    }
}
