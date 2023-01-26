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


        public Benefit() { }
        public Benefit(int BenefitId, string BenefitName)
        {
            this.BenefitId = BenefitId;
            this.BenefitName = BenefitName;

        }

    }
}