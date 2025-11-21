using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
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
        #endregion

        #region PRIVATE FIELDS
        private Dictionary<int, string>? _answerFormats = null;
        private IEnumerable<string>? _sessions = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;
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
        private string? _testEligibilityMessage = null;

        private readonly UserServiceMethods _service = new();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _answerFormats = await _service.GetAnswerFormats(Database);

            // GET ALL SESSIONS
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database);

            //TODO: THE CODE BELOW MAY NOT BE NEEDED IF THE SESSION ID WILL BE PASSED TO THIS PAGE VIA QUERYSTRING OR SOMEOTHER METHOD. IF IT IS NEEDED, THEN IT IS REDUNDNANT WITH THE ADMINISTRATOR PAGE AND A COMMON METHOD SHOULD BE IMPLEMENTED.
            if (sessionInfo != null && sessionInfo.Any() == true)
            {
                List<string>? sessions = [];

                foreach (SessionInformationModel? session in sessionInfo)
                {
                    string item = new($"{session.Session_ID} ({session.DocTitle})");
                    sessions.Add(item);
                }
                _sessions = sessions;
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

        private void NextQuestionClicked()
        {
            if (AtLastQuestion() == false)
            {
                SetCurrentFields_Main(1);
                StateHasChanged();
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
            _testEligibilityMessage = await TestEligibilityMessage();
            _questions = (await _service.GetQuestionsBySessionID(_selectedSession!.Session_ID!.Value, Database))?.ToArray();
            _currentSelectedAnswers_DropDown = new UserAnswersModel[_questions?.Length ?? 0];
            _currentQuestionIndex = 0;
            _questionIndexLimit = _questions?.GetUpperBound(0) ?? -1;
            _score = null;
            SetCurrentFields_Main(0);
            StateHasChanged();
        }

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
                    int testAttemptID = await _service.InsertTestResult
                        (_selectedSession!.Session_ID!.Value, Globals.UserID(ApplicationState), _score.Value, Database);

                    foreach (UserAnswersModel? userAnswer in _currentSelectedAnswers_DropDown!)
                        await _service.InsertIndividualAnswer(testAttemptID, userAnswer, Database);
                }
            }
        }

        private async Task<string?> TestEligibilityMessage()
        {
            IEnumerable<double>? scores = await _service.GetScoresBySessionIDandUserID(_selectedSession!.Session_ID!.Value!, Globals.UserID(ApplicationState), Database);
            int passes = scores?.Where(q => q >= Globals.TestPassingThreshold).Count() ?? 0;

            if (passes > 0)
                return "You have already taken this questionnaire and passed.";
            else
            {
                int attempts = scores?.Count() ?? 0;

                if (attempts < Globals.MaximumTestAttemptsPerSession)
                    return null;
                else
                    return $"You have attempted this questionnaire the maximum number of times ({Globals.MaximumTestAttemptsPerSession}) without passing (minimum passing grade: {Globals.TestPassingThreshold}%).";
            }
        }
    }
}
