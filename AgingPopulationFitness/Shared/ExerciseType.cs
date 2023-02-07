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
    public class ExerciseType
    {
        public int ExerciseTypeId { get; set; }
        public string Type { get; set;}

        public ExerciseType()
        {
            ExerciseTypeId = -1;
            Type = "All";
        }
        public ExerciseType(int exerciseTypeId, string type)
        {
            ExerciseTypeId = exerciseTypeId;
            Type = type;
        }
    }
}
