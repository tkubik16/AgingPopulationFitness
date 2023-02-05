using AgingPopulationFitness.Shared;

namespace AgingPopulationFitness.Client
{
    public class ExerciseFilterState
    {
        public ExerciseFilterState()
        {
            ShowingConfigureDialog = false;
            ExerciseFilter = new ExerciseFilter();
            benefitSelectValue = "-1";
            typeSelectValue = "-1";
        }
        public bool ShowingConfigureDialog { get; set; }
        public ExerciseFilter ExerciseFilter { get; set; }
        public string benefitSelectValue { get; set; }
        public string typeSelectValue { get; set; }
    }
}
