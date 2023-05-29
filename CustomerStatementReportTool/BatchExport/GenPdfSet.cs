using System;
using System.Windows.Forms;

namespace CustomerStatementReportTool.BatchExport
{
    public partial class GenPdfSet : Form
    {
        public GenPdfSet()
        {
            InitializeComponent();
            OnRegisterEvents();
        }

        private void OnRegisterEvents()
        {
            cbMix.Click += CbMix_Click;
            cbnormal.Click += Cbnormal_Click;
            btnnext.Click += Btnnext_Click;
            tmclose.Click += Tmclose_Click;
            cbcheck.Visible = false;
            cbsplitdui.Visible = false;
            lb.Text = $"若勾选‘常规导出’而都没有选择下面两单选项时,\n其导出方式如下:\n"+
                        $"1.'对账单'一次性导出\n"+
                        $"2.'销售发货清单'按客户拆分导出\n"+
                        $"3.'签收确认单'一次性导出";
        }

        /// <summary>
        /// 合拼导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbMix_Click(object sender, EventArgs e)
        {
            cbnormal.Enabled = !cbMix.Checked;
        }

        /// <summary>
        /// 常规导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cbnormal_Click(object sender, EventArgs e)
        {
            if (cbnormal.Checked)
            {
                cbMix.Enabled = false;
                cbcheck.Visible = true;
                cbsplitdui.Visible = true;
            }
            else
            {
                cbMix.Enabled = true;
                cbcheck.Checked = false;
                cbsplitdui.Checked = false;
                cbcheck.Visible = false;
                cbsplitdui.Visible = false;
            }
            
        }

        /// <summary>
        /// 继续下一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnnext_Click(object sender, EventArgs e)
        {
            try
            {
                if(!cbcheck.Checked && !cbMix.Checked && !cbsplitdui.Checked & !cbnormal.Checked) throw new Exception("不能继续,请至少选择一项再继续");

                //合拼导出
                GlobalClasscs.RmMessage.IsuseMixExport = cbMix.Checked;
                //常规导出
                GlobalClasscs.RmMessage.IsuseNormual = cbnormal.Checked;

                //调用二级客户对账单模板
                GlobalClasscs.RmMessage.Isusesecondcustomer = cbcheck.Checked;
                //按‘客户’进行拆分
                GlobalClasscs.RmMessage.IsuseSplitdui = cbsplitdui.Checked;

                //退出
                this.Close();
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
            //todo:关闭前将所有控件初始化

            this.Close();
        }
    }
}
