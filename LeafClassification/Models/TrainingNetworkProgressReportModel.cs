using System;
using System.Collections.Generic;
using System.Text;

namespace LeafClassification.Models
{
    public class TrainingNetworkProgressReportModel
    {
        public int Progress { get; set; }
        public string Status { get; set; }
        public int Iteration { get; set; }
        public int MaxIterations { get; set; }
        public double Error { get; set; }
        public string Iterations 
        {
            get
            {
                return $"{Iteration}/{MaxIterations}";
            } 
        }
    }
}
