using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoxyOwl.Converters;
using FoxyOwl.Models;
using FoxyOwl.Indicators;
using Python.Runtime;

namespace FoxyOwl
{
    public static class DataExport
    {
        #region Delegates
        public delegate void WriteToConsoleHandler(string message, Color consoleColor);
        public static event WriteToConsoleHandler WriteToConsole;
        #endregion
        static MLTrader _mlTrader;
        public static void ExportTrainingData()
        {
            //Runtime.PythonDLL = @"C:\Python\37\python37.dll";          

            //using (Py.GIL())
            //{
            try
            {
                //dynamic np = Py.Import("numpy");
                //dynamic tensorflow = Py.Import("tensorflow");
                //dynamic model = tensorflow?.keras?.models?.load_model(@"C:\python-repos\step-model-2");

                _mlTrader = new MLTrader();

                using (var myWriter = new StreamWriter(@"C:\Users\MikeSifanele\OneDrive - Optimi\Documents\Data\step super data.DAT"))
                {
                    StringBuilder states = new StringBuilder();
                    StringBuilder labels = new StringBuilder();
                    StringBuilder json = new StringBuilder();

                    for (var x = 0; x < _mlTrader.MaximumRates && !_mlTrader.IsLastStep; x++)
                    {
                        states.Append($"{_mlTrader.GetObservation(moveForward: true)},");
                        labels.Append($"[{_mlTrader.CurrentMacdColour}],");
                    }

                    json.Append($"{{\"states\":[{states.ToString().TrimEnd(',')}],\"labels\":[{labels.ToString().TrimEnd(',')}]}}");

                    myWriter.Write(json.ToString());
                }
            }
            catch (Exception ex)
            {
                WriteToConsole?.Invoke(ex.Message, Color.Red);
            }
            //}
        }
        public static void ExportRates(Rates[] rates, string filename = "step rates m3")
        {
            using (var myWriter = new StreamWriter($@"C:\Users\MikeSifanele\OneDrive - Optimi\Documents\Data\{filename}.csv"))
            {
                for (var x = 0; x < rates.Length; x++)
                {
                    myWriter.WriteLine($"");
                }
            }
        }
    }
    public class MLTrader
    {
        #region Private fields
        private Rates[] _rates;
        private Macds[] _macds;
        private readonly int _observationLength = 180;
        private int _startIndex = 179;
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
        public bool IsLastStep => _index == MaximumRates;
        public int MaximumRates => _rates.Length - 5;
        public int Points => 100;
        public float MaximumReward => _maximumReward;
        public float AccumulativeReward => _accumulativeReward;
        public Rates PreviousRates => _rates[_index - 1];
        public Rates CurrentRates => _rates[_index];
        public int CurrentMacdColour => Macd.CalculateCandleColour(_macds[_index - 1].Close, _macds[_index].Close);
        public Rates FutureRates => _rates[_index + 1];
        public RollingWindow RollingMacds;
        public List<Position> OpenPositions => _openPositions;
        public List<Position> ClosedPositions => _closedPositions;
        #endregion
        #region Delegates
        public delegate void PrintHandler(string message, TextColourEnum textColour);
        public static event PrintHandler Print;
        #endregion
        private static MLTrader _instance;
        public static MLTrader Instance => _instance ?? (_instance = new MLTrader());
        public MLTrader(int startIndex = 182)
        {
            using (var streamReader = new StreamReader(@"C:\Users\MikeSifanele\OneDrive - Optimi\Documents\Data\Step Index M1.csv"))
            {
                var rates = new List<Rates>();

                _ = streamReader.ReadLine();

                while (!streamReader.EndOfStream)
                {
                    if (rates.Count > 2 && rates[rates.Count - 2].Time.AddMinutes(1) != rates[rates.Count - 1].Time)
                        break;

                    rates.Add(new Rates(streamReader.ReadLine().Split('\t')));
                }

                _rates = rates.ToArray();
                _macds = ObservationConvert.ToMacds(rates.ToArray());

                SetIndex(Math.Max(startIndex, _observationLength-1));
                WarmUpRollingWindows();
            }

            Reset();
        }
        public void WarmUpRollingWindows()
        {
            RollingMacds = new RollingWindow(ObservationLength);

            for (int i = 0; i <= RollingMacds.Length; i++)
            {
                _ = RollingMacds.Append(_macds[i].ToFloatArray());
            }
        }
        public Rates[] GetObservation()
        {
            var observation = new List<Rates>();

            for (int i = _index - (_observationLength - 1); i <= _index; i++)
            {
                observation.Add(_rates[i]);
            }

            return observation.ToArray();
        }
        public RollingWindow GetObservation(bool moveForward = false)
        {
            _ = RollingMacds.Append(_macds[moveForward ? _index++ : _index].ToFloatArray());

            return RollingMacds;
        }
        public ExpertAction GetExpertAction()
        {
            return default;
        }
        public void OpenPosition(MarketOrderEnum marketOrder, int? stopLoss = null)
        {
            try
            {
                var position = new Position()
                {
                    PositionTime = new PositionTime(_rates[_index].Time),
                    MarketOrder = marketOrder,
                    PriceOpen = _rates[_index].Open,
                    StopLoss = stopLoss,
                    PriceLow = _rates[_index].Low,
                    PriceHigh = _rates[_index].High,
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
                    _openPositions[i].Profit = GetPoints(_openPositions[i].MarketOrder, _openPositions[i].PriceOpen, _rates[_index].Close);

                    _openPositions[i].PriceHigh = Math.Max(_rates[_index].High, _openPositions[i].PriceHigh);
                    _openPositions[i].PriceLow = Math.Min(_rates[_index].Low, _openPositions[i].PriceLow);

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

                position.PriceClose = _rates[_index].Close;
                position.PositionTime.Close = Convert.ToDateTime(_rates[_index].Time);

                AddReward(position.Profit);

                _openPositions.RemoveAt(index);
                _closedPositions.Add(position);

                Print?.Invoke($"Closed position, Open time: {position.PositionTime.Open}, Close time: {position.PositionTime.Close}, Profit: ${FormatNumber(position.Profit)}, Accumulatice reward: ${FormatNumber(_accumulativeReward)}", position.Profit > 0 ? TextColourEnum.Success : TextColourEnum.Error);
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
        public void Reset(int? startIndex = null)
        {
            if (startIndex != null)
                _startIndex = (int)startIndex;

            _epoch++;
            _accumulativeReward = 0;
            _index = _startIndex;

            _openPositions.Clear();
            _closedPositions.Clear();
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
        public void SetIndex(int index)
        {
            if (index >= _observationLength - 1)
                _index = index;
        }
        public float[] GetHighLow()
        {
            try
            {
                float[] highLow = new float[] { _rates[_index].High, _rates[_index].Low };

                int i = 1;
                while (!IsLastStep)
                {
                    if (highLow[0] < _rates[_index + i].Close)
                    {
                        highLow[0] = _rates[_index + i].Close;
                    }

                    if (highLow[1] > _rates[_index + i].Close)
                    {
                        highLow[1] = _rates[_index + i].Close;
                    }

                    i++;
                }

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
