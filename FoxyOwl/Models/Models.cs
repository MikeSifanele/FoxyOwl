using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxyOwl.Models
{
    public class CandleGraphics
    {
        public int WickHeight { get; set; }
        public int WickWidth { get; set; }
        public int BodyHeight { get; set; }
        public int BodyWidth { get; set; }
    }
    public class MqlRates
    {
        public DateTime Timestamp;
        public float Open;
        public float High;
        public float Low;
        public float Close;
    }
    public class MacdRates
    {
        public DateTime Timestamp;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float Macd;

        public Brush Colour;
        public CandleGraphics CandleGraphics;
        public void SetCandleColour(float macdColour)
        {
            try
            {
                switch (macdColour)
                {
                    case (float)MacdColour.LimeGreen:
                        Colour = new SolidBrush(Color.LimeGreen);
                        break;
                    case (float)MacdColour.Green:
                        Colour = new SolidBrush(Color.Green);
                        break;
                    case (float)MacdColour.Red:
                        Colour = new SolidBrush(Color.Red);
                        break;
                    case (float)MacdColour.Firebrick:
                        Colour = new SolidBrush(Color.Firebrick);
                        break;
                    default:
                        Colour = new SolidBrush(Color.DimGray);
                        break;
                }
            }
            catch (Exception)
            {
                Colour = new SolidBrush(Color.DimGray);
            }
        }
        public void SetCandleDimensions(int bodyWidth = 3, float points = 1_000)
        {
            CandleGraphics = new CandleGraphics
            {
                BodyWidth = bodyWidth,
                WickWidth = bodyWidth / 3,
                BodyHeight = (int)((Open > Close ? Open - Close : Close - Open) * points),
                WickHeight = (int)((High - Low) * points)
            };
        }
        public int GetRelativeValue(int value, double maxPoints, int panelHeight)
        {
            try
            {
                var valuePercentage = value / maxPoints * 100;

                return (int)Math.Floor(valuePercentage / 100 * panelHeight);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
    //ticket time  type magic  identifier reason  volume price_open       sl tp  price_current swap  profit symbol comment
    public class TradePosition
    {
        public int Ticket;
        public DateTime Time;
        public int Type;
        public int Magic;
        public int Identifier;
        public int Reason;
        public float Volume;
        public float PriceOpen;
        public float StopLoss;
        public float TakeProfit;
        public float PriceCurrent;
        public float Swap;
        public float Profit;
        public string Symbol;
        public string Comment;
    }
}
