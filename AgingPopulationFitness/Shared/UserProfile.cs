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
    public class UserProfile
    {
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public void PrintUserProfile()
        {
            Console.WriteLine("UID: " + UserId);
            Console.WriteLine("Username: " + Username);
            Console.WriteLine("Password: " + Password);
        }

        public bool PasswordLengthCheck()
        {
            if (Password == null)
            {
                return false;
            }
            if ((Password.Length > 4) && (Password.Length < 50))
            {
                return true;
            }
            return false;
        }

    }


}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UserProfile))]
[JsonSerializable(typeof(UserInjury))]
public partial class UserProfileContext : JsonSerializerContext { }