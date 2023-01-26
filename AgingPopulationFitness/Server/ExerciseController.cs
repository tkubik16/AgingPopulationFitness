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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace AgingPopulationFitness.Server
{
    [Route("exercise")]
    [ApiController]
    public class ExerciseController : Controller
    {
        public DatabaseCredentials databaseCredentials = new DatabaseCredentials();

        [HttpGet]
        public async Task<List<Exercise>> GetExercises()
        {
            List<Exercise> responseExercises = new List<Exercise>();

            responseExercises = await Task.Run(() => GetAllExercisesCall());
            

            return responseExercises;
        }

        public List<Exercise> GetAllExercisesCall()
        {
            List<Exercise> exercises = new List<Exercise>();




            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT * FROM exercise";


            using var cmd = new NpgsqlCommand(sql, con);


            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Exercise anExercise = new Exercise();
                anExercise.ExerciseId = rdr.GetInt32(0);
                anExercise.ExerciseName = rdr.GetString(1);
                anExercise.ExerciseDescription = rdr.GetString(2);
                anExercise.ExerciseLink = rdr.GetString(3);
                anExercise.ExerciseMainImage = (byte[])rdr[4]; 
                anExercise.ExerciseType = rdr.GetString(5);
                anExercise.ExerciseInstructions = rdr.GetString(6);

                exercises.Add(anExercise);
            }

            return exercises;
        }

        [HttpGet("benefits")]
        public async Task<List<Benefit>> GetBenefits()
        {
            List<Benefit> responseBenefits = new List<Benefit>();

            responseBenefits = await Task.Run(() => GetBenefitsCall());


            return responseBenefits;
        }

        public List<Benefit> GetBenefitsCall()
        {
            List<Benefit> benefits = new List<Benefit>();




            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT * FROM benefit";


            using var cmd = new NpgsqlCommand(sql, con);


            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Benefit benefit = new Benefit();
                benefit.BenefitId = rdr.GetInt32(0);
                benefit.BenefitName = rdr.GetString(1);
                //Console.WriteLine(rdr.GetInt32(0) + rdr.GetString(1));
                benefits.Add(benefit);
            }

            return benefits;
        }

        [HttpPost("suggested")]
        public async Task<ActionResult<bool>> PostSuggestedExercise(SuggestedExercise suggestedExercise)
        {
            bool success = false;
            success = await Task.Run(() => PostSuggestedExerciseHelper(suggestedExercise.Exercise));
            Console.WriteLine("~~~~~~~~~~~");
            Console.WriteLine(suggestedExercise.UserId);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseName);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseDescription);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseInstructions);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseType);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseLink);
            return success;
        }

        public bool PostSuggestedExerciseHelper(Exercise exercise)
        {
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "INSERT INTO exercise ( exercise_name, exercise_description, exercise_link, exercise_main_image, exercise_type, exercise_instructions) VALUES" +
                "(@exercise_name, @exercise_description, @exercise_link, @exercise_main_image, @exercise_type, @exercise_instructions)";


            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
            cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
            cmd.Parameters.AddWithValue("exercise_link", exercise.ExerciseLink);
            cmd.Parameters.AddWithValue("exercise_main_image", exercise.ExerciseMainImage);
            cmd.Parameters.AddWithValue("exercise_type", exercise.ExerciseType);
            cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);
            cmd.Prepare();

            try
            {
                cmd.ExecuteNonQuery();
                AddAllInjuryLocations(exercise);
                AddAllBenefits(exercise);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;

        }

        public bool AddAllInjuryLocations(Exercise exercise)
        {
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT exercise_id FROM exercise WHERE exercise_name = @exercise_name AND exercise_description = @exercise_description AND exercise_instructions = @exercise_instructions";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
            cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
            cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);

            try
            {
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    exercise.ExerciseId = rdr.GetInt32(0);
                }

                for (int i = 0; i < exercise.InjuryLocations.Count; i++)
                {
                    AddOneInjuryLocation(exercise.ExerciseId, exercise.InjuryLocations[i].InjuryLocationId);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public void AddOneInjuryLocation(long? exerciseId, long injuryLocationId)
        {
            if ((exerciseId == null) || (injuryLocationId == null))
            {
                return;
            }
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                        "Username=" + DatabaseCredentials.Username + ";" +
                        "Password=" + DatabaseCredentials.Password + ";" +
                        "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "INSERT INTO exercise_injury_location ( exercise_id, injury_location_id) VALUES" +
                "(@exercise_id, @injury_location_id)";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_id", exerciseId);
            cmd.Parameters.AddWithValue("injury_location_id", injuryLocationId);
            

            cmd.ExecuteNonQuery();
        }

        public bool AddAllBenefits(Exercise exercise)
        {
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT exercise_id FROM exercise WHERE exercise_name = @exercise_name AND exercise_description = @exercise_description AND exercise_instructions = @exercise_instructions";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
            cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
            cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);

            try
            {
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    exercise.ExerciseId = rdr.GetInt32(0);
                }

                for (int i = 0; i < exercise.Benefits.Count; i++)
                {
                    AddOneBenefit(exercise.ExerciseId, exercise.Benefits[i].BenefitId);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public void AddOneBenefit(long? exerciseId, long benefitId)
        {
            if ((exerciseId == null) || (benefitId == null))
            {
                return;
            }
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                        "Username=" + DatabaseCredentials.Username + ";" +
                        "Password=" + DatabaseCredentials.Password + ";" +
                        "Database=" + DatabaseCredentials.Database + "";

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "INSERT INTO exercise_benefit ( exercise_id, benefit_id) VALUES" +
                "(@exercise_id, @benefit_id)";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_id", exerciseId);
            cmd.Parameters.AddWithValue("benefit_id", benefitId);
            

            cmd.ExecuteNonQuery();
        }



    }

    

}
