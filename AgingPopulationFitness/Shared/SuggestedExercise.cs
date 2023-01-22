using AgingPopulationFitness;
using AgingPopulationFitness.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgingPopulationFitness
{
    public class SuggestedExercise
    {

        public Guid? UserId { get; set; }
        public Exercise Exercise { get; set; }

        public SuggestedExercise() {
            Exercise = new Exercise();
        }

    }
}
