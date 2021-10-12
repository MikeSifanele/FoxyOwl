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
        public static float CalculateCandleColour(float previousMacd, float currentMacd)
        {
            try
            {
                if (currentMacd > 0)
                {
                    if (currentMacd > previousMacd)
                        return (float)MacdColour.LimeGreen;
                    if (currentMacd < previousMacd)
                        return (float)MacdColour.Green;
                }
                else if (currentMacd < 0)
                {
                    if (currentMacd < previousMacd)
                        return (float)MacdColour.Red;
                    if (currentMacd > previousMacd)
                        return (float)MacdColour.Firebrick;
                }

                return (float)MacdColour.DimGray;
            }
            catch (Exception)
            {
                return (float)MacdColour.DimGray;
            }
        }
    }
}
