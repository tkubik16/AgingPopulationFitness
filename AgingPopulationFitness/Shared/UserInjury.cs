using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgingPopulationFitness.Shared
{
    public class UserInjury
    {
        public UserInjury() 
        { 
            InjuryName= "";
            InjuryDescription = "";
            InjurySeverity = 0;
            InjuryDate = DateOnly.FromDateTime(DateTime.Now);

        }
        public long? InjuryId { get; set; }
        public Guid? UserId { get; set; }
        public string InjuryName { get; set; }
        public string InjuryDescription { get; set; }
        public int InjurySeverity { get; set; }
        public DateOnly InjuryDate { get; set; }


    }
}
