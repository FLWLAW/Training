using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Threading.Tasks;
using Training.Website.Models;
using Training.Website.Models.Reviews;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class PerformanceReview
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private const int _userID_OPS = 296;        // TEMP
        private const int _userID_CMS = 2134;      // TEMP
        private const int _reviewYear = 2025;   // TEMP
        private int _selectedRadioChoiceID = -1;
        private Dictionary<int, string>? _answerFormats = null;
        private EmployeeInformationModel? _headerInfo = null;
        private PerformanceReviewQuestionModel?[]? _questions = null;
        private RadioChoiceModel?[]? _radioChoices = null;
        private PerformanceReviewServiceMethods _service = new();


        #endregion



        protected override async Task OnInitializedAsync()
        {
            _answerFormats = await _service.GetPerformanceReviewAnswerFormats(Database);
            _headerInfo = await _service.GetEmployeeInformation(_userID_OPS, _reviewYear, Database);
            _questions = (await _service.GetPerformanceReviewQuestions(_reviewYear, Database))?.ToArray();
        }

        private void RadioChoiceHandler(object newValue)
        {
            RadioChoiceModel? radioChoice = _radioChoices?.FirstOrDefault(q => q?.RadioChoice_ID == _selectedRadioChoiceID);
            PerformanceReviewQuestionModel? question = _questions?.FirstOrDefault(q => q?.Question_ID == radioChoice?.ReviewQuestion_ID);

            question!.Answer = radioChoice?.RadioChoice_Text;
        }

        private async Task SubmitReviewClicked()
        {
            if (_questions != null)
            {
                foreach (PerformanceReviewQuestionModel? question in _questions)
                    if (question != null && question.Question_ID != null && question.Answer != null)
                        await _service.InsertPerformanceReviewAnswer
                            (question.Question_ID.Value, ApplicationState!.LoggedOnUser!.EmpID!.Value, _userID_OPS, question.Answer, ApplicationState!.LoggedOnUser!.AppUserID, _userID_CMS, Database);

                StateHasChanged();
            }
        }
    }
}
