using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.NetworkTrainer.SupportClasses
{
    public class TrainingProgressReport
    {
        public int Iteration { get; set; }
        public double Error { get; set; }

        public TrainingProgressReport(int iteration, double error)
        {
            Iteration = iteration;
            Error = error;
        }
    }
}
