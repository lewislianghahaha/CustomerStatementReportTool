using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CustomerStatementReportTool.BatchExport
{
    public partial class MessageFrm : Form
    {
        #region 变量参数

            //接收是否成功标记
            private int _showid = 0;

            //接收执行结果=>从CustBatchExport窗体返回的DT
            private DataTable _resultdt;

            //保存查询出来的GridView记录(对账单)
            private DataTable _duidtl;
            //保存查询出来的GridView记录(销售发货清单)
            private DataTable _salesoutdtl;

            #region 对账单使用
            //记录当前页数(GridView页面跳转使用)
            private int _pageCurrent = 1;
            //记录计算出来的总页数(GridView页面跳转使用)
            private int _totalpagecount;
            //记录初始化标记(GridView页面跳转 初始化时使用)
            private bool _pageChange;
            #endregion

            #region 销售发货清单使用
            //记录当前页数(GridView页面跳转使用)
            private int _pageCurrent1 = 1;
            //记录计算出来的总页数(GridView页面跳转使用)
            private int _totalpagecount1;
            //记录初始化标记(GridView页面跳转 初始化时使用)
            private bool _pageChange1;
            #endregion

        #endregion

        #region Set
        /// <summary>
        /// 接收执行结果
        /// </summary>
        public DataTable Resultdt { set { _resultdt = value; } }

        #endregion

        public MessageFrm()
        {
            InitializeComponent();
            OnRegisterEvents();
        }

        private void OnRegisterEvents()
        {
            tmclose.Click += Tmclose_Click;
            btnsearch.Click += Btnsearch_Click;
            rbtrue.Click += Rbtrue_Click;
            rbfalse.Click += Rbfalse_Click;
            btnclear.Click += Btnclear_Click;
            tbpage.SelectedIndexChanged += Tbpage_SelectedIndexChanged;

            ////////////////////////////对账单使用/////////////////////////////////////
            bnMoveFirstItem.Click += BnMoveFirstItem_Click;
            bnMovePreviousItem.Click += BnMovePreviousItem_Click;
            bnMoveNextItem.Click += BnMoveNextItem_Click;
            bnMoveLastItem.Click += BnMoveLastItem_Click;
            bnPositionItem.Leave += BnPositionItem_Leave;
            tmshowrows.DropDownClosed += Tmshowrows_DropDownClosed;
            panel4.Visible = false;

            ///////////////////////////销售发货清单使用////////////////////////////////////
            bnMoveFirstItem1.Click += BnMoveFirstItem1_Click;
            bnMovePreviousItem1.Click += BnMovePreviousItem1_Click;
            bnMoveNextItem1.Click += BnMoveNextItem1_Click;
            bnMoveLastItem1.Click += BnMoveLastItem1_Click;
            bnPositionItem1.Leave += BnPositionItem1_Leave;
            tmshowrows1.DropDownClosed += Tmshowrows1_DropDownClosed;
            panel6.Visible = false;
        }

        /// <summary>
        /// 接收‘自定义批量导出’功能的返回结果
        /// 注：_resultdt=>保存所有记录 _duidtl=>保存根据指定条件得到的‘对账单’记录 _salesoutdtl=>保存根据指定条件得到的‘销售发货清单’记录
        /// </summary>
        public void OnShow()
        {
            if (_resultdt.Rows.Count > 0)
            {
                //分别将_duidtl 以及 _salesoutdtl放到对应方法内
                //选择后赋值至对应Grid View内=> _duidtl(对账单) 以及_salesoutdtl(销售发货清单) 
                _showid = rbtrue.Checked ? 0 : 1;
                GetduiDt(_resultdt, _showid,"","");
                GetsalesoutDt(_resultdt, _showid,"","");
            }
            else
            {
                gvdtl.DataSource = _resultdt.Clone();
                gvdtl1.DataSource = _resultdt.Clone();
            }
        }

        /// <summary>
        /// '对账单'获取DT使用
        /// 注:里面包含将选择记录返回至gvdtl内
        /// 1）数据初始化；2）查询 使用
        /// </summary>
        /// <param name="sourcedt">返回记录集</param>
        /// <param name="istrue">是否成功标记 0:是 1:否</param>
        /// <param name="custcode">客户编码</param>
        /// <param name="custname">客户名称</param>
        /// <returns></returns>
        private void GetduiDt(DataTable sourcedt,int istrue,string custcode,string custname)
        {
            DataRow[] dtlRows=new DataRow[0];
            var tempdt = sourcedt.Clone();

            //根据sourcedt获取指定记录
            if (custcode !="" && custname=="")
            {
                dtlRows = sourcedt.Select("客户编码 like '%" + custcode + "%' and 导出单据类型 = '0' and remarkid='"+istrue+"'");
            }
            else if (custcode == "" && custname !="")
            {
                dtlRows = sourcedt.Select("客户名称 like '%" + custname + "%' and 导出单据类型 = '0' and remarkid='" + istrue + "'");
            }
            else if (custcode != "" && custname != "")
            {
                dtlRows = sourcedt.Select("客户编码 like '%" + custcode + "%' and 客户名称 like '%" + custname + "%' and 导出单据类型='0' and remarkid='" + istrue + "'");
            }
            //当‘客户编码’以及‘客户名称’都为空时
            else if(custcode == "" && custname == "")
            {
                dtlRows = sourcedt.Select("导出单据类型='0' and remarkid='" + istrue + "'");
            }

            //定义显示信息变量
            var message = @"本次进行生成‘对账单’PDF的客户有" + sourcedt.Rows.Count / 2 + "个,其中," +
                       "执行成功客户有:" + sourcedt.Select("导出单据类型='0' and remarkid='0'").Length + "个," +
                       "执行失败客户有:" + sourcedt.Select("导出单据类型='0' and remarkid='1'").Length + "个,详情请查阅以下信息.";

            //整合数据
            _duidtl = GetSearchRecord(dtlRows, tempdt).Copy();

            if (_duidtl.Rows.Count > 0)
            {
                //设置显示信息
                lbldui.Text = message;

                //将整合的结果赋值至gvdtl内显示
                gvdtl.DataSource = _duidtl.Copy();
                //设置不显示5 6 项
                gvdtl.Columns[5].Visible = false;
                gvdtl.Columns[6].Visible = false;

                //修正-初始化时,将页面跳转按钮都显示
                panel4.Visible = true;
                //初始化下拉框所选择的默认值
                tmshowrows.SelectedItem = Convert.ToInt32(tmshowrows.SelectedItem) == 0
                ? (object)"10"
                : Convert.ToInt32(tmshowrows.SelectedItem);
                //定义初始化标记
                _pageChange = _pageCurrent <= 1;
                //GridView分页
                GridViewPageChange();
            }
            else
            {
                //判断若gvdtl1的行数>0，即将gvdtl1的内容清空。再赋空表给它;反之,即直接用_salesoutdtl赋值(因为此时_salesoutdtl也是空表,相当于赋空表至gvdtl内)
                if (gvdtl?.Rows.Count > 0)
                {
                    //清空gvdtl1.datasource
                    var dt = (DataTable)gvdtl.DataSource;
                    dt.Rows.Clear();
                    gvdtl.DataSource = dt;
                }
                else
                {
                    gvdtl.DataSource = _duidtl.Clone();
                }
            }
        }

        /// <summary>
        /// '销售发货清单'获取DT使用
        /// 注:里面包含将选择记录返回至gvdtl内
        /// 1）数据初始化；2）查询 使用
        /// </summary>
        /// <param name="sourcedt"></param>
        /// <param name="istrue"></param>
        /// <param name="custcode"></param>
        /// <param name="custname"></param>
        private void GetsalesoutDt(DataTable sourcedt, int istrue, string custcode, string custname)
        {
            DataRow[] dtlRows = new DataRow[0];
            var tempdt = sourcedt.Clone();

            //根据sourcedt获取指定记录
            if (custcode != "" && custname == "")
            {
                dtlRows = sourcedt.Select("客户编码 LIKE '%" + custcode + "%' and 导出单据类型='1' and remarkid='" + istrue + "'");
            }
            else if (custcode == "" && custname != "")
            {
                dtlRows = sourcedt.Select("客户名称 LIKE '%" + custname + "%' and 导出单据类型='1' and remarkid='" + istrue + "'");
            }
            else if (custcode != "" && custname != "")
            {
                dtlRows = sourcedt.Select("客户编码 like '%" + custcode + "%' and 客户名称 LIKE '%" + custname + "%' and 导出单据类型='1' and remarkid='" + istrue + "'");
            }
            //当‘客户编码’以及‘客户名称’都为空时
            else if (custcode == "" && custname == "")
            {
                dtlRows = sourcedt.Select("导出单据类型='1' and remarkid='" + istrue + "'");
            }

            //定义显示信息变量
            var message = @"本次进行生成‘销售发货清单’PDF的客户有"+ sourcedt.Rows.Count/2 +"个,其中," +
                       "执行成功有:"+ sourcedt.Select("导出单据类型='1' and remarkid='0'").Length + "个," +
                       "执行失败有:"+ sourcedt.Select("导出单据类型='1' and remarkid='1'").Length + "个,详情请查阅以下信息.";

            //整合数据
            _salesoutdtl = GetSearchRecord(dtlRows, tempdt).Copy();

            if (_salesoutdtl.Rows.Count > 0)
            {
                //设置显示信息
                lblsalesout.Text = message;

                //将整合的结果赋值至gvdtl内显示
                gvdtl1.DataSource = _salesoutdtl.Copy();
                //设置不显示5 6 项
                gvdtl1.Columns[5].Visible = false;
                gvdtl1.Columns[6].Visible = false;

                //修正-初始化时,将页面跳转按钮都显示
                panel6.Visible = true;
                //初始化下拉框所选择的默认值
                tmshowrows1.SelectedItem = Convert.ToInt32(tmshowrows1.SelectedItem) == 0
                ? (object)"10"
                : Convert.ToInt32(tmshowrows1.SelectedItem);
                //定义初始化标记
                _pageChange1 = _pageCurrent1 <= 1;
                //GridView分页
                GridViewPageChange1();
            }
            else
            {
                //判断若gvdtl1的行数>0，即将gvdtl1的内容清空。再赋空表给它;反之,即直接用_salesoutdtl赋值(因为此时_salesoutdtl也是空表,相当于赋空表至gvdtl内)
                if (gvdtl1?.Rows.Count > 0)
                {
                    //清空gvdtl1.datasource
                    var dt = (DataTable) gvdtl1.DataSource;
                    dt.Rows.Clear();
                    gvdtl1.DataSource = dt;
                }
                else
                {
                    gvdtl1.DataSource = _salesoutdtl.Clone();
                }
            }
        }

        /// <summary>
        /// 根据指定的查询条件整合一个DT记录集
        /// </summary>
        /// <param name="dtlrows"></param>
        /// <param name="tempdt"></param>
        /// <returns></returns>
        private DataTable GetSearchRecord(DataRow[] dtlrows,DataTable tempdt)
        {
            for (var i = 0; i < dtlrows.Length; i++)
            {
                var newrow = tempdt.NewRow();
                newrow[0] = dtlrows[i][0]; //客户编码
                newrow[1] = dtlrows[i][1]; //客户名称
                newrow[2] = dtlrows[i][2]; //开始执行时间
                newrow[3] = dtlrows[i][3]; //结束执行时间
                newrow[4] = dtlrows[i][4]; //执行结果
                newrow[5] = dtlrows[i][5]; //导出单据类型
                newrow[6] = dtlrows[i][6]; //是否成功标记
                tempdt.Rows.Add(newrow);
            }
            return tempdt;
        }

        /// <summary>
        /// 生成结果状态：成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rbtrue_Click(object sender, EventArgs e)
        {
            try
            {
                //根据选择的Tab Page来判断使用那一个查询功能
                if (tbpage.SelectedTab.Text == $"对账单")
                {
                    GetduiDt(_resultdt, 0, txtcustcode.Text, txtcustname.Text);
                }
                else
                {
                    GetsalesoutDt(_resultdt, 0, txtcustcode.Text, txtcustname.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 生成结果状态：失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rbfalse_Click(object sender, EventArgs e)
        {
            try
            {
                //根据选择的Tab Page来判断使用那一个查询功能
                if (tbpage.SelectedTab.Text == $"对账单")
                {

                    GetduiDt(_resultdt, 1, txtcustcode.Text, txtcustname.Text);
                }
                else
                {
                    GetsalesoutDt(_resultdt, 1, txtcustcode.Text, txtcustname.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 选项卡发生变化时执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tbpage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //通过‘生成状态-成功’判断是否显示‘成功’或‘失败’信息
                var showid = !rbtrue.Checked ? 1 : 0;

                if (tbpage.SelectedTab.Text == $"对账单")
                {
                    GetduiDt(_resultdt, showid, txtcustcode.Text, txtcustname.Text);
                }
                else
                {
                    GetsalesoutDt(_resultdt, showid, txtcustcode.Text, txtcustname.Text);
                }
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
            try
            {
                //通过‘生成状态-成功’判断是否显示‘成功’或‘失败’信息
                var showid = !rbtrue.Checked ? 1 : 0;

                if (tbpage.SelectedTab.Text == $"对账单")
                {
                    GetduiDt(_resultdt, showid, txtcustcode.Text, txtcustname.Text);
                }
                else
                {
                    GetsalesoutDt(_resultdt, showid, txtcustcode.Text, txtcustname.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 重置文本框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnclear_Click(object sender, EventArgs e)
        {
            txtcustcode.Text = "";
            txtcustname.Text = "";
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


        #region 对账单使用
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
                    var dtltotalrows = _duidtl.Rows.Count;
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
                    tstotalrow.Text = $"共 {_duidtl.Rows.Count} 行";

                    //根据“当前页” 及 “固定行数” 计算出新的行数记录并进行赋值
                    //计算进行循环的起始行
                    var startrow = (_pageCurrent - 1) * pageCount;
                    //计算进行循环的结束行
                    var endrow = _pageCurrent == _totalpagecount ? dtltotalrows : _pageCurrent * pageCount;
                    //复制 查询的DT的列信息（不包括行）至临时表内
                    var tempdt = _duidtl.Clone();
                    //循环将所需的_dtl的行记录复制至临时表内
                    for (var i = startrow; i < endrow; i++)
                    {
                        tempdt.ImportRow(_duidtl.Rows[i]);
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
        #endregion

        #region 销售发货清单使用
            #region 控制GridView单元格显示
            /// <summary>
            /// 首页按钮(GridView页面跳转时使用)
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void BnMoveFirstItem1_Click(object sender, EventArgs e)
            {
                try
                {
                    //1)将当前页变量PageCurrent=1; 2)并将“首页” 及 “上一页”按钮设置为不可用 将“下一页” “末页”按设置为可用
                    _pageCurrent1 = 1;
                    bnMoveFirstItem1.Enabled = false;
                    bnMovePreviousItem1.Enabled = false;

                    bnMoveNextItem1.Enabled = true;
                    bnMoveLastItem1.Enabled = true;
                    GridViewPageChange1();
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
            private void BnMovePreviousItem1_Click(object sender, EventArgs e)
            {
                try
                {
                    //1)将PageCurrent自减 2)将“下一页” “末页”按钮设置为可用
                    _pageCurrent1--;
                    bnMoveNextItem1.Enabled = true;
                    bnMoveLastItem1.Enabled = true;
                    //判断若PageCurrent=1的话,就将“首页” “上一页”按钮设置为不可用
                    if (_pageCurrent1 == 1)
                    {
                        bnMoveFirstItem1.Enabled = false;
                        bnMovePreviousItem1.Enabled = false;
                    }
                    GridViewPageChange1();
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
            private void BnMoveNextItem1_Click(object sender, EventArgs e)
            {
                try
                {
                    //1)将PageCurrent自增 2)将“首页” “上一页”按钮设置为可用
                    _pageCurrent1++;
                    bnMoveFirstItem1.Enabled = true;
                    bnMovePreviousItem1.Enabled = true;
                    //判断若PageCurrent与“总页数”一致的话,就将“下一页” “末页”按钮设置为不可用
                    if (_pageCurrent1 == _totalpagecount1)
                    {
                        bnMoveNextItem1.Enabled = false;
                        bnMoveLastItem1.Enabled = false;
                    }
                    GridViewPageChange1();
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
            private void BnMoveLastItem1_Click(object sender, EventArgs e)
            {
                try
                {
                    //1)将“总页数”赋值给PageCurrent 2)将“下一页” “末页”按钮设置为不可用 并将 “上一页” “首页”按钮设置为可用
                    _pageCurrent1 = _totalpagecount1;
                    bnMoveNextItem1.Enabled = false;
                    bnMoveLastItem1.Enabled = false;

                    bnMovePreviousItem1.Enabled = true;
                    bnMoveFirstItem1.Enabled = true;

                    GridViewPageChange1();
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
            private void BnPositionItem1_Leave(object sender, EventArgs e)
            {
                try
                {
                    //判断所输入的跳转数必须为整数
                    if (!Regex.IsMatch(bnPositionItem1.Text, @"^-?[1-9]\d*$|^0$")) throw new Exception("请输入整数再继续");
                    //判断所输入的跳转数不能大于总页数
                    if (Convert.ToInt32(bnPositionItem1.Text) > _totalpagecount1) throw new Exception("所输入的页数不能超出总页数,请修改后继续");
                    //判断若所填跳转数为0时跳出异常
                    if (Convert.ToInt32(bnPositionItem1.Text) == 0) throw new Exception("请输入大于0的整数再继续");

                    //将所填的跳转页赋值至“当前页”变量内
                    _pageCurrent1 = Convert.ToInt32(bnPositionItem1.Text);
                    //根据所输入的页数动态控制四个方向键是否可用
                    //若为第1页，就将“首页” “上一页”按钮设置为不可用 将“下一页” “末页”设置为可用
                    if (_pageCurrent1 == 1)
                    {
                        bnMoveFirstItem1.Enabled = false;
                        bnMovePreviousItem1.Enabled = false;

                        bnMoveNextItem1.Enabled = true;
                        bnMoveLastItem1.Enabled = true;
                    }
                    //若为末页,就将"下一页" “末页”按钮设置为不可用 将“上一页” “首页”设置为可用
                    else if (_pageCurrent1 == _totalpagecount1)
                    {
                        bnMoveNextItem1.Enabled = false;
                        bnMoveLastItem1.Enabled = false;

                        bnMovePreviousItem1.Enabled = true;
                        bnMoveFirstItem1.Enabled = true;
                    }
                    //否则四个按钮都可用
                    else
                    {
                        bnMoveFirstItem1.Enabled = true;
                        bnMovePreviousItem1.Enabled = true;
                        bnMoveNextItem1.Enabled = true;
                        bnMoveLastItem1.Enabled = true;
                    }
                    GridViewPageChange1();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bnPositionItem1.Text = Convert.ToString(_pageCurrent1);
                }
            }

            /// <summary>
            /// 每页显示行数 下拉框关闭时执行
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Tmshowrows1_DropDownClosed(object sender, EventArgs e)
            {
                try
                {
                    //每次选择新的“每页显示行数”，都要 1)将_pageChange标记设为true(即执行初始化方法) 2)将“当前页”初始化为1
                    _pageChange1 = true;
                    _pageCurrent1 = 1;
                    //将“上一页” “首页”设置为不可用
                    bnMovePreviousItem1.Enabled = false;
                    bnMoveFirstItem1.Enabled = false;
                    GridViewPageChange1();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            /// <summary>
            /// GridView分页功能
            /// </summary>
            private void GridViewPageChange1()
            {
                try
                {
                    //获取查询的总行数
                    var dtltotalrows = _salesoutdtl.Rows.Count;
                    //获取“每页显示行数”所选择的行数
                    var pageCount = Convert.ToInt32(tmshowrows1.SelectedItem);
                    //计算出总页数
                    _totalpagecount1 = dtltotalrows % pageCount == 0 ? dtltotalrows / pageCount : dtltotalrows / pageCount + 1;
                    //赋值"总页数"项
                    bnCountItem1.Text = $"/ {_totalpagecount1} 页";

                    //初始化BindingNavigator控件内的各子控件 及 对应初始化信息
                    if (_pageChange1)
                    {
                        bnPositionItem1.Text = Convert.ToString(1);                      //初始化填充跳转页为1
                        tmshowrows1.Enabled = true;                                      //每页显示行数（下拉框）  

                        //初始化时判断;若“总页数”=1，四个按钮不可用；若>1,“下一页” “末页”按钮可用
                        if (_totalpagecount1 == 1)
                        {
                            bnMoveFirstItem1.Enabled = false;                            //'首页'按钮
                            bnMovePreviousItem1.Enabled = false;                         //'上一页'按钮
                            bnMoveNextItem1.Enabled = false;                             //'下一页'按钮
                            bnMoveLastItem1.Enabled = false;                             //'末页'按钮
                            bnPositionItem1.Enabled = false;                             //跳转页文本框
                        }
                        else
                        {
                            bnMoveNextItem1.Enabled = true;
                            bnMoveLastItem1.Enabled = true;
                            bnPositionItem1.Enabled = true;                             //跳转页文本框
                        }
                        _pageChange1 = false;
                    }

                    //显示_dtl的查询总行数
                    tstotalrow1.Text = $"共 {_salesoutdtl.Rows.Count} 行";

                    //根据“当前页” 及 “固定行数” 计算出新的行数记录并进行赋值
                    //计算进行循环的起始行
                    var startrow = (_pageCurrent1 - 1) * pageCount;
                    //计算进行循环的结束行
                    var endrow = _pageCurrent1 == _totalpagecount1 ? dtltotalrows : _pageCurrent1 * pageCount;
                    //复制 查询的DT的列信息（不包括行）至临时表内
                    var tempdt = _salesoutdtl.Clone();
                    //循环将所需的_dtl的行记录复制至临时表内
                    for (var i = startrow; i < endrow; i++)
                    {
                        tempdt.ImportRow(_salesoutdtl.Rows[i]);
                    }

                    //最后将刷新的DT重新赋值给GridView
                    gvdtl1.DataSource = tempdt;
                    //将“当前页”赋值给"跳转页"文本框内
                    bnPositionItem1.Text = Convert.ToString(_pageCurrent1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, $"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            #endregion
        #endregion
    }
}
