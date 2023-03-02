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
    public class InjuryLocation
    {
        public int InjuryLocationId { get; set; }
        public string BodyPart { get; set; }

        
        public InjuryLocation() {
            InjuryLocationId = -1;
            BodyPart = string.Empty;
        }
        public InjuryLocation(int InjuryId, string BodyPart)
        {
            this.InjuryLocationId = InjuryId;
            this.BodyPart = BodyPart;

        }
        
    }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(InjuryLocation))]
[JsonSerializable(typeof(UserInjury))]
[JsonSerializable(typeof(List<InjuryLocation>))]
[JsonSerializable(typeof(List<UserInjury>))]
public partial class InjuryLocationContext : JsonSerializerContext { }
