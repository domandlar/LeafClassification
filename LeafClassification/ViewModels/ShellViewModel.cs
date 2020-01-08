using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeafClassification.ViewModels
{
    class ShellViewModel : Conductor<object>
    {
        public ShellViewModel()
        {
            ActivateItem(new LeafRecognizerViewModel());
            NotifyOfPropertyChange(() => CanLoadAdministration);
            NotifyOfPropertyChange(() => CanLoadLeafRecognizer);
        }

        public bool CanLoadAdministration => ActiveItem.GetType() != typeof(AdministrationViewModel);      
        public void LoadAdministration()
        {
            ActivateItem(new AdministrationViewModel());
            NotifyOfPropertyChange(() => CanLoadAdministration);
            NotifyOfPropertyChange(() => CanLoadLeafRecognizer);
        }

        public bool CanLoadLeafRecognizer => ActiveItem.GetType() != typeof(LeafRecognizerViewModel);     
        public void LoadLeafRecognizer()
        {
            ActivateItem(new LeafRecognizerViewModel());
            NotifyOfPropertyChange(() => CanLoadAdministration);
            NotifyOfPropertyChange(() => CanLoadLeafRecognizer);
        }
    }
}
