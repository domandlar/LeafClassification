using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace LeafClassification.Models
{
    public class PlantModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Bitmap> LeafImages { get; set; } = new List<Bitmap>();
        public double[] Output { get; set; }
        public double Probability { get; set; }

        public PlantModel(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }
}
