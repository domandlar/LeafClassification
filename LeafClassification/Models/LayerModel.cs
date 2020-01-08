using NeuralNetwork.NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeafClassification.Models
{
     public class LayerModel
    {
        public int Layer { get; private set; }
        public TransferFunction TransferFunction { get; set; }
        public LayerModel(int layer)
        {
            Layer = layer;
        }
        public LayerModel(int layer, TransferFunction transferFunction)
        {
            Layer = layer;
            TransferFunction = transferFunction;
        }
    }
}
