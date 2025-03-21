using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibration
{
    public class Manager
    {
        public List<double> Dots { get; set; }

        public Function Function { get; set; }
        public Prediction Prediction { get; set; }

        double Step = 0.001;
        int DotsCount = 51;
        int MasterDotIndex { get; set; }

        public Manager()
        {
            Dots = Enumerable.Range(0, DotsCount).Select(i => i * Step).ToList();
            Function = new Function(DotsCount);
            Prediction = new Prediction(DotsCount);
            MasterDotIndex = DotsCount / 2;

            Function.EvaluateValues(Dots, Step);
        }

        public void Run()
        {
            IncreaseDots();
            Function.EvaluateValues(Dots, Step);
            Prediction.EvaluatePredictedValues(Function, Dots);
            Prediction.AdjustWeights(Function, MasterDotIndex);


        }

        private void IncreaseDots()
        {
            Dots = Dots.Select(d => d + Step).ToList();
        }

    }
}
