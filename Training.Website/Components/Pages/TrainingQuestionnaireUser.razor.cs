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
            }
        }

        // ================================================================================================================================================================================================================================================================================================

        private bool AtFirstQuestion() => _currentQuestionIndex == 0;

        private bool AtLastQuestion() => _currentQuestionIndex == _questionIndexLimit;

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

        private async Task SessionChanged(string newValue)
        {
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            _questions = (await _service.GetQuestionsBySessionID(_selectedSession!.Session_ID!.Value, Database))?.ToArray();
            _currentQuestionIndex = 0;
            _questionIndexLimit = _questions?.GetUpperBound(0) ?? -1;
            SetCurrentFields_Main(0);
            StateHasChanged();
        }

        private void SetCurrentAnswerFormat() => _currentAnswerFormat = Globals.CurrentAnswerFormat(_answerFormats, _questions?[_currentQuestionIndex]);

        private void SetCurrentMultipleChoiceAnswers()
        {
            if (_currentAnswerFormat == Globals.MultipleChoice)
            {
                int? questionID = _questions?[_currentQuestionIndex]?.Question_ID;

                _currentMultipleChoiceAnswers = (questionID != null)
                    ? _service.GetAnswerChoicesByQuestionID(questionID.Value, Database)
                    : null;
            }
            else
                _currentMultipleChoiceAnswers = null;
        }

        private void SetCurrentFields_Main(int indexIncrement)
        {
            _currentQuestionIndex += indexIncrement;
            SetCurrentAnswerFormat();
            SetCurrentMultipleChoiceAnswers();
        }
    }
}
