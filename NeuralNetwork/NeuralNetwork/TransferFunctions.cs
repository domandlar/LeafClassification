using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork.NeuralNetwork
{
    static class TransferFunctions
    {
        public static double Evaluate(TransferFunction tFunc, double input)
        {
            switch (tFunc)
            {
                case TransferFunction.Sigmoid:
                    return Sigmoid(input);

                case TransferFunction.Linear:
                    return Linear(input);

                case TransferFunction.Gaussian:
                    return Gaussian(input);

                case TransferFunction.RationalSigmoid:
                    return RationalSigmoid(input);

                case TransferFunction.None:
                default:
                    return 0.0;
            }
        }
        public static double EvaluateDerivative(TransferFunction tFunc, double input)
        {
            switch (tFunc)
            {
                case TransferFunction.Sigmoid:
                    return SigmoidDerivative(input);

                case TransferFunction.Linear:
                    return LinearDerivative(input);

                case TransferFunction.Gaussian:
                    return GaussianDerivative(input);

                case TransferFunction.RationalSigmoid:
                    return RationalSigmoidDerivative(input);

                case TransferFunction.None:
                default:
                    return 0.0;
            }
        }

        private static double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }
        private static double SigmoidDerivative(double x)
        {
            return Sigmoid(x) * (1 - Sigmoid(x));
        }

        private static double Linear(double x)
        {
            return x;
        }
        private static double LinearDerivative(double x)
        {
            return 1.0;
        }

        private static double Gaussian(double x)
        {
            return Math.Exp(-Math.Pow(x, 2));
        }
        private static double GaussianDerivative(double x)
        {
            return -2.0 * x * Gaussian(x);
        }

        private static double RationalSigmoid(double x)
        {
            return x / (1.0 + Math.Sqrt(1.0 + x * x));
        }
        private static double RationalSigmoidDerivative(double x)
        {
            double val = Math.Sqrt(1.0 + x * x);
            return 1.0 / (val * (1 + val));
        }

    }
}
