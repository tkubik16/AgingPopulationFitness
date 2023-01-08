using AgingPopulationFitness.Shared;

namespace AgingPopulationFitness.Client
{
    public class InjuryState
    {
        public InjuryState() 
        {
            ShowingConfigureDialog = false;
            ConfiguringUserInjury = new UserInjury();
        }
        public bool ShowingConfigureDialog { get; set; }
        public UserInjury ConfiguringUserInjury { get; set; }

        public void ShowConfigureUserInjuryDialog()
        {
            ConfiguringUserInjury = new UserInjury();
            ShowingConfigureDialog = true;
        }

        public void CancelConfigureUserInjuryDialog()
        {
            ShowingConfigureDialog = false;
        }

        public void ConfirmConfigureUserInjuryDialog()
        {
            ShowingConfigureDialog = false;
        }
    }
}
