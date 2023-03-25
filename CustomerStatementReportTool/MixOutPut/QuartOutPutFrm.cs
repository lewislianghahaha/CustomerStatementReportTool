using System.Threading;
using System.Windows.Forms;
using CustomerStatementReportTool.BatchExport;
using CustomerStatementReportTool.Task;

namespace CustomerStatementReportTool.MixOutPut
{
    public partial class QuartOutPutFrm : Form
    {
        Load load=new Load();
        TaskLogic taskLogic = new TaskLogic();
        MessageFrm messageFrm = new MessageFrm();


        public QuartOutPutFrm()
        {
            InitializeComponent();
            OnRegisterEvents();
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
            panel4.Visible = false;
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmimport_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 运算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGenerate_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 添加地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnsetadd_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tmclose_Click(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
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

        #region 控制GridView单元格显示



        #endregion
    }
}
