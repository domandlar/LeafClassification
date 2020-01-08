using System;
using System.Collections.Generic;
using System.Text;

namespace LeafClassification.Models
{
    public class GettingTokensProgressReportModel
    {
        public float PercentageComplete { get; set; }
        public float ImagesProcessed { get; set; }
        public float MaxProgress { get; set; }
        public string Status { get; set; }
        public List<PlantModel> PlantsProcessed { get; set; } = new List<PlantModel>();
    }
}
