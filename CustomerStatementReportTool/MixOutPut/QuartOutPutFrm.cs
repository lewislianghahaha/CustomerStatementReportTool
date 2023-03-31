using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CustomerStatementReportTool.Task;

namespace CustomerStatementReportTool.MixOutPut
{
    public partial class QuartOutPutFrm : Form
    {
        Load load=new Load();
        private TaskLogic taskLogic = new TaskLogic();

        #region 变量参数
        //保存查询出来的GridView记录
        private DataTable _dtl;
        //记录当前页数(GridView页面跳转使用)
        private int _pageCurrent = 1;
        //记录计算出来的总页数(GridView页面跳转使用)
        private int _totalpagecount;
        //记录初始化标记(GridView页面跳转 初始化时使用)
        private bool _pageChange;
        #endregion


        public QuartOutPutFrm()
        {
            InitializeComponent();
            OnRegisterEvents();
            OnShowTypeList();
        }

        private void OnRegisterEvents()
        {
            tmclose.Click += Tmclose_Click;
            tmimport.Click += Tmimport_Click;
            btnsetadd.Click += Btnsetadd_Click;
            btnGenerate.Click += BtnGenerate_Click;

            bnMoveFirstItem.Click += BnMoveFirstItem_Click;
            bnMovePreviousItem.Click += BnMovePreviousItem_Click;
            bnMoveNextItem.Click += BnMoveNextItem_Click;
            bnMoveLastItem.Click += BnMoveLastItem_Click;
            bnPositionItem.Leave += BnPositionItem_Leave;
            tmshowrows.DropDownClosed += Tmshowrows_DropDownClosed;
            panel3.Visible = false;
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmimport_Click(object sender, EventArgs e)
        {
            try
            {
                //设置导入地址
                var openFileDialog = new OpenFileDialog { Filter = $"Xlsx文件|*.xlsx" };
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;
                var fileAdd = openFileDialog.FileName;

                //所需的值赋到Task类内
                taskLogic.TaskId = 5;
                taskLogic.FileAddress = fileAdd;

                //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                new Thread(Start).Start();
                load.StartPosition = FormStartPosition.CenterScreen;
                load.ShowDialog();

                var importdt = taskLogic.ResultImportDt.Copy();

                if (importdt.Rows.Count == 0) throw new Exception("不能成功导入EXCEL内容,请检查模板是否正确.");
                else
                {
                    MessageBox.Show($"导入数据成功,请输入相关值进行查询", $"信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //将数据存放至Gridview内
                    _dtl = importdt.Copy();
                    gvdtl.DataSource = _dtl.Copy();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 运算-注意要点:
        /// 1.'按季度导出'主要针对'签收确定单'及'销售发货清单'进行‘合拼’导出
        /// 2.导出顺序 >签收确定单 >销售发货清单
        /// 3.导出方式,以'客户'为循环条件,对‘签收确定单’以及‘销售发货清单’进行分组导出;如:客户A=>‘签收确定单’ ‘销售发货清单’ 然后到客户B=>‘签收确定单’ ‘销售发货清单’ 这种方式显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var chooseValue = string.Empty;
                var temp = string.Empty;
                var sdt = string.Empty;
                var edt = string.Empty;
                var dt = new DateTime();

                var message = string.Empty;
                var customerlist = string.Empty;

                //获取‘季度选择’下拉列表所选值
                var dvordertylelist = (DataRowView)comtype.Items[comtype.SelectedIndex];
                //检查‘输出地址’是否有填
                if (txtadd.Text == "") throw new Exception($"请设置导出地址.");
                //判断若gvdtl没有记录,不能进行运算
                if (gvdtl.RowCount == 0) throw new Exception($"请添加记录后再进行运算");

                //根据选择的‘季度’显示对应的描述
                switch (Convert.ToInt32(dvordertylelist["Id"]))
                {
                    case 0:
                        chooseValue = "第一季度";
                        dt = DateTime.Parse($"{DateTime.Now.Year},1, 01");
                        break;
                    case 1:
                        chooseValue = "第二季度";
                        dt = DateTime.Parse($"{DateTime.Now.Year},4, 01");
                        break;
                    case 2:
                        chooseValue = "第三季度";
                        dt = DateTime.Parse($"{DateTime.Now.Year},7, 01");
                        break;
                    case 3:
                        chooseValue = "第四季度";
                        dt = DateTime.Parse($"{DateTime.Now.Year},10, 01");
                        break;
                }

                //根据所选择的‘季度’生成对应的‘开始’及‘结束’日期
                sdt = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day).ToString("yyyy-MM-dd");  //获取本季度第一天

                edt = dt.AddMonths(0 - (dt.Month - 1) % 3).AddDays(1 - dt.Day).AddMonths(3).AddDays(-1).ToString("yyyy-MM-dd");//获取本季度最后一天 

                message = $"准备执行,\n请注意:" +
                          $"\n1.季度选择:'{chooseValue}',执行日期从'{sdt}'开始 至 '{edt}'结束" +
                          $"\n2.执行成功的结果会下载至'{txtadd.Text}'指定文件夹内," +
                          "\n3.执行过程中不要关闭软件,不然会导致运算失败\n是否继续执行?";

                //开始执行
                if (MessageBox.Show(message, $"提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    //将相关按钮设置为不可操作;直至运行完成后才恢复
                    tmclose.Enabled = false;
                    tmimport.Enabled = false;
                    btnGenerate.Enabled = false;
                    btnsetadd.Enabled = false;

                    var customerdt = _dtl.Copy(); //(DataTable)gvdtl.DataSource;
                    //对已添加的‘客户列表’整合,合拼为一行并以,分隔
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

                    taskLogic.TaskId = 7;
                    taskLogic.Sdt = sdt;
                    taskLogic.Edt = edt;
                    taskLogic.Customerlist = customerlist;
                    taskLogic.FileAddress = txtadd.Text;
                    taskLogic.Custdtlist = _dtl;
                    taskLogic.Genid = 0;  //运算类别:0=>按‘季度’导出使用 1=>按‘年度’导出使用

                    //使用子线程工作(作用:通过调用子线程进行控制Load窗体的关闭情况)
                    new Thread(Start).Start();
                    load.StartPosition = FormStartPosition.CenterScreen;
                    load.ShowDialog();

                    //若检测到GlobalClasscs.Printerrmessge不为空,即跳转到异常处理
                    if (!string.IsNullOrEmpty(GlobalClasscs.RmMessage.Printerrmessge)) throw new Exception($"生成PDF出现异常,原因:{GlobalClasscs.RmMessage.Printerrmessge}");
                    //若检测到GlobalClasscs.Errmessage不为空,即跳转到异常处理
                    if (!string.IsNullOrEmpty(GlobalClasscs.RmMessage.Errormesage)) throw new Exception($"运行出现异常,原因:{GlobalClasscs.RmMessage.Errormesage}");

                    if (string.IsNullOrEmpty(GlobalClasscs.RmMessage.Printerrmessge) && string.IsNullOrEmpty(GlobalClasscs.RmMessage.Errormesage))
                    {
                        MessageBox.Show($"执行成功,请到设置的下载地址进行查阅", $"通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //运算完成后,将原来设置的文本框(按钮)设置为可用（且将记录清空）
                        tmclose.Enabled = true;
                        tmimport.Enabled = true;
                        btnGenerate.Enabled = true;
                        btnsetadd.Enabled = true;

                        txtadd.Text = "";
                        var gvdt = (DataTable)gvdtl.DataSource;
                        gvdt.Rows.Clear();
                        gvdt.Columns.Clear();
                        gvdtl.DataSource = gvdt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //当出现异常后，也将所有项清空
                tmclose.Enabled = true;
                tmimport.Enabled = true;
                btnGenerate.Enabled = true;
                btnsetadd.Enabled = true;

                txtadd.Text = "";
                var gvdt = (DataTable)gvdtl.DataSource;
                gvdt.Rows.Clear();
                gvdt.Columns.Clear();
                gvdtl.DataSource = gvdt;
            }
        }

        /// <summary>
        /// 添加地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnsetadd_Click(object sender, EventArgs e)
        {
            try
            {
                //todo:若txtadd不为空,即先清空此控件
                txtadd.Text = "";
                //todo:设置输出地址
                var folder = new FolderBrowserDialog();
                folder.Description = $"请选择导出文件夹";
                if (folder.ShowDialog() != DialogResult.OK) return;
                txtadd.Text = folder.SelectedPath;
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
            this.Close();
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
            for (var j = 0; j < 4; j++)
            {
                var dr = dt.NewRow();

                switch (j)
                {
                    case 0:
                        dr[0] = "0";
                        dr[1] = "第一季度";
                        break;
                    case 1:
                        dr[0] = "1";
                        dr[1] = "第一季度";
                        break;
                    case 2:
                        dr[0] = "2";
                        dr[1] = "第三季度";
                        break;
                    case 3:
                        dr[0] = "3";
                        dr[1] = "第四季度";
                        break;
                }
                dt.Rows.Add(dr);
            }

            comtype.DataSource = dt;
            comtype.DisplayMember = "Name"; //设置显示值
            comtype.ValueMember = "Id";    //设置默认值内码
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
                gvdtl.DataSource = tempdt;
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
