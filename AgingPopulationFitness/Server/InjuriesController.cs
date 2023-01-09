using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgingPopulationFitness;
using AgingPopulationFitness.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace AgingPopulationFitness.Server
{
    [Route("injuries")]
    [ApiController]
    public class InjuriesController : Controller
    {
        public DatabaseCredentials databaseCredentials = new DatabaseCredentials();


        [HttpGet]
        public async Task<List<InjuryLocation>> GetInjuries()
        {
            List<InjuryLocation> responseInjuryLocations = new List<InjuryLocation>();
            
            responseInjuryLocations = await Task.Run(() => GetInjuriesCall());
            for(int i = 0; i < responseInjuryLocations.Count; i++)
            {
                Console.WriteLine(responseInjuryLocations[i].InjuryLocationId + responseInjuryLocations[i].BodyPart);
                
            }

            return responseInjuryLocations;
        }

        [HttpGet("string")]
        public async Task<string> GetInjuriesString()
        {
            List<InjuryLocation> responseInjuryLocations = new List<InjuryLocation>();
            responseInjuryLocations.Add(new InjuryLocation(1,"elbow"));
            responseInjuryLocations.Add(new InjuryLocation(2, "knee"));
            //responseInjuryLocations = await Task.Run(() => GetInjuriesCall());
            //responseInjuryLocations = GetInjuriesCall();
            for (int i = 0; i < responseInjuryLocations.Count; i++)
            {
                Console.WriteLine(responseInjuryLocations[i].InjuryLocationId + responseInjuryLocations[i].BodyPart);

            }
            string jsonString = JsonSerializer.Serialize<List<InjuryLocation>>( responseInjuryLocations);
            Console.WriteLine("json String: " + jsonString);
            return jsonString;
        }

        public List<InjuryLocation> GetInjuriesCall()
        {
            List<InjuryLocation> injuryLocations = new List<InjuryLocation>();




            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT * FROM injury_location";


            using var cmd = new NpgsqlCommand(sql, con);


            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                InjuryLocation injuryLocation = new InjuryLocation();
                injuryLocation.InjuryLocationId = rdr.GetInt32(0);
                injuryLocation.BodyPart = rdr.GetString(1);
                //Console.WriteLine(rdr.GetInt32(0) + rdr.GetString(1));
                injuryLocations.Add(injuryLocation);
            }

            return injuryLocations;
        }

        [HttpPost]
        public async Task<ActionResult<UserProfile>> VerifyUser(UserProfile userProfile)
        {
            UserProfile responseUserProfile = new UserProfile();

            responseUserProfile = await Task.Run(() => VerifyUserCall(userProfile));

            return responseUserProfile;
        }

        public UserProfile VerifyUserCall(UserProfile userProfile)
        {
            List<UserProfile> userProfileList = new List<UserProfile>();



            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT * FROM user_profile WHERE username = @username AND user_password = crypt( @user_password, user_password )";


            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("username", userProfile.Username);
            cmd.Parameters.AddWithValue("user_password", userProfile.Password);
            cmd.Prepare();

            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                UserProfile newUserProfile = new UserProfile();
                newUserProfile.UserId = rdr.GetGuid(0);
                newUserProfile.Username = rdr.GetString(1);
                newUserProfile.PrintUserProfile();
                userProfileList.Add(newUserProfile);
            }
            if (userProfileList.Count() == 1)
            {
                return userProfileList[0];
            }

            return userProfile;
        }

    }
}
