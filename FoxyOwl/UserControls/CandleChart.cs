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

namespace FoxyOwl.UserControls
{
    public partial class CandleChart : UserControl
    {
        private int X, Y = 0;
        public List<MacdRates> DataSource = null;
        public CandleChart()
        {
            InitializeComponent();
        }

        private void CandleChart_Load(object sender, EventArgs e)
        {
            DataSource = new List<MacdRates>();            
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            LoadChart(DataSource);
        }

        public void LoadChart(List<MacdRates> candlesticks)
        {
            try
            {
                Y = X = 0;

                mainPanel.CreateGraphics().Clear(mainPanel.BackColor);

                var candlesMaxHeight = candlesticks.Max(x => x.High);
                var candlesMinHeight = candlesticks.Min(x => x.Low);

                var points = 1_000;

                var maxPoints = Math.Floor((candlesMaxHeight - candlesMinHeight) * points);
                var candlePadding = candlesticks[0].CandleGraphics.WickWidth;

                for (int i = 1; i < candlesticks.Count; i++)
                {
                    X +=  candlesticks[i].CandleGraphics.BodyWidth + candlePadding;

                    var bodyHeight = candlesticks[i].GetRelativeValue(candlesticks[i].CandleGraphics.BodyHeight, maxPoints, mainPanel.Height);
                    var wickHeight = candlesticks[i].GetRelativeValue(candlesticks[i].CandleGraphics.WickHeight, maxPoints, mainPanel.Height);

                    //TODO: add body & wick Offset.
                    var bodyOffset = candlesticks[i].GetRelativeValue((int)Math.Floor((candlesMaxHeight - Math.Max(candlesticks[i].Open, candlesticks[i].Close)) * points), maxPoints, mainPanel.Height);
                    var wickOffset = candlesticks[i].GetRelativeValue((int)Math.Floor((candlesMaxHeight - candlesticks[i].High) * points), maxPoints, mainPanel.Height);

                    var candleGraphics = mainPanel.CreateGraphics();

                    candlesticks[i].SetCandleColour(0);

                    candlesticks[i].SetCandleDimensions();

                    candleGraphics.FillRectangle(candlesticks[i].Colour, new Rectangle(X, Y + bodyOffset, candlesticks[i].CandleGraphics.BodyWidth, bodyHeight));
                    candleGraphics.FillRectangle(candlesticks[i].Colour, new Rectangle(X + candlePadding, Y + wickOffset, candlesticks[i].CandleGraphics.WickWidth, wickHeight));
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
