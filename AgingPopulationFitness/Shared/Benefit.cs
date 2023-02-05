using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgingPopulationFitness.Shared;
using AgingPopulationFitness;

namespace AgingPopulationFitness
{
    public class Benefit
    {
        public int BenefitId { get; set; }
        public string BenefitName { get; set; }
        public string BenefitSpecificity { get; set; }


        public Benefit() {
            BenefitId = -1;
            BenefitName = string.Empty;
            BenefitSpecificity = string.Empty;
        }
        public Benefit(int BenefitId, string BenefitName)
        {
            this.BenefitId = BenefitId;
            this.BenefitName = BenefitName;
            this.BenefitSpecificity = string.Empty;
        }

    }
}