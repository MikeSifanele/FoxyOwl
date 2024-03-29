﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using FoxyOwl.Models;
using FoxyOwl.Indicators;
using System.Drawing;

namespace FoxyOwl.Converters
{
    public static class PyConvert
    {
        public static DateTime ToDateTime(dynamic value)
        {
            // Unix timestamp is seconds past epoch
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((int)PyInt.AsInt(value));
        }
        public static double ToDouble(dynamic value)
        {
            return (double)PyFloat.AsFloat(value);
        }
        public static float ToFloat(dynamic value)
        {
            return (float)PyFloat.AsFloat(value);
        }
        public static int ToInt(dynamic value)
        {
            return (int)PyInt.AsInt(value);
        }
        public static bool ToBool(dynamic value)
        {
            return (int)PyInt.AsInt(value) == 1;
        }
        public static float[] ToFloatArray(dynamic value, float[] extras = null)
        {
            var valueLength = ToInt(value.shape[1]);
            var results = new float[extras == null ? valueLength : valueLength + extras?.Length];

            int i = 0;
            for (; i < valueLength; i++)
                results[i] = PyFloat.AsFloat(value[0][i]);

            for (var j = 0; j < extras.Length; j++)
                results[i++] = extras[j];

            return results;
        }
        public static float[] ToFloatArray(dynamic value, float? extra = null)
        {
            var valueLength = ToInt(value.shape[1]);
            var result = new float[extra.HasValue ? valueLength + 1 : valueLength];

            int i = 0;
            for (; i < valueLength; i++)
                result[i] = PyFloat.AsFloat(value[0][i]);

            if (extra.HasValue)
                result[i] = extra.Value;

            return result;
        }
        public static Rates ToMqlRates(dynamic value)
        {
            try
            {
                return new Rates()
                {
                    Time = PyConvert.ToDateTime(value[0]),
                    Open = PyConvert.ToFloat(value[1]),
                    High = PyConvert.ToFloat(value[2]),
                    Low = PyConvert.ToFloat(value[3]),
                    Close = PyConvert.ToFloat(value[4])
                };
            }
            catch (Exception)
            {
                return default;
            }
        }
        public static TradePosition ToTradePosition(dynamic value)
        {
            try
            {
                return new TradePosition()
                {
                    Ticket = PyConvert.ToInt(value.ticket),
                    Time = PyConvert.ToDateTime(value.time),
                    Profit = PyConvert.ToFloat(value.profit),
                    Symbol = (string)value.symbol,
                    Comment = (string)value.comment,
                    Volume = PyConvert.ToFloat(value.volume),
                    PriceCurrent = PyConvert.ToFloat(value.price_current),
                    PriceOpen = PyConvert.ToFloat(value.price_open),
                    Type = PyConvert.ToInt(value.type),
                    Magic = PyConvert.ToInt(value.magic)
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static double Trancate(double value, int digits)
        {
            try
            {
                var results = value.ToString().Split('.');

                return double.Parse($"{results[0]}.{results[1].Substring(0, digits)}");
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
    public static class ChartConvert
    {
        private static readonly int _points = 100;
        public static int ToRelativeValue(int value, double maxPoints, int panelHeight)
        {
            try
            {
                var valuePercentage = value / maxPoints * 100;

                var results = (int)Math.Floor(valuePercentage / 100 * panelHeight);

                return results == 0 ? 1 : results;
            }
            catch (Exception)
            {
                return 1;
            }
        }
        public static int GetBodyHeight(float open, float close)
        {
            return (int)((open > close ? open - close : close - open) * _points);
        }
        public static int GetBodyHeight(Rates rates)
        {
            return (int)((rates.Open > rates.Close ? rates.Open - rates.Close : rates.Close - rates.Open) * _points);
        }
        public static int GetWickHeight(float high, float low)
        {
            return (int)((high - low) * _points);
        }
        public static int GetWickHeight(Rates rates)
        {
            return (int)((rates.High - rates.Low) * _points);
        }
        public static Brush GetCandleColour(Rates rates)
        {
            return rates.Close > rates.Open ? new SolidBrush(Color.LimeGreen) : new SolidBrush(Color.Red);
        }
    }
    public static class NumberConvert
    {
        public static Color ToColour(TextColourEnum textColour)
        {
            switch (textColour)
            {
                case TextColourEnum.Warning:
                    return Color.Orange;
                case TextColourEnum.Error:
                    return Color.Red;
                case TextColourEnum.Info:
                    return Color.LightGray;
                case TextColourEnum.Success:
                    return Color.LimeGreen;
            }

            return Color.Purple;
        }
    }
    public static class CandleConvert
    {
        public static Rates[] ToResolution(Rates[] rates, Resolution resolution)
        {
            try
            {
                int i = 0;
                while (rates[i].Time.Minute % (int)resolution > 0)
                    i++;

                List<Rates> results = new List<Rates>();

                Rates currentRates = new Rates()
                {
                    Time = rates[0].Time,
                    Open = rates[0].Open,
                    Low = rates[0].Low,
                };

                DateTime time = rates[0].Time.AddMinutes((int)resolution);

                for (; i < rates.Length; i++)
                {
                    if (rates[i].Time >= time)
                    {
                        currentRates.Close = rates[i - 1].Close;
                        results.Add(currentRates);

                        currentRates = new Rates()
                        {
                            Time = rates[i].Time,
                            Open = rates[i].Open,
                            Low = rates[i].Low,
                        };

                        time = time.AddMinutes((int)resolution);
                    }

                    if (rates[i].High > currentRates.High)
                        currentRates.High = rates[i].High;
                    if (rates[i].Low < currentRates.Low)
                        currentRates.Low = rates[i].Low;
                }

                return results.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public static class ObservationConvert
    {
        public static Macds[] ToMacds(Rates[] rates)
        {
            try
            {
                Macds macds = new Macds();
                List<Macds> results = new List<Macds>();

                float FastOpenEma = 0, FastCloseEma = 0, FastHighEma = 0, FastLowEma = 0;
                float SlowOpenEma = 0, SlowCloseEma = 0, SlowHighEma = 0, SlowLowEma = 0;

                #region Set rates
                FastOpenEma = Macd.CalculateEMA(rates[0].Open, FastOpenEma, (int)EmaPeriod.Fast);
                SlowOpenEma = Macd.CalculateEMA(rates[0].Open, SlowOpenEma, (int)EmaPeriod.Slow);

                macds.Open = Macd.CalculateMacd(FastOpenEma, SlowOpenEma);

                FastCloseEma = Macd.CalculateEMA(rates[0].Close, FastCloseEma, (int)EmaPeriod.Fast);
                SlowCloseEma = Macd.CalculateEMA(rates[0].Close, SlowCloseEma, (int)EmaPeriod.Slow);

                macds.Close = Macd.CalculateMacd(FastCloseEma, SlowCloseEma);

                FastHighEma = Macd.CalculateEMA(rates[0].High, FastHighEma, (int)EmaPeriod.Fast);
                SlowHighEma = Macd.CalculateEMA(rates[0].High, SlowHighEma, (int)EmaPeriod.Slow);

                macds.High = Macd.CalculateMacd(FastHighEma, SlowHighEma);

                FastLowEma = Macd.CalculateEMA(rates[0].Low, FastLowEma, (int)EmaPeriod.Fast);
                SlowLowEma = Macd.CalculateEMA(rates[0].Low, SlowLowEma, (int)EmaPeriod.Slow);

                macds.Low = Macd.CalculateMacd(FastLowEma, SlowLowEma);

                macds.Sentiment = 0;

                results.Add(macds);
                #endregion

                for (var i = 1; i < rates.Length; i++)
                {
                    FastOpenEma = Macd.CalculateEMA(rates[i].Open, FastOpenEma, (int)EmaPeriod.Fast);
                    SlowOpenEma = Macd.CalculateEMA(rates[i].Open, SlowOpenEma, (int)EmaPeriod.Slow);

                    macds.Open = Macd.CalculateMacd(FastOpenEma, SlowOpenEma);

                    FastCloseEma = Macd.CalculateEMA(rates[i].Close, FastCloseEma, (int)EmaPeriod.Fast);
                    SlowCloseEma = Macd.CalculateEMA(rates[i].Close, SlowCloseEma, (int)EmaPeriod.Slow);

                    macds.Close = Macd.CalculateMacd(FastCloseEma, SlowCloseEma);

                    FastHighEma = Macd.CalculateEMA(rates[i].High, FastHighEma, (int)EmaPeriod.Fast);
                    SlowHighEma = Macd.CalculateEMA(rates[i].High, SlowHighEma, (int)EmaPeriod.Slow);

                    macds.High = Macd.CalculateMacd(FastHighEma, SlowHighEma);

                    FastLowEma = Macd.CalculateEMA(rates[i].Low, FastLowEma, (int)EmaPeriod.Fast);
                    SlowLowEma = Macd.CalculateEMA(rates[i].Low, SlowLowEma, (int)EmaPeriod.Slow);

                    macds.Low = Macd.CalculateMacd(FastLowEma, SlowLowEma);

                    macds.Sentiment = (float)Macd.CalculateCandleColour(results[i - 1].Close, macds.Close) / (int)MacdColour.Count;

                    results.Add(macds);
                }

                return results.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
