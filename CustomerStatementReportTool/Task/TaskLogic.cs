using System.Data;

namespace CustomerStatementReportTool.Task
{
    //功能分配
    public class TaskLogic
    {
        SearchDt serchDt = new SearchDt();
        Generate generate=new Generate();

        #region 变量参数

        private int _taskid;
        private string _sdt;           //开始日期(运算时使用)
        private string _edt;           //结束日期(运算时使用)
        private string _customerlist;  //客户列表(运算时使用)
        private int _typeid;           //-1:全查找记录 0:按‘客户编码’查找 1:按'客户名称'查找(查询客户时使用)
        private string _value;         //查询文件框值

        private bool _resultmark;                //返回是否成功标记
        private DataTable _resultTable;          //返回DT(客户列表初始化获取记录时使用)
        private DataTable _searchcustomertypeDt; //根据不同条件查询客户列表信息
        private DataTable _resultFinalRecord;    //返回运算后的记录(针对纵向记录)
        private DataTable _resultProductRecord;  //返回运算后的记录(针对横向记录)
        private DataTable _resultSalesOutListRecord;//返回运算后的记录(针对销售发货清单)
        #endregion

        #region Set
        /// <summary>
        /// 中转ID
        /// </summary>
        public int TaskId { set { _taskid = value; } }

        /// <summary>
        ///开始日期(运算时使用)
        /// </summary>
        public string Sdt { set { _sdt = value; } }

        /// <summary>
        ///结束日期(运算时使用)
        /// </summary>
        public string Edt { set { _edt = value; } }

        /// <summary>
        /// 客户列表(运算时使用)
        /// </summary>
        public string Customerlist { set { _customerlist = value; } }

        /// <summary>
        /// 查询客户时使用
        /// </summary>
        public int Typeid { set { _typeid = value; } }

        /// <summary>
        /// 查询文件框值
        /// </summary>
        public string Value { set { _value = value; } }

        #endregion

        #region Get
        /// <summary>
        ///  返回是否成功标记
        /// </summary>
        public bool ResultMark => _resultmark;

        /// <summary>
        ///返回DataTable至主窗体
        /// </summary>
        public DataTable ResultTable => _resultTable;

        /// <summary>
        /// 返回运算后的记录(针对纵向记录)
        /// </summary>
        public DataTable ResultFinalRecord => _resultFinalRecord;

        /// <summary>
        /// 根据不同条件查询客户列表信息
        /// </summary>
        public DataTable SearchcustomertypeDt => _searchcustomertypeDt;

        /// <summary>
        /// 返回运算后的记录(针对横向记录)
        /// </summary>
        public DataTable ResultProductRecord=>_resultProductRecord;

        /// <summary>
        /// 返回运算后的记录(针对销售发货清单)
        /// </summary>
        public DataTable ResultSalesOutListRecord=>_resultSalesOutListRecord;
        #endregion

        public void StartTask()
        {
            switch (_taskid)
            {
                //查询-客户列表(初始化使用)
                case 0:
                    SearchCustomerList();
                    break;
                //根据不同条件查询客户列表信息
                case 1:
                    SearchCustomTypeList(_typeid, _value);
                    break;
                //对账单生成(纵向)
                case 2:
                    Generate(_sdt,_edt,_customerlist);
                    break;
                //工业对账单生成(横向)
                case 3:
                    GenerateProduct(_sdt,_edt,_customerlist);
                    break;
                //销售发货清单
                case 4:
                    GenerateSalesOutList(_sdt, _edt, _customerlist);
                    break;
            }
        }

        /// <summary>
        /// 查询客户列表信息
        /// </summary>
        private void SearchCustomerList()
        {
            _resultTable = serchDt.SearchCustomerList().Copy();
        }

        /// <summary>
        /// 根据不同条件查询客户列表信息
        /// </summary>
        private void SearchCustomTypeList(int typeid, string value)
        {
            _searchcustomertypeDt = serchDt.SearchCustomTypeList(typeid,value).Copy();
        }

        /// <summary>
        /// 运算(针对纵向记录)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        private void Generate(string sdt, string edt, string customerlist)
        {
            _resultFinalRecord = generate.GenerateFincal(sdt, edt, customerlist);
        }

        /// <summary>
        /// 返回运算后的记录(针对横向记录)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        private void GenerateProduct(string sdt, string edt, string customerlist)
        {
            _resultProductRecord = generate.GenerateProduct(sdt, edt, customerlist);
        }

        /// <summary>
        /// 销售发货清单查询
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        private void GenerateSalesOutList(string sdt, string edt, string customerlist)
        {
            _resultSalesOutListRecord = generate.GenerateSalesOutList(sdt, edt, customerlist);
        }

    }
}
