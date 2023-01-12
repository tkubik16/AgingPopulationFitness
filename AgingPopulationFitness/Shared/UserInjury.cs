using AgingPopulationFitness;
using AgingPopulationFitness.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgingPopulationFitness
{
    public class UserInjury
    {
        
        public UserInjury()
        {
            InjuryName = "";
            InjuryDescription = "";
            InjurySeverity = 1;
            InjuryDate = DateOnly.FromDateTime(DateTime.Now);
            InjuryLocations= new List<InjuryLocation>();

        }
        public long? InjuryId { get; set; }
        public Guid? UserId { get; set; }
        public string InjuryName { get; set; }
        public string InjuryDescription { get; set; }
        public int InjurySeverity { get; set; }
        public DateOnly InjuryDate { get; set; }

        public List<InjuryLocation> InjuryLocations { get; set; }

        public void PrintUserInjury()
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine(InjuryId);
            Console.WriteLine(UserId);
            Console.WriteLine(InjuryName);
            Console.WriteLine(InjuryDescription);
            Console.WriteLine(InjurySeverity);
            Console.WriteLine(InjuryDate);

            for( int i = 0; i < InjuryLocations.Count; i++)
            {
                Console.WriteLine(InjuryLocations[i].InjuryLocationId + " " + InjuryLocations[i].BodyPart);
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }
    }
}


