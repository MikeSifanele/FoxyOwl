using MtApi5;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly MtApi5Client _mtapi = new MtApi5Client();
        public MqlHelper()
        {
            _mtapi.BeginConnect(8228);

            while (_mtapi.ConnectionState != Mt5ConnectionState.Connected) { }
        }
        public async Task<MqlRates[]> GetMqlRates(string symbol, ENUM_TIMEFRAMES period, int index, int count = 1)
        {
            MqlRates[] mqlRates = null;

            try
            {
                _ = await Execute(() => _mtapi.CopyRates(symbol, period, index, count, out mqlRates));
            }
            catch (Exception)
            {

            }

            return mqlRates;
        }
        public async Task<MqlRates[]> GetMqlRates(string symbol, ENUM_TIMEFRAMES period, DateTime start, DateTime end)
        {
            MqlRates[] mqlRates = null;

            try
            {
                _ = await Execute(() => _mtapi.CopyRates(symbol, period, start, end, out mqlRates));
            }
            catch (Exception)
            {

            }

            return mqlRates;
        }
        public async Task<MqlTradeResult> Buy(string symbol, double volume)
        {
            MqlTradeResult tradeResult = null;
            _ = await Execute(() => _mtapi.Buy(out tradeResult, volume, symbol));
            return tradeResult;
        }
        public async Task<MqlTradeResult> Sell(string symbol, double volume)
        {
            MqlTradeResult tradeResult = null;
            _ = await Execute(() => _mtapi.Sell(out tradeResult, volume, symbol));
            return tradeResult;
        }
        private async Task<TResult> Execute<TResult>(Func<TResult> func)
        {
            return await Task.Factory.StartNew(() =>
            {
                var result = default(TResult);
                try
                {
                    result = func();
                }
                catch (ExecutionException ex)
                {
                    Debug.WriteLine($"Exception: {ex.ErrorCode} - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                }

                return result;
            });
        }
    }
}
