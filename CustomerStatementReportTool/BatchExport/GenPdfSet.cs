using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            btnnext.Click += Btnnext_Click;
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
                if(!cbcheck.Checked && !cbMix.Checked && !cbsplitdui.Checked) throw new Exception("不能继续,请选择任意一项");

                //调用二级客户对账单模板
                GlobalClasscs.RmMessage.Isusesecondcustomer = cbcheck.Checked;
                //合拼导出
                GlobalClasscs.RmMessage.IsuseMixExport = cbMix.Checked;
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
    }
}
