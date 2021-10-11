﻿using System;
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
        public string Symbol = "Volatility 10 Index";
        public MqlHelper()
        {
            Runtime.PythonDLL = @"C:\Python\36\python36.dll";
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
