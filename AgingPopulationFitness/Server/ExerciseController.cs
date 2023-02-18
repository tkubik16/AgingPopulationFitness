using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgingPopulationFitness;
using AgingPopulationFitness.Client.Pages;
using AgingPopulationFitness.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace AgingPopulationFitness.Server
{
    [Route("exercise")]
    [ApiController]
    public class ExerciseController : Controller
    {
        public DatabaseCredentials databaseCredentials = new DatabaseCredentials();
        public string cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + ";" +
                "Application Name=" + "ExerciseController" + ";" +
                "Pooling=" + DatabaseCredentials.Pooling + ";" +
                "Maximum Pool Size=" + DatabaseCredentials.MaxPoolSize + ";" +
                "Minimum Pool Size=" + DatabaseCredentials.MinPoolSize + "";

        [HttpPost]
        public async Task<List<Exercise>> GetExercises( ExerciseFilter exerciseFilter)
        {
            List<Exercise> responseExercises = new List<Exercise>();

            
            responseExercises = await Task.Run(() => GetAllExercisesCall( exerciseFilter));
            
            


            return responseExercises;
        }

        public List<Exercise> GetAllExercisesCall(ExerciseFilter exerciseFilter)
        {
            List<Exercise> exercises = new List<Exercise>();



            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            string sql = "SELECT * FROM exercise";
            

            sql = "SELECT * FROM exercise " +
                    "WHERE " +
                        "exercise.exercise_id NOT IN ( " +
                            "SELECT exercise_id FROM exercise_injury_location " +
                            "WHERE injury_location_id = ANY (@injury_list) " +
                            "GROUP BY exercise_id " +
                            ") " +
                        "AND exercise.exercise_id IN ( " +
                            "SELECT exercise_id FROM exercise_benefit " +
                            "WHERE benefit_id = ANY (@benefit_list) " +
                            "GROUP BY exercise_id " +
                        ") " +
                        "AND exercise.exercise_type = ANY (@type_list)";


            using var cmd = new NpgsqlCommand(sql, con);

            List<int> benefitIdList = new List<int>();
            for( int i = 0; i < exerciseFilter.BenefitsList.Count; i++)
            {
                benefitIdList.Add(exerciseFilter.BenefitsList[i].BenefitId);
                Console.WriteLine(exerciseFilter.BenefitsList[i].BenefitName);
            }
            List<string> exerciseTypesList = new List<string>();
            for (int i = 0; i < exerciseFilter.ExerciseTypesList.Count; i++)
            {
                exerciseTypesList.Add(exerciseFilter.ExerciseTypesList[i].Type);
                Console.WriteLine(exerciseFilter.ExerciseTypesList[i].Type);
            }
            List<int> injuryLocationIdList = new List<int>();
            if (exerciseFilter.ExcludeBasedOnInjuries == true)
            {
                for (int i = 0; i < exerciseFilter.InjuryLocations.Count; i++)
                {
                    injuryLocationIdList.Add(exerciseFilter.InjuryLocations[i].InjuryLocationId);
                    Console.WriteLine(exerciseFilter.InjuryLocations[i].BodyPart);
                }
            }

            cmd.Parameters.AddWithValue( "benefit_list", benefitIdList.ToArray() ) ;
            cmd.Parameters.AddWithValue( "type_list", exerciseTypesList.ToArray() ) ;
            cmd.Parameters.AddWithValue("injury_list", injuryLocationIdList.ToArray());
            cmd.Prepare();
            
            


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
            con.Close();
            return exercises;
        }

        [HttpGet("benefits")]
        public async Task<List<Benefit>> GetBenefits()
        {
            List<Benefit> responseBenefits = new List<Benefit>();

            responseBenefits = await Task.Run(() => GetBenefitsCall());


            return responseBenefits;
        }

        [HttpGet("benefits/general")]
        public async Task<List<Benefit>> GetGeneralBenefits()
        {
            List<Benefit> responseBenefits = new List<Benefit>();

            responseBenefits = await Task.Run(() => GetGeneralBenefitsCall());


            return responseBenefits;
        }

        [HttpGet("types")]
        public async Task<List<ExerciseType>> GetTypes()
        {
            List<ExerciseType> responseTypes = new List<ExerciseType>();

            responseTypes = await Task.Run(() => GetTypesCall());


            return responseTypes;
        }

        public List<ExerciseType> GetTypesCall()
        {
            List<ExerciseType> types = new List<ExerciseType>();




            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT * FROM exercise_type " +
                "ORDER BY exercise_type ASC";


            using var cmd = new NpgsqlCommand(sql, con);


            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                ExerciseType type = new ExerciseType();
                type.ExerciseTypeId = rdr.GetInt32(0);
                type.Type = rdr.GetString(1);
                //Console.WriteLine(rdr.GetInt32(0) + rdr.GetString(1));
                types.Add(type);
            }
            con.Close();
            return types;
        }

        public List<Benefit> GetBenefitsCall()
        {
            List<Benefit> benefits = new List<Benefit>();




            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                benefit.BenefitSpecificity = rdr.GetString(2);
                //Console.WriteLine(rdr.GetInt32(0) + rdr.GetString(1));
                benefits.Add(benefit);
            }
            con.Close();
            return benefits;
        }

        public List<Benefit> GetGeneralBenefitsCall()
        {
            List<Benefit> benefits = new List<Benefit>();




            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT * FROM benefit " +
                "WHERE benefit.benefit_specificity = 'General'";


            using var cmd = new NpgsqlCommand(sql, con);


            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Benefit benefit = new Benefit();
                benefit.BenefitId = rdr.GetInt32(0);
                benefit.BenefitName = rdr.GetString(1);
                benefit.BenefitSpecificity = rdr.GetString(2);
                //Console.WriteLine(rdr.GetInt32(0) + rdr.GetString(1));
                benefits.Add(benefit);
            }
            con.Close();
            return benefits;
        }

        [HttpPost("suggested")]
        public async Task<ActionResult<bool>> PostSuggestedExercise(SuggestedExercise suggestedExercise)
        {

            bool success = false;
            bool adminPrivileges = false;

            adminPrivileges = await Task.Run(() => HasAdminPrivileges(suggestedExercise.UserId));

            if (adminPrivileges)
            {
                success = await Task.Run(() => PostSuggestedExerciseHelper(suggestedExercise.Exercise));
            }
            else
            {
                success = await Task.Run(() => PostSuggestedExerciseAsSuggestionHelper(suggestedExercise.Exercise));
            }
            Console.WriteLine("~~~~~~~~~~~");
            Console.WriteLine(suggestedExercise.UserId);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseName);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseDescription);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseInstructions);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseType);
            Console.WriteLine(suggestedExercise.Exercise.ExerciseLink);
            return success;
        }

        public bool HasAdminPrivileges(Guid? uid)
        {
            int count = 0;

            if( uid == null)
            {
                return false;
            }

            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "SELECT COUNT(DISTINCT user_uid) " +
                "FROM admin_privileges " +
                "WHERE user_uid = @uid";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("uid", uid);

            try
            {
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    count = rdr.GetInt32(0);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if( count == 1)
            {
                con.Close();
                return true;
            }
            con.Close();
            return false;
        }

        public bool PostSuggestedExerciseHelper(Exercise exercise)
        {
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                con.Close();
                AddAllInjuryLocations(exercise);
                AddAllBenefits(exercise);
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            con.Close();
            return false;

        }

        public bool PostSuggestedExerciseAsSuggestionHelper(Exercise exercise)
        {
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
            using var con = new NpgsqlConnection(cs);
            

            try
            {
                con.Open();

                var sql = "INSERT INTO suggested_exercise ( suggested_exercise_name, suggested_exercise_description, suggested_exercise_link, suggested_exercise_main_image, suggested_exercise_type, suggested_exercise_instructions) VALUES" +
                    "(@exercise_name, @exercise_description, @exercise_link, @exercise_main_image, @exercise_type, @exercise_instructions)";


                using var cmd = new NpgsqlCommand(sql, con);

                cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
                cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
                cmd.Parameters.AddWithValue("exercise_link", exercise.ExerciseLink);
                cmd.Parameters.AddWithValue("exercise_main_image", exercise.ExerciseMainImage);
                cmd.Parameters.AddWithValue("exercise_type", exercise.ExerciseType);
                cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);
                cmd.Prepare();

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

        public bool AddAllInjuryLocations(Exercise exercise)
        {
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                con.Close();
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
            con.Close();
            return false;
        }

        public void AddOneInjuryLocation(long? exerciseId, long injuryLocationId)
        {
            if ((exerciseId == null) || (injuryLocationId == null))
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

            var sql = "INSERT INTO exercise_injury_location ( exercise_id, injury_location_id) VALUES" +
                "(@exercise_id, @injury_location_id)";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_id", exerciseId);
            cmd.Parameters.AddWithValue("injury_location_id", injuryLocationId);
            

            cmd.ExecuteNonQuery();

            con.Close();
        }

        public bool AddAllBenefits(Exercise exercise)
        {
            /*
            var cs = "host=" + DatabaseCredentials.Host + ";" +
                "Username=" + DatabaseCredentials.Username + ";" +
                "Password=" + DatabaseCredentials.Password + ";" +
                "Database=" + DatabaseCredentials.Database + "";
            */
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
                con.Close();
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
            con.Close();
            return false;
        }

        public void AddOneBenefit(long? exerciseId, long benefitId)
        {
            if ((exerciseId == null) || (benefitId == null))
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

            var sql = "INSERT INTO exercise_benefit ( exercise_id, benefit_id) VALUES" +
                "(@exercise_id, @benefit_id)";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("exercise_id", exerciseId);
            cmd.Parameters.AddWithValue("benefit_id", benefitId);
            

            cmd.ExecuteNonQuery();
            con.Close();
        }



    }

    

}
