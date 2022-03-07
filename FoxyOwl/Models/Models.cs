using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxyOwl.Models
{
    public class Position
    {
        public PositionTime PositionTime;
        public MarketOrderEnum MarketOrder;
        public float PriceOpen;
        public float PriceClose;
        public float PriceHigh;
        public float PriceLow;
        public int? StopLoss;
        public int Profit;
    }
    public struct ExpertAction
    {
        public MarketOrderEnum MarketOrder;
        public int? StopLoss;
        public ExpertAction(MarketOrderEnum marketOrder, int stopLoss = 30)
        {
            MarketOrder = marketOrder;
            StopLoss = stopLoss;
        }
    }
    public enum MarketOrderEnum
    {
        Buy,
        Sell,
        Nothing,
        Count
    }
    public struct PositionTime
    {
        public DateTime Open;
        public DateTime? Close;
        public PositionTime(DateTime timestamp)
        {
            Open = timestamp;
            Close = null;
        }
    }
    public struct Rates
    {
        public DateTime Time;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public Rates(string[] data)
        {
            Time = Convert.ToDateTime(data[0]);

            Open = float.Parse(data[1], CultureInfo.InvariantCulture.NumberFormat);
            High = float.Parse(data[2], CultureInfo.InvariantCulture.NumberFormat);
            Low = float.Parse(data[3], CultureInfo.InvariantCulture.NumberFormat);
            Close = float.Parse(data[4], CultureInfo.InvariantCulture.NumberFormat);
        }
        public float[] ToFloatArray()
        {
            return new float[] { Open, High, Low, Close };
        }
        public string DateTimeString()
        {
            return $"{Time.Month},{(int)Time.DayOfWeek},{Time.Day},{Time.Hour},{Time.Minute}";
        }
    }
    public struct Macds
    {
        public float Close;
        public float M1MacdHigh;
        public float M1MacdLow;
        public float M1MacdClose;
        public float M2MacdClose;
        public float M3MacdClose;
        public float[] ToFloatArray()
        {
            return new float[] { M1MacdHigh, M1MacdLow, M1MacdClose, M2MacdClose, M3MacdClose };
        }
    }
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
