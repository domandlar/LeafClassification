using Caliburn.Micro;
using LeafClassification.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace LeafClassification.ViewModels
{
    public class GettingTokensProgressBarViewModel : Screen, IViewAware
    {

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


        public PopupAnimation PopupAnimation { get; set; }
        public PlacementMode Placement { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }

        public GettingTokensProgressBarViewModel(ref GettingTokensProgressReportModel progress, dynamic settings, AdministrationViewModel administration)
        {
            Progress = progress;
            PopupAnimation = settings.PopupAnimation;
            Placement = settings.Placement;
            HorizontalOffset = settings.HorizontalOffset;
            VerticalOffset = settings.VerticalOffset;
            Width = settings.Width;
            Height = settings.Height;
            administration.ProgressChanged += Administration_ProgressChanged;
        }

        private void Administration_ProgressChanged(object sender, GettingTokensProgressReportModel e)
        {
            Progress.ImagesProcessed = e.ImagesProcessed;
            Progress.PercentageComplete = e.ImagesProcessed / Progress.MaxProgress;
            if (Progress.ImagesProcessed == Progress.MaxProgress)
            {
                Progress.Status = "Obrađeno!";
            }
            NotifyOfPropertyChange(() => Progress);
        }

        public void AttachView(object view, object context = null)
        {
            var viewPopup = view as Popup;
            if(viewPopup != null) 
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
