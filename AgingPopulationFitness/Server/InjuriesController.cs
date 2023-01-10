using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgingPopulationFitness;
using AgingPopulationFitness.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Npgsql.Internal.TypeHandlers;

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

        [HttpPost]
        public async Task<ActionResult<bool>> PostUserInjury( UserInjury userInjury)
        {
            bool success = false;
            success = await Task.Run(() => PostUserInjuryHelper(userInjury));
            return success;
        }

        public bool PostUserInjuryHelper(UserInjury userInjury)
        {
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "INSERT INTO user_injury ( user_uid, user_injury_name, user_injury_description, user_injury_severity, user_injury_date) VALUES" +
                "(@user_uid, @user_injury_name, @user_injury_description, @user_injury_severity, @user_injury_date)";


            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("user_uid", userInjury.UserId);
            cmd.Parameters.AddWithValue("user_injury_name", userInjury.InjuryName);
            cmd.Parameters.AddWithValue("user_injury_description", userInjury.InjuryDescription);
            cmd.Parameters.AddWithValue("user_injury_severity", userInjury.InjurySeverity);
            cmd.Parameters.AddWithValue("user_injury_date", userInjury.InjuryDate);
            cmd.Prepare();

            try {
                cmd.ExecuteNonQuery();
                AddAllInjuryLocations(userInjury);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
            
        }

        public bool AddAllInjuryLocations( UserInjury userInjury)
        {
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT user_injury_id FROM user_injury WHERE user_uid = @user_uid AND user_injury_name = @user_injury_name";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("user_uid", userInjury.UserId);
            cmd.Parameters.AddWithValue("user_injury_name", userInjury.InjuryName);

            try
            {
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    userInjury.InjuryId = rdr.GetInt32(0);
                }

                for( int i = 0; i < userInjury.InjuryLocations.Count; i++)
                {
                    AddOneInjuryLocation( userInjury.InjuryId, userInjury.InjuryLocations[i].InjuryLocationId);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public void AddOneInjuryLocation( long? userInjuryId, long injuryLocationId)
        {
            if( (userInjuryId == null) || (injuryLocationId == null))
            {
                return;
            }
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                        "Username=" + DatabaseCredentials.Username + ";" +
                        "Password=" + DatabaseCredentials.Password + ";" +
                        "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "INSERT INTO user_injury_injury_location ( user_injury_id, injury_location_id) VALUES" +
                "(@user_injury_id, @injury_location_id)";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("user_injury_id", userInjuryId);
            cmd.Parameters.AddWithValue("injury_location_id", injuryLocationId);
            Console.WriteLine("adding injury loc: " + userInjuryId + " " + injuryLocationId);

            cmd.ExecuteNonQuery();
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

        /*
        [HttpPost]
        public async Task<ActionResult<UserProfile>> VerifyUser(UserProfile userProfile)
        {
            UserProfile responseUserProfile = new UserProfile();

            responseUserProfile = await Task.Run(() => VerifyUserCall(userProfile));

            return responseUserProfile;
        }
        */

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
