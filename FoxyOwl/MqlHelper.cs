using System;
using System.Collections.Generic;
using Python.Runtime;
using FoxyOwl.Models;
using FoxyOwl.Indicators;

namespace FoxyOwl
{
    public class MqlHelper
    {
        #region Static Properties
        static MqlHelper _instance;
        public static MqlHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MqlHelper();

                return _instance;
            }
            set { _instance = value; }
        }
        #endregion
        public MqlHelper(string pythonPath = @"C:\Python\36\python36.dll")
        {
            Runtime.PythonDLL = pythonPath;
        }
        public int GetPoints(string symbol)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    return int.Parse("1".PadRight(PyConvert.ToInt(mt5.symbol_info(symbol).digits) + 1, '0'));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public int GetDigits(string symbol)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    return PyConvert.ToInt(mt5.symbol_info(symbol).digits);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public int GetTotalPositions(string symbol = "")
        {
            try
            {
                using (Py.GIL())
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    TradePosition tradePosition = null;

                    int totalPositions = PyConvert.ToInt(mt5.positions_total());

                    if (!string.IsNullOrEmpty(symbol))
                    {
                        int positionsCount = 0;
                        dynamic positions = mt5.positions_get(symbol: symbol);

                        for (int i = 0; i < totalPositions; i++)
                        {
                            tradePosition = PyConvert.ToTradePosition(positions[i]);

                            if (tradePosition?.Symbol == symbol)
                                positionsCount++;
                        }

                        return positionsCount;
                    }
                    else
                        return totalPositions;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public TradePosition[] GetTradePositions(string symbol)
        {
            try
            {
                using (Py.GIL())
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    TradePosition[] tradePositions = new TradePosition[PyConvert.ToInt(mt5.positions_total())];

                    dynamic positions = mt5.positions_get(symbol: symbol);

                    for (int i = 0; i < tradePositions.Length; i++)
                    {
                        tradePositions[i] = PyConvert.ToTradePosition(positions[i]);
                    }

                    return tradePositions;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<MqlRates> GetMqlRates(string symbol, int period = 1, int index = 1, int count = 200_000)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    dynamic rates = mt5.copy_rates_from_pos(symbol, period, index, count);

                    var results = new List<MqlRates>();

                    for (int i = 0; i < PyConvert.ToInt(rates.size); i++)
                    {
                        results.Add(PyConvert.ToMqlRates(rates[i]));
                    }

                    return results;
                }
                catch (Exception)
                {
                    return new List<MqlRates>();
                }
            }
        }
        public List<MacdRates> GetMacdRates(string symbol, int period = 1, int index = 0, int count = 200_000)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    dynamic rates = mt5.copy_rates_from_pos(symbol, period, index, count);

                    var results = new List<MacdRates>();

                    results.Add(PyConvert.ToMacdRates(rates[0]));
                    results[0].SetCandleColour((int)MacdColour.Neutral);

                    float fastEma, slowEma = 0;

                    fastEma = slowEma = results[0].Close;

                    for (int i = results.Count; i < PyConvert.ToInt(rates.size); i++)
                    {
                        results.Add(PyConvert.ToMacdRates(rates[i]));

                        fastEma = Macd.CalculateEMA(results[i].Close, fastEma, (int)EmaPeriod.Fast);
                        slowEma = Macd.CalculateEMA(results[i].Close, slowEma, (int)EmaPeriod.Slow);

                        results[i].Macd = Macd.CalculateMacd(fastEma, slowEma);
                        results[i].SetCandleColour(Macd.CalculateCandleColour(prevMacd: results[i - 1].Macd, currMacd: results[i].Macd));
                    }

                    return results;
                }
                catch (Exception)
                {
                    return new List<MacdRates>();
                }
            }
        }
        public List<MqlRates> GetMqlRates(string symbol, int period, DateTime dateFrom, DateTime dateTo)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    dynamic rates = mt5.copy_rates_range(symbol, period, dateFrom, dateTo);

                    var results = new List<MqlRates>();

                    for (int i = 0; i < PyConvert.ToInt(rates.size); i++)
                    {
                        results.Add(PyConvert.ToMqlRates(rates[i]));
                    }

                    return results;
                }
                catch (Exception)
                {
                    return new List<MqlRates>();
                }
            }
        }
        public List<MqlRates> GetMqlRates(string symbol, int period, DateTime dateFrom, int count)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    dynamic rates = mt5.copy_rates_range(symbol, period, dateFrom, count);

                    var results = new List<MqlRates>();

                    for (int i = 0; i < PyConvert.ToInt(rates.size); i++)
                    {
                        results.Add(PyConvert.ToMqlRates(rates[i]));
                    }

                    return results;
                }
                catch (Exception)
                {
                    return new List<MqlRates>();
                }
            }
        }
        public bool OpenBuyOrder(string symbol, double volume, string comment = "")
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    _ = mt5.Buy(symbol, volume, comment: comment);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public bool OpenSellOrder(string symbol, double volume, string comment = "")
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    _ = mt5.Sell(symbol, volume, comment: comment);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public bool PositionCloseAll(string symbol)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    return PyConvert.ToBool(mt5.Close(symbol));
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public int GetTickVolume(string symbol, int period, int index = 0)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    dynamic rates = mt5.copy_rates_from_pos(symbol, period, index, 1);

                    return PyConvert.ToInt(rates[0][5]);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public int GetCandleInterval(int period)
        {
            try
            {
                var timeCurrent = DateTime.Now;
                var nextTimestamp = timeCurrent.AddMinutes(-(timeCurrent.Minute % period)).AddSeconds(-(timeCurrent.Second % 59)).AddMinutes(period);

                return (int)(nextTimestamp - timeCurrent).TotalMilliseconds;
            }
            catch (Exception)
            {
                return 1_000;
            }
        }
        public double GetLotSize(string symbol, int percent = 10)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    var symbolInfo = mt5.symbol_info(symbol);

                    var volume_min = PyConvert.ToDouble(symbolInfo.volume_min);
                    var volume_max = PyConvert.ToDouble(symbolInfo.volume_max);
                    var volume_step = PyConvert.ToDouble(symbolInfo.volume_step);
                    var digits = PyConvert.ToInt(symbolInfo.digits) + 1;

                    var equity = PyConvert.ToDouble(mt5.account_info().equity);

                    var lotSize = PyConvert.Trancate(equity * volume_step * (percent / 100f), digits);

                    if (lotSize > volume_max)
                        return volume_max;
                    else if (lotSize < volume_min)
                        return volume_min;
                    else
                        return lotSize;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
    }
}
