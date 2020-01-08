using Caliburn.Micro;
using LeafClassification.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace LeafClassification
{
    public class Bootstraper : BootstrapperBase
    {
        public Bootstraper()
        {
            Initialize();
        }
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
