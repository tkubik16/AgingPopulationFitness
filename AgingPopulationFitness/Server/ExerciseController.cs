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



    }

    

}
