using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CustomerStatementReportTool.DB;
using CustomerStatementReportTool.Task;
using Stimulsoft.Report;

namespace CustomerStatementReportTool
{
    public partial class Main : Form
    {
        Load load=new Load();
        TaskLogic taskLogic=new TaskLogic();
        TempDtList tempDt=new TempDtList();

        #region 变量参数
        //保存GridView内需要进行添加的临时表
        private DataTable _adddt;
        //保存查询出来的GridView记录
        private DataTable _dtl;
        //记录当前页数(GridView页面跳转使用)
        private int _pageCurrent = 1;
        //记录计算出来的总页数(GridView页面跳转使用)
        private int _totalpagecount;
        //记录初始化标记(GridView页面跳转 初始化时使用)
        private bool _pageChange;
        #endregion


        public Main()
        {
            InitializeComponent();
            OnRegisterEvents();
            OnInitialize();
        }

        private void OnRegisterEvents()
        {
            btnsearch.Click += Btnsearch_Click;
            btnclear.Click += Btnclear_Click;
            btngen.Click += Btngen_Click;
            btngenproduct.Click += Btngenproduct_Click;
            btnsalesoutlist.Click += Btnsalesoutlist_Click;

            tmclose.Click += Tmclose_Click;
            tmadd.Click += Tmadd_Click;
            tmdel.Click += Tmdel_Click;
            
            bnMoveFirstItem.Click += BnMoveFirstItem_Click;
            bnMovePreviousItem.Click += BnMovePreviousItem_Click;
            bnMoveNextItem.Click += BnMoveNextItem_Click;
            bnMoveLastItem.Click += BnMoveLastItem_Click;
            bnPositionItem.Leave += BnPositionItem_Leave;
            tmshowrows.DropDownClosed += Tmshowrows_DropDownClosed;
           // panel3.Visible = false;

            comtype.SelectedIndexChanged += Comtype_SelectedIndexChanged;
        }

        /// <summary>
        /// 初始化相关记录
        /// </summary>
        private void OnInitialize()
        {
            //下拉列表
            OnShowTypeList();
            //客户列表DT
            OnSearchCustomerList();
        }

        /// <summary>
        /// 下拉列表改变时执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Comtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtValue.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnsearch_Click(object sender, EventArgs e)
        {
            var ordertypeId = 0;

            try
            {
                //获取下拉列表所选值
                var dvordertylelist = (DataRowView)comtype.Items[comtype.SelectedIndex];

                ordertypeId = txtValue.Text == "" ? -1 : Convert.ToInt32(dvordertylelist["Id"]);

                taskLogic.TaskId = 1;
                taskLogic.Typeid = ordertypeId;
                taskLogic.Value = txtValue.Text;

                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                if (taskLogic.SearchcustomertypeDt.Rows.Count > 0)
                {
                    _dtl = taskLogic.SearchcustomertypeDt;
                    panel3.Visible = true;
                    //初始化下拉框所选择的默认值
                    //tmshowrows.SelectedItem = "10"; 
                    tmshowrows.SelectedItem = Convert.ToInt32(tmshowrows.SelectedItem) == 0
                    ? (object)"10"
                    : Convert.ToInt32(tmshowrows.SelectedItem);
                    //定义初始化标记
                    _pageChange = _pageCurrent <= 1;
                    //GridView分页
                    GridViewPageChange();
                }
                //注:当为空记录时,不显示跳转页;只需将临时表赋值至GridView内
                else
                {
                    gvsearchdtl.DataSource = taskLogic.SearchcustomertypeDt;
                    panel3.Visible = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 添加至明细记录(注:可多行选择)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmadd_Click(object sender, EventArgs e)
        {
            try
            {
                //定义‘_adddt’临时表的行数
                var addcount = 0;

                if (gvsearchdtl.RowCount == 0) throw new Exception("没有查询结果,不能添加");

                //获取临时表
                var temp = tempDt.GetSearchCustomerListTempDt();

                foreach (DataGridViewRow row in gvsearchdtl.SelectedRows)
                {
                    var newrow = temp.NewRow();
                    newrow[0] = row.Cells[0].Value; //客户编码
                    newrow[1] = row.Cells[1].Value; //客户名称
                    temp.Rows.Add(newrow);
                }
                //若_adddt为空,即返回0
                addcount = _adddt?.Rows.Count ?? 0;
                //若_addit为空的话,就将temp.Clone() 给它
                if (_adddt == null) _adddt = temp.Clone();
                //若_adddt+temp.rowscount得出的总行数>10行时,即提示异常
                if (addcount + temp.Rows.Count > 10) throw new Exception("添加行数已超过10行,不能继续");
                //判断若需要添加的记录,已在_adddt存在,即提示异常
                if (!CheckRecord(temp)) throw new Exception("已添加,不能再次进行添加");
                //将要添加的记录添加至‘添加明细记录’GridView内
                gvdtl.DataSource = AddsoucetoDt(temp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 删除记录(明细窗体使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmdel_Click(object sender, EventArgs e)
        {
            try
            {
                if(gvdtl.RowCount==0) throw new Exception("没有明细记录,不能进行删除操作");
                for (var i = gvdtl.SelectedRows.Count; i > 0; i--)
                {
                    gvdtl.Rows.RemoveAt(gvdtl.SelectedRows[i - 1].Index);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 对账单生成(纵向)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btngen_Click(object sender, EventArgs e)
        {
            try
            {
                var customerlist = string.Empty;
                var temp = string.Empty;

                var sdt = dtStr.Value.ToString("yyyy-MM-dd");
                var edt = dtEnd.Value.ToString("yyyy-MM-dd");

                //判断若gvdtl没有记录,不能进行运算
                if (gvdtl.RowCount == 0) throw new Exception("请添加记录后再进行运算");

                //若结束日期小于开始日期,报异常提示
                if (DateTime.Compare(Convert.ToDateTime(sdt), Convert.ToDateTime(edt)) > 0) throw new Exception("异常:结束日期不能小于开始日期,请重新选择日期并进行运算");

                //将‘对账单生成’,‘工业对账单生成(横向)‘按钮设置为不可操作;直至运行完成才恢复
                btngen.Enabled = false;
                btngenproduct.Enabled = false;
                btnsalesoutlist.Enabled = false;

                //对已添加的‘客户列表’整合,合拼为一行并以,分隔
                var customerdt = (DataTable)gvdtl.DataSource;

                //通过循环将选中行的客户编码进行收集(注:去除重复的选项,只保留不重复的主键记录)
                foreach (DataRow rows in customerdt.Rows)
                {
                    if (string.IsNullOrEmpty(customerlist))
                    {
                        customerlist = "'" + Convert.ToString(rows[0]) + "'";
                        temp = Convert.ToString(rows[0]);
                    }
                    else
                    {
                        if (temp != Convert.ToString(rows[0]))
                        {
                            customerlist += "," + "'" + Convert.ToString(rows[0]) + "'";
                            temp = Convert.ToString(rows[0]);
                        }
                    }
                }

                //var a = customerlist;

                taskLogic.TaskId = 2;
                taskLogic.Sdt = sdt;
                taskLogic.Edt = edt;
                taskLogic.Customerlist = customerlist;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                //完成后将文本框 及 gvdtl内容清空
                //change date:20221014 取消对gvdtl内空清空设计
                txtValue.Text = "";
                //var dt = (DataTable)gvdtl.DataSource;
                //dt.Rows.Clear();
                //gvdtl.DataSource = dt;
                btngen.Enabled = true;
                btngenproduct.Enabled = true;
                btnsalesoutlist.Enabled = true;

                if (taskLogic.ResultFinalRecord.Rows.Count == 0) throw new Exception($@"运算异常,检测到进行运算的客户在'{sdt}'至'{edt}'没有交易记录,请修改查询日期再进行运算.");
                else
                {
                    //调用STI模板并执行导出代码
                    //加载STI模板
                    //定义模板地址
                    var filepath = Application.StartupPath + "/Report/CustomerStatementReport.mrt";
                    var stireport = new StiReport();
                    stireport.Load(filepath);
                    //加载DATASET 或 DATATABLE
                    stireport.RegData("CustomerStatement", taskLogic.ResultFinalRecord);
                    stireport.Compile();
                    stireport.Show();  //调用预览功能
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 工业对账单生成(横向)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btngenproduct_Click(object sender, EventArgs e)
        {
            try
            {
                var customerlist = string.Empty;
                var temp = string.Empty;

                var sdt = dtStr.Value.ToString("yyyy-MM-dd");
                var edt = dtEnd.Value.ToString("yyyy-MM-dd");

                //判断若gvdtl没有记录,不能进行运算
                if (gvdtl.RowCount == 0) throw new Exception("请添加记录后再进行运算");

                //若结束日期小于开始日期,报异常提示
                if (DateTime.Compare(Convert.ToDateTime(sdt), Convert.ToDateTime(edt)) > 0) throw new Exception("异常:结束日期不能小于开始日期,请重新选择日期并进行运算");

                //将‘对账单生成’,‘工业对账单生成(横向)‘按钮设置为不可操作;直至运行完成才恢复
                btngen.Enabled = false;
                btngenproduct.Enabled = false;
                btnsalesoutlist.Enabled = false;

                //对已添加的‘客户列表’整合,合拼为一行并以,分隔
                var customerdt = (DataTable)gvdtl.DataSource;

                //通过循环将选中行的客户编码进行收集(注:去除重复的选项,只保留不重复的主键记录)
                foreach (DataRow rows in customerdt.Rows)
                {
                    if (string.IsNullOrEmpty(customerlist))
                    {
                        customerlist = "'" + Convert.ToString(rows[0]) + "'";
                        temp = Convert.ToString(rows[0]);
                    }
                    else
                    {
                        if (temp != Convert.ToString(rows[0]))
                        {
                            customerlist += "," + "'" + Convert.ToString(rows[0]) + "'";
                            temp = Convert.ToString(rows[0]);
                        }
                    }
                }

                taskLogic.TaskId = 3;
                taskLogic.Sdt = sdt;
                taskLogic.Edt = edt;
                taskLogic.Customerlist = customerlist;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                //完成后将文本框 及 gvdtl内容清空
                //change date:20221014 取消对gvdtl内空清空设计
                txtValue.Text = "";
                //var dt = (DataTable)gvdtl.DataSource;
                //dt.Rows.Clear();
                //gvdtl.DataSource = dt;
                btngen.Enabled = true;
                btngenproduct.Enabled = true;
                btnsalesoutlist.Enabled = true;

                if (taskLogic.ResultProductRecord.Rows.Count == 0) throw new Exception($@"运算异常,检测到进行运算的客户在'{sdt}'至'{edt}'没有交易记录,请修改查询日期再进行运算.");
                else
                {
                    //调用STI模板并执行导出代码
                    //加载STI模板
                    //定义模板地址
                    var filepath = Application.StartupPath + "/Report/ProductCustomerReport.mrt";
                    var stireport = new StiReport();
                    stireport.Load(filepath);
                    //加载DATASET 或 DATATABLE
                    stireport.RegData("ProductCustomer", taskLogic.ResultProductRecord);
                    stireport.Compile();
                    stireport.Show();  //调用预览功能
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 销售发货清单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnsalesoutlist_Click(object sender, EventArgs e)
        {
            try
            {
                var customerlist = string.Empty;
                var temp = string.Empty;

                var sdt = dtStr.Value.ToString("yyyy-MM-dd");
                var edt = dtEnd.Value.ToString("yyyy-MM-dd");

                //判断若gvdtl没有记录,不能进行运算
                if (gvdtl.RowCount == 0) throw new Exception("请添加记录后再进行运算");

                //若结束日期小于开始日期,报异常提示
                if (DateTime.Compare(Convert.ToDateTime(sdt), Convert.ToDateTime(edt)) > 0) throw new Exception("异常:结束日期不能小于开始日期,请重新选择日期并进行运算");

                //将‘对账单生成’,‘工业对账单生成(横向)‘按钮设置为不可操作;直至运行完成才恢复
                btngen.Enabled = false;
                btngenproduct.Enabled = false;
                btnsalesoutlist.Enabled = false;

                //对已添加的‘客户列表’整合,合拼为一行并以,分隔
                var customerdt = (DataTable)gvdtl.DataSource;

                //通过循环将选中行的客户编码进行收集(注:去除重复的选项,只保留不重复的主键记录)
                foreach (DataRow rows in customerdt.Rows)
                {
                    if (string.IsNullOrEmpty(customerlist))
                    {
                        customerlist = "'" + Convert.ToString(rows[0]) + "'";
                        temp = Convert.ToString(rows[0]);
                    }
                    else
                    {
                        if (temp != Convert.ToString(rows[0]))
                        {
                            customerlist += "," + "'" + Convert.ToString(rows[0]) + "'";
                            temp = Convert.ToString(rows[0]);
                        }
                    }
                }

                taskLogic.TaskId = 4;
                taskLogic.Sdt = sdt;
                taskLogic.Edt = edt;
                taskLogic.Customerlist = customerlist;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                //完成后将文本框 及 gvdtl内容清空
                //change date:20221014 取消对gvdtl内空清空设计
                txtValue.Text = "";
                //var dt = (DataTable)gvdtl.DataSource;
                //dt.Rows.Clear();
                //gvdtl.DataSource = dt;
                btngen.Enabled = true;
                btngenproduct.Enabled = true;
                btnsalesoutlist.Enabled = true;

                if (taskLogic.ResultSalesOutListRecord.Rows.Count == 0) throw new Exception($@"运算异常,检测到进行运算的客户在'{sdt}'至'{edt}'没有交易记录,请修改查询日期再进行运算.");
                else
                {
                    //调用STI模板并执行导出代码
                    //加载STI模板
                    //定义模板地址
                    var filepath = Application.StartupPath + "/Report/SalesOutListReport.mrt";
                    var stireport = new StiReport();
                    stireport.Load(filepath);
                    //加载DATASET 或 DATATABLE
                    stireport.RegData("SalesOutList", taskLogic.ResultSalesOutListRecord);
                    stireport.Compile();
                    stireport.Show();  //调用预览功能
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmclose_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 重置(清空查询框时使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnclear_Click(object sender, EventArgs e)
        {
            try
            {
                txtValue.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///下拉列表
        /// </summary>
        private void OnShowTypeList()
        {
            var dt = new DataTable();

            //创建表头
            for (var i = 0; i < 2; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0:
                        dc.ColumnName = "Id";
                        break;
                    case 1:
                        dc.ColumnName = "Name";
                        break;
                }
                dt.Columns.Add(dc);
            }

            //创建行内容
            for (var j = 0; j < 2; j++)
            {
                var dr = dt.NewRow();

                switch (j)
                {
                    case 0:
                        dr[0] = "0";
                        dr[1] = "客户编码";
                        break;
                    case 1:
                        dr[0] = "1";
                        dr[1] = "客户名称";
                        break;
                }
                dt.Rows.Add(dr);
            }

            comtype.DataSource = dt;
            comtype.DisplayMember = "Name"; //设置显示值
            comtype.ValueMember = "Id";    //设置默认值内码
        }

        /// <summary>
        /// 初始化获取客户列表记录
        /// </summary>
        /// <returns></returns>
        private void OnSearchCustomerList()
        {
            try
            {
                taskLogic.TaskId = 0;
                taskLogic.StartTask();

                _dtl = taskLogic.ResultTable.Copy();
                gvsearchdtl.DataSource = _dtl.Copy();

                //修正-初始化时,将页面跳转按钮都显示
                panel3.Visible = true;
                //初始化下拉框所选择的默认值
                //tmshowrows.SelectedItem = "10"; 
                tmshowrows.SelectedItem = Convert.ToInt32(tmshowrows.SelectedItem) == 0
                ? (object)"10"
                : Convert.ToInt32(tmshowrows.SelectedItem);
                //定义初始化标记
                _pageChange = _pageCurrent <= 1;
                //GridView分页
                GridViewPageChange();
            }
            catch (Exception)
            {
                _dtl.Rows.Clear();
            }
        }

        /// <summary>
        ///子线程使用(重:用于监视功能调用情况,当完成时进行关闭LoadForm)
        /// </summary>
        private void Start()
        {
            taskLogic.StartTask();

            //当完成后将Form2子窗体关闭
            this.Invoke((ThreadStart)(() =>
            {
                load.Close();
            }));
        }

        /// <summary>
        /// 判断若需要添加的记录,已在_adddt存在,即提示异常
        /// </summary>
        /// <param name="sourcedt"></param>
        /// <returns></returns>
        private bool CheckRecord(DataTable sourcedt)
        {
            var result = true;
            try
            {
                //判断若‘添加’的记录是否在_adddt,若存在返回FALSE
                for (var i = 0; i < sourcedt.Rows.Count; i++)
                {
                    for (var j = 0; j < _adddt.Rows.Count; j++)
                    {
                        if (Convert.ToString(sourcedt.Rows[i][0]) == Convert.ToString(_adddt.Rows[j][0]))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 将查询明细的记录添加至‘明细记录’GridView内
        /// </summary>
        /// <returns></returns>
        private DataTable AddsoucetoDt(DataTable sourcedt)
        {
            try
            {
                //判断若reslut为空,即直接插入,反之作更新操作
                if (_adddt.Rows.Count == 0)
                {
                    _adddt = sourcedt.Copy();
                }
                else
                {
                    foreach (DataRow row in sourcedt.Rows)
                    {
                        var newrow = _adddt.NewRow();
                        for (var i = 0; i < _adddt.Columns.Count; i++)
                        {
                            newrow[i] = row[i];
                        }
                        _adddt.Rows.Add(newrow);
                    }
                }
            }
            catch (Exception)
            {
                _adddt.Columns.Clear();
                _adddt.Rows.Clear();
            }
            return _adddt;
        }

        #region 控制GridView单元格显示

        /// <summary>
        /// 首页按钮(GridView页面跳转时使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BnMoveFirstItem_Click(object sender, EventArgs e)
        {
            try
            {
                //1)将当前页变量PageCurrent=1; 2)并将“首页” 及 “上一页”按钮设置为不可用 将“下一页” “末页”按设置为可用
                _pageCurrent = 1;
                bnMoveFirstItem.Enabled = false;
                bnMovePreviousItem.Enabled = false;

                bnMoveNextItem.Enabled = true;
                bnMoveLastItem.Enabled = true;
                GridViewPageChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 上一页(GridView页面跳转时使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BnMovePreviousItem_Click(object sender, EventArgs e)
        {
            try
            {
                //1)将PageCurrent自减 2)将“下一页” “末页”按钮设置为可用
                _pageCurrent--;
                bnMoveNextItem.Enabled = true;
                bnMoveLastItem.Enabled = true;
                //判断若PageCurrent=1的话,就将“首页” “上一页”按钮设置为不可用
                if (_pageCurrent == 1)
                {
                    bnMoveFirstItem.Enabled = false;
                    bnMovePreviousItem.Enabled = false;
                }
                GridViewPageChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 下一页按钮(GridView页面跳转时使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BnMoveNextItem_Click(object sender, EventArgs e)
        {
            try
            {
                //1)将PageCurrent自增 2)将“首页” “上一页”按钮设置为可用
                _pageCurrent++;
                bnMoveFirstItem.Enabled = true;
                bnMovePreviousItem.Enabled = true;
                //判断若PageCurrent与“总页数”一致的话,就将“下一页” “末页”按钮设置为不可用
                if (_pageCurrent == _totalpagecount)
                {
                    bnMoveNextItem.Enabled = false;
                    bnMoveLastItem.Enabled = false;
                }
                GridViewPageChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 末页按钮(GridView页面跳转使用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BnMoveLastItem_Click(object sender, EventArgs e)
        {
            try
            {
                //1)将“总页数”赋值给PageCurrent 2)将“下一页” “末页”按钮设置为不可用 并将 “上一页” “首页”按钮设置为可用
                _pageCurrent = _totalpagecount;
                bnMoveNextItem.Enabled = false;
                bnMoveLastItem.Enabled = false;

                bnMovePreviousItem.Enabled = true;
                bnMoveFirstItem.Enabled = true;

                GridViewPageChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 跳转页文本框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BnPositionItem_Leave(object sender, EventArgs e)
        {
            try
            {
                //判断所输入的跳转数必须为整数
                if (!Regex.IsMatch(bnPositionItem.Text, @"^-?[1-9]\d*$|^0$")) throw new Exception("请输入整数再继续");
                //判断所输入的跳转数不能大于总页数
                if (Convert.ToInt32(bnPositionItem.Text) > _totalpagecount) throw new Exception("所输入的页数不能超出总页数,请修改后继续");
                //判断若所填跳转数为0时跳出异常
                if (Convert.ToInt32(bnPositionItem.Text) == 0) throw new Exception("请输入大于0的整数再继续");

                //将所填的跳转页赋值至“当前页”变量内
                _pageCurrent = Convert.ToInt32(bnPositionItem.Text);
                //根据所输入的页数动态控制四个方向键是否可用
                //若为第1页，就将“首页” “上一页”按钮设置为不可用 将“下一页” “末页”设置为可用
                if (_pageCurrent == 1)
                {
                    bnMoveFirstItem.Enabled = false;
                    bnMovePreviousItem.Enabled = false;

                    bnMoveNextItem.Enabled = true;
                    bnMoveLastItem.Enabled = true;
                }
                //若为末页,就将"下一页" “末页”按钮设置为不可用 将“上一页” “首页”设置为可用
                else if (_pageCurrent == _totalpagecount)
                {
                    bnMoveNextItem.Enabled = false;
                    bnMoveLastItem.Enabled = false;

                    bnMovePreviousItem.Enabled = true;
                    bnMoveFirstItem.Enabled = true;
                }
                //否则四个按钮都可用
                else
                {
                    bnMoveFirstItem.Enabled = true;
                    bnMovePreviousItem.Enabled = true;
                    bnMoveNextItem.Enabled = true;
                    bnMoveLastItem.Enabled = true;
                }
                GridViewPageChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                bnPositionItem.Text = Convert.ToString(_pageCurrent);
            }
        }

        /// <summary>
        /// 每页显示行数 下拉框关闭时执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmshowrows_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                //每次选择新的“每页显示行数”，都要 1)将_pageChange标记设为true(即执行初始化方法) 2)将“当前页”初始化为1
                _pageChange = true;
                _pageCurrent = 1;
                //将“上一页” “首页”设置为不可用
                bnMovePreviousItem.Enabled = false;
                bnMoveFirstItem.Enabled = false;
                GridViewPageChange();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// GridView分页功能
        /// </summary>
        private void GridViewPageChange()
        {
            try
            {
                //获取查询的总行数
                var dtltotalrows = _dtl.Rows.Count;
                //获取“每页显示行数”所选择的行数
                var pageCount = Convert.ToInt32(tmshowrows.SelectedItem);
                //计算出总页数
                _totalpagecount = dtltotalrows % pageCount == 0 ? dtltotalrows / pageCount : dtltotalrows / pageCount + 1;
                //赋值"总页数"项
                bnCountItem.Text = $"/ {_totalpagecount} 页";

                //初始化BindingNavigator控件内的各子控件 及 对应初始化信息
                if (_pageChange)
                {
                    bnPositionItem.Text = Convert.ToString(1);                       //初始化填充跳转页为1
                    tmshowrows.Enabled = true;                                      //每页显示行数（下拉框）  

                    //初始化时判断;若“总页数”=1，四个按钮不可用；若>1,“下一页” “末页”按钮可用
                    if (_totalpagecount == 1)
                    {
                        bnMoveFirstItem.Enabled = false;                            //'首页'按钮
                        bnMovePreviousItem.Enabled = false;                         //'上一页'按钮
                        bnMoveNextItem.Enabled = false;                             //'下一页'按钮
                        bnMoveLastItem.Enabled = false;                             //'末页'按钮
                        bnPositionItem.Enabled = false;                             //跳转页文本框
                    }
                    else
                    {
                        bnMoveNextItem.Enabled = true;
                        bnMoveLastItem.Enabled = true;
                        bnPositionItem.Enabled = true;                             //跳转页文本框
                    }
                    _pageChange = false;
                }

                //显示_dtl的查询总行数
                tstotalrow.Text = $"共 {_dtl.Rows.Count} 行";

                //根据“当前页” 及 “固定行数” 计算出新的行数记录并进行赋值
                //计算进行循环的起始行
                var startrow = (_pageCurrent - 1) * pageCount;
                //计算进行循环的结束行
                var endrow = _pageCurrent == _totalpagecount ? dtltotalrows : _pageCurrent * pageCount;
                //复制 查询的DT的列信息（不包括行）至临时表内
                var tempdt = _dtl.Clone();
                //循环将所需的_dtl的行记录复制至临时表内
                for (var i = startrow; i < endrow; i++)
                {
                    tempdt.ImportRow(_dtl.Rows[i]);
                }

                //最后将刷新的DT重新赋值给GridView
                gvsearchdtl.DataSource = tempdt;
                //将“当前页”赋值给"跳转页"文本框内
                bnPositionItem.Text = Convert.ToString(_pageCurrent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}
