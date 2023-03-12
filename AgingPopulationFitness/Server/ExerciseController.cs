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
using AgingPopulationFitness.Client;
using System.Linq.Expressions;

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

        [HttpGet("{exerciseId}")]
        public async Task<Exercise> GetExercise(long exerciseId)
        {
            Exercise responseExercise = new Exercise();


            responseExercise = await Task.Run(() => GetExerciseCall(exerciseId));
            /*

            Console.WriteLine(responseExercise.ExerciseId);
            Console.WriteLine(responseExercise.ExerciseName);
            Console.WriteLine(responseExercise.ExerciseDescription);
            Console.WriteLine(responseExercise.ExerciseLink);
            Console.WriteLine(responseExercise.ExerciseType);
            Console.WriteLine(responseExercise.ExerciseInstructions);
            
            Console.WriteLine(string.Format("{0} Benefits", responseExercise.ExerciseName));
            for (int j = 0; j < responseExercise.Benefits.Count; j++)
            {
                Console.WriteLine(string.Format("ID: {0} NAME: {1}", responseExercise.Benefits[j].BenefitId, responseExercise.Benefits[j].BenefitName));
            }
            Console.WriteLine(string.Format("{0} Injury Locations", responseExercise.ExerciseName));
            for (int j = 0; j < responseExercise.InjuryLocations.Count; j++)
            {
                Console.WriteLine(string.Format("ID: {0} NAME: {1}", responseExercise.InjuryLocations[j].InjuryLocationId, responseExercise.InjuryLocations[j].BodyPart));
            }
            */
            return responseExercise;
        }

        [HttpPost("count")]
        public async Task<int> GetExercisesCount(ExerciseFilter exerciseFilter)
        {
            int numExercises = 0;


            numExercises = await Task.Run(() => GetExercisesCountCall(exerciseFilter));




            return numExercises;
        }

        public async Task<Exercise> GetExerciseCall(long exerciseId)
        {
            Exercise exercise = new Exercise();

            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();


                string sql = "SELECT * FROM exercise " +
                        "FULL JOIN exercise_benefit ON exercise.exercise_id = exercise_benefit.exercise_id " +
                        "FULL JOIN benefit ON exercise_benefit.benefit_id = benefit.benefit_id " +
                        "FULL JOIN exercise_injury_location ON exercise.exercise_id = exercise_injury_location.exercise_id " +
                        "FULL JOIN injury_location ON exercise_injury_location.injury_location_id = injury_location.injury_location_id " +
                        "WHERE exercise.exercise_id = @exercise_id ";


                using var cmd = new NpgsqlCommand(sql, connection);


                cmd.Parameters.AddWithValue("exercise_id", exerciseId);
                cmd.Prepare();
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    exercise.ExerciseId = rdr.GetInt32(0);
                    exercise.ExerciseName = rdr.GetString(1);
                    exercise.ExerciseDescription = rdr.GetString(2);
                    exercise.ExerciseLink = rdr.GetString(3);
                    //exercise.ExerciseMainImage = (byte[])rdr[4]; 
                    exercise.ExerciseType = rdr.GetString(5);
                    exercise.ExerciseInstructions = rdr.GetString(6);

                    Benefit currentBenefit = new Benefit(rdr.GetInt32(9), rdr.GetString(10), rdr.GetString(11));
                    InjuryLocation currentInjuryLocation = new InjuryLocation(rdr.GetInt32(14), rdr.GetString(15));

                    if (exercise.Benefits.Find(pt => pt.BenefitId == currentBenefit.BenefitId) == null)
                    {
                        exercise.Benefits.Add(new Benefit(currentBenefit.BenefitId, currentBenefit.BenefitName));
                    }
                    if (exercise.InjuryLocations.Find(pt => pt.InjuryLocationId == currentInjuryLocation.InjuryLocationId) == null)
                    {
                        exercise.InjuryLocations.Add(new InjuryLocation(currentInjuryLocation.InjuryLocationId, currentInjuryLocation.BodyPart));
                    }
                }
                connection.Close();
            }
            catch( Exception e){
                Console.WriteLine(e);
            }

            return exercise;
        }

        public async Task<List<Exercise>> GetAllExercisesCall(ExerciseFilter exerciseFilter)
        {
            List<Exercise> exercises = new List<Exercise>();



            
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                


                string sql = "SELECT * FROM exercise " +
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
                            "AND exercise.exercise_type = ANY (@type_list) " +
                            "ORDER BY exercise.exercise_name " +
                            "LIMIT @exercises_per_page OFFSET @page_number ";


                using var cmd = new NpgsqlCommand(sql, connection);

                List<int> benefitIdList = new List<int>();
                for (int i = 0; i < exerciseFilter.BenefitsList.Count; i++)
                {
                    benefitIdList.Add(exerciseFilter.BenefitsList[i].BenefitId);
                    //Console.WriteLine(exerciseFilter.BenefitsList[i].BenefitName);
                }
                List<string> exerciseTypesList = new List<string>();
                for (int i = 0; i < exerciseFilter.ExerciseTypesList.Count; i++)
                {
                    exerciseTypesList.Add(exerciseFilter.ExerciseTypesList[i].Type);
                    //Console.WriteLine(exerciseFilter.ExerciseTypesList[i].Type);
                }
                List<int> injuryLocationIdList = new List<int>();
                Console.WriteLine("ExcludeBasedOnInjuries:");
                Console.WriteLine(exerciseFilter.ExcludeBasedOnInjuries);
                if (exerciseFilter.ExcludeBasedOnInjuries == true)
                {
                    for (int i = 0; i < exerciseFilter.InjuryLocations.Count; i++)
                    {
                        injuryLocationIdList.Add(exerciseFilter.InjuryLocations[i].InjuryLocationId);
                        Console.WriteLine(exerciseFilter.InjuryLocations[i].BodyPart);
                    }
                }

                cmd.Parameters.AddWithValue("benefit_list", benefitIdList.ToArray());
                cmd.Parameters.AddWithValue("type_list", exerciseTypesList.ToArray());
                cmd.Parameters.AddWithValue("injury_list", injuryLocationIdList.ToArray());
                cmd.Parameters.AddWithValue("exercises_per_page", exerciseFilter.ExercisesPerPage);
                if (exerciseFilter.PageNumber == 0)
                {
                    cmd.Parameters.AddWithValue("page_number", 0);
                }
                else
                {
                    cmd.Parameters.AddWithValue("page_number", (exerciseFilter.PageNumber - 1) * exerciseFilter.ExercisesPerPage);
                }
                cmd.Prepare();




                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Exercise anExercise = new Exercise();
                    anExercise.ExerciseId = rdr.GetInt32(0);
                    anExercise.ExerciseName = rdr.GetString(1);
                    anExercise.ExerciseDescription = rdr.GetString(2);
                    anExercise.ExerciseLink = rdr.GetString(3);
                    //anExercise.ExerciseMainImage = (byte[])rdr[4]; 
                    anExercise.ExerciseType = rdr.GetString(5);
                    anExercise.ExerciseInstructions = rdr.GetString(6);

                    exercises.Add(anExercise);
                    //Console.WriteLine(anExercise.ExerciseName );
                }
                connection.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            GetBenefitsAndInjuryLocationsForExercises( exercises);

            
            return exercises;
        }

        public async Task<bool> GetBenefitsAndInjuryLocationsForExercises(List<Exercise> exercises)
        {
            List<long> exercisesIdList = new List<long>();
            for (int i = 0; i < exercises.Count; i++)
            {
                exercisesIdList.Add(exercises[i].ExerciseId);
                Console.WriteLine(exercisesIdList[i]);
            }

            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                string sql = "SELECT exercise.exercise_id, exercise.exercise_name, benefit.benefit_id, benefit.benefit_name, benefit.benefit_specificity, injury_location.injury_location_id, injury_location.body_part " +
                        "FROM exercise " +
                        "FULL JOIN exercise_benefit ON exercise.exercise_id = exercise_benefit.exercise_id " +
                        "FULL JOIN benefit ON exercise_benefit.benefit_id = benefit.benefit_id " +
                        "FULL JOIN exercise_injury_location ON exercise.exercise_id = exercise_injury_location.exercise_id " +
                        "FULL JOIN injury_location ON exercise_injury_location.injury_location_id = injury_location.injury_location_id " +
                        "WHERE exercise.exercise_id = ANY (@exercise_id_list) " +
                        "ORDER BY exercise.exercise_name ";


                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("exercise_id_list", exercisesIdList.ToArray());
                cmd.Prepare();




                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Console.WriteLine(string.Format("Id: {0} Name: {1} benefit_id: {2} benefit_name: {3} benefit_specificity {4} body_part_id: {5} body_part: {6}", rdr.GetInt32(0), rdr.GetString(1), rdr.GetInt32(2), rdr.GetString(3), rdr.GetString(4), rdr.GetInt32(5), rdr.GetString(6)));
                    int currentExerciseId = rdr.GetInt32(0);
                    Benefit currentBenefit = new Benefit(rdr.GetInt32(2), rdr.GetString(3), rdr.GetString(4));
                    InjuryLocation currentInjuryLocation = new InjuryLocation(rdr.GetInt32(5), rdr.GetString(6));

                    for (int i = 0; i < exercises.Count; i++)
                    {
                        if (currentExerciseId == exercises[i].ExerciseId)
                        {
                            if (exercises[i].Benefits.Find(pt => pt.BenefitId == currentBenefit.BenefitId) == null)
                            {
                                exercises[i].Benefits.Add(new Benefit(currentBenefit.BenefitId, currentBenefit.BenefitName));
                            }
                            if (exercises[i].InjuryLocations.Find(pt => pt.InjuryLocationId == currentInjuryLocation.InjuryLocationId) == null)
                            {
                                exercises[i].InjuryLocations.Add(new InjuryLocation(currentInjuryLocation.InjuryLocationId, currentInjuryLocation.BodyPart));
                            }
                        }
                    }


                }
                connection.Close();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }


        public async Task<int> GetExercisesCountCall(ExerciseFilter exerciseFilter)
        {
            
            int count = 0;


           
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();



                string sql = "SELECT COUNT(*) FROM exercise " +
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


                using var cmd = new NpgsqlCommand(sql, connection);

                List<int> benefitIdList = new List<int>();
                for (int i = 0; i < exerciseFilter.BenefitsList.Count; i++)
                {
                    benefitIdList.Add(exerciseFilter.BenefitsList[i].BenefitId);
                    //Console.WriteLine(exerciseFilter.BenefitsList[i].BenefitName);
                }
                List<string> exerciseTypesList = new List<string>();
                for (int i = 0; i < exerciseFilter.ExerciseTypesList.Count; i++)
                {
                    exerciseTypesList.Add(exerciseFilter.ExerciseTypesList[i].Type);
                    //Console.WriteLine(exerciseFilter.ExerciseTypesList[i].Type);
                }
                List<int> injuryLocationIdList = new List<int>();
                Console.WriteLine("ExcludeBasedOnInjuries:");
                Console.WriteLine(exerciseFilter.ExcludeBasedOnInjuries);
                if (exerciseFilter.ExcludeBasedOnInjuries == true)
                {
                    for (int i = 0; i < exerciseFilter.InjuryLocations.Count; i++)
                    {
                        injuryLocationIdList.Add(exerciseFilter.InjuryLocations[i].InjuryLocationId);
                        Console.WriteLine(exerciseFilter.InjuryLocations[i].BodyPart);
                    }
                }

                cmd.Parameters.AddWithValue("benefit_list", benefitIdList.ToArray());
                cmd.Parameters.AddWithValue("type_list", exerciseTypesList.ToArray());
                cmd.Parameters.AddWithValue("injury_list", injuryLocationIdList.ToArray());
                cmd.Prepare();




                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    count = rdr.GetInt32(0);

                    Console.WriteLine(string.Format("Number of exercises returned: {0}", count));
                }
                connection.Close();
            }
            catch( Exception e)
            {
                Console.WriteLine(e);
            }
            return count;
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

        public async Task<List<ExerciseType>> GetTypesCall()
        {
            List<ExerciseType> types = new List<ExerciseType>();




          
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT * FROM exercise_type " +
                    "ORDER BY exercise_type ASC";


                using var cmd = new NpgsqlCommand(sql, connection);


                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    ExerciseType type = new ExerciseType();
                    type.ExerciseTypeId = rdr.GetInt32(0);
                    type.Type = rdr.GetString(1);
                    //Console.WriteLine(rdr.GetInt32(0) + rdr.GetString(1));
                    types.Add(type);
                }
                connection.Close();
            }
            catch( Exception e)
            {
                Console.WriteLine(e);
            }
            return types;
        }

        public async Task<List<Benefit>> GetBenefitsCall()
        {
            List<Benefit> benefits = new List<Benefit>();





            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT * FROM benefit";


                using var cmd = new NpgsqlCommand(sql, connection);


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
                connection.Close();
            }
            catch(Exception e){
                Console.WriteLine(e);
            }
            return benefits;
        }

        public async Task<List<Benefit>> GetGeneralBenefitsCall()
        {
            List<Benefit> benefits = new List<Benefit>();




            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT * FROM benefit " +
                    "WHERE benefit.benefit_specificity = 'General'";


                using var cmd = new NpgsqlCommand(sql, connection);


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
                connection.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
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

        public async Task<bool> HasAdminPrivileges(Guid? uid)
        {
            int count = 0;

            if( uid == null)
            {
                return false;
            }

            try {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT COUNT(DISTINCT user_uid) " +
                "FROM admin_privileges " +
                "WHERE user_uid = @uid";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("uid", uid);

            
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    count = rdr.GetInt32(0);
                }
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if( count == 1)
            {
                
                return true;
            }
            
            return false;
        }

        public async Task<bool> PostSuggestedExerciseHelper(Exercise exercise)
        {
           
            try {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "INSERT INTO exercise ( exercise_name, exercise_description, exercise_link, exercise_main_image, exercise_type, exercise_instructions) VALUES" +
                "(@exercise_name, @exercise_description, @exercise_link, @exercise_main_image, @exercise_type, @exercise_instructions)";


                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
                cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
                cmd.Parameters.AddWithValue("exercise_link", exercise.ExerciseLink);
                cmd.Parameters.AddWithValue("exercise_main_image", exercise.ExerciseMainImage);
                cmd.Parameters.AddWithValue("exercise_type", exercise.ExerciseType);
                cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);
                cmd.Prepare();

            
                cmd.ExecuteNonQuery();
                connection.Close();
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

        public async Task<bool> PostSuggestedExerciseAsSuggestionHelper(Exercise exercise)
        {
       

            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "INSERT INTO suggested_exercise ( suggested_exercise_name, suggested_exercise_description, suggested_exercise_link, suggested_exercise_main_image, suggested_exercise_type, suggested_exercise_instructions) VALUES" +
                    "(@exercise_name, @exercise_description, @exercise_link, @exercise_main_image, @exercise_type, @exercise_instructions)";


                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
                cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
                cmd.Parameters.AddWithValue("exercise_link", exercise.ExerciseLink);
                cmd.Parameters.AddWithValue("exercise_main_image", exercise.ExerciseMainImage);
                cmd.Parameters.AddWithValue("exercise_type", exercise.ExerciseType);
                cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);
                cmd.Prepare();

                cmd.ExecuteNonQuery();
                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;

        }

        public async Task<bool> AddAllInjuryLocations(Exercise exercise)
        {
          
            try {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT exercise_id FROM exercise WHERE exercise_name = @exercise_name AND exercise_description = @exercise_description AND exercise_instructions = @exercise_instructions";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
                cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
                cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);

            
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    exercise.ExerciseId = rdr.GetInt32(0);
                }
                connection.Close();
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

        public async Task<bool> AddOneInjuryLocation(long? exerciseId, long injuryLocationId)
        {
            if ((exerciseId == null) || (injuryLocationId == null))
            {
                return false;
            }
           
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "INSERT INTO exercise_injury_location ( exercise_id, injury_location_id) VALUES" +
                "(@exercise_id, @injury_location_id)";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("exercise_id", exerciseId);
                cmd.Parameters.AddWithValue("injury_location_id", injuryLocationId);


                cmd.ExecuteNonQuery();

                connection.Close();
                return true; ;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> AddAllBenefits(Exercise exercise)
        {
         
            try {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "SELECT exercise_id FROM exercise WHERE exercise_name = @exercise_name AND exercise_description = @exercise_description AND exercise_instructions = @exercise_instructions";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("exercise_name", exercise.ExerciseName);
                cmd.Parameters.AddWithValue("exercise_description", exercise.ExerciseDescription);
                cmd.Parameters.AddWithValue("exercise_instructions", exercise.ExerciseInstructions);

            
                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    exercise.ExerciseId = rdr.GetInt32(0);
                }
                connection.Close();
                for (int i = 0; i < exercise.Benefits.Count; i++)
                {
                    AddOneBenefit(exercise.ExerciseId, exercise.Benefits[i].BenefitId);
                }
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            
        }

        public async Task<bool> AddOneBenefit(long? exerciseId, long benefitId)
        {
            if ((exerciseId == null) || (benefitId == null))
            {
                return false;
            }
          
            try
            {
                NpgsqlConnection connection = await PostgresDatabaseDataSource.Instance.GetConnection();

                var sql = "INSERT INTO exercise_benefit ( exercise_id, benefit_id) VALUES" +
                "(@exercise_id, @benefit_id)";

                using var cmd = new NpgsqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("exercise_id", exerciseId);
                cmd.Parameters.AddWithValue("benefit_id", benefitId);


                cmd.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }



    }

    

}
