using AgingPopulationFitness;
using AgingPopulationFitness.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace AgingPopulationFitness
{
    public class Exercise
    {
        public long ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public string ExerciseDescription { get; set; }
        public string ExerciseLink { get; set; }
        public byte[] ExerciseMainImage { get; set; }
        public string ExerciseType { get; set; }
        public string ExerciseInstructions { get; set; }
        public List<Benefit> Benefits { get; set; }
        public List<InjuryLocation> InjuryLocations { get; set; }

        public Exercise()
        {
            this.ExerciseId = -1;
            this.ExerciseName = "";
            this.ExerciseDescription = "";
            this.ExerciseLink = "";
            this.ExerciseMainImage = new byte[64];
            this.ExerciseType = "";
            this.ExerciseInstructions = "";
            this.Benefits = new List<Benefit>();
            this.InjuryLocations = new List<InjuryLocation>();
        }
    }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Exercise))]
[JsonSerializable(typeof(List<Exercise>))]
[JsonSerializable(typeof(Benefit))]
[JsonSerializable(typeof(List<Benefit>))]
[JsonSerializable(typeof(SuggestedExercise))]
public partial class ExerciseContext : JsonSerializerContext { }
