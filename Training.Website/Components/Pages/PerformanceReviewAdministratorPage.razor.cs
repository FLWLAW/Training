using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models.Reviews;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class PerformanceReviewAdministratorPage
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database_OPS { get; set; }
        /*
        [Inject]
        private IJSRuntime? JS { get; set; }
        */
        #endregion

        #region PRIVATE FIELDS
        private int? _selectedReviewYear = null;
        private string[]? _reviewYears = null;
        private PerformanceReviewQuestionModel?[]? _questions = null;
        private PerformanceReviewServiceMethods? _service = new();
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _reviewYears = ReviewYears();
            await base.OnInitializedAsync();        //TODO: DELETE THIS LINE ONCE THERE IS AWAITABLECODE IN IT.
        }

        // =========================================================================================================================================================================================================================================================================================================

        private async Task ReviewYearChanged(string newValue)
        {
            if (int.TryParse(newValue, out int selectedReviewYear) == true)
            {
                _selectedReviewYear = selectedReviewYear;
                /*
                _questions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, Database_OPS))?.ToArray();
                _allRadioChoices = (await _service.GetAllRadioButtonChoicesByYear(_selectedReviewYear.Value, Database_OPS))?.ToArray();
                _selectedUser = null;
                _headerInfo = null;
                _answers = null;
                _showChangeStatusReminder = false;
                _showMustClickSubmitReviewReminder = false;
                */
                await Task.Delay(1);        //TODO: REMOVE ONCE THERE IS AWAITABLE CODE IN HERE.
                StateHasChanged();
            }
        }

        private string[]? ReviewYears()
        {
            // FOR POPULATING "REVIEW YEAR" DROPDOWN

            List<string> reviewYears = [];

            for (int year = Globals.FirstReviewYear; year <= DateTime.Now.Year; year++)
                reviewYears.Add(year.ToString());

            return [.. reviewYears];
        }
    }
}
