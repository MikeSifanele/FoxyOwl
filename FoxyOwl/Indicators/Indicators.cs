using FoxyOwl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxyOwl.Indicators
{
    public static class Macd
    {

        public static float CalculateEMA(float close, float previousEma, int period)
        {
            try
            {
                float alpha = (float)(2.0 / (1.0 + period));

                return previousEma + alpha * (close - previousEma);
            }
            catch (Exception)
            {
                return close;
            }
        }
        public static float CalculateMacd(float fastEMA, float slowEMA)
        {
            try
            {
                return fastEMA - slowEMA;
            }
            catch (Exception)
            {
                return 0f;
            }
        }
        public static float CalculateCandleColour(float prevMacd, float currMacd)
        {
            try
            {
                if (currMacd > 0)
                {
                    if (currMacd > prevMacd)
                        return (float)MacdColour.LimeGreen;
                    if (currMacd < prevMacd)
                        return (float)MacdColour.Green;
                }
                else if (currMacd < 0)
                {
                    if (currMacd < prevMacd)
                        return (float)MacdColour.Red;
                    if (currMacd > prevMacd)
                        return (float)MacdColour.Firebrick;
                }

                return (float)MacdColour.Neutral;
            }
            catch (Exception)
            {
                return (float)MacdColour.Neutral;
            }
        }
    }
}
