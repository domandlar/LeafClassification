using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NeuralNetwork.NeuralNetwork
{
    public class BackPropagationNetwork
    {
        #region Private data

        private int _layerCount;
        private int[] _layerSize;
        private int _inputSize;
        private TransferFunction[] _transferFunction;

        private double[][] _layerOutput;
        private double[][] _layerInput;
        private double[][] _bias;
        private double[][] _delta;
        private double[][] _previousBiasDelta;

        private double[][][] _weight;
        private double[][][] _previousWeightDelta;

        private XmlDocument _doc = null;

        #endregion

        #region Public data

        public string Name { get; set; } = "Default";
        public int InputSize 
        {
            get
            {
                return _inputSize;
            }
            set
            {
                _inputSize = value;
            }
        }
        

        #endregion

        #region Constructors

        public BackPropagationNetwork(int[] layerSizes, TransferFunction[] transferFunctions)
        {
            if (transferFunctions.Length != layerSizes.Length || transferFunctions[0] != TransferFunction.None)
                throw new ArgumentException("Mreza se ne moze izgraditi s tim parametrima.");
            _layerCount = layerSizes.Length - 1;
            InputSize = layerSizes[0];
            _layerSize = new int[_layerCount];

            for (int i = 0; i < _layerCount; i++)
                _layerSize[i] = layerSizes[i + 1];

            _transferFunction = new TransferFunction[_layerCount];
            for (int i = 0; i < _layerCount; i++)
                _transferFunction[i] = transferFunctions[i + 1];
            _bias = new double[_layerCount][];
            _previousBiasDelta = new double[_layerCount][];
            _delta = new double[_layerCount][];
            _layerOutput = new double[_layerCount][];
            _layerInput = new double[_layerCount][];

            _weight = new double[_layerCount][][];
            _previousWeightDelta = new double[_layerCount][][];

            for (int l = 0; l < _layerCount; l++)
            {
                _bias[l] = new double[_layerSize[l]];
                _previousBiasDelta[l] = new double[_layerSize[l]];
                _delta[l] = new double[_layerSize[l]];
                _layerOutput[l] = new double[_layerSize[l]];
                _layerInput[l] = new double[_layerSize[l]];

                _weight[l] = new double[l == 0 ? InputSize : _layerSize[l - 1]][];
                _previousWeightDelta[l] = new double[l == 0 ? InputSize : _layerSize[l - 1]][];

                for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                {
                    _weight[l][i] = new double[_layerSize[l]];
                    _previousWeightDelta[l][i] = new double[_layerSize[l]];
                }
            }

            for (int l = 0; l < _layerCount; l++)
            {
                for (int j = 0; j < _layerSize[l]; j++)
                {
                    _bias[l][j] = Math.Round(Gaussian.GetRandomGaussian(), 1);
                    _previousBiasDelta[l][j] = 0.0;
                    _layerOutput[l][j] = 0.0;
                    _layerInput[l][j] = 0.0;
                    _delta[l][j] = 0.0;
                }

                for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                {
                    for (int j = 0; j < _layerSize[l]; j++)
                    {
                        _weight[l][i][j] = Math.Round(Gaussian.GetRandomGaussian(), 1);
                        _previousWeightDelta[l][i][j] = 0.0;
                    }
                }
            }
        }

        public BackPropagationNetwork(string filePath)
        {
            Load(filePath);
        }

        #endregion

        #region Methods

        public void Run(ref double[] input, out double[] output)
        {
            if (input.Length != InputSize)
                throw new ArgumentException("Ulazni podaci nisu dobre dimenzije.");

            output = new double[_layerSize[_layerCount - 1]];

            for (int l = 0; l < _layerCount; l++)
            {
                for (int j = 0; j < _layerSize[l]; j++)
                {
                    double sum = 0.0;
                    for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                        sum += _weight[l][i][j] * (l == 0 ? input[i] : _layerOutput[l - 1][i]);

                    sum += _bias[l][j];
                    _layerInput[l][j] = sum;

                    _layerOutput[l][j] = TransferFunctions.Evaluate(_transferFunction[l], sum);
                }
            }

            for (int i = 0; i < _layerSize[_layerCount - 1]; i++)
                output[i] = _layerOutput[_layerCount - 1][i];
        }

        public double Train(ref double[] input, ref double[] desired, double trainingRate, double momentum)
        {
            if (input.Length != InputSize)
                throw new ArgumentException("Neizpravan ulazni parametar", "input");
            if (desired.Length != _layerSize[_layerCount - 1])
                throw new ArgumentException("Neispravan ulazni parametar", "desired");

            double error = 0.0, sum = 0.0, weightDelta = 0.0, biasDelta = 0.0;
            double[] output = new double[_layerSize[_layerCount - 1]];

            Run(ref input, out output);

            for (int l = _layerCount - 1; l >= 0; l--)
            {
                // Izlazni sloj
                if (l == _layerCount - 1)
                {
                    for (int k = 0; k < _layerSize[l]; k++)
                    {
                        _delta[l][k] = output[k] - desired[k];
                        error += Math.Pow(_delta[l][k], 2);
                        _delta[l][k] *= TransferFunctions.EvaluateDerivative(_transferFunction[l],
                                                                            _layerInput[l][k]);
                    }
                }
                else //Skriveni sloj
                {
                    for (int i = 0; i < _layerSize[l]; i++)
                    {
                        sum = 0.0;
                        for (int j = 0; j < _layerSize[l + 1]; j++)
                        {
                            sum += _weight[l + 1][i][j] * _delta[l + 1][j];
                        }
                        sum *= TransferFunctions.EvaluateDerivative(_transferFunction[l], _layerInput[l][i]);

                        _delta[l][i] = sum;
                    }
                }
            }

            for (int l = 0; l < _layerCount; l++)
                for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                    for (int j = 0; j < _layerSize[l]; j++)
                    {
                        weightDelta = trainingRate * _delta[l][j] * (l == 0 ? input[i] : _layerOutput[l - 1][i])
                                        + momentum * _previousWeightDelta[l][i][j];
                        _weight[l][i][j] -= weightDelta;

                        _previousWeightDelta[l][i][j] = weightDelta;
                    }

            for (int l = 0; l < _layerCount; l++)
                for (int i = 0; i < _layerSize[l]; i++)
                {
                    biasDelta = trainingRate * _delta[l][i];
                    _bias[l][i] -= biasDelta + momentum * _previousBiasDelta[l][i];

                    _previousBiasDelta[l][i] = biasDelta;
                }

            return error;
        }

        public void Save(string filePath)
        {
            if (filePath == null)
                return;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";

            XmlWriter writer = XmlWriter.Create(filePath, settings);


            // Pocetak elementa
            writer.WriteStartElement("NeuralNetwork");
            writer.WriteAttributeString("Type", "BackPropagation");

            // Parametri element
            writer.WriteStartElement("Parameters");

            writer.WriteElementString("Name", Name);
            writer.WriteElementString("inputSize", InputSize.ToString());
            writer.WriteElementString("layerCount", _layerCount.ToString());

            // Slojevi
            writer.WriteStartElement("Layers");

            for (int l = 0; l < _layerCount; l++)
            {
                writer.WriteStartElement("Layer");

                writer.WriteAttributeString("Index", l.ToString());
                writer.WriteAttributeString("Size", _layerSize[l].ToString());
                writer.WriteAttributeString("Type", _transferFunction[l].ToString());

                writer.WriteEndElement();   // Sloj
            }

            writer.WriteEndElement();   // Slojevi

            writer.WriteEndElement();   // Parametri

            // Tezine i biasi
            writer.WriteStartElement("Weights");

            for (int l = 0; l < _layerCount; l++)
            {
                writer.WriteStartElement("Layer");
                writer.WriteAttributeString("Index", l.ToString());

                for (int j = 0; j < _layerSize[l]; j++)
                {
                    writer.WriteStartElement("Node");
                    writer.WriteAttributeString("Index", j.ToString());
                    writer.WriteAttributeString("Bias", _bias[l][j].ToString());

                    for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                    {
                        writer.WriteStartElement("Axon");
                        writer.WriteAttributeString("Index", i.ToString());

                        writer.WriteString(_weight[l][i][j].ToString());

                        writer.WriteEndElement();   // Axon
                    }

                    writer.WriteEndElement();   // Cvor
                }

                writer.WriteEndElement();   // Sloj
            }

            writer.WriteEndElement();   // Tezine

            writer.WriteEndElement();   // NeuralNetwork

            writer.Flush();
            writer.Close();
        }

        public void Load(string filePath)
        {
            if (filePath == null)
                return;

            _doc = new XmlDocument();
            _doc.Load(filePath);

            string basePath = "", nodePath = "";
            double value;

            if (XPathValue("NeuralNetwork/@Type") != "BackPropagation")
                return;

            basePath = "NeuralNetwork/Parameters/";
            Name = XPathValue(basePath + "Name");

            int.TryParse(XPathValue(basePath + "inputSize"), out _inputSize);
            int.TryParse(XPathValue(basePath + "layerCount"), out _layerCount);

            _layerSize = new int[_layerCount];
            _transferFunction = new TransferFunction[_layerCount];

            basePath = "NeuralNetwork/Parameters/Layers/Layer";
            for (int l = 0; l < _layerCount; l++)
            {
                int.TryParse(XPathValue(basePath + "[@Index='" + l.ToString() + "']/@Size"), out _layerSize[l]);
                Enum.TryParse<TransferFunction>(XPathValue(basePath + "[@Index='" + l.ToString() + "']/@Type"), out _transferFunction[l]);
            }

            _bias = new double[_layerCount][];
            _previousBiasDelta = new double[_layerCount][];
            _delta = new double[_layerCount][];
            _layerOutput = new double[_layerCount][];
            _layerInput = new double[_layerCount][];

            _weight = new double[_layerCount][][];
            _previousWeightDelta = new double[_layerCount][][];

            for (int l = 0; l < _layerCount; l++)
            {
                _bias[l] = new double[_layerSize[l]];
                _previousBiasDelta[l] = new double[_layerSize[l]];
                _delta[l] = new double[_layerSize[l]];
                _layerOutput[l] = new double[_layerSize[l]];
                _layerInput[l] = new double[_layerSize[l]];

                _weight[l] = new double[l == 0 ? InputSize : _layerSize[l - 1]][];
                _previousWeightDelta[l] = new double[l == 0 ? InputSize : _layerSize[l - 1]][];

                for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                {
                    _weight[l][i] = new double[_layerSize[l]];
                    _previousWeightDelta[l][i] = new double[_layerSize[l]];
                }
            }

            for (int l = 0; l < _layerCount; l++)
            {
                basePath = "NeuralNetwork/Weights/Layer[@Index='" + l.ToString() + "']/";
                for (int j = 0; j < _layerSize[l]; j++)
                {
                    nodePath = "Node[@Index='" + j.ToString() + "']/@Bias";
                    double.TryParse(XPathValue(basePath + nodePath), out value);

                    _bias[l][j] = value;
                    _previousBiasDelta[l][j] = 0.0;
                    _layerOutput[l][j] = 0.0;
                    _layerInput[l][j] = 0.0;
                    _delta[l][j] = 0.0;
                }

                for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                {
                    for (int j = 0; j < _layerSize[l]; j++)
                    {
                        nodePath = "Node[@Index='" + j.ToString() + "']/Axon[@Index='" + i.ToString() + "']";
                        double.TryParse(XPathValue(basePath + nodePath), out value);

                        _weight[l][i][j] = value;
                        _previousWeightDelta[l][i][j] = 0.0;
                    }
                }
            }
            _doc = null;
        }

        public void Nudge(double scalar)
        {
            for (int l = 0; l < _layerCount; l++)
            {
                for (int j = 0; j < _layerSize[l]; j++)
                {
                    for (int i = 0; i < (l == 0 ? InputSize : _layerSize[l - 1]); i++)
                    {
                        double w = _weight[l][i][j];
                        double u = Gaussian.GetRandomGaussian(0f, w * scalar);
                        _weight[l][i][j] += u;
                        _previousWeightDelta[l][i][j] = 0f;
                    }
                    double b = _bias[l][j];
                    double v = Gaussian.GetRandomGaussian(0f, b * scalar);
                    _bias[l][j] += v;
                    _previousBiasDelta[l][j] = 0f;
                }
            }
        }

        private string XPathValue(string xPath)
        {
            XmlNode node = _doc.SelectSingleNode(xPath);

            if (node == null)
                throw new ArgumentException("Cannot find specified node", xPath);

            return node.InnerText;
        }

        #endregion
    }
}
