using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
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
        }
        #endregion
        public string Symbol = "";
        public MqlHelper(string symbol = "Volatility 10 Index")
        {
            Runtime.PythonDLL = @"C:\Python\36\python36.dll";

            Symbol = symbol;
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
        public TradePosition[] GetOpenPositions(string symbol)
        {
            try
            {
                using (Py.GIL())
                {
                    dynamic mt5 = Py.Import("MetaTrader5");

                    _ = mt5.initialize();

                    TradePosition[] openPositions = new TradePosition[PyConvert.ToInt(mt5.positions_total())];

                    dynamic positions = mt5.positions_get(symbol: symbol);

                    for (int i = 0; i < openPositions.Length; i++)
                    {
                        openPositions[i] = PyConvert.ToTradePosition(positions[i]);
                    }

                    return openPositions;
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
        public List<MacdRates> GetMacdRates(string symbol, int period = 1, int index = 1, int count = 200_000)
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
    }
}
