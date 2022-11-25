using System.Data;

namespace CustomerStatementReportTool.Task
{
    //功能分配
    public class TaskLogic
    {
        SearchDt serchDt = new SearchDt();
        Generate generate=new Generate();
        ImportDt importDt=new ImportDt();

        #region 变量参数

        private int _taskid;
        private string _sdt;           //开始日期(运算时使用)
        private string _edt;           //结束日期(运算时使用)
        private string _customerlist;  //客户列表(运算时使用)
        private int _typeid;           //-1:全查找记录 0:按‘客户编码’查找 1:按'客户名称'查找(查询客户时使用)
        private string _value;         //查询文件框值
        private string _fileAddress;   //文件地址('自定义批量导出'-导入EXCEL 及 导出地址收集使用)
        private int _duiprintpagenumber;       //对账单打印次数
        private int _salesoutprintpagenumber;  //销售出库清单打印次数

        private bool _resultmark;                   //返回是否成功标记
        private DataTable _resultTable;             //返回DT(客户列表初始化获取记录时使用)
        private DataTable _searchcustomertypeDt;    //根据不同条件查询客户列表信息
        private DataTable _resultFinalRecord;       //返回运算后的记录(针对纵向记录)
        private DataTable _resultProductRecord;     //返回运算后的记录(针对横向记录)
        private DataTable _resultSalesOutListRecord;//返回运算后的记录(针对销售发货清单)
        private DataTable _resultImportDt;          //返回导入EXCEL信息
        private DataTable _resultMessageDt;         //返回‘自定义批量打印’返回结果
        private DataTable _custdtlist;              //获取前端的客户列表DT(自定义批量导出功能使用)

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

        /// <summary>
        /// 接收文件地址信息
        /// </summary>
        public string FileAddress { set { _fileAddress = value; } }

        /// <summary>
        /// 对账单打印次数
        /// </summary>
        public int Duiprintpagenumber { set { _duiprintpagenumber = value; } }

        /// <summary>
        /// 销售出库清单打印次数
        /// </summary>
        public int Salesoutprintpagenumber { set { _salesoutprintpagenumber = value; } }

        /// <summary>
        /// 获取前端的客户列表DT(自定义批量导出功能使用)
        /// </summary>
        public DataTable Custdtlist { set { _custdtlist = value; } }
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

        /// <summary>
        /// 返回导入EXCEL结果
        /// </summary>
        public DataTable ResultImportDt => _resultImportDt;

        /// <summary>
        /// 返回‘自定义批量打印’返回结果
        /// </summary>
        public DataTable ResultMessageDt=>_resultMessageDt;
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
                //销售发货清单(纵向)
                case 4:
                    GenerateSalesOutList(_sdt, _edt, _customerlist);
                    break;
                //导入-自定义批量导出功能使用
                case 5:
                    ImportExcelRecord(_fileAddress);
                    break;
                //‘自定义批量功能’-运算
                case 6:
                    GenerateBatchexport(_sdt, _edt, _fileAddress,_customerlist,_duiprintpagenumber,_salesoutprintpagenumber, _custdtlist);
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
        /// '对账单'运算(针对纵向记录)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        private void Generate(string sdt, string edt, string customerlist)
        {
            _resultFinalRecord = generate.GenerateFincal(sdt, edt, customerlist).Copy();
        }

        /// <summary>
        /// 返回运算后的记录(针对横向记录)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        private void GenerateProduct(string sdt, string edt, string customerlist)
        {
            _resultProductRecord = generate.GenerateProduct(sdt, edt, customerlist).Copy();
        }

        /// <summary>
        /// 销售发货清单查询
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        private void GenerateSalesOutList(string sdt, string edt, string customerlist)
        {
            _resultSalesOutListRecord = generate.GenerateSalesOutList(sdt, edt, customerlist).Copy();
        }

        /// <summary>
        /// 导入EXCEL-自定义批量导出功能使用
        /// </summary>
        /// <param name="fileadd"></param>
        private void ImportExcelRecord(string fileadd)
        {
            _resultImportDt = importDt.OpenExcelImporttoDt(fileadd).Copy();
        }

        /// <summary>
        /// '自定义批量导出'-运算执行
        /// </summary>
        /// <param name="sdt">开始日期</param>
        /// <param name="edt">结束日期</param>
        /// <param name="exportaddress">输出地址</param>
        /// <param name="customerlist">客户列表信息</param>
        /// <param name="duiprintpagenum">对账单打印次数</param>
        /// <param name="salesoutprintpagenum">销售发货清单打印次数</param>
        /// <param name="custdtlist">接收前端客户DT</param>
        private void GenerateBatchexport(string sdt,string edt,string exportaddress,string customerlist,
                                        int duiprintpagenum,int salesoutprintpagenum,DataTable custdtlist)
        {
            _resultMessageDt = generate.GenerateBatchexport(sdt,edt, exportaddress,customerlist, custdtlist,
                                        duiprintpagenum,salesoutprintpagenum).Copy();
        }
    }
}
