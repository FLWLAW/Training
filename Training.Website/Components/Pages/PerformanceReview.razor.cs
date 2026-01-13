using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Threading.Tasks;
using Telerik.SvgIcons;
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
        private int? _opsReviewerID = null;
        private bool _loading = false;
        private AllUsers_OPS_DB?[]? _allUsers_OPS_DB = null;
        private AllUsers_CMS_DB?[]? _allUsers_CMS_DB = null;
        private IEnumerable<UsersForManagerModel?>? _allUsersForManager = null;
        private UsersForManagerModel? _selectedUserForManager = null;
        private Dictionary<int, string>? _answerFormats = null;
        private EmployeeInformationModel? _headerInfo = null;
        private PerformanceReviewQuestionModel?[]? _questions = null;
        private RadioChoiceModel?[]? _allRadioChoices = null;
        //private RadioChoiceModel?[]? _radioChoices = null;
        private AnswersByReviewYearOpsReviewerOpsRevieweeModel?[]? _answers = null;
        private bool _subReviewEnabled = false;
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private PerformanceReviewServiceMethods _service = new();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _allUsers_OPS_DB = (await _service.GetAllUsers_OPS_DB(Database_OPS))?.ToArray();
            
            _opsReviewerID =
                _allUsers_OPS_DB?.FirstOrDefault(q => q?.UserName?.Equals(ApplicationState!.LoggedOnUser!.LoginID, StringComparison.InvariantCultureIgnoreCase) == true)?.Emp_ID;
            
            if (_opsReviewerID == null)
                throw new NoNullAllowedException("_opsReviewerID cannot be null in OnInitializedAsync() method.");
            else
            {
                _allUsers_CMS_DB = (await _service.GetAllUsers_CMS_DB(_dbCMS))?.ToArray();
                _allUsersForManager = await _service.GetUsersForManager_CMS_DB(_allUsers_CMS_DB, _allUsers_OPS_DB, ApplicationState!.LoggedOnUser!.AppUserID, _dbCMS);
                _answerFormats = await _service.GetPerformanceReviewAnswerFormats(Database_OPS);
                _questions = (await _service.GetPerformanceReviewQuestions(_reviewYear, Database_OPS))?.ToArray();
                _allRadioChoices = (await _service.GetAllRadioButtonChoicesByYear(_reviewYear, Database_OPS))?.ToArray();
            }
        }

        // =====================================================================================================================================================================================================================================================================================================================================================================================================

        private void RadioChoiceHandler(object? newValue)
        {
            if (newValue != null)
            {
                if (int.TryParse(newValue.ToString(), out int selectedRadioChoiceID) == true)
                {
                    RadioChoiceModel? radioChoice = _allRadioChoices?.FirstOrDefault(q => q?.RadioChoice_ID == selectedRadioChoiceID);
                    PerformanceReviewQuestionModel? question = _questions?.FirstOrDefault(q => q?.Question_ID == radioChoice?.ReviewQuestion_ID);

                    question!.Answer = radioChoice?.RadioChoice_Text;
                    question!.RadioChoice_ID = radioChoice?.RadioChoice_ID;
                }
            }
        }

        private IEnumerable<RadioChoiceModel?>? RadioChoicesForQuestion(int? questionID)
        {
            if (questionID != null)
                return _allRadioChoices?.Where(q => q?.ReviewQuestion_ID == questionID).OrderBy(q => q?.RadioChoice_Sequence);
            else
                return null;
        }

        private async Task SubmitReviewClicked()
        {
            if (_questions != null)
            {
                foreach (PerformanceReviewQuestionModel? question in _questions)
                    if (question != null && question.Question_ID != null && question.Answer != null)
                        await _service.InsertPerformanceReviewAnswer
                            (question.Question_ID.Value, ApplicationState!.LoggedOnUser!.EmpID!.Value, _selectedUserForManager!.OPS_UserID!.Value, question.Answer, ApplicationState!.LoggedOnUser!.AppUserID, _selectedUserForManager.CMS_UserID, Database_OPS);

                //_selectedRadioChoiceID = -1;
                _selectedUserForManager = null;

                StateHasChanged();
            }
        }

        private bool SubmitReviewEnabled() =>
            _questions != null && _questions.All(q => q != null && !string.IsNullOrWhiteSpace(q.Answer));

        private async Task UserForManagerChanged(string newValue)
        {
            _loading = true;
            StateHasChanged();
            await Task.Delay(2000);

            if (_answerFormats != null && _answerFormats.Count() > 0)
            {
                _selectedUserForManager = _allUsersForManager?.FirstOrDefault(q => q?.FullName == newValue);

                if (_selectedUserForManager != null)
                {
                    if (_selectedUserForManager.OPS_UserID != null)
                    {
                        _headerInfo = await _service.GetEmployeeInformation(_selectedUserForManager.OPS_UserID.Value, _reviewYear, Database_OPS);
                        _answers =
                            (await _service.GetAnswersByReviewYearOpsReviewerOpsReviewee(_reviewYear, _opsReviewerID!.Value, _selectedUserForManager!.OPS_UserID.Value, Database_OPS))?.ToArray();

                        if (_questions != null)
                        {
                            if (_answers != null && _answers.Any() == true)
                            {
                                foreach (AnswersByReviewYearOpsReviewerOpsRevieweeModel? answer in _answers)
                                {
                                    if (answer != null)
                                    {
                                        PerformanceReviewQuestionModel? question = _questions.FirstOrDefault(q => q?.Question_ID == answer.Question_ID);

                                        if (question == null)
                                            throw new NoNullAllowedException("[question] cannot be NULL in UserForManagerChanged().");
                                        else if (question.AnswerFormat == null)
                                            throw new NoNullAllowedException("question.AnswerFormat cannot be NULL in UserForManagearChanged().");
                                        else
                                        {
                                            question.Answer = answer.Answer;
                                            if (_answerFormats[question.AnswerFormat.Value] == Globals.RadioButtons)
                                                question.RadioChoice_ID = _allRadioChoices?.FirstOrDefault
                                                    (
                                                        q => q?.ReviewQuestion_ID == question.Question_ID &&
                                                        q?.RadioChoice_Text == answer.Answer
                                                    )?.RadioChoice_ID;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (PerformanceReviewQuestionModel? question in _questions)
                                {
                                    question!.Answer = null;
                                    question!.RadioChoice_ID = null;
                                }
                            }
                        }
                    }
                    else
                        throw new NoNullAllowedException(".OPS_User_ID cannot be null in UserForManagerChanged() method.");
                }
            }
            else
                throw new NoNullAllowedException("_answerFormats cannot be null or empty in UserForManagerChanged() method..");

            _loading = false;
            StateHasChanged();
        }
    }
}
