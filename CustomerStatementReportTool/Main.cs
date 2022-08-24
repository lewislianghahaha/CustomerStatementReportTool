using System;
using System.Windows.Forms;
using BomOfferOrder.UI;

namespace CustomerStatementReportTool
{
    public partial class Main : Form
    {
        Load load=new Load();

        public Main()
        {
            InitializeComponent();
            OnRegisterEvents();
        }

        private void OnRegisterEvents()
        {
            btngenerate.Click += Btngenerate_Click;
            btnsearch.Click += Btnsearch_Click;
            btnclose.Click += Btnclose_Click;
        }

        /// <summary>
        /// 运算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btngenerate_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                
                throw;
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

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnclose_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }
}
