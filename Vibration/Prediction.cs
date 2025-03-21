using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibration
{
    public class Prediction
    {
        public List<double> Weights { get; set; }
        public List<double> PredictedValues { get; set; }
        double ErrorTreshold { get; set; } = 0.1;
        double WeightCorrection { get; set; } = 0.001;



        public double GetPredictedValue(Function function, int dotIndex)
        {
            var predictedValue = Weights[0] * function.CurrentValues[dotIndex - 1] +
                Weights[1] * function.CurrentValues[dotIndex] +
                Weights[2] * function.CurrentValues[dotIndex + 1] +
                Weights[3] * function.PreviousValues[dotIndex];
            return predictedValue;
        }

        public void AdjustWeights(Function function, int dotIndex)
        {
            var predictionError = PredictedValues[dotIndex] - function.NextValues[dotIndex];
            var errorSign = predictionError > ErrorTreshold ? -1 : 1;

            Weights[0] += WeightCorrection * errorSign * Math.Sign(function.CurrentValues[dotIndex - 1]);
            Weights[1] += WeightCorrection * errorSign * Math.Sign(function.CurrentValues[dotIndex]);
            Weights[2] += WeightCorrection * errorSign * Math.Sign(function.CurrentValues[dotIndex + 1]);
            Weights[3] += WeightCorrection * errorSign * Math.Sign(function.PreviousValues[dotIndex - 1]);
        }

        public void EvaluatePredictedValues(Function function, List<double> dots)
        {
            PredictedValues.Clear();
            PredictedValues.Add(0);
            for (var dotIndex = 1; dotIndex < dots.Count - 1; dotIndex++)
            {
                PredictedValues.Add(GetPredictedValue(function, dotIndex));
            }
            PredictedValues[0] = PredictedValues[1];
            PredictedValues.Add(PredictedValues.Last());
        }

        public Prediction(int size)
        {
            Weights = new List<double> { 0, 0, 0, 0 };
            PredictedValues = new List<double>(size);
        }
    }
}
