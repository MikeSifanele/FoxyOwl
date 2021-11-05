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

namespace FoxyOwl
{
    public partial class Form1 : Form
    {
        #region private fields
        private string _symbol = "Volatility 75 Index";
        private int _lotPercent = 10;
        private int _period = 3;

        private List<MacdRates> _macdRates = null;
        private int _numChartCandles = 210;

        private Timer CandleTimer = null;
        private bool _isCandleTimerSynced = false;
        #endregion

        public Form1()
        {
            InitializeComponent();

            CandleTimer = new Timer()
            { 
                Enabled = true,
                Interval = MqlHelper.Instance.GetCandleInterval(_period)
            };

            CandleTimer_Tick(null, null);

            CandleTimer.Tick += CandleTimer_Tick;
        }

        #region Navigation events
        private void btnBack_Click(object sender, EventArgs e)
        {

        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            
        }
        #endregion

        #region Click events
        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (!MqlHelper.Instance.OpenBuyOrder(_symbol, MqlHelper.Instance.GetLotSize(_symbol, _lotPercent), "Bought manually"))
            {

            }

            chartPanel.Refresh();
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            if (!MqlHelper.Instance.OpenSellOrder(_symbol, MqlHelper.Instance.GetLotSize(_symbol, _lotPercent), "Sold manually"))
            {

            }

            chartPanel.Refresh();
        }

        private void btnPositionsCloseAll_Click(object sender, EventArgs e)
        {
            if (!MqlHelper.Instance.PositionCloseAll(_symbol))
            {

            }

            chartPanel.Refresh();
        }

        private void cbAutoTrade_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                btnBack.Enabled = btnForward.Enabled = btnBuy.Enabled = btnSell.Enabled = btnPositionsCloseAll.Enabled = cbAutoTrade.Checked;
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Custom methods
        private int XPoint;
        private void LoadChart(List<MacdRates> candlesticks)
        {
            try
            {
                XPoint = 0;

                chartPanel.CreateGraphics().Clear(chartPanel.BackColor);

                var candlesMaxHeight = candlesticks.Max(x => x.High);
                var candlesMinHeight = candlesticks.Min(x => x.Low);

                var points = MqlHelper.Instance.GetPoints(_symbol);

                var maxPoints = Math.Floor((candlesMaxHeight - candlesMinHeight) * points);
                var candlePadding = candlesticks[0].CandleGraphics.WickWidth;

                PlotOpenPositions(_symbol, points, maxPoints, candlesMaxHeight);

                for (int i = 0; i < candlesticks.Count; i++)
                {
                    XPoint += candlesticks[i].CandleGraphics.BodyWidth + candlePadding;

                    var bodyHeight = ChartConvert.ToRelativeValue(candlesticks[i].CandleGraphics.BodyHeight, maxPoints, chartPanel.Height);
                    var wickHeight = ChartConvert.ToRelativeValue(candlesticks[i].CandleGraphics.WickHeight, maxPoints, chartPanel.Height);

                    //TODO: add body & wick Offset.
                    var bodyOffset = ChartConvert.ToRelativeValue((int)Math.Floor((candlesMaxHeight - Math.Max(candlesticks[i].Open, candlesticks[i].Close)) * points), maxPoints, chartPanel.Height);
                    var wickOffset = ChartConvert.ToRelativeValue((int)Math.Floor((candlesMaxHeight - candlesticks[i].High) * points), maxPoints, chartPanel.Height);

                    candlesticks[i].SetCandleDimensions(points: points);

                    var candleGraphics = chartPanel.CreateGraphics();

                    candleGraphics.FillRectangle(candlesticks[i].CandleGraphics.Brush, new Rectangle(XPoint, bodyOffset, candlesticks[i].CandleGraphics.BodyWidth, bodyHeight));
                    candleGraphics.FillRectangle(candlesticks[i].CandleGraphics.Brush, new Rectangle(XPoint + candlePadding, wickOffset, candlesticks[i].CandleGraphics.WickWidth, wickHeight));
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

                var positionsOpen = MqlHelper.Instance.GetTradePositions(symbol);

                int yPoint = 0;

                for (int i = 0; i < positionsOpen.Length; i++)
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
                _macdRates = MqlHelper.Instance.GetMacdRates(_symbol, period: _period, count: _numChartCandles);

                LoadChart(_macdRates);
            }
            catch (Exception)
            {

            }
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                LoadChart(_macdRates);
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
                if (!_isCandleTimerSynced)
                {
                    CandleTimer.Interval = MqlHelper.Instance.GetCandleInterval(_period);

                    if (sender != null)
                        _isCandleTimerSynced = true;
                    else
                        return;
                }

                _macdRates = MqlHelper.Instance.GetMacdRates(_symbol, _period, count: _numChartCandles);

                if (cbAutoTrade.Checked)
                {
                    var lotSize = MqlHelper.Instance.GetLotSize(_symbol, _lotPercent);
                    var tradeComment = MqlHelper.Instance.GetTradePositions(_symbol)?.FirstOrDefault()?.Comment;

                    if (_macdRates[_numChartCandles - 1].Colour == (int)MacdColour.LimeGreen)
                    {
                        MqlHelper.Instance.OpenBuyOrder(_symbol, lotSize, "bought on Lime");

                        if (MqlHelper.Instance.GetTotalPositions(_symbol) > 0 && cbCloseOppositeTrade.Checked && tradeComment == "sold on Red")
                        {
                            MqlHelper.Instance.PositionCloseAll(_symbol);
                        }
                    }
                    else if (_macdRates[_numChartCandles - 1].Colour == (int)MacdColour.Red)
                    {
                        MqlHelper.Instance.OpenSellOrder(_symbol, lotSize, "sold on Red");

                        if (MqlHelper.Instance.GetTotalPositions(_symbol) > 0 && cbCloseOppositeTrade.Checked && tradeComment == "bought on Lime")
                        {
                            MqlHelper.Instance.PositionCloseAll(_symbol);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion
    }
}
