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
            DataSource.Add(new Candlestick("2021.09.09	09:15:00	6686.052	6688.263	6686.013	6687.713".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:18:00	6687.666	6688.087	6686.812	6687.579".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:21:00	6687.941	6688.362	6685.380	6685.679".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:24:00	6685.817	6687.896	6685.817	6687.896".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:27:00	6688.247	6692.953	6688.229	6692.844".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:30:00	6692.810	6694.348	6691.264	6694.146".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:33:00	6694.280	6694.898	6693.540	6693.714".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:36:00	6693.943	6696.408	6693.585	6696.395".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:39:00	6696.400	6697.224	6695.534	6695.907".Split('\t')));
            DataSource.Add(new Candlestick("2021.09.09	09:42:00	6696.025	6697.714	6694.475	6697.407".Split('\t')));
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

                    candlesticks[i].SetCandleColor(candlesticks[i - 1], candlesticks[i]);

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
