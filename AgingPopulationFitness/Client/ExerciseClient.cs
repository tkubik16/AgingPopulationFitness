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

        public async Task<List<Exercise>> GetAllExercises(ExerciseFilter exerciseFilter)
        {

            var response = await httpClient.PostAsJsonAsync($"exercise", exerciseFilter, ExerciseContext.Default.ExerciseFilter);
            response.EnsureSuccessStatusCode();
            var exercises = await response.Content.ReadFromJsonAsync<List<Exercise>>();
            return exercises;
            


        }

        public async Task<List<ExerciseType>> GetExerciseTypes()
        {
            var types = await httpClient.GetFromJsonAsync("exercise/types", ExerciseContext.Default.ListExerciseType);

            return types;
        }

        public async Task<List<Benefit>> GetBenefits()
        {
            var benefits = await httpClient.GetFromJsonAsync("exercise/benefits", ExerciseContext.Default.ListBenefit);
            
            return benefits;
        }

        public async Task<List<Benefit>> GetGeneralBenefits()
        {
            var generalBenefits = await httpClient.GetFromJsonAsync("exercise/benefits/general", ExerciseContext.Default.ListBenefit);

            return generalBenefits;
        }

        public async Task<bool> PostSuggestedExercise(SuggestedExercise suggestedExercise)
        {
            var response = await httpClient.PostAsJsonAsync($"exercise/suggested", suggestedExercise, ExerciseContext.Default.SuggestedExercise);
            response.EnsureSuccessStatusCode();
            var success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

    }
}





