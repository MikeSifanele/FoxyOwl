using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxyOwl.Models
{
    public static class EmaPeriod
    {
        public static int Fast = 8;
        public static int Slow = 17;
    }
    public class CandleGraphics
    {
        public int WickHeight {  get; set; }
        public int WickWidth {  get; set; }
        public int BodyHeight {  get; set; }
        public int BodyWidth {  get; set; }
    }
    public class Candlestick
    {
        public DateTime Timestamp { get; set; }
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public float FastEMA { get; set; }
        public float SlowEMA { get; set; }
        public Brush Colour { get; set; }
        public CandleGraphics CandleGraphics {  get; set; }
        public Candlestick(string[] data)
        {
            Timestamp = Convert.ToDateTime($"{data[0]} {data[1]}");
            Open = Convert.ToSingle(data[2]);
            High = Convert.ToSingle(data[3]);
            Low = Convert.ToSingle(data[4]);
            Close = Convert.ToSingle(data[5]);

            SetCandleDimensions();
        }
        public void SetCandleColor(Candlestick previousCandle, Candlestick currentCandle)
        {
            try
            {
                FastEMA = CalculateEMA(previousCandle.FastEMA, EmaPeriod.Fast);
                SlowEMA = CalculateEMA(previousCandle.SlowEMA, EmaPeriod.Slow);

                var currentMacd = CalculateMacd(currentCandle);
                var previousMacd = CalculateMacd(previousCandle);

                Colour = new SolidBrush(Color.DimGray);

                if (currentMacd > 0)
                {
                    if (currentMacd > previousMacd)
                        Colour = new SolidBrush(Color.LimeGreen);
                    if (currentMacd < previousMacd)
                        Colour = new SolidBrush(Color.Green);
                }

                if (currentMacd < 0)
                {
                    if (currentMacd < previousMacd)
                        Colour = new SolidBrush(Color.Red);
                    if (currentMacd > previousMacd)
                        Colour = new SolidBrush(Color.Firebrick);
                }
            }
            catch (Exception)
            {

            }
        }
        public float CalculateMacd(Candlestick candlestick)
        {
            try
            {
                return candlestick.FastEMA - candlestick.SlowEMA;
            }
            catch (Exception)
            {
                return 0f;
            }
        }
        public float CalculateEMA(float previousEma, int period)
        {
            try
            {
                float alpha = (float)(2.0 / (1.0 + period));

                return previousEma + alpha * (Close - previousEma);
            }
            catch (Exception)
            {
                return Close;
            }
        }
        public void SetCandleDimensions(int bodyWidth = 30, int wickWidth = 10, int point = 1_000)
        {
            CandleGraphics = new CandleGraphics
            {
                BodyWidth = bodyWidth,
                WickWidth = wickWidth,
                BodyHeight = (int)((Open > Close ? Open - Close : Close - Open) * point),
                WickHeight = (int)((High - Low) * point)
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
}
