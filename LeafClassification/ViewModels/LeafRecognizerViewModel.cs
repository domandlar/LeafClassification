using Caliburn.Micro;
using ImageProcessing;
using LeafClassification.Models;
using Microsoft.Win32;
using NeuralNetwork.NeuralNetwork;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeafClassification.ViewModels
{
    public class LeafRecognizerViewModel : Screen
    {
        private double[] output = null;
        private BackPropagationNetwork bpn = null;

        public Bitmap LeafImage{ get; set; }
        public string LeafImageUrl{ get; set; }
        public List<PlantModel> Plants { get; set; } = new List<PlantModel>();
        public List<PlantModel> Results { get; set; }
        public uint Threshold { get; set; } = 40;
        public int MinLine { get; set; } = 10;
        public int Distance { get; set; } = 10;
        public int NumberOfInputNodes { get; set; }
       
        public LeafRecognizerViewModel()
        {
            Task.Run(() =>
            {
                if (File.Exists(@"tezine.xml"))
                {
                    bpn = new BackPropagationNetwork(@"tezine.xml");
                    NumberOfInputNodes = bpn.InputSize;
                }
                if (File.Exists(@"vrste.txt"))
                {
                    List<string> plants = new List<string>(File.ReadLines(@"vrste.txt").ToList());
                    int id = 0;
                    foreach (var plant in plants)
                    {
                        Plants.Add(new PlantModel(plant, id++));
                    }
                    NotifyOfPropertyChange(() => Plants);
                }
                NotifyOfPropertyChange(() => CanRecognizeLeaf);
            });
        }

        public void LoadLeaf()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
            Nullable<bool> dialogOK = fileDialog.ShowDialog();
            if (dialogOK == true)
            {
                LeafImageUrl = fileDialog.FileName;
                LeafImage = new Bitmap(fileDialog.FileName);
                NotifyOfPropertyChange(() => LeafImageUrl);
            }
        }

        public bool CanRecognizeLeaf => bpn != null && Plants != null;
        public void RecognizeLeaf()
        {
            Task.Run(() =>
            {
                ImageProcessor imageProcessor = new ImageProcessor(LeafImage);
                imageProcessor.EdgeDetect(Threshold);
                imageProcessor.Thinning();
                imageProcessor.CheckLines(MinLine);
                imageProcessor.MarkPoints(Distance);
                imageProcessor.CalcAngels();
                List<LeafToken> leafTokens = imageProcessor.GetTokens();
                double[] input = new double[NumberOfInputNodes];
                for (int i = 0; i < NumberOfInputNodes; i++)
                {
                    input[i] = i < leafTokens.Count - 1 ? leafTokens[i].Sin : 0;
                }
                bpn.Run(ref input, out output);
                for (int i = 0; i < Plants.Count; i++)
                {
                    Plants[i].Probability = output[i];
                }
                Results = Plants.OrderByDescending(x => x.Probability).ToList();
                NotifyOfPropertyChange(() => Results);
            });
        }
    }
}
