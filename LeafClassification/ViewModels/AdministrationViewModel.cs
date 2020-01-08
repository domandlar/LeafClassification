using Caliburn.Micro;
using ImageProcessing;
using LeafClassification.Models;
using Microsoft.Win32;
using NeuralNetwork.NetworkTrainer;
using NeuralNetwork.NetworkTrainer.SupportClasses;
using NeuralNetwork.NeuralNetwork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml;

namespace LeafClassification.ViewModels
{
    public class AdministrationViewModel : Screen
    {
        private int _numberOfInputNodes = 1500;
        private uint _threshold = 40;
        private string _newPlant;
        private BindableCollection<PlantModel> _plants = new BindableCollection<PlantModel>();
        private PlantModel _selectedPlant = null;
        private float _maxError = 0.1f;
        private int _maxIterations = 10000;
        private int _nudgeWindow = 500;
        private int _numberOfHiddenLayers;
        private BindableCollection<LayerModel> _layers;
        private LayerModel _selectedLayer;
        private BindableCollection<TransferFunction> _transferFunctions = new BindableCollection<TransferFunction> { TransferFunction.None, TransferFunction.Gaussian, TransferFunction.Linear, TransferFunction.RationalSigmoid, TransferFunction.Sigmoid };
        private TransferFunction _selectedFunction;

        private DataSet _dataSet = new DataSet();

        public int NumberOfInputNodes
        {
            get 
            {
                return _numberOfInputNodes; 
            }
            set 
            {
                _numberOfInputNodes = value;
                NotifyOfPropertyChange(() => CanTrainNetwork);
            }
        }
        public uint Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }
        public string NewPlant
        {
            get
            {
                return _newPlant;
            }
            set
            {
                _newPlant = value;
                NotifyOfPropertyChange(() => NewPlant);
                NotifyOfPropertyChange(() => CanAddNewPlant);
            }
        }
        public BindableCollection<PlantModel> Plants
        {
            get { return _plants; }
            set { _plants = value; }
        }
        public PlantModel SelectedPlant
        {
            get
            {
                return _selectedPlant;
            }
            set
            {
                _selectedPlant = value;
                NotifyOfPropertyChange(() => SelectedPlant);
                NotifyOfPropertyChange(() => CanLoadLeafs);
                NotifyOfPropertyChange(() => NumberOfSamples);
            }
        }
        public float MaxError
        {
            get 
            { 
                return _maxError; 
            }
            set 
            {
                _maxError = value;
                NotifyOfPropertyChange(() => CanTrainNetwork);
            }
        }
        public int MaxIterations
        {
            get 
            { 
                return _maxIterations; 
            }
            set 
            { 
                _maxIterations = value;
                NotifyOfPropertyChange(() => CanTrainNetwork);
            }
        }
        public int NudgeWindow
        {
            get
            {
                return _nudgeWindow; 
            }
            set 
            { 
                _nudgeWindow = value;
                NotifyOfPropertyChange(() => CanTrainNetwork);
            }
        }
        public int NumberOfHiddenLayers
        {
            get { return _numberOfHiddenLayers; }
            set
            {
                _numberOfHiddenLayers = value;
                FillComboBoxWithLayers();
                NotifyOfPropertyChange(() => Layers);
                NotifyOfPropertyChange(() => CanTrainNetwork);
            }
        }
        public BindableCollection<LayerModel> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }
        public LayerModel SelectedLayer
        {
            get
            {
                return _selectedLayer;
            }
            set
            {
                _selectedLayer = value;
                if (value != null)
                {
                    SelectedFunction = value.TransferFunction;
                }
                else
                {
                    SelectedFunction = TransferFunction.None;
                }
                NotifyOfPropertyChange(() => SelectedLayer);
                NotifyOfPropertyChange(() => SelectedFunction);
                NotifyOfPropertyChange(() => CanTrainNetwork);
            }
        }
        public TransferFunction SelectedFunction
        {
            get { return _selectedFunction; }
            set
            {
                _selectedFunction = value;
                if (SelectedLayer != null)
                    SelectedLayer.TransferFunction = value;
                NotifyOfPropertyChange(() => CanTrainNetwork);

            }
        }
        public BindableCollection<TransferFunction> TransferFunctions
        {
            get { return _transferFunctions; }
            set { _transferFunctions = value; }
        }

        public int MinLine { get; set; } = 10;
        public int Distance { get; set; } = 10;

        public int NumberOfSamples
        {
            get
            {
                return SelectedPlant != null ? SelectedPlant.LeafImages.Count : 0;
            }
        }

        public int TotalNumberOfSamples
        {
            get
            {
                return Plants.Sum(x => x.LeafImages.Count);
            }
        }

        private GettingTokensProgressReportModel _progress = new GettingTokensProgressReportModel();
        public GettingTokensProgressReportModel Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                NotifyOfPropertyChange(() => Progress);
            }
        }

        private TrainingNetworkProgressReportModel _trainingNetworkReport = new TrainingNetworkProgressReportModel();

        public TrainingNetworkProgressReportModel TrainingNetworkReport
        {
            get { return _trainingNetworkReport; }
            set { _trainingNetworkReport = value; }
        }


        public event EventHandler<GettingTokensProgressReportModel> ProgressChanged;
        public event EventHandler<string> NetworkTrained;


        public bool CanAddNewPlant => NewPlant != "" && NewPlant != null;
        public void AddNewPlant()
        {
            Plants.Add(new PlantModel(NewPlant, Plants.Count));
            SelectedPlant = Plants[Plants.Count - 1];
            NewPlant = "";
            NotifyOfPropertyChange(() => Plants);
            NotifyOfPropertyChange(() => NewPlant);
            NotifyOfPropertyChange(() => SelectedPlant);
            NotifyOfPropertyChange(() => CanAddNewPlant);
            NotifyOfPropertyChange(() => CanGetTokens);
        }
        public bool CanLoadLeafs => SelectedPlant != null;
        public void LoadLeafs()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
            Nullable<bool> dialogOK = fileDialog.ShowDialog();
            if (dialogOK == true)
            {
                foreach (string file in fileDialog.FileNames)
                {
                    SelectedPlant.LeafImages.Add(new Bitmap(file));
                }
            }
            NotifyOfPropertyChange(() => NumberOfSamples);
            NotifyOfPropertyChange(() => TotalNumberOfSamples);
            NotifyOfPropertyChange(() => CanGetTokens);
        }
        public bool CanGetTokens =>
            Plants.Count != 0 && (Plants.Where(x => x.LeafImages.Count == 0).ToList().Count == 0 || _dataSet != null);
        public void GetTokens()
        {
            Progress = new GettingTokensProgressReportModel();
            Progress.MaxProgress = Plants.Sum(x => x.LeafImages.Count); ;
            Progress.ImagesProcessed = 0;
            Progress.Status = "Obrađivanje...";
            dynamic settings = SetSettingsForProgressReportPopup();
            Task.Run(() =>
            {
                foreach (var plant in Plants)
                {
                    foreach (var leafImage in plant.LeafImages)
                    {
                        ImageProcessor imageProcessor = new ImageProcessor(leafImage);
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
                        double[] output = new double[Plants.Count];
                        for (int i = 0; i < Plants.Count; i++)
                            output[i] = i == plant.Id ? 1.0 : 0.0;
                        DataPoint dataPoint = new DataPoint(input, output);
                        _dataSet.Data.Add(dataPoint);
                        Progress.ImagesProcessed++;
                        NotifyOfPropertyChange(() => Progress);
                        ProgressChanged?.Invoke(this, Progress);
                    }
                }
                SaveData();
                NotifyOfPropertyChange(() => CanTrainNetwork);
            });
            WindowManager window = new WindowManager();
            window.ShowDialog(new GettingTokensProgressBarViewModel(ref _progress, settings, this));
        }

        private dynamic SetSettingsForProgressReportPopup()
        {          
            dynamic settings = new ExpandoObject();
            settings.Width = 400;
            settings.Height = 200;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.Placement = PlacementMode.Center;
            settings.HorizontalOffset = SystemParameters.FullPrimaryScreenWidth / 2 - 200;
            settings.VerticalOffset = SystemParameters.FullPrimaryScreenHeight / 2 - 100;
            return settings;
        }

        public bool CanTrainNetwork
        {
            get
            {
                return MaxIterations > 0 && MaxError > 0
                    && NumberOfInputNodes > 0 && NumberOfHiddenLayers > 0
                    && _dataSet != null && _dataSet.Data.Count != 0 
                    && Plants.Count != 0 && Layers.Count != 0 
                    && Layers.Where(x => x.TransferFunction == TransferFunction.None).ToList().Count == 0;
            }
        }
        public async Task TrainNetworkAsync()
        {
            int[] layerSize = CreateLayersSize();
            TransferFunction[] tfunc = AddTransferFunctionToLayers();
            BackPropagationNetwork bpn = null;
            NetworkTrainer nt;

            if (bpn == null)
                bpn = new BackPropagationNetwork(layerSize, tfunc);
            await FixInputs();

            nt = new NetworkTrainer(bpn, _dataSet);
            Task.Run(() =>
            {
                nt.MaxError = MaxError;
                nt.MaxIterations = MaxIterations;
                nt.NudgeWindow = NudgeWindow;

                nt.TrainDataSet();

                nt.Network.Save(@"tezine.xml");

                double[] error = nt.GetErrorHistory();
                string[] filedata = new string[error.Length];
                for (int i = 0; i < error.Length; i++)
                    filedata[i] = i.ToString() + " " + error[i].ToString();

                File.WriteAllLines(@"greske.txt", filedata);
                NetworkTrained?.Invoke(this, "Završeno treniranje.");
            });

            dynamic settings = SetSettingsForProgressReportPopup();
            TrainingNetworkReport.Status = "Treniranje mreže...";
            TrainingNetworkReport.Progress = 0;
            TrainingNetworkReport.MaxIterations = MaxIterations;
            TrainingNetworkReport.Error = 0;
            WindowManager window = new WindowManager();
            window.ShowDialog(new TrainingNetworkProgresBarViewModel(ref _trainingNetworkReport, settings, this, nt));
        }

        private async Task FixInputs()
        {
            await Task.Run(() =>
            {
                if (_dataSet.Data[0].InputSize != NumberOfInputNodes)
                {
                    foreach (var dataPoint in _dataSet.Data)
                    {
                        if (dataPoint.InputSize > NumberOfInputNodes)
                            dataPoint.Input = dataPoint.Input.Take(NumberOfInputNodes).ToArray();
                        else
                        {
                            List<double> input = dataPoint.Input.Take(dataPoint.InputSize).ToList();
                            var difference = NumberOfInputNodes - dataPoint.InputSize;
                            for (int i = 0; i < difference; i++)
                            {
                                input.Add(0);
                            }
                            dataPoint.Input = input.ToArray();
                        }
                    }
                }
            });
        }

        private TransferFunction[] AddTransferFunctionToLayers()
        {
            TransferFunction[] tfunc = new TransferFunction[NumberOfHiddenLayers + 2];
            tfunc[0] = TransferFunction.None;
            for (int i = 0; i < Layers.Count; i++)
            {
                tfunc[i + 1] = Layers[i].TransferFunction;
            }

            return tfunc;
        }

        private int[] CreateLayersSize()
        {
            int[] layerSize = new int[NumberOfHiddenLayers + 2];
            layerSize[0] = NumberOfInputNodes;
            for (int i = 1; i <= NumberOfHiddenLayers; i++)
            {
                layerSize[i] = NumberOfInputNodes * 2;
            }
            layerSize[layerSize.Length - 1] = Plants.Count;
            return layerSize;
        }

        public AdministrationViewModel()
        {
            Task.Run(()=> {
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
                _dataSet = new DataSet();
                if (File.Exists(@"podaci.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(@"podaci.xml");

                    _dataSet.Load((XmlElement)doc.DocumentElement.ChildNodes[0]);

                    NumberOfInputNodes = _dataSet.Data[0].InputSize;
                    NotifyOfPropertyChange(() => NumberOfInputNodes);
                }
                
            });          
        }

        private void FillComboBoxWithLayers()
        {
            Layers = new BindableCollection<LayerModel>();
            for (int i = 0; i < NumberOfHiddenLayers + 1; i++)
            {
                Layers.Add(new LayerModel(i + 1));
            }
        }

        private void SaveData()
        {
            List<string> plants = new List<string>();
            foreach (var plant in Plants)
            {
                plants.Add(plant.Name);
            }
            File.WriteAllLines(@"vrste.txt", plants);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<Root/>");
            _dataSet.ToXml(doc);
            doc.DocumentElement.AppendChild(_dataSet.ToXml(doc));
            doc.Save(@"podaci.xml");
        }
    }
}
