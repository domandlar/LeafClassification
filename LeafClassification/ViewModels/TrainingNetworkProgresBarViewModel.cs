using Caliburn.Micro;
using LeafClassification.Models;
using NeuralNetwork.NetworkTrainer;
using NeuralNetwork.NetworkTrainer.SupportClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace LeafClassification.ViewModels
{
    public class TrainingNetworkProgresBarViewModel : Screen, IViewAware
    {
        private TrainingNetworkProgressReportModel _progress = new TrainingNetworkProgressReportModel();
        public TrainingNetworkProgressReportModel Progress
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


        public PopupAnimation PopupAnimation { get; set; }
        public PlacementMode Placement { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }

        public TrainingNetworkProgresBarViewModel(ref TrainingNetworkProgressReportModel progress, dynamic settings, AdministrationViewModel administration, NetworkTrainer networkTrainer)
        {
            Progress = progress;
            PopupAnimation = settings.PopupAnimation;
            Placement = settings.Placement;
            HorizontalOffset = settings.HorizontalOffset;
            VerticalOffset = settings.VerticalOffset;
            Width = settings.Width;
            Height = settings.Height;
            administration.NetworkTrained += Administration_NetworkTrained;
            networkTrainer.IterationCompleted += NetworkTrainer_IterationCompleted;
            RunProgressBar();
        }

        private void NetworkTrainer_IterationCompleted(object sender, TrainingProgressReport e)
        {
            Progress.Iteration = e.Iteration;
            Progress.Error = e.Error;
            NotifyOfPropertyChange(() => Progress);
        }

        private void Administration_NetworkTrained(object sender, string e)
        {
            Progress.Status = e;
        }

        private void RunProgressBar()
        {
            Task.Run(() =>
            {
                while (Progress.Status != "Završeno treniranje.")
                {
                    for (int i = 0; i <= 10; i++)
                    {
                        Progress.Progress = Progress.Status == "Završeno treniranje." ? 10 : i;
                        NotifyOfPropertyChange(() => Progress);
                        Thread.Sleep(500);
                        if (Progress.Status == "Završeno treniranje.")
                        {
                            Progress.Progress = 10;                           
                            NotifyOfPropertyChange(() => Progress);
                            break;
                        }                      
                    }
                }
            });
        }

        public void AttachView(object view, object context = null)
        {
            var viewPopup = view as Popup;
            if (viewPopup != null)
            {
                popup = viewPopup;
                popup.StaysOpen = false;
                popup.Width = Width;
                popup.Height = Height;
                popup.Placement = Placement;
                popup.PopupAnimation = PopupAnimation;
                popup.HorizontalOffset = HorizontalOffset;
                popup.VerticalOffset = VerticalOffset;
                NotifyOfPropertyChange(() => popup);
            }
            ViewAttached(this, new ViewAttachedEventArgs() { Context = context, View = popup });
        }

        public object GetView(object context = null)
        {
            return popup;
        }
        private Popup popup;
        public event EventHandler<ViewAttachedEventArgs> ViewAttached = delegate { };

    }
}
