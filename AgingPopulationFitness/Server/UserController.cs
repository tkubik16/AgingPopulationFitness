using System.Data;
using System.Security.Claims;
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
    [Route("user")]
    [ApiController]
    public class UserController : Controller
    {
        
        
        public DatabaseCredentials databaseCredentials = new DatabaseCredentials();
        public string cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + ";" +
                "Application Name=" + "UserController" + ";" +
                "Pooling=" + DatabaseCredentials.Pooling + ";" +
                "Maximum Pool Size=" + DatabaseCredentials.MaxPoolSize + ";" +
                "Minimum Pool Size=" + DatabaseCredentials.MinPoolSize + "";


        [HttpGet]
        public void GetUser()
        {
            Console.WriteLine("In user controller");
            Console.WriteLine("Username");
            Console.WriteLine("Password");
        }



        [HttpPost]
        public async Task<ActionResult<UserProfile>> VerifyUser(UserProfile userProfile)
        {
            UserProfile responseUserProfile = new UserProfile();

            responseUserProfile = await Task.Run(() => VerifyUserCall(userProfile));

            return responseUserProfile;
        }

        [HttpPost("PostFeedback")]
        public async Task<ActionResult<bool>> PostFeedback(Feedback feedback)
        {
            bool response;

            response = await Task.Run(() => PostFeedbackCall(feedback));

            return response;
        }

        [HttpPost("UserExistsCheck")]
        public async Task<ActionResult<bool>> UserExistsCheck(UserProfile userProfile)
        {
            bool userExists = false;

            userExists = await Task.Run(() => UserExistsCheckCall(userProfile));

            return userExists;
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult<bool>> AddUser(UserProfile userProfile)
        {
            bool createdUser = false;
            createdUser = await Task.Run(() => AddUserCall(userProfile));

            return createdUser;
        }

        [HttpPost("IsLoggedIn")]
        public async Task<ActionResult<bool>> IsLoggedIn(UserProfile userProfile)
        {
            bool isLoggedIn = false;
            isLoggedIn = await Task.Run(() => IsLoggedInCall(userProfile));

            return isLoggedIn;
        }

        public async Task<bool> IsLoggedInCall(UserProfile userProfile)
        {
            if (userProfile.UserId == null)
            {
                return false;
            }
            bool isLoggedIn = false;

            List<UserProfile> userProfileList = new List<UserProfile>();

            

            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT user_uid FROM user_profile WHERE user_uid = @user_id ";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("user_id", userProfile.UserId);
                cmd.Prepare();

                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    UserProfile newUserProfile = new UserProfile();
                    newUserProfile.UserId = rdr.GetGuid(0);
                    userProfileList.Add(newUserProfile);
                }
                if (userProfileList.Count() == 1)
                {
                    isLoggedIn = true;
                }
                connection.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return isLoggedIn;
        }

        public async Task<bool> AddUserCall(UserProfile userProfile)
        {
            if (userProfile.Username == null) { return false; }
            if (userProfile.Password == null) { return false; }
            if ( await UserExistsCheckCall(userProfile)) { return false; }

           
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "INSERT INTO user_profile( user_uid, username, user_password ) VALUES ( uuid_generate_v4(), @username, crypt(@password, gen_salt('bf')) )";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("username", userProfile.Username);
                cmd.Parameters.AddWithValue("password", userProfile.Password);
                cmd.Prepare();

                cmd.ExecuteNonQuery();

                if (await UserExistsCheckCall(userProfile)) { return true; }
                connection.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        public async Task<bool> UserExistsCheckCall(UserProfile userProfile)
        {
            if (userProfile.Username == null)
            {
                return false;
            }
            bool userExists = false;

            List<UserProfile> userProfileList = new List<UserProfile>();

           
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT username FROM user_profile WHERE username = @username ";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("username", userProfile.Username);
                cmd.Prepare();

                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    UserProfile newUserProfile = new UserProfile();
                    newUserProfile.Username = rdr.GetString(0);
                    newUserProfile.PrintUserProfile();          // console print statement
                    userProfileList.Add(newUserProfile);
                }
                if (userProfileList.Count() > 0)
                {
                    userExists = true;
                }
                connection.Close();
            }
            catch(Exception e) {
                Console.WriteLine(e);
            }

            return userExists;
        }

        public async Task<UserProfile> VerifyUserCall(UserProfile userProfile)
        {
            List<UserProfile> userProfileList = new List<UserProfile>();



         
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT * FROM user_profile WHERE username = @username AND user_password = crypt( @user_password, user_password )";


                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("username", userProfile.Username);
                cmd.Parameters.AddWithValue("user_password", userProfile.Password);
                cmd.Prepare();

                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    UserProfile newUserProfile = new UserProfile();
                    newUserProfile.UserId = rdr.GetGuid(0);
                    newUserProfile.Username = rdr.GetString(1);
                    newUserProfile.Password = rdr.GetString(2);
                    newUserProfile.PrintUserProfile();
                    userProfileList.Add(newUserProfile);
                }
                if (userProfileList.Count() == 1)
                {
                    connection.Close();
                    return userProfileList[0];
                }

                connection.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return userProfile;
        }

        public async Task<bool> PostFeedbackCall(Feedback feedback)
        {



            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "INSERT INTO feedback (title, type, body) VALUES " +
                    "(@title, @type, @body)";


                using var cmd = new NpgsqlCommand(sql, connection);

                
                    cmd.Parameters.AddWithValue("title", feedback.Title);
                    cmd.Parameters.AddWithValue("type", feedback.Type);
                    cmd.Parameters.AddWithValue("body", feedback.Body);
                    cmd.Prepare();

                    cmd.ExecuteNonQuery();
                
                    
                

                connection.Close();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

    }
}
