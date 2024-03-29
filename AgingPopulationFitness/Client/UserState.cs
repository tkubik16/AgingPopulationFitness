﻿using Blazored.SessionStorage;
using System.ComponentModel;
using AgingPopulationFitness;

namespace AgingPopulationFitness.Client
{
    public class UserState
    {
        public UserProfile? userProfile { get; set; }
        public bool isLoggedIn { get; set; }

        public List<UserInjury> userInjuries { get; set; }

        public List<InjuryLocation> injuryLocations { get; set; }

        public UserState()
        {
            userProfile = new UserProfile();
            injuryLocations = new List<InjuryLocation>();
            isLoggedIn = false;
        }

        public async Task<bool> Refresh( Blazored.SessionStorage.ISessionStorageService sessionStorage)
        {

            this.userProfile.UserId = await sessionStorage.GetItemAsync<Guid>("UserId");
            this.userProfile.Username = await sessionStorage.GetItemAsync<String>("Username");
            this.userProfile.Password = await sessionStorage.GetItemAsync<String>("Password");
            this.isLoggedIn = await sessionStorage.GetItemAsync<bool>("IsLoggedin");

            if (isLoggedIn) { return true; }
            else return false;
        }

    }
}
