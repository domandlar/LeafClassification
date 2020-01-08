using NeuralNetwork.NetworkTrainer.SupportClasses;
using NeuralNetwork.NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.NetworkTrainer
{
    public class NetworkTrainer
    {
        private double _error;
        private int _iterations;
        private Permutator _idx;
        private List<double> _errorHistory;

        public double MaxError { get; set; } = 0.1;
        public double MaxIterations { get; set; } = 100000;
        public double TrainingRate { get; set; } = 0.25;
        public double Momentum { get; set; } = 0.15;
        public int NudgeWindow { get; set; } = 50;
        public double NudgeScale { get; set; } = 0.25;
        public double NudgeTolerance { get; set; } = 0.0001;     

        public BackPropagationNetwork Network { get; set; }
        public DataSet DataSet { get; set; }

        public event EventHandler<TrainingProgressReport> IterationCompleted;

        public NetworkTrainer(BackPropagationNetwork BPN, DataSet DS)
        {
            Network = BPN; DataSet = DS;
            _idx = new Permutator(DataSet.Size);
            _iterations = 0;

            _errorHistory = new List<double>();
        }

        public void TrainDataSet()
        {
            do
            {

                _iterations++; _error = 0.0;
                _idx.Permute(DataSet.Size);

                for (int i = 0; i < DataSet.Size; i++)
                {
                    var input = DataSet.Data[_idx[i]].Input;
                    var output = DataSet.Data[_idx[i]].Output;
                    _error += Network.Train(ref input,
                                            ref output,
                                            TrainingRate, Momentum);
                    DataSet.Data[_idx[i]].Input = input;
                    DataSet.Data[_idx[i]].Output = output;
                }

                _errorHistory.Add(_error);

                if (_iterations % NudgeWindow == 0)
                    CheckNudge();

                IterationCompleted?.Invoke(this, new TrainingProgressReport(_iterations, _error));
            } while (_error > MaxError && _iterations < MaxIterations);
        }

        public double[] GetErrorHistory()
        {
            return _errorHistory.ToArray();
        }

        private void CheckNudge()
        {
            double oldAvg = 0f, newAvg = 0f;
            int l = _errorHistory.Count;

            if (_iterations < 2 * NudgeWindow) return;

            for (int i = 0; i < NudgeWindow; i++)
            {
                oldAvg += _errorHistory[l - 2 * NudgeWindow + i];
                newAvg += _errorHistory[l - NudgeWindow + i];
            }

            oldAvg /= NudgeWindow; newAvg /= NudgeWindow;

            if (((double)Math.Abs(newAvg - oldAvg)) / NudgeWindow < NudgeTolerance)
            {
                Network.Nudge(NudgeScale);
            }
        }
    }
}
