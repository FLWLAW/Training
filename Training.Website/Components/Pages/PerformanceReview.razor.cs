using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using Telerik.Blazor.Components;
using Telerik.Documents.SpreadsheetStreaming;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;
using Training.Website.Services;
using Training.Website.Services.WordDocument;

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

        [Inject]
        private IJSRuntime? JS { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private const int _firstReviewYear = 2025;

        private bool _areQuestionsDirty = false;
        private bool _reviewNotStatusNotChangedWindow_Visible = false;
        private bool _reviewSubmittedWindow_Visible = false;
        private bool _answersSavedWindow_Visible = false;
        private int? _selectedReviewYear = null;
        private string[]? _reviewYears = null;
        private int? _cmsReviewerID = null;
        private int? _opsReviewerID = null;
        private bool _loading = false;
        private AllUsers_OPS_DB?[]? _allUsers_OPS_DB = null;
        private AllUsers_CMS_DB?[]? _allUsers_CMS_DB = null;
        private IEnumerable<UsersForDropDownModel?>? _allUsersForDropDown = null;
        private UsersForDropDownModel? _selectedUser = null;
        private Dictionary<int, string>? _answerFormats = null;
        //private Dictionary<int, string>? _reviewStatuses = null;
        private string? _selectedNewReviewStatus = null;
        private ReviewModel? _selectedReview = null;
        private EmployeeInformationModel? _headerInfo = null;
        private PerformanceReviewQuestionModel?[]? _questions = null;
        private RadioChoiceModel?[]? _allRadioChoices = null;
        private AnswersByReviewIdModel?[]? _answers = null;
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private PerformanceReviewServiceMethods _service = new();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _cmsReviewerID = ApplicationState?.LoggedOnUser?.AppUserID;
            if (_cmsReviewerID == null)
                throw new NoNullAllowedException("_cmsReviewerID cannot be null in OnInitializedAsync() method.");
            else
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
        }

        // =====================================================================================================================================================================================================================================================================================================================================================================================================

        private bool AllQuestionsAnswered() =>
            _questions != null && _questions.All(q => q != null && !string.IsNullOrWhiteSpace(q.Answer));

        private void AnswersSavedWindowClicked()
        {
            _answersSavedWindow_Visible = false;
            StateHasChanged();
        }

        private bool CanEditAnswer()
        {
            if (_selectedReview != null && _selectedReview.Status_ID_Type == Globals.ReviewStatusType.SentToHR)
                return false;
            else if (ApplicationState!.LoggedOnUser!.IsPerformanceReviewAdministrator == true)
                return true;
            else
                return _selectedReview != null && _selectedReview.Status_ID_Type == Globals.ReviewStatusType.Pending && WasReviewStatusChanged() == false;
        }

        private async Task ExportPerformanceReviewToWord_Main()
        {
            CreatePerformanceReviewInWordClass export = new CreatePerformanceReviewInWordClass(_selectedReviewYear!.Value, _selectedUser, _selectedReview, _headerInfo);
            RadFlowDocument document = await export.Create();
            string filename = $"{_selectedReviewYear.Value} Performance Review - {_selectedUser?.FirstName} {_selectedUser?.LastName}.docx";

            await Globals.ExportToWordFile(document, filename, JS);
        }

        private async Task ExportPerformanceReviewStatusHistoryToExcel_Main()
        {
            string sheetName = $"Review Status History {_selectedReviewYear} {_selectedUser?.FullName}";

            PerformanceReviewExcelExport export =
                new(sheetName, _selectedUser, _selectedReview, _allUsers_OPS_DB, _allUsers_CMS_DB, _service, Database_OPS);

            using (MemoryStream? stream = await export.Go())
            {
                if (stream != null)
                {
                    stream.Position = 0;    // VERY IMPORTANT!!!!!
                    using var streamRef = new DotNetStreamReference(stream: stream);
                    await JS!.InvokeVoidAsync("downloadFileFromStream", $"{sheetName}.xlsx", streamRef);
                }
            }
        }

        private async Task GetCurrentReviewStatusByReviewID_Main()
        {
            if (_selectedReview != null && _selectedReview.ID != null && _selectedReview.Status_ID == null)
            {
                string? statusID_String = await _service.GetCurrentReviewStatusByReviewID(_selectedReview.ID!.Value, Database_OPS);

                foreach (KeyValuePair<Globals.ReviewStatusType, string> status in Globals.ReviewStatuses)
                {
                    if (status.Value.Equals(statusID_String, StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        _selectedReview.Status_ID = (int)status.Key;
                        break;
                    }
                }
            }
        }

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
                    ApplicationState!.LoggedOnUser!.IsPerformanceReviewAdministrator == false
                        ? await _service.GetUsersForManager_CMS_DB(_allUsers_CMS_DB, _allUsers_OPS_DB, cmsUserID, _dbCMS)
                        : await _service.GetAllUsersExceptUserLoggedOn(_allUsers_CMS_DB, _allUsers_OPS_DB, cmsUserID);
        }

        private async Task InsertAndGetReview()
        {
            // NOTE: DOES NOT GET REVIEW STATUS - THAT IS A SEPARATE QUERY.

            if (_selectedReview == null)
            {
                int? reviewID = await _service.InsertReviewAndFirstStatusChange
                    (
                        _selectedReviewYear!.Value,
                        _opsReviewerID!.Value, _selectedUser!.OPS_UserID!.Value,
                        _cmsReviewerID!.Value, _selectedUser!.CMS_UserID!.Value,
                        ApplicationState!.LoggedOnUser!.LoginID!, _selectedUser.OPS_LoginID!,
                        Database_OPS
                    );

                if (reviewID == null)
                    throw new NoNullAllowedException("[reviewID] cannot be null in InsertAndGetReview().");
                else
                    _selectedReview = await _service.GetReviewByReviewID(reviewID!.Value, Database_OPS);
            }
        }

        private void RadioChoiceHandler(object? newValue)
        {
            if (newValue != null && int.TryParse(newValue.ToString(), out int selectedRadioChoiceID) == true)
            {
                RadioChoiceModel? radioChoice = _allRadioChoices?.FirstOrDefault(q => q?.RadioChoice_ID == selectedRadioChoiceID);
                PerformanceReviewQuestionModel? question = _questions?.FirstOrDefault(q => q?.Question_ID == radioChoice?.ReviewQuestion_ID);

                question!.Answer = radioChoice?.RadioChoice_Text;
                question!.RadioChoice_ID = radioChoice?.RadioChoice_ID;
                _areQuestionsDirty = true;
            }
        }

        private IEnumerable<RadioChoiceModel?>? RadioChoicesForQuestion(int? questionID)
        {
            if (questionID != null)
                return _allRadioChoices?.Where(q => q?.ReviewQuestion_ID == questionID).OrderBy(q => q?.RadioChoice_Sequence);
            else
                return null;
        }

        private void ReviewStatusChanged(string newValue)
        {
            _selectedNewReviewStatus = newValue;
            StateHasChanged();
        }

        private bool ReviewStatusEnabled() =>
            _areQuestionsDirty == false && AllQuestionsAnswered() == true && _selectedReview != null && _selectedReview.Status_ID_Type != Globals.ReviewStatusType.SentToHR;

        private string?[]? ReviewStatuses()
        {
            bool isAdministrator = ApplicationState!.LoggedOnUser!.IsPerformanceReviewAdministrator;
            IOrderedEnumerable<Globals.ReviewStatusType> keys = Globals.ReviewStatuses.Keys.Order();
            List<string?> reviewStatuses = [];

            foreach(Globals.ReviewStatusType key in keys)
            {
                if (isAdministrator == true || (int?)key > _selectedReview?.Status_ID)  // DON'T ALLOW MANAGERS TO BACKTRACK THE REVIEW STATUS, BUT ADMINISTRATORS CAN DO IT.
                {
                    string? status = Globals.ReviewStatuses[key];
                    reviewStatuses.Add(status);
                }
            }

            return [.. reviewStatuses];
        }

        private void ReviewStatusNotChangedWindowClicked()
        {
            _reviewNotStatusNotChangedWindow_Visible = false;
            StateHasChanged();
        }

        private void ReviewSubmittedWindowClicked()
        {
            _reviewSubmittedWindow_Visible = false;
            StateHasChanged();
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

        private async Task SaveAnswers()
        {
            foreach (PerformanceReviewQuestionModel? question in _questions!)
                if (question != null && question.Question_ID != null && string.IsNullOrWhiteSpace(question.Answer) == false)
                    await _service.UpsertPerformanceReviewAnswer_Main
                        (
                            _selectedReview!.ID!.Value,
                            question.Question_ID.Value, question.Answer,
                            _cmsReviewerID!.Value!, _opsReviewerID!.Value!, ApplicationState!.LoggedOnUser!.LoginID,
                            ApplicationState!.LoggedOnUser!.IsPerformanceReviewAdministrator,
                            Database_OPS
                        );

            _areQuestionsDirty = false;
        }

        private async Task SaveAnswersClicked()
        {
            if (_questions != null)
            {
                await SaveAnswers();
                _areQuestionsDirty = false;
                _answersSavedWindow_Visible = true;
                StateHasChanged();
            }
        }

        private bool SaveAnswersEnabled()
        {
            if (_selectedReview != null && _selectedReview.Status_ID_Type == Globals.ReviewStatusType.SentToHR)
                return false;
            if (ApplicationState!.LoggedOnUser!.IsPerformanceReviewAdministrator == true)
                return true;
            else
                return _selectedReview?.Status_ID_Type == Globals.ReviewStatusType.Pending && WasReviewStatusChanged() == false;
        }

        private async Task SubmitReviewClicked()
        {
            if (WasReviewStatusChanged() == true)
            {
                await _service.InsertReviewStatusChangeOnly
                    (
                        _selectedReview?.ID,
                        _opsReviewerID, _cmsReviewerID, ApplicationState!.LoggedOnUser!.LoginID,
                        Globals.ReviewStatuses[_selectedReview!.Status_ID_Type],
                        _selectedNewReviewStatus!, Database_OPS
                    );

                _selectedNewReviewStatus = null;
                _selectedReview = null;
                _selectedUser = null;
                _areQuestionsDirty = false;
                _reviewSubmittedWindow_Visible = true;
            }
            else
                _reviewNotStatusNotChangedWindow_Visible = true;

            StateHasChanged();
        }

        private bool SubmitReviewEnabled() =>
            _areQuestionsDirty == false && ((AllQuestionsAnswered() == true && string.IsNullOrWhiteSpace(_selectedNewReviewStatus) == false) || WasReviewStatusChanged() == true);

        private void TextBoxAreaChanged() => _areQuestionsDirty = true;

        private async Task UserForDropDownChanged(string newValue)
        {
            if (_questions != null)
            {
                _loading = true;
                StateHasChanged();

                await Task.Delay(2000);

                if (_answerFormats == null || _answerFormats.Count() == 0)
                    throw new NoNullAllowedException("_answerFormats cannot be null or empty in UserForManagerChanged() method..");
                else
                {
                    _selectedUser = _allUsersForDropDown?.FirstOrDefault(q => q?.FullName == newValue);

                    if (_selectedUser != null)
                    {
                        if (_selectedUser.OPS_UserID == null)
                            throw new NoNullAllowedException(".OPS_User_ID cannot be null in UserForManagerChanged() method.");
                        else
                        {
                            _headerInfo = await _service.GetEmployeeInformation(_selectedUser.OPS_UserID.Value, _selectedReviewYear!.Value, Database_OPS);
                            _selectedReview = await _service.GetReviewByReviewYearAndRevieweeID
                                (_selectedReviewYear!.Value, _selectedUser.OPS_UserID, _selectedUser.CMS_UserID, _selectedUser.OPS_LoginID, Database_OPS);
                            await InsertAndGetReview();
                            await GetCurrentReviewStatusByReviewID_Main();
                            _answers = (await _service.GetAnswersByReviewID(_selectedReview!.ID!.Value, Database_OPS))?.ToArray();

                            // ASSIGN ANSWER INFORMATION TO _questions ARRAY
                            if (_answers != null && _answers.Length > 0)
                            {
                                foreach (AnswersByReviewIdModel? answer in _answers)
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
                                            string? answerToUse = (string.IsNullOrWhiteSpace(answer.AdministratorAnswer) == false) ? answer.AdministratorAnswer : answer.ManagerAnswer;
                                            question.Answer = answerToUse;
                                            if (_answerFormats[question.AnswerFormat.Value] == Globals.RadioButtons)
                                                question.RadioChoice_ID = _allRadioChoices?.FirstOrDefault
                                                    (
                                                        q => q?.ReviewQuestion_ID == question.Question_ID &&
                                                        q?.RadioChoice_Text == answerToUse
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
                }
                _loading = false;
                StateHasChanged();
            }
        }

        private bool WasReviewStatusChanged() =>
            string.IsNullOrWhiteSpace(_selectedNewReviewStatus) == false &&
            _selectedNewReviewStatus.Equals(Globals.ReviewStatuses[_selectedReview!.Status_ID_Type], StringComparison.InvariantCultureIgnoreCase) == false;
    }
}
