using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgingPopulationFitness.Shared
{
    public class ExerciseFilter
    {
        public Guid UserId { get; set; }
        
        public bool ExcludeBasedOnInjuries { get; set; }
        public List<InjuryLocation> InjuryLocations { get; set; }
        public List<Benefit> BenefitsList { get; set; }
        public List<ExerciseType> ExerciseTypesList { get; set; }
        public int PageNumber { get; set; }
        public int ExercisesPerPage { get; set; }

        public ExerciseFilter()
        {
            UserId = Guid.NewGuid();
            ExcludeBasedOnInjuries = false;
            InjuryLocations = new List<InjuryLocation>();
            BenefitsList = new List<Benefit>();
            ExerciseTypesList = new List<ExerciseType>();
            PageNumber = 1;
            ExercisesPerPage = 10;
        }
    }
}
