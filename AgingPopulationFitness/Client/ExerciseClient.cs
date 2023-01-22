using AgingPopulationFitness.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AgingPopulationFitness.Client
{
    public class ExerciseClient
    {
        private readonly HttpClient httpClient;

        public ExerciseClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<Exercise>> GetAllExercises()
        {

            var exercises = await httpClient.GetFromJsonAsync($"exercise", ExerciseContext.Default.ListExercise);
            return exercises;
            


        }

    }
}





