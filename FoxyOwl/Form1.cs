using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoxyOwl.Models;
using FoxyOwl.Converters;

namespace FoxyOwl
{
    public partial class Form1 : Form
    {
        #region private fields
        private string _symbol = "Volatility 75 Index";
        private int _lotPercent = 10;
        private int _period = 3;
        /// <summary>
        /// Width of a candle's wick.
        /// </summary>
        private int _wickWidth = 1;
        /// <summary>
        /// Width of a candle's body(3x the wick).
        /// </summary>
        private int _bodyWidth = 3;

        private Rates[] _rates = null;
        private int _numChartCandles = 220;

        private Timer _timer = null;
        private Timer CurrentCandleTimer = null;
        private bool _isCandleTimerSynced = false;
        private bool _isCurrentCandleTimerSynced = false;
        #endregion
        private void MLTrader_Print(string message, TextColourEnum textColour)
        {
            var listItem = new ListViewItem()
            {
                Text = $"{DateTime.Now.ToShortTimeString()}: {message}.",
                ForeColor = NumberConvert.ToColour(textColour),
            };

            lbOutput.Items.Add(listItem);
        }
        public Form1()
        {
            InitializeComponent();

            //DataExport.Export();
        }

        #region Navigation events
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (MLTrader.Instance.CanGoBack)
            {
                _ = MLTrader.Instance.Step(-1);

                LoadChart(MLTrader.Instance.GetObservation());
            }
            else
            {
                btnBack.Enabled = false;
            }

            if (!btnForward.Enabled)
                btnForward.Enabled = MLTrader.Instance.IsLastStep;
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (!MLTrader.Instance.IsLastStep)
            {
                _ = MLTrader.Instance.Step();

                LoadChart(MLTrader.Instance.GetObservation());
            }
            else
            {
                btnForward.Enabled = false;
            }

            if (!btnBack.Enabled)
                btnBack.Enabled = MLTrader.Instance.CanGoBack;
        }
        #endregion

        #region Click events
        private void btnBuy_Click(object sender, EventArgs e)
        {
            _ = MLTrader.Instance.Step();
            chartPanel.Refresh();
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            _ = MLTrader.Instance.Step();
            chartPanel.Refresh();
        }

        private void btnPositionsCloseAll_Click(object sender, EventArgs e)
        {
            chartPanel.Refresh();
        }

        private void cbAutoTrade_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                btnBack.Enabled = btnForward.Enabled = btnBuy.Enabled = btnSell.Enabled = btnPositionsCloseAll.Enabled = !cbAutoTrade.Checked;
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Custom methods
        private void GetTrainingLabels()
        {
            try
            {
                MLTrader.Instance.Reset();

                while(!MLTrader.Instance.IsLastStep)
                {

                }
            }
            catch (Exception)
            {

            }
        }
        private int XPoint;
        private void LoadChart(Rates[] candlesticks)
        {
            try
            {
                XPoint = 0;

                chartPanel.CreateGraphics().Clear(chartPanel.BackColor);

                var candlesMaxHeight = candlesticks.Max(x => x.High);
                var candlesMinHeight = candlesticks.Min(x => x.Low);

                var points = MLTrader.Instance.Points;

                var maxPoints = Math.Floor((candlesMaxHeight - candlesMinHeight) * points);
                var candlePadding = 1;

                PlotOpenPositions(_symbol, points, maxPoints, candlesMaxHeight);

                for (int i = 0; i < candlesticks.Length; i++)
                {
                    XPoint += _bodyWidth + candlePadding;

                    if (i > 0 && candlesticks[i - 1].Time.Day != candlesticks[i].Time.Day)
                    {
                        var periodSeparator = chartPanel.CreateGraphics();
                        periodSeparator.DrawLine(new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dash }, new Point(XPoint, y: 0), new Point(XPoint, chartPanel.Height));

                        XPoint++;
                    }

                    var bodyHeight = ChartConvert.ToRelativeValue(ChartConvert.GetBodyHeight(candlesticks[i]), maxPoints, chartPanel.Height);
                    var wickHeight = ChartConvert.ToRelativeValue(ChartConvert.GetWickHeight(candlesticks[i]), maxPoints, chartPanel.Height);

                    //TODO: add body & wick Offset.
                    var bodyOffset = ChartConvert.ToRelativeValue((int)Math.Floor((candlesMaxHeight - Math.Max(candlesticks[i].Open, candlesticks[i].Close)) * points), maxPoints, chartPanel.Height);
                    var wickOffset = ChartConvert.ToRelativeValue((int)Math.Floor((candlesMaxHeight - candlesticks[i].High) * points), maxPoints, chartPanel.Height);

                    var candleGraphics = chartPanel.CreateGraphics();

                    var candleColour = ChartConvert.GetCandleColour(candlesticks[i]);

                    candleGraphics.FillRectangle(candleColour, new Rectangle(XPoint, bodyOffset, _bodyWidth, bodyHeight));
                    candleGraphics.FillRectangle(candleColour, new Rectangle(XPoint + candlePadding, wickOffset, _wickWidth, wickHeight));
                }
            }
            catch (Exception)
            {

            }
        }

        private void PlotOpenPositions(string symbol, int points, double maxPoints, float maxHeight)
        {
            try
            {
                var positionsGraphics = chartPanel.CreateGraphics();

                var positionsOpen = MLTrader.Instance.OpenPositions;

                int yPoint = 0;

                for (int i = 0; i < positionsOpen.Count; i++)
                {
                    yPoint = ChartConvert.ToRelativeValue((int)Math.Floor((maxHeight - positionsOpen[i].PriceOpen) * points), maxPoints, chartPanel.Height);
                    positionsGraphics.DrawLine(new Pen(Color.Green, 1) { DashStyle = DashStyle.Dash }, new Point(0, y: yPoint), new Point(chartPanel.Width, yPoint));
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Auto events
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                MLTrader.Print += MLTrader_Print;
                _rates = MLTrader.Instance.GetObservation();

                //_timer = new Timer();

                LoadChart(_rates);
            }
            catch (Exception)
            {

            }
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                LoadChart(_rates);
            }
            catch (Exception)
            {

            }
        }

        private void chartPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            try
            {
                mainPanel_Paint(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void CandleTimer_Tick(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception)
            {

            }
        }

        private void CurrentCandleTimer_Tick(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception)
            {

            }
        }
        #endregion
    }
}
