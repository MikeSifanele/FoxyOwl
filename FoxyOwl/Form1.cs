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
        private float _volume = 1f;

        private List<MacdRates> _macdRates = null;
        private int _numChartCandles = 120;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {

        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (!MqlHelper.Instance.OpenBuyOrder(_symbol, _volume, "Bought manually"))
            {

            }
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            if (!MqlHelper.Instance.OpenSellOrder(_symbol, _volume, "Sold manually"))
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _macdRates = MqlHelper.Instance.GetMacdRates(_symbol, period: (int)Resolution.M3);

                LoadChart(_macdRates.Skip(_macdRates.Count - _numChartCandles).Take(_numChartCandles).ToList());
            }
            catch (Exception)
            {

            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                var mqlRates = MqlHelper.Instance.GetMqlRates(_symbol, period: (int)Resolution.M3);
            }
            catch (Exception)
            {

            }
        }
        private int XPoint, YPoint = 0;

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                LoadChart(_macdRates.Skip(_macdRates.Count - _numChartCandles).Take(_numChartCandles).ToList());
            }
            catch (Exception)
            {

            }
        }

        private void LoadChart(List<MacdRates> candlesticks)
        {
            try
            {
                YPoint = XPoint = 0;

                mainPanel.CreateGraphics().Clear(mainPanel.BackColor);

                var candlesMaxHeight = candlesticks.Max(x => x.High);
                var candlesMinHeight = candlesticks.Min(x => x.Low);

                var points = 1_000;

                var maxPoints = Math.Floor((candlesMaxHeight - candlesMinHeight) * points);
                var candlePadding = candlesticks[0].CandleGraphics.WickWidth;

                for (int i = 0; i < candlesticks.Count; i++)
                {
                    XPoint += candlesticks[i].CandleGraphics.BodyWidth + candlePadding;

                    var bodyHeight = candlesticks[i].GetRelativeValue(candlesticks[i].CandleGraphics.BodyHeight, maxPoints, mainPanel.Height);
                    var wickHeight = candlesticks[i].GetRelativeValue(candlesticks[i].CandleGraphics.WickHeight, maxPoints, mainPanel.Height);

                    //TODO: add body & wick Offset.
                    var bodyOffset = candlesticks[i].GetRelativeValue((int)Math.Floor((candlesMaxHeight - Math.Max(candlesticks[i].Open, candlesticks[i].Close)) * points), maxPoints, mainPanel.Height);
                    var wickOffset = candlesticks[i].GetRelativeValue((int)Math.Floor((candlesMaxHeight - candlesticks[i].High) * points), maxPoints, mainPanel.Height);

                    var candleGraphics = mainPanel.CreateGraphics();

                    candlesticks[i].SetCandleDimensions();

                    candleGraphics.FillRectangle(candlesticks[i].Colour, new Rectangle(XPoint, YPoint + bodyOffset, candlesticks[i].CandleGraphics.BodyWidth, bodyHeight));
                    candleGraphics.FillRectangle(candlesticks[i].Colour, new Rectangle(XPoint + candlePadding, YPoint + wickOffset, candlesticks[i].CandleGraphics.WickWidth, wickHeight));
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
