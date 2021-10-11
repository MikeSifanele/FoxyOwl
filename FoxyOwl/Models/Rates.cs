using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxyOwl.Models
{
    public class MqlRates
    {
        public DateTime Timestamp;
        public float Open;
        public float High;
        public float Low;
        public float Close;
    }
    public class MacdRates
    {
        public DateTime Timestamp;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float Macd;
    }
}
