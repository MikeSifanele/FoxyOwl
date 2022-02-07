using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoxyOwl.Models;

namespace FoxyOwl
{
    public static class DataExport
    {
        #region Delegates
        public delegate void WriteToConsoleHandler(string message, Color consoleColor);
        public static event WriteToConsoleHandler WriteToConsole;
        #endregion
        static MLTrader _mlTrader;
        static void Export()
        {
            _mlTrader = new MLTrader();

            using (var myWriter = new StreamWriter(@"C:\Users\MikeSifanele\OneDrive - Optimi\Documents\Data\xauusd.DAT"))
            {
                StringBuilder states = new StringBuilder();
                StringBuilder labels = new StringBuilder();
                StringBuilder state = new StringBuilder();
                StringBuilder json = new StringBuilder();

                for (var x = 0; x < 30_000; x++)
                {
                    state = new StringBuilder();
                    var obs = _mlTrader.GetObservation();

                    for (int i = 0; i < _mlTrader.ObservationLength; i++)
                    {
                        state.Append($",[{obs[i].Open},{obs[i].High},{obs[i].Low},{obs[i].Close}]");
                    }

                    states.Append($",[{state.ToString().TrimStart(',')}]");

                    labels.Append($",{_mlTrader.FutureRates.Close}");
                }

                json.Append($"{{\"states\":[{states.ToString().TrimStart(',')}],\"labels\":[{labels.ToString().TrimStart(',')}]}}");

                myWriter.Write(json.ToString());
            }
        }
    }
    public class MLTrader
    {
        #region Private fields
        private Rates[] _rates;
        private readonly int _observationLength = 240;
        private readonly int _startIndex = 239;
        private int _index;
        private int _epoch = 0;
        private float _accumulativeReward = 0;
        private float _maximumReward = 0;
        private List<Position> _openPositions = new List<Position>();
        private List<Position> _closedPositions = new List<Position>();
        #endregion
        #region Public properties
        public int ObservationLength => _observationLength;
        public int CurrentIndex => _index - _startIndex;
        public bool CanGoBack => _index > _startIndex;
        public bool IsLastStep => _index == MaximumRates - 2;
        public int MaximumRates => _rates.Length - 1;
        public int Points => 100;
        public float MaximumReward => _maximumReward;
        public float AccumulativeReward => _accumulativeReward;
        public Rates PreviousRates => _rates[_index - 1];
        public Rates CurrentRates => _rates[_index];
        public Rates FutureRates => _rates[_index + 1];
        public List<Position> OpenPositions => _openPositions;
        public List<Position> ClosedPositions => _closedPositions;
        #endregion
        #region Delegates
        public delegate void PrintHandler(string message, TextColourEnum textColour);
        public static event PrintHandler Print;
        #endregion
        private static MLTrader _instance;
        public static MLTrader Instance => _instance ?? (_instance = new MLTrader());
        public MLTrader()
        {
            using (var streamReader = new StreamReader(@"C:\Users\MikeSifanele\OneDrive - Optimi\Documents\Data\XAUUSD.csv"))
            {
                List<Rates> rates = new List<Rates>();

                _ = streamReader.ReadLine();

                int i = 0;
                while (!streamReader.EndOfStream)
                {
                    rates.Add(new Rates(streamReader.ReadLine().Split(',')));
                }

                _rates = rates.ToArray();
            }

            Reset();
        }
        public Rates[] GetObservation()
        {
            List<Rates> observation = new List<Rates>();

            for (int i = _index - (_observationLength - 1); i <= _index; i++)
            {
                observation.Add(_rates[i]);
            }

            return observation.ToArray();
        }
        public ExpertAction GetExpertAction()
        {
            return default;
        }
        public void OpenPosition(MarketOrderEnum marketOrder, int? stopLoss = null, bool isLive = false)
        {
            try
            {
                var position = new Position()
                {
                    PositionTime = new PositionTime(_rates[_index].Time),
                    MarketOrder = marketOrder,
                    OpenPrice = _rates[_index].Open,
                    StopLoss = stopLoss
                };

                _openPositions.Add(position);

                Print?.Invoke($"Opened position, Open time: {position.PositionTime.Open}", TextColourEnum.Info);
            }
            catch (Exception)
            {

            }
        }
        public void UpdatePositions()
        {
            try
            {
                for (int i = 0; i < _openPositions.Count; i++)
                {
                    _openPositions[i].Profit = GetPoints(_openPositions[i].MarketOrder, _openPositions[i].OpenPrice, _rates[i].Close);

                    if (_openPositions[i].StopLoss.HasValue)
                        if (_openPositions[i].Profit < -_openPositions[i].StopLoss)
                            ClosePosition(i);
                }
            }
            catch (Exception)
            {

            }
        }
        public void ClosePosition(int index)
        {
            try
            {
                var position = _openPositions[index];

                position.ClosePrice = _rates[_index].Close;
                position.PositionTime.Close = Convert.ToDateTime(_rates[_index].Time);

                AddReward(position.Profit);

                _openPositions.RemoveAt(index);
                _closedPositions.Add(position);

                Print?.Invoke($"Closed position, Open time: {position.PositionTime.Open}, Close time: {position.PositionTime.Close}, Profit: ${FormatNumber(position.Profit)}, Accumulatice reward: ${FormatNumber(_accumulativeReward)}", position.Profit > 0? TextColourEnum.Success: TextColourEnum.Error);
            }
            catch (Exception)
            {

            }
        }
        public void ClosePositions()
        {
            try
            {
                for (int i = 0; i < _openPositions.Count; i++)
                {
                    ClosePosition(i);
                }
            }
            catch (Exception)
            {

            }
        }
        public float GetReward(MarketOrderEnum action)
        {
            return GetPoints(action);
        }
        public void AddReward(float reward)
        {
            _accumulativeReward += reward;

            _maximumReward += reward > 0 ? reward : Math.Abs(reward);
        }
        public int GetRisk(MarketOrderEnum action, bool isExpert = false)
        {
            //var index = isExpert ? _currentSignal.Index : _index;
            //var openPrice = action == (int)MarketOrderEnum.Buy ? _rates[index].Low : _rates[index].High;

            return default;//GetPoints(action, openPrice, _rates[_index].Open) ?? 0;
        }
        public int GetPoints(MarketOrderEnum action, float? openPrice = null, float? closePrice = null)
        {
            openPrice = openPrice ?? _rates[_index].Open;
            closePrice = closePrice ?? _rates[_index].Close;

            int points = 0;

            if (action == MarketOrderEnum.Buy)
                points = (int)((closePrice - openPrice) * 10);
            else if (action == MarketOrderEnum.Sell)
                points = (int)((openPrice - closePrice) * 10);

            if (action == MarketOrderEnum.Nothing)
                points = points > 0 ? -points : points;

            return points;
        }
        public void Reset()
        {
            _epoch++;
            _accumulativeReward = 0;
            _index = _startIndex;
        }
        public string GetReport()
        {
            var rewardString = FormatNumber(_accumulativeReward);
            var maximumRewardString = FormatNumber(_maximumReward);

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Episode ended: {_epoch}\n");
            stringBuilder.AppendLine($"Reward: ${rewardString}/${maximumRewardString}\n");
            stringBuilder.AppendLine($"Accuracy: {FormatNumber(_accumulativeReward / _maximumReward * 100)}%\n");
            stringBuilder.AppendLine($"Total trades: {_closedPositions.Count}");
            stringBuilder.AppendLine($"Trades won: {_closedPositions.Where(x => x.Profit > 0).Count()}");
            stringBuilder.AppendLine($"Trades lost: {_closedPositions.Where(x => x.Profit <= 0).Count()}");
            stringBuilder.AppendLine($"Maximum profit: ${FormatNumber(_closedPositions.Max(x => x.Profit))}");
            stringBuilder.AppendLine($"Maximum drawdown: ${FormatNumber(_closedPositions.Min(x => x.Profit))}");

            return stringBuilder.ToString();
        }
        public string FormatNumber(float value)
        {
            return value.ToString("N", CultureInfo.CreateSpecificCulture("sv-SE"));
        }
        public string FormatNumber(int value)
        {
            return value.ToString("N", CultureInfo.CreateSpecificCulture("sv-SE"));
        }
        public bool Step(int increment = 1)
        {
            _index += increment;

            return IsLastStep;
        }
    }
}
