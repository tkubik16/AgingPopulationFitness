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
            List<Exercise> exercises = new List<Exercise>();
            var response = await httpClient.PostAsJsonAsync($"exercise", exerciseFilter, ExerciseContext.Default.ExerciseFilter);
            response.EnsureSuccessStatusCode();
            var exercisesResponse = await response.Content.ReadFromJsonAsync<List<Exercise>>();
            if( exercisesResponse is null)
            {
                return exercises;
            }
            else
            {
                return exercisesResponse;
            }
            
            


        }

        public async Task<Exercise> GetExercise(long exerciseId)
        {
            Exercise exercise = new Exercise();

            var theExercise = await httpClient.GetFromJsonAsync($"exercise/{exerciseId}", ExerciseContext.Default.Exercise);

            if (theExercise is null)
            {
                return exercise;
            }
            else
            {
                return theExercise;
            }

        }

        public async Task<int> GetAllExercisesCount(ExerciseFilter exerciseFilter)
        {

            var response = await httpClient.PostAsJsonAsync($"exercise/count", exerciseFilter, ExerciseContext.Default.ExerciseFilter);
            response.EnsureSuccessStatusCode();
            var count = await response.Content.ReadFromJsonAsync<int>();
            return count;



        }

        public async Task<List<ExerciseType>> GetExerciseTypes()
        {
            List<ExerciseType> exerciseTypes = new List<ExerciseType>();

            var types = await httpClient.GetFromJsonAsync("exercise/types", ExerciseContext.Default.ListExerciseType);

            if( types is null)
            {
                return exerciseTypes;
            }
            else
            {
                return types;
            }
            
        }

        public async Task<List<Benefit>> GetBenefits()
        {
            List<Benefit> benefitsList = new List<Benefit>();

            var benefits = await httpClient.GetFromJsonAsync("exercise/benefits", ExerciseContext.Default.ListBenefit);

            if (benefits is null)
            {
                return benefitsList;
            }
            else { 
                return benefits;
            }
        }

        public async Task<List<Benefit>> GetGeneralBenefits()
        {
            List<Benefit> benefits = new List<Benefit>();

            var generalBenefits = await httpClient.GetFromJsonAsync("exercise/benefits/general", ExerciseContext.Default.ListBenefit);
            if (generalBenefits is null)
            {
                return benefits;
            }
            else
            {
                return generalBenefits;
            }
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





