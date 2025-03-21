using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibration
{
    public class Function
    {
        const double A0 = 0.36;
        const double A1 = 2.16;
        const double A2 = 3.65;
        const double A3 = -7.15;
        const double A4 = -53.01;
        const double A5 = -6.11;
        const double A6 = 16.9;

        public List<double> CurrentValues { get; set; } = new List<double>();
        public List<double> PreviousValues { get; set; } = new List<double>();
        public List<double> NextValues { get; set; } = new List<double>();
        

        public void ClearValues()
        {
            PreviousValues = new List<double>(CurrentValues);
            CurrentValues.Clear();
            NextValues.Clear();
        }

        public void EvaluateValues(List<double> dots, double step)
        {
            ClearValues();

            foreach(var dot in dots)
            {
                CurrentValues.Add(GetValue(dot));
                //Console.WriteLine(CurrentValues[0]);
                NextValues.Add(GetValue(dot + step));
            }
        }
        public double GetValue(double x)
        {
            return (A0 * Math.Pow(x, 6) + A1 * Math.Pow(x, 5) + A2 * Math.Pow(x, 4) + A3 * Math.Pow(x, 3) + A4 * Math.Pow(x, 2) + A5 * x + A6);
        }

        public Function(int size)
        {
            CurrentValues = new List<double>(size);
            PreviousValues = new List<double>(size);
            NextValues = new List<double>(size);
        }
    }
}
