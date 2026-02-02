using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using Telerik.Blazor.Components;
using Training.Website.Models;
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
        private string? _selectedAnswerFormatDropDownValue = null;
        private string[]? _reviewYears = null;
        private Dictionary<int, string>? _answerFormats = null;
        private List<PerformanceReviewQuestionModel?>? _activeQuestions = null;
        private List<PerformanceReviewQuestionModel?>? _deletedQuestions = null;
        private PerformanceReviewServiceMethods _service = new();
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _reviewYears = ReviewYears();
            //_answerFormats = (await _service.GetPerformanceReviewAnswerFormats_AdministratorPage(Database_OPS))?.ToArray();
            _answerFormats = await _service.GetPerformanceReviewAnswerFormats(Database_OPS);
        }

        // =========================================================================================================================================================================================================================================================================================================

        private void ActiveQuestionCreateHandler(GridCommandEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void ActiveQuestionDeleteHandler(GridCommandEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void ActiveQuestionUpdateHandler(GridCommandEventArgs args)
        {
            var updatedQuestion = (PerformanceReviewQuestionModel?)args.Item;

            if (updatedQuestion != null && string.IsNullOrWhiteSpace(_selectedAnswerFormatDropDownValue) == false)
            {
                foreach(KeyValuePair<int, string> kvp in _answerFormats!)
                {
                    if (kvp.Value == _selectedAnswerFormatDropDownValue)
                    {
                        updatedQuestion.AnswerFormat = kvp.Key;
                        break;
                    }
                }
                _selectedAnswerFormatDropDownValue = null;
            }
        }

        private void RestoreHandler(GridCommandEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private async Task ReviewYearChanged(string newValue)
        {
            if (int.TryParse(newValue, out int selectedReviewYear) == true)
            {
                _selectedReviewYear = selectedReviewYear;
                _activeQuestions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, false, Database_OPS))?.ToList();
                _deletedQuestions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, true, Database_OPS))?.ToList();
                /*
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
