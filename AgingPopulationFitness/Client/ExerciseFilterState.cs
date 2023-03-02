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
            ExerciseResults = new List<Exercise>();
            allExerciseImages = new List<string>();
            AllBenefitsList = new List<Benefit>();
            AllExerciseTypesList = new List<ExerciseType>();
            ExercisesCount = 0;
            CurrentPage = 1;
            PageCount = 1;
            ExercisesPerPage = 10;
        }
        public bool ShowingConfigureDialog { get; set; }
        public ExerciseFilter ExerciseFilter { get; set; }
        public string benefitSelectValue { get; set; }
        public string typeSelectValue { get; set; }
        public List<Exercise> ExerciseResults { get; set; }
        public List<string> allExerciseImages { get; set; }
        public List<Benefit> AllBenefitsList { get; set; }
        public List<ExerciseType> AllExerciseTypesList { get; set; }

        //For the page selector
        public int ExercisesCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int ExercisesPerPage { get; set; }

        public void ConvertToImages()
        {
            if (this.ExerciseResults != null)
            {
                this.allExerciseImages.Clear();
                for (int i = 0; i < this.ExerciseResults.Count; i++)
                {
                    this.allExerciseImages.Add( new string( string.Format("data:image/jpeg;base64,{0}", Convert.ToBase64String(this.ExerciseResults[i].ExerciseMainImage))));
                    //Console.WriteLine(this.allExerciseImages[i]);
                }
            }
        }

        public void SetUpPageSelector()
        {
            int fullPageNum = 0;

            if(this.ExercisesCount == 0)
            {
                this.PageCount = 0;
                this.CurrentPage = 0;
                return;
            }

            fullPageNum = this.ExercisesCount / this.ExercisesPerPage;
            this.PageCount = fullPageNum;

            if((this.ExercisesCount - (fullPageNum * this.ExercisesPerPage)) > 0)
            {
                this.PageCount++;
            }

            this.CurrentPage = 1;
            return;

        }

        public void PrevPage()
        {
            if( this.CurrentPage == 1)
            {
                return;
            }
            this.CurrentPage--;
    }

        public void NextPage()
        {
            if (this.CurrentPage == this.PageCount)
            {
                return;
            }
            this.CurrentPage++;
        }

        public void GoToFirstPage()
        {
            this.CurrentPage = 1;
        }

        public void GoToLastPage()
        {
            this.CurrentPage = this.PageCount;
        }

        public void SetExerciseFilterPageInfo()
        {
            this.ExerciseFilter.ExercisesPerPage = this.ExercisesPerPage;
            this.ExerciseFilter.PageNumber = this.CurrentPage;
        }
    }
}
