using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AgingPopulationFitness;
using AgingPopulationFitness.Client.Shared;
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
        public DatabaseCredentials databaseCredentials;
        public string connectionString;
        public NpgsqlDataSource dataSource;
        public string cs =  "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + ";" +
                "Application Name=" + "InjuriesController" + ";" +
                "Pooling=" + DatabaseCredentials.Pooling + ";" +
                "Maximum Pool Size=" + DatabaseCredentials.MaxPoolSize + ";" +
                "Minimum Pool Size=" + DatabaseCredentials.MinPoolSize + "";

        public InjuriesController() {
            DatabaseCredentials databaseCredentials = new DatabaseCredentials();
            connectionString = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + ";" +
                "Application Name=" + "InjuriesController" + ";" +
                "Pooling=" + DatabaseCredentials.Pooling + ";" +
                "Maximum Pool Size=" + DatabaseCredentials.MaxPoolSize + ";" +
                "Minimum Pool Size=" + DatabaseCredentials.MinPoolSize + "";
            dataSource = NpgsqlDataSource.Create(connectionString);
        }

        [HttpGet]
        public async Task<List<InjuryLocation>> GetInjuries()
        {
            List<InjuryLocation> responseInjuryLocations = new List<InjuryLocation>();
            
            responseInjuryLocations = await Task.Run(() => GetInjuriesCall());
            for(int i = 0; i < responseInjuryLocations.Count; i++)
            {
                //Console.WriteLine(responseInjuryLocations[i].InjuryLocationId + responseInjuryLocations[i].BodyPart);
                
            }

            return responseInjuryLocations;
        }

        [HttpGet("{userUid}")]
        public async Task<List<UserInjury>> GetUsersInjuries(Guid userUid)
        {
            List<UserInjury> responseUserInjuries = new List<UserInjury>();
            //Console.WriteLine("In GetUsersInjuries in InjuriesController");
            responseUserInjuries = await Task.Run(() => GetUsersInjuriesCall( userUid));
            //await Task.Run(() => GetUsersInjuriesCall(userUid));


            if (responseUserInjuries != null)
            {
                for (int i = 0; i < responseUserInjuries.Count; i++)
                {
                    //responseUserInjuries[i].PrintUserInjury();

                }
            }
            return responseUserInjuries;
            
        }

        [HttpGet("locations/{userUid}")]
        public async Task<List<InjuryLocation>> GetUsersInjuryLocations(Guid userUid)
        {
            List<InjuryLocation> responseUserInjuryLocations = new List<InjuryLocation>();

            responseUserInjuryLocations = await Task.Run(() => GetUsersInjuryLocationsCall(userUid));


            return responseUserInjuryLocations;

        }

        public List<InjuryLocation> GetUsersInjuryLocationsCall(Guid userUid)
        {
            List<InjuryLocation> userInjuryLocations = new List<InjuryLocation>();
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT injury_location.injury_location_id, injury_location.body_part FROM user_injury " +
                        "FULL JOIN user_injury_injury_location " +
                        "ON user_injury.user_injury_id = user_injury_injury_location.user_injury_id " +
                        "FULL JOIN injury_location " +
                        "ON user_injury_injury_location.injury_location_id = injury_location.injury_location_id " +
                        "WHERE user_uid = @UserUid " +
                        "GROUP BY injury_location.body_part, injury_location.injury_location_id " +
                        "ORDER BY injury_location.body_part";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("UserUid", userUid);



            try
            {
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    InjuryLocation injuryLocation = new InjuryLocation();
                    injuryLocation.InjuryLocationId = rdr.GetInt32(0);
                    injuryLocation.BodyPart = rdr.GetString(1);
                    userInjuryLocations.Add(injuryLocation);
                }



            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return userInjuryLocations;

        }

        public List<UserInjury> GetUsersInjuriesCall( Guid userUid)
        {
            List<UserInjury> userInjuries = new List<UserInjury>();

            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();
            
            var sql = "SELECT user_injury.user_injury_id, user_uid, user_injury_name, user_injury_description, user_injury_severity, user_injury_date, injury_location.injury_location_id, body_part " +
                "FROM user_injury " +
                "FULL JOIN user_injury_injury_location ON user_injury.user_injury_id = user_injury_injury_location.user_injury_id " +
                "FULL JOIN injury_location ON user_injury_injury_location.injury_location_id = injury_location.injury_location_id " +
                "WHERE user_injury.user_uid = @UserUid " +
                "ORDER BY  user_injury_date, user_injury.user_injury_id, injury_location.injury_location_id";

            using var cmd = new NpgsqlCommand(sql, con);

            
            //using var cmd = dataSource.CreateCommand(sql);

            cmd.Parameters.AddWithValue("UserUid", userUid);



            try
            {
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                
                int currentUserInjury = 0;
                while (rdr.Read())
                {
                    //Console.WriteLine(rdr.GetInt32(0) + " " + rdr.GetGuid(1) + " " + rdr.GetString(2) + " " + rdr.GetString(3) + " " + rdr.GetInt32(4) + " " + rdr.GetDateTime(5) + " " + rdr.GetInt32(6) + " " + rdr.GetString(7));
                    if (userInjuries.Count == 0)
                    {
                        userInjuries.Add(new UserInjury());
                        userInjuries[currentUserInjury].InjuryId = rdr.GetInt32(0);
                        userInjuries[currentUserInjury].UserId = rdr.GetGuid(1);
                        userInjuries[currentUserInjury].InjuryName = rdr.GetString(2);
                        userInjuries[currentUserInjury].InjuryDescription = rdr.GetString(3);
                        userInjuries[currentUserInjury].InjurySeverity = rdr.GetInt32(4);
                        userInjuries[currentUserInjury].InjuryDate = DateOnly.FromDateTime(rdr.GetDateTime(5));
                        if ( !rdr.IsDBNull(6) && !rdr.IsDBNull(7))
                        {
                            userInjuries[currentUserInjury].InjuryLocations.Add(new InjuryLocation(rdr.GetInt32(6), rdr.GetString(7)));
                        }

                    }
                    else if ( rdr.GetInt32(0) != userInjuries[currentUserInjury].InjuryId)
                    {
                        currentUserInjury++;

                        userInjuries.Add(new UserInjury());
                        userInjuries[currentUserInjury].InjuryId = rdr.GetInt32(0);
                        userInjuries[currentUserInjury].UserId = rdr.GetGuid(1);
                        userInjuries[currentUserInjury].InjuryName = rdr.GetString(2);
                        userInjuries[currentUserInjury].InjuryDescription = rdr.GetString(3);
                        userInjuries[currentUserInjury].InjurySeverity = rdr.GetInt32(4);
                        userInjuries[currentUserInjury].InjuryDate = DateOnly.FromDateTime(rdr.GetDateTime(5));

                        if (!rdr.IsDBNull(6) && !rdr.IsDBNull(7))
                        {
                            userInjuries[currentUserInjury].InjuryLocations.Add(new InjuryLocation(rdr.GetInt32(6), rdr.GetString(7)));
                        }

                    }
                    else
                    {
                        if (!rdr.IsDBNull(6) && !rdr.IsDBNull(7))
                        {
                            userInjuries[currentUserInjury].InjuryLocations.Add(new InjuryLocation(rdr.GetInt32(6), rdr.GetString(7)));
                        }
                    }
                }

                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return userInjuries;

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
                //Console.WriteLine(responseInjuryLocations[i].InjuryLocationId + responseInjuryLocations[i].BodyPart);

            }
            string jsonString = JsonSerializer.Serialize<List<InjuryLocation>>( responseInjuryLocations);
            //Console.WriteLine("json String: " + jsonString);
            return jsonString;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> PostUserInjury( UserInjury userInjury)
        {
            bool success = false;
            success = await Task.Run(() => PostUserInjuryHelper(userInjury));
            return success;
        }

        [HttpPost("update")]
        public async Task<ActionResult<bool>> UpdateUserInjury(UserInjury userInjury)
        {
            bool updateInjurySuccess = false;
            bool updateInjuryLocationsSuccess = false;
            bool deleteInjuryLocationsSuccess = false;

            updateInjurySuccess = await Task.Run(() => UpdateUserInjuryHelper(userInjury));
            if (updateInjurySuccess)
            {
                deleteInjuryLocationsSuccess = await Task.Run(() => DeleteUserInjuryInjuryLocationsHelper(userInjury));
            }
            if (deleteInjuryLocationsSuccess)
            {
                updateInjuryLocationsSuccess = await Task.Run(() => AddAllInjuryLocations(userInjury));
            }
            return updateInjurySuccess;
        }


        [HttpPost("delete")]
        public async Task<ActionResult<bool>> DeleteUserInjury(UserInjury userInjury)
        {
            bool success = true;

            userInjury.PrintUserInjury();
            success = await Task.Run(() => DeleteUserInjuryHelper(userInjury));
            return success;
        }

        public bool DeleteUserInjuryInjuryLocationsHelper(UserInjury userInjury)
        {
            if (userInjury == null)
            {
                return false;
            }
            if (userInjury.InjuryId == null)
            {
                return false;
            }
            if (userInjury.UserId == null)
            {
                return false;
            }

            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "DELETE FROM user_injury_injury_location " +
                "WHERE user_injury_id = @user_injury_id ";


            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("user_injury_id", userInjury.InjuryId);
            cmd.Prepare();

            try
            {
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return false;

        }

        public bool UpdateUserInjuryHelper(UserInjury userInjury)
        {
            if (userInjury == null)
            {
                return false;
            }
            if (userInjury.InjuryId == null)
            {
                return false;
            }
            if (userInjury.UserId == null)
            {
                return false;
            }

            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "UPDATE user_injury " +
                "SET user_injury_name = @user_injury_name, " +
                "user_injury_description = @user_injury_description, " +
                "user_injury_severity = @user_injury_severity, " +
                "user_injury_date = @user_injury_date " +
                "WHERE user_uid = @user_uid AND user_injury_id = @injury_id";


            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("injury_id", userInjury.InjuryId);
            cmd.Parameters.AddWithValue("user_uid", userInjury.UserId);
            cmd.Parameters.AddWithValue("user_injury_name", userInjury.InjuryName);
            cmd.Parameters.AddWithValue("user_injury_description", userInjury.InjuryDescription);
            cmd.Parameters.AddWithValue("user_injury_severity", userInjury.InjurySeverity);
            cmd.Parameters.AddWithValue("user_injury_date", userInjury.InjuryDate);
            cmd.Prepare();

            try
            {
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return false;

        }

        public bool DeleteUserInjuryHelper(UserInjury userInjury)
        {
            if (userInjury == null)
            {
                return false;
            }
            if (userInjury.InjuryId == null)
            {
                return false;
            }
            if (userInjury.UserId == null)
            {
                return false;
            }

            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "DELETE FROM user_injury " +
                "WHERE user_uid = @user_uid AND user_injury_id = @injury_id";


            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("injury_id", userInjury.InjuryId);
            cmd.Parameters.AddWithValue("user_uid", userInjury.UserId);
            cmd.Prepare();

            try
            {
                cmd.ExecuteNonQuery();
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return false;

        }

        public bool PostUserInjuryHelper(UserInjury userInjury)
        {
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                con.Close();
                AddAllInjuryLocations(userInjury);
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return false;
            
        }

        public bool AddAllInjuryLocations( UserInjury userInjury)
        {
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                con.Close();
                for ( int i = 0; i < userInjury.InjuryLocations.Count; i++)
                {
                    AddOneInjuryLocation( userInjury.InjuryId, userInjury.InjuryLocations[i].InjuryLocationId);
                }
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return false;
        }

        public void AddOneInjuryLocation( long? userInjuryId, long injuryLocationId)
        {
            if( (userInjuryId == null) || (injuryLocationId == null))
            {
                return;
            }
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "INSERT INTO user_injury_injury_location ( user_injury_id, injury_location_id) VALUES" +
                "(@user_injury_id, @injury_location_id)";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("user_injury_id", userInjuryId);
            cmd.Parameters.AddWithValue("injury_location_id", injuryLocationId);
            //Console.WriteLine("adding injury loc: " + userInjuryId + " " + injuryLocationId);

            cmd.ExecuteNonQuery();
            con.Close();
        }

        public List<InjuryLocation> GetInjuriesCall()
        {
            List<InjuryLocation> injuryLocations = new List<InjuryLocation>();




            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
            con.Close();
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



            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                con.Close();
                return userProfileList[0];
            }
            con.Close();
            return userProfile;
        }

    }
}
