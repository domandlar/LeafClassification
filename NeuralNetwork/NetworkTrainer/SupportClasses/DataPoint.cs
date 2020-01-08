using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NeuralNetwork.NetworkTrainer.SupportClasses
{
    public class DataPoint
    {
        public double[] Input { get; set; }
        public double[] Output { get; set; }
        public int InputSize { get { return Input.Length; } }
        public int OutputSize { get { return Output.Length; } }

        public DataPoint() { }
        public DataPoint(double[] Input, double[] Output) { Load(Input, Output); }
        public DataPoint(XmlElement Elem) { Load(Elem); }

        public void Load(double[] Input, double[] Output)
        {
            this.Input = new double[Input.Length]; this.Output = new double[Output.Length];

            Array.Copy(Input, this.Input, Input.Length);
            Array.Copy(Output, this.Output, Output.Length);
        }
        public void Load(XmlElement elem)
        {
            XmlNode nType;
            int lIn, lOut, i;

            nType = elem.SelectSingleNode("Input");
            lIn = nType.ChildNodes.Count;

            Input = new double[lIn];
            foreach (XmlNode node in nType.ChildNodes)
            {
                XmlElement Node = (XmlElement)node;
                double val;

                int.TryParse(Node.GetAttribute("Index"), out i);
                double.TryParse(Node.InnerText, out val);

                Input[i] = val;
            }

            nType = elem.SelectSingleNode("Output");
            lOut = nType.ChildNodes.Count;

            Output = new double[lOut];
            foreach (XmlNode node in nType.ChildNodes)
            {
                XmlElement Node = (XmlElement)node;
                double val;

                int.TryParse(Node.GetAttribute("Index"), out i);
                double.TryParse(Node.InnerText, out val);

                Output[i] = val;
            }
        }

        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement nDataPoint, nType, node;
            int lIn = Input.Length, lOut = Output.Length;

            nDataPoint = doc.CreateElement("DataPoint");
            nType = doc.CreateElement("Input");

            for (int i = 0; i < lIn; i++)
            {
                node = doc.CreateElement("Data");
                node.SetAttribute("Index", i.ToString());
                node.AppendChild(doc.CreateTextNode(Input[i].ToString()));
                nType.AppendChild(node);
            }

            nDataPoint.AppendChild(nType);

            nType = doc.CreateElement("Output");

            for (int i = 0; i < lOut; i++)
            {
                node = doc.CreateElement("Data");
                node.SetAttribute("Index", i.ToString());
                node.AppendChild(doc.CreateTextNode(Output[i].ToString()));
                nType.AppendChild(node);
            }

            nDataPoint.AppendChild(nType);

            return nDataPoint;
        }
  
    }
}
