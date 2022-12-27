﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AgingPopulationFitness.Client
{
    public class UserClient
    {
        private readonly HttpClient httpClient;

        public UserClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        /*
        public async Task<UserProfile> GetUser(UserProfile userProfile) =>
            await httpClient.GetFromJsonAsync($"user/{userProfile}", UserProfileContext.Default.UserProfile);
        */
        public async Task<UserProfile> VerifyUser(UserProfile userProfile)
        {


            var response = await httpClient.PostAsJsonAsync($"user", userProfile, UserProfileContext.Default.UserProfile);
            response.EnsureSuccessStatusCode();
            var fullUserProfile = await response.Content.ReadFromJsonAsync<UserProfile>();
            return fullUserProfile;
        }

        public async Task<bool> UserExistsCheck(UserProfile userProfile)
        {
            var response = await httpClient.PostAsJsonAsync($"user/UserExistsCheck", userProfile, UserProfileContext.Default.UserProfile);
            response.EnsureSuccessStatusCode();
            var userExists = await response.Content.ReadFromJsonAsync<bool>();
            return userExists;
        }

        public async Task<bool> IsLoggedIn(UserProfile userProfile)
        {
            var response = await httpClient.PostAsJsonAsync($"user/IsLoggedIn", userProfile, UserProfileContext.Default.UserProfile);
            response.EnsureSuccessStatusCode();
            var isLoggedIn = await response.Content.ReadFromJsonAsync<bool>();
            return isLoggedIn;
        }

        public async Task<bool> AddUser(UserProfile userProfile)
        {
            var response = await httpClient.PostAsJsonAsync($"user/AddUser", userProfile, UserProfileContext.Default.UserProfile);
            response.EnsureSuccessStatusCode();
            var userAdded = await response.Content.ReadFromJsonAsync<bool>();
            return userAdded;
        }

        public async void GetUser() =>
            await httpClient.GetFromJsonAsync($"user", UserProfileContext.Default.UserProfile);
    }
}