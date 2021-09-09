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
        public List<Candlestick> DataSource = null;
        public CandleChart()
        {
            InitializeComponent();
        }

        private void CandleChart_Load(object sender, EventArgs e)
        {
            DataSource = new List<Candlestick>();

            #region load dummy data
            DataSource.Add(new Candlestick("2021.09.09	09:00:00	6687.297	6688.826	6686.044	6688.636".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:03:00	6688.588	6688.588	6687.166	6687.733".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:06:00	6687.635	6688.628	6685.341	6685.887".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:09:00	6685.941	6688.370	6685.865	6687.876".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:12:00	6688.018	6688.464	6685.748	6685.853".Split('\t')));
            #endregion
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            LoadChart(DataSource);
        }

        public void LoadChart(List<Candlestick> candlesticks)
        {
            try
            {
                Y = X = 0;

                if (candlesticks.Any())
                {
                    candlesticks[0].FastEMA = candlesticks[0].SlowEMA = candlesticks[0].Close;

                    candlesticks[0].Colour = new SolidBrush(Color.DimGray);
                }

                var candlesMaxHeight = candlesticks.Max(x => x.High);
                var candlesMinHeight = candlesticks.Min(x => x.Low);

                var maxPoints = Math.Floor((candlesMaxHeight - candlesMinHeight) * 1_000);

                for (int i = 1; i < candlesticks.Count; i++)
                {
                    X += 50;

                    var bodyHeightPercentage = candlesticks[i].CandleGraphics.BodyHeight / maxPoints * 100;
                    var wickHeightPercentage = candlesticks[i].CandleGraphics.WickHeight / maxPoints * 100;

                    var bodyHeight = (int)Math.Floor(bodyHeightPercentage / 100 * mainPanel.Height);
                    var wickHeight = (int)Math.Floor(wickHeightPercentage / 100 * mainPanel.Height);

                    //TODO: add body & wick Offset.
                    var bodyOffset = (int)Math.Floor(candlesMaxHeight - Math.Max(candlesticks[i].Open, candlesticks[i].Close));
                    var wickOffset = (int)Math.Floor(candlesMaxHeight - candlesticks[i].High);

                    var candleGraphics = mainPanel.CreateGraphics();

                    candlesticks[i].SetCandleColor(candlesticks[i - 1], candlesticks[i]);

                    candlesticks[i].SetCandleDimensions();

                    candleGraphics.FillRectangle(candlesticks[i].Colour, new Rectangle(X, Y, candlesticks[i].CandleGraphics.BodyWidth, bodyHeight));
                    candleGraphics.FillRectangle(candlesticks[i].Colour, new Rectangle(X+10, Y, candlesticks[i].CandleGraphics.WickWidth, wickHeight));
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
