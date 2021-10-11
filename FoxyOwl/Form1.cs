using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoxyOwl.Models;

namespace FoxyOwl
{
    public partial class Form1 : Form
    {
        private string _symbol = "Volatility 10 Index";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {

        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            var closed = MqlHelper.Instance.PositionCloseAll(_symbol);
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            var bought = MqlHelper.Instance.OpenBuyOrder(_symbol, 1.0, "Bought");
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            var sold = MqlHelper.Instance.OpenSellOrder(_symbol, 1.0, "Sold");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                var mqlRates = MqlHelper.Instance.GetMqlRates(_symbol, period: (int)Resolutions.M3);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
