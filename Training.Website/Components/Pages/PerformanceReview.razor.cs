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
        private const int _firstReviewYear = 2025;
        private int? _selectedReviewYear = null;
        private string[]? _reviewYears = null;
        private int? _opsReviewerID = null;
        private bool _loading = false;
        private AllUsers_OPS_DB?[]? _allUsers_OPS_DB = null;
        private AllUsers_CMS_DB?[]? _allUsers_CMS_DB = null;
        private IEnumerable<UsersForDropDownModel?>? _allUsersForDropDown = null;
        private UsersForDropDownModel? _selectedUser = null;
        private Dictionary<int, string>? _answerFormats = null;
        private EmployeeInformationModel? _headerInfo = null;
        private PerformanceReviewQuestionModel?[]? _questions = null;
        private RadioChoiceModel?[]? _allRadioChoices = null;
        private AnswersByReviewYearOpsReviewerOpsRevieweeModel?[]? _answers = null;
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private PerformanceReviewServiceMethods _service = new();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _allUsers_OPS_DB = (await _service.GetAllUsers_OPS_DB(Database_OPS))?.ToArray();

            if (_allUsers_OPS_DB == null || _allUsers_OPS_DB.Length == 0)
                throw new NoNullAllowedException("_allUsers_OPS_DB cannot be null or empty in OnInitializedAsync() method.");
            else
            {
                _opsReviewerID = ApplicationState?.LoggedOnUser?.EmpID;

                if (_opsReviewerID == null)
                    throw new NoNullAllowedException("_opsReviewerID cannot be null in OnInitializedAsync() method.");
                else
                {
                    _reviewYears = ReviewYears();
                    if (_reviewYears == null || _reviewYears.Length == 0)
                        throw new NoNullAllowedException("_reviewYears cannot be null in OnInitializedAsync()");
                    else
                    {
                        if (int.TryParse(_reviewYears[0], out int selectedReviewYear) == false)
                            throw new NoNullAllowedException("_reviewYears[0] must be an integer in OnItializedAsync()");
                        else
                        {
                            _allUsers_CMS_DB = (await _service.GetAllUsers_CMS_DB(_dbCMS))?.ToArray();
                            _allUsersForDropDown = await GetUsers_Main();
                            _answerFormats = await _service.GetPerformanceReviewAnswerFormats(Database_OPS);
                        }
                    }
                }
            }
        }

        // =====================================================================================================================================================================================================================================================================================================================================================================================================

        private async Task<IEnumerable<UsersForDropDownModel?>?> GetUsers_Main()
        {
            int? cmsUserID = ApplicationState?.LoggedOnUser?.AppUserID;

            if (cmsUserID == null)
                throw new NoNullAllowedException("cmsUserID cannot be null in GetUsers_Main() method.");
            else if (_allUsers_CMS_DB == null || _allUsers_CMS_DB.Length == 0)
                throw new NoNullAllowedException("_allUsers_CMS_DB cannot be null or empty in GetUsers_Main() method.");
            else if (_allUsers_OPS_DB == null || _allUsers_OPS_DB.Length == 0)
                throw new NoNullAllowedException("_allUsers_OPS_DB cannot be null or empty in GetUsers_Main() method.");
            else
                return
                    ApplicationState!.LoggedOnUser!.Administrator == false
                        ? await _service.GetUsersForManager_CMS_DB(_allUsers_CMS_DB, _allUsers_OPS_DB, cmsUserID, _dbCMS)
                        : await _service.GetAllUsersExceptUserLoggedOn(_allUsers_CMS_DB, _allUsers_OPS_DB, cmsUserID);
        }

        private void RadioChoiceHandler(object? newValue)
        {
            if (newValue != null && int.TryParse(newValue.ToString(), out int selectedRadioChoiceID) == true)
            {
                RadioChoiceModel? radioChoice = _allRadioChoices?.FirstOrDefault(q => q?.RadioChoice_ID == selectedRadioChoiceID);
                PerformanceReviewQuestionModel? question = _questions?.FirstOrDefault(q => q?.Question_ID == radioChoice?.ReviewQuestion_ID);

                question!.Answer = radioChoice?.RadioChoice_Text;
                question!.RadioChoice_ID = radioChoice?.RadioChoice_ID;
            }
        }

        private IEnumerable<RadioChoiceModel?>? RadioChoicesForQuestion(int? questionID)
        {
            if (questionID != null)
                return _allRadioChoices?.Where(q => q?.ReviewQuestion_ID == questionID).OrderBy(q => q?.RadioChoice_Sequence);
            else
                return null;
        }

        private async Task ReviewYearChanged(string newValue)
        {
            if (int.TryParse(newValue, out int selectedReviewYear) == true)
            {
                _selectedReviewYear = selectedReviewYear;
                _questions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, Database_OPS))?.ToArray();
                _allRadioChoices = (await _service.GetAllRadioButtonChoicesByYear(_selectedReviewYear.Value, Database_OPS))?.ToArray();
                _selectedUser = null;
                _headerInfo = null;
                _answers = null;
                StateHasChanged();
            }
        }

        private string[]? ReviewYears()
        {
            // FOR POPULATING "REVIEW YEAR" DROPDOWN

            int mostRecentPastReviewYear = DateTime.Now.Year - 1;
            List<string> reviewYears = [];

            for (int year = mostRecentPastReviewYear; year >= _firstReviewYear; year--)
                reviewYears.Add(year.ToString());

            return [.. reviewYears];
        }

        private async Task SubmitReviewClicked()
        {
            if (_questions != null)
            {
                foreach (PerformanceReviewQuestionModel? question in _questions)
                    if (question != null && question.Question_ID != null && question.Answer != null)
                        await _service.InsertPerformanceReviewAnswer
                            (question.Question_ID.Value, ApplicationState!.LoggedOnUser!.EmpID!.Value, _selectedUser!.OPS_UserID!.Value, question.Answer, ApplicationState!.LoggedOnUser!.AppUserID, _selectedUser.CMS_UserID, Database_OPS);

                //_selectedRadioChoiceID = -1;
                _selectedUser = null;

                StateHasChanged();
            }
        }

        private bool SubmitReviewEnabled() =>
            _questions != null && _questions.All(q => q != null && !string.IsNullOrWhiteSpace(q.Answer));

        private async Task UserForDropDownChanged(string newValue)
        {
            _loading = true;
            StateHasChanged();
            await Task.Delay(2000);

            if (_answerFormats != null && _answerFormats.Count() > 0)
            {
                _selectedUser = _allUsersForDropDown?.FirstOrDefault(q => q?.FullName == newValue);

                if (_selectedUser != null)
                {
                    if (_selectedUser.OPS_UserID != null)
                    {
                        _headerInfo = await _service.GetEmployeeInformation(_selectedUser.OPS_UserID.Value, _selectedReviewYear!.Value, Database_OPS);
                        _answers =
                            (await _service.GetAnswersByReviewYearOpsReviewerOpsReviewee(_selectedReviewYear!.Value, _opsReviewerID!.Value, _selectedUser!.OPS_UserID.Value, Database_OPS))?.ToArray();

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
