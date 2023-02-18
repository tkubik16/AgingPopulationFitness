using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgingPopulationFitness.Shared;
using AgingPopulationFitness;

namespace AgingPopulationFitness
{
    public class Feedback
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }

        public Feedback() {
            Title = string.Empty;
            Body = string.Empty;
            Type = string.Empty;
        }
    }
}
