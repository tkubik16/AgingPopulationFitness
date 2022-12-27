
namespace AgingPopulationFitness.Client
{
    public class UserState
    {
        public UserProfile? userProfile { get; set; }
        public bool isLoggedIn { get; set; }
        public UserState()
        {
            isLoggedIn = false;
        }


    }
}
