using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Threading.Tasks;
using Training.Website.Models;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;
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
        private IDatabase? Database_OPS { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private const int _reviewYear = 2025;   // TEMP
        private int _selectedRadioChoiceID = -1;
        private AllUsers_OPS_DB?[]? _allUsers_OPS_DB = null;
        private AllUsers_CMS_DB?[]? _allUsers_CMS_DB = null;
        private IEnumerable<UsersForManagerModel?>? _allUsersForManager = null;
        private UsersForManagerModel? _selectedUserForManager = null;
        private Dictionary<int, string>? _answerFormats = null;
        private EmployeeInformationModel? _headerInfo = null;
        private PerformanceReviewQuestionModel?[]? _questions = null;
        private RadioChoiceModel?[]? _radioChoices = null;
        private bool _subReviewEnabled = false;
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private PerformanceReviewServiceMethods _service = new();


        #endregion



        protected override async Task OnInitializedAsync()
        {
            _allUsers_CMS_DB = (await _service.GetAllUsers_CMS_DB(_dbCMS))?.ToArray();
            _allUsers_OPS_DB = (await _service.GetAllUsers_OPS_DB(Database_OPS))?.ToArray();
            _allUsersForManager = await _service.GetUsersForManager_CMS_DB(_allUsers_CMS_DB, _allUsers_OPS_DB, ApplicationState!.LoggedOnUser!.AppUserID, _dbCMS);
            _answerFormats = await _service.GetPerformanceReviewAnswerFormats(Database_OPS);
            _questions = (await _service.GetPerformanceReviewQuestions(_reviewYear, Database_OPS))?.ToArray();
        }

        // =====================================================================================================================================================================================================================================================================================================================================================================================================

        private bool SubmitReviewEnabled() =>
            _questions != null && _questions.All(q => q != null && !string.IsNullOrWhiteSpace(q.Answer));

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
                            (question.Question_ID.Value, ApplicationState!.LoggedOnUser!.EmpID!.Value, _selectedUserForManager!.OPS_UserID!.Value, question.Answer, ApplicationState!.LoggedOnUser!.AppUserID, _selectedUserForManager.CMS_UserID, Database_OPS);

                _selectedRadioChoiceID = -1;
                _selectedUserForManager = null;

                StateHasChanged();
            }
        }

        private async Task UserForManagerChanged(string newValue)
        {
            _selectedUserForManager = _allUsersForManager?.FirstOrDefault(q => q?.FullName == newValue);

            if (_selectedUserForManager != null)
            {
                if (_selectedUserForManager.OPS_UserID != null)
                    _headerInfo = await _service.GetEmployeeInformation(_selectedUserForManager.OPS_UserID.Value, _reviewYear, Database_OPS);
                else
                    throw new NoNullAllowedException(".OPS_User_ID cannot be null in UserForManagerChanged() method.");
            }
            StateHasChanged();
        }
    }
}
