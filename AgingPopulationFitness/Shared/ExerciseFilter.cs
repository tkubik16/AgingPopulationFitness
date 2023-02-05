using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgingPopulationFitness.Shared
{
    public class ExerciseFilter
    {
        public Benefit benefit { get; set; }
        
        public bool ExcludeBasedOnInjuries { get; set; }
        public ExerciseType exerciseType { get; set; }

        public ExerciseFilter()
        {
            ExcludeBasedOnInjuries = false;
            benefit = new Benefit();
            benefit.BenefitId = -1;
            benefit.BenefitName = "All";
            benefit.BenefitSpecificity = "All";
            exerciseType = new ExerciseType();
        }
    }
}
