using System.Collections.Generic;

namespace FoxyOwl.Models
{
    public class RollingWindow
    {
        public List<float[]> Window = new List<float[]>();
        public int Length;
        public RollingWindow(int length = 10)
        {
            Length = length;
        }
        public List<float[]> Append(float[] value)
        {
            Window.Add(value);

            if(Window.Count > Length)
                Window.RemoveAt(0);

            return Window;
        }
        public List<float[]> Prepend(float[] value)
        {
            Window.Insert(0, value);

            if (Window.Count > Length)
                Window.RemoveAt(Length);

            return Window;
        }
        public override string ToString()
        {
            string rowResults;
            string results = string.Empty;

            for (int r = 0; r < Length; r++)
            {
                rowResults = string.Empty;

                for (int c = 0; c < Window[0].Length; c++)
                    rowResults += $",{Window[r][c]}";

                results += $"[{rowResults.TrimStart(',')}],";
            }

            return $"[{results.TrimEnd(',')}]";
        }
    }
}
