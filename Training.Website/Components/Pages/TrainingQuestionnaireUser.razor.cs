using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Text;
using Telerik.Blazor.Components;
using Training.Website.Models;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class TrainingQuestionnaireUser
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database { get; set; }

        [Inject]
        private NavigationManager? NavManager { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private Dictionary<int, string>? _answerFormats = null;
        private IEnumerable<string>? _sessions_FullText = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;
        private int _currentQuestionnaireNumber = 0;
        private int _currentQuestionIndex;
        private int _questionIndexLimit;
        private QuestionsModel[]? _questions = null;
        private string? _currentAnswerFormat = null;
        private IEnumerable<AnswerChoicesModel?>? _currentMultipleChoiceAnswers = null;
        private IEnumerable<string?>? _currentAnswerChoices_DropDown = null;
        private string? _currentSelectedAnswer_DropDown = null;
        //private string?[]? _currentSelectedAnswers_DropDown = null;
        private UserAnswersModel?[]? _currentSelectedAnswers_DropDown = null;
        private double? _score = null;
        private DateTime? _whenMustRetakeTestBy = null;
        private EligibilityClass? _testEligibility = null;
        private int? _testAttemptID = null;
        private IEnumerable<UserResponsesModel?>? _userResponses = null;
        private readonly UserServiceMethods _service = new();
        private IEnumerable<string?>? _sessions_IDs = null;
        private string? _keypressedSessionID = null;
        private TelerikAutoComplete<string?> _sessionIdAutoComplete = new();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _answerFormats = await _service.GetAnswerFormats(Database);

            // GET ALL SESSIONS
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database);

            //TODO: THE CODE BELOW MAY NOT BE NEEDED IF THE SESSION ID WILL BE PASSED TO THIS PAGE VIA QUERYSTRING OR SOMEOTHER METHOD. IF IT IS NEEDED, THEN IT IS REDUNDNANT WITH THE ADMINISTRATOR PAGE AND A COMMON METHOD SHOULD BE IMPLEMENTED.
            if (sessionInfo != null && sessionInfo.Any() == true)
            {
                _sessions_FullText = Globals.ConcatenateSessionInfoForDropDown(sessionInfo);
                _sessions_IDs = sessionInfo.Select(q => q.Session_ID.ToString());
                _selectedSessionString = ApplicationState!.SessionID_String;
                if (string.IsNullOrWhiteSpace(_selectedSessionString) == false)
                    await SessionChanged(_selectedSessionString);
            }
        }

        // ================================================================================================================================================================================================================================================================================================

        private bool AtFirstQuestion() => _currentQuestionIndex == 0;

        private bool AtLastQuestion() => _currentQuestionIndex == _questionIndexLimit;

        private int CorrectAnswerCount()
        {
            int correctAnswerCount = 0;

            for (int index = 0; index <= _questionIndexLimit; index++)
                if (_currentSelectedAnswers_DropDown?[index]?.UserAnswer == _questions?[index].CorrectAnswer)
                    correctAnswerCount++;

            return correctAnswerCount;
        }

        private string? CorrectAnswerText(UserResponsesModel? response, bool correctAnswer)
        {
            if (response == null)
                return null;
            else
            {
                StringBuilder responseStatusText = new();

                if (correctAnswer == true)
                    responseStatusText.Append(' ', 3);
                else
                {
                    responseStatusText.Append($"The correct answer is: {response.CorrectAnswer}");
                    if (string.IsNullOrWhiteSpace(response.CorrectAnswerText) == false)
                        responseStatusText.Append($" ({response.CorrectAnswerText})");
                }

                return responseStatusText.ToString();
            }
        }

        private void CurrentAnswerChanged(string newValue)
        {
            _currentSelectedAnswer_DropDown = newValue;

            if (_currentSelectedAnswers_DropDown![_currentQuestionIndex] == null)
            {
                _currentSelectedAnswers_DropDown![_currentQuestionIndex] = new UserAnswersModel();
                _currentSelectedAnswers_DropDown![_currentQuestionIndex]!.QuestionID = _questions![_currentQuestionIndex].Question_ID!.Value;
            }

            _currentSelectedAnswers_DropDown![_currentQuestionIndex]!.UserAnswer = newValue;
        }

        private int GetCurrentQuestionnaireNumber()
        {
            if (_testEligibility == null)
                return 1;
            else if (_testEligibility.Finished == true)
                return _testEligibility.Count;
            else
                return _testEligibility.Count + 1;
        }

        private async Task<EligibilityClass> GetTestEligibility()
        {
            int previousAttempts = 0;
            string? message = null;
            bool noMore = false;
            bool wasUserAssignedThisQuestionnaire =
                await _service.WasUserAssignedQuestionnaire(_selectedSession!.Session_ID!.Value, ApplicationState!.LoggedOnUser!.AppUserID!.Value, Database);

            if (wasUserAssignedThisQuestionnaire == false)
                message = "You have not been assigned this questionnaire.";
            else
            {
                IEnumerable<ScoresAndWhenSubmittedModel>? scores =
                    await _service.GetScoresBySessionIDandUserID(_selectedSession!.Session_ID!.Value!, Globals.UserID(ApplicationState), Database);

                previousAttempts = scores?.Count() ?? 0;

                if (previousAttempts > 0)
                {
                    ScoresAndWhenSubmittedModel? passingScore = scores?.FirstOrDefault(q => q.Score >= Globals.TestPassingThreshold);

                    if (passingScore != null)
                    {
                        message = $"You already took this questionnaire on {passingScore.WhenSubmitted} and passed with a score of {passingScore.Score}%.";
                        noMore = true;
                    }
                    else if (previousAttempts >= Globals.MaximumTestAttemptsPerSession)
                    {
                        message = $"You have attempted this questionnaire the maximum number of times ({Globals.MaximumTestAttemptsPerSession}) without passing (minimum passing grade: {Globals.TestPassingThreshold}%).";
                        noMore = true;
                    }
                    else
                    {
                        DateTime? deadline = scores?.FirstOrDefault(q => q?.WhenMustRetakeBy != null && q?.WhenMustRetakeBy < DateTime.Now)?.WhenMustRetakeBy;

                        if (deadline != null)
                        {
                            message = $"The deadline to re-take this questionnaire expired on {deadline.Value}.";
                            noMore = true;
                        }
                    }
                }
            }

            return new EligibilityClass { Count = previousAttempts, Finished = noMore, Message = message, WasAssigned = wasUserAssignedThisQuestionnaire };
        }

        private void NextQuestionClicked()
        {
            if (AtLastQuestion() == false)
            {
                SetCurrentFields_Main(1);
                StateHasChanged();
            }
        }

        private async Task OnCloseSessionIdAutoComplete(AutoCompleteCloseEventArgs args)
        {
            if (_keypressedSessionID != null)
            {
                string? newValue = _sessions_FullText?.FirstOrDefault(q => q.StartsWith(_keypressedSessionID));

                if (newValue != null)
                    await SessionChanged(newValue);
            }
        }

        private void PreviousQuestionClicked()
        {
            if (AtFirstQuestion() == false)
            {
                SetCurrentFields_Main(-1);
                StateHasChanged();
            }
        }

        private int QuestionsAnswered() => _currentSelectedAnswers_DropDown?.Count(q => string.IsNullOrWhiteSpace(q?.UserAnswer) == false) ?? 0;

        private void CloseClicked() => NavManager?.NavigateTo("/");

        private double? Score()
        {
            if (_questions != null)
            {
                double questionCount = Convert.ToDouble(_questions?.Length);
                double correctAnswerCount = Convert.ToDouble(CorrectAnswerCount());

                return Math.Round(correctAnswerCount / questionCount, 2) * 100D;
            }
            else
                return null;
        }

        private async Task SessionChanged(string newValue)
        {
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            _testEligibility = await GetTestEligibility();
            _currentQuestionnaireNumber = GetCurrentQuestionnaireNumber();
            _questions = (await _service.GetQuestionsBySessionIDandQuestionnaireNumber(_selectedSession!.Session_ID!.Value, _currentQuestionnaireNumber, Database))?.ToArray();
            _currentSelectedAnswers_DropDown = new UserAnswersModel[_questions?.Length ?? 0];
            _currentQuestionIndex = 0;
            _questionIndexLimit = _questions?.GetUpperBound(0) ?? -1;
            _score = null;
            SetCurrentFields_Main(0);
            StateHasChanged();
        }

        private void SessionIdAutoCompleteValueChanged(string newValue) => _keypressedSessionID = newValue;

        private void SetCurrentAnswerDropDownItems()
        {
            if (_currentAnswerFormat == Globals.MultipleChoice)
            {
                int? questionID = _questions?[_currentQuestionIndex]?.Question_ID;

                _currentMultipleChoiceAnswers = (questionID != null)
                    ? _service.GetAnswerChoicesByQuestionID(questionID.Value, Database)
                    : null;
            }

            _currentAnswerChoices_DropDown = _currentAnswerFormat switch
            {
                Globals.MultipleChoice => _currentMultipleChoiceAnswers?.Select(q => q?.AnswerLetter.ToString()).Order(),
                Globals.TrueFalse => Globals.TrueFalse_Choices,
                Globals.YesNo => Globals.YesNo_Choices,
                _ => throw new Exception(Globals.CurrentAnswerFormatError)
            };
        }

        private void SetCurrentFields_Main(int indexIncrement)
        {
            _currentQuestionIndex += indexIncrement;
            _currentAnswerFormat = Globals.CurrentAnswerFormat(_answerFormats, _questions?[_currentQuestionIndex]);
            _currentSelectedAnswer_DropDown = _currentSelectedAnswers_DropDown?[_currentQuestionIndex]?.UserAnswer ?? null;
            SetCurrentAnswerDropDownItems();
        }

        private async Task SubmitClicked()
        {
            if (_questions == null)
                throw new Exception("LOGIC ERROR IN SubmitClicked(): {_questions} == null, which should not be happening in this method.");
            else if (_questions.Length == 0)
                throw new Exception("LOGIC ERROR IN SubmitClicked(): {questionCount} should never be zero in SubmitClicked().");
            else
            {
                _score = Score();
                if (_score != null)
                {
                    int previousAttempts =
                        (
                            await Database!.QueryByStoredProcedureAsync<int, object?>
                            (
                                "usp_Training_Questionnaire_GetCountOfTestAttemptsBySessionIDandUserID", new { Session_ID = _selectedSession!.Session_ID!.Value!, CMS_User_ID = Globals.UserID(ApplicationState) }
                            )
                        )?.First()
                        ?? 0;

                    int currentAttempt = previousAttempts + 1;

                    _whenMustRetakeTestBy = (currentAttempt < Globals.MaximumTestAttemptsPerSession) ? DateTime.Now.AddDays(Globals.RetakeTestDeadlineDays) : null;

                    _testAttemptID = await _service.InsertTestResult
                        (_selectedSession!.Session_ID!.Value, Globals.UserID(ApplicationState), _score.Value, currentAttempt, _whenMustRetakeTestBy, Database);

                    foreach (UserAnswersModel? userAnswer in _currentSelectedAnswers_DropDown!)
                        await _service.InsertIndividualAnswer(_testAttemptID!.Value, userAnswer, Database);
                }
                _testEligibility = await GetTestEligibility();
                _userResponses = (_score >= Globals.TestPassingThreshold) ? null : await _service.GetUserResponses(_testAttemptID!.Value, Database);
            }

            StateHasChanged();
        }

        private bool ShowUserResponses() => _testEligibility?.Finished == false && _userResponses != null;

        private string? UserAnswerText(UserResponsesModel? response)
        {
            if (response == null)
                return null;
            else
            {
                StringBuilder responseStatusText = new($"Your Answer: {response.UserAnswer}");

                if (string.IsNullOrWhiteSpace(response.UserAnswerText) == false)
                    responseStatusText.Append($" ({response.UserAnswerText})");
                else
                    responseStatusText.Append(' ', 3);

                return responseStatusText.ToString();
            }
        }
    }
}
