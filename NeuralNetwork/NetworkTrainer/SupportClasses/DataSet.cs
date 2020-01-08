using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NeuralNetwork.NetworkTrainer.SupportClasses
{
    public class DataSet
    {
        public List<DataPoint> Data { get; set; }
        public int Size
        {
            get { return Data.Count; }
        }

        public DataSet() { Data = new List<DataPoint>(); }
        public XmlElement ToXml(XmlDocument doc)
        {
            XmlElement nDataSet;

            nDataSet = doc.CreateElement("DataSet");

            foreach (DataPoint d in Data)
                nDataSet.AppendChild(d.ToXml(doc));

            return nDataSet;
        }

        public void Load(XmlElement nDataSet)
        {
            foreach (XmlNode node in nDataSet.ChildNodes)
            {
                DataPoint d = new DataPoint((XmlElement)node);
                Data.Add(d);
            }
        }
    }
}
