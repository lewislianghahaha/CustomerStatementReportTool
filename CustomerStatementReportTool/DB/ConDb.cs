using System.Data.SqlClient;

//获取连接字符串,并创建SqlConnection
namespace CustomerStatementReportTool.DB
{
    public class ConDb
    {
        ConnString connString=new ConnString();

        /// <summary>
        /// 获取K3数据库连接
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetK3CloudConn()
        {
            var sqlcon=new SqlConnection(connString.GetConnectionString());
            return sqlcon;
        }
    }
}
