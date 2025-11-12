using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Runtime.CompilerServices;
using System.Text;
using Telerik.Blazor.Components;
using Telerik.SvgIcons;
using Training.Website.Models;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class TrainingQuestionnaireAdministrator
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database { get; set; }

        [Inject]
        private NavigationManager? NavigationManager { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private const string _yesNo = "Yes/No";
        private const string _trueFalse = "True/False";
        private const string _multipleChoice = "Multiple Choice";

        private Dictionary<int, string>? _answerFormats = null;
        private IEnumerable<string>? _sessions = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;

        //ivate bool _currentQuestionControlsEnabled = false;
        private int? _currentQuestionIndex = null;
        private string? _currentQuestionText = null;
        private string? _currentAnswerFormat = null;
        private bool _sessionHasQuestions;
        private List<QuestionsModel>? _questions = null;

        private bool _addMode = false;
        private bool _editMode = false;

        private List<AnswerChoicesModel>? _currentAnswerChoices = null;

        private IEnumerable<string>? _currentCorrectAnswerPossibilities = null;
        private string? _currentSelectedCorrectAnswer = null;

        #endregion

        protected override async Task OnInitializedAsync()
        {
            _answerFormats = await CommonServiceMethods.GetAnswerFormats(Database);

            // GET ALL SESSIONS
            IEnumerable<SessionInformationModel>? sessionInfo = await CommonServiceMethods.GetSessionInformation(Database);

            if (sessionInfo != null && sessionInfo.Count() > 0)
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

        private void AddAnswerChoiceClicked()
        {
            char letter = (char)('a' + (_currentAnswerChoices?.Count ?? 0));

            _currentAnswerChoices!.Add(new AnswerChoicesModel { AnswerLetter = letter, AnswerText = "" });
            StateHasChanged();
        }

        private bool AddOrEditMode() => _addMode == true || _editMode == true;

        private void AddQuestionClicked()
        {
            _addMode = true;
            _editMode = false;
            _currentQuestionIndex = null;
            _currentQuestionText = null;
            _currentAnswerFormat = null;
            _currentAnswerChoices = [];
            _currentCorrectAnswerPossibilities = [];
            _currentSelectedCorrectAnswer = null;
            _sessionHasQuestions = false;

            StateHasChanged();
        }

        private async Task AnswerFormatChanged(string newValue)
        {
            int? questionID = _addMode == false ? _questions?[_currentQuestionIndex!.Value].Question_ID : null;

            _currentAnswerFormat = newValue;
            _currentSelectedCorrectAnswer = string.Empty;
            await PopulateCorrectAnswerDropDown(questionID);
            StateHasChanged();
        }

        private bool AnswerFormatMatch(string? answerFormat, string answerType) =>
            answerFormat?.Equals(answerType, StringComparison.InvariantCultureIgnoreCase) ?? false;

        private async Task CancelButtonClicked()
        {
            string sessionID_String = ApplicationState!.SessionID_String ?? string.Empty;   // should never be null
            await SessionChanged(sessionID_String); // this is just a roundabout way of refreshing the page, since NavManager wasn't working ocrrectly.
        }

        private SessionInformationModel? ConvertSessionStringToClass(string newValue)
        {
            SessionInformationModel? result = null;
            int openParenthesis = newValue.IndexOf('(');

            if (openParenthesis > -1)
            {
                int closeParenthesis = newValue.LastIndexOf(')');

                if (closeParenthesis > -1 && (int.TryParse(newValue[..openParenthesis].Trim(), out int sessionId) == true))
                    result = new SessionInformationModel()
                    {
                        Session_ID = sessionId,
                        DocTitle = newValue.Substring(openParenthesis + 1, closeParenthesis - openParenthesis - 1).Trim()
                    };
            }

            return result;
        }

        private void CorrectAnswerChanged(string newValue)
        {
            _currentSelectedCorrectAnswer = newValue;
            StateHasChanged();
        }

        private async Task MoveDownButtonClicked()
        {
            if (_currentQuestionIndex < _questions?.Count - 1)
                _currentQuestionIndex++;

            await SetQuestionsControls();
            StateHasChanged();
        }

        private async Task MoveUpButtonClicked()
        {
            if (_currentQuestionIndex > 0)
                _currentQuestionIndex--;

            await SetQuestionsControls();
            StateHasChanged();
        }

        private bool OkToSave() =>
            string.IsNullOrWhiteSpace(_currentQuestionText) == false &&
            string.IsNullOrWhiteSpace(_currentAnswerFormat) == false &&
            string.IsNullOrWhiteSpace(_currentSelectedCorrectAnswer) == false;

        private async Task PopulateCorrectAnswerDropDown(int? questionID)
        {
            _currentCorrectAnswerPossibilities = [];

            switch(_currentAnswerFormat)
            {
                case _multipleChoice:
                    _currentCorrectAnswerPossibilities = await CommonServiceMethods.GetAnswerLettersByQuestionID(questionID!.Value, Database); // ?? [];
                    break;
                case _yesNo:
                    _currentCorrectAnswerPossibilities = ["Yes", "No"];
                    break;
                case _trueFalse:
                    _currentCorrectAnswerPossibilities = ["True", "False"];
                    break;
                case null:
                    _currentCorrectAnswerPossibilities = null;
                    break;
                default:
                    throw new Exception("Invalid current answer format in PopulateCorrectAnswerDropDown()");
            }

            _currentSelectedCorrectAnswer = (_addMode == false)
                ? _questions?.FirstOrDefault(q => q.Question_ID == questionID)?.CorrectAnswer
                : null;
        }

        private async Task SaveButtonClicked()
        {
            await Task.Delay(1);
            throw new NotImplementedException();
        }

        private async Task SessionChanged(string newValue)
        {
            ApplicationState!.SessionID_String = newValue;
            _addMode = false;
            _editMode = false;
            _selectedSessionString = newValue;
            _selectedSession = ConvertSessionStringToClass(newValue);
            _questions = (await CommonServiceMethods.GetQuestionsBySessionID(_selectedSession!.Session_ID!.Value, Database))?.ToList();
            _sessionHasQuestions = (_questions != null && _questions.Count > 0);

            if (_sessionHasQuestions == true)
            {
                _currentQuestionIndex = 0;
                _currentQuestionText = _questions![_currentQuestionIndex.Value].Question;
                SetCurrentAnswerFormat();
                await PopulateCorrectAnswerDropDown(_questions![_currentQuestionIndex.Value].Question_ID);
            }
            else
            {
                _currentQuestionIndex = null;
                _currentQuestionText = null;
                _currentAnswerFormat = null;
            }
            StateHasChanged();
        }

        private void SetCurrentAnswerFormat()
        {
            int? answerFormatID = _questions![_currentQuestionIndex!.Value].AnswerFormat;
            _currentAnswerFormat = (answerFormatID != null) ? _answerFormats?[answerFormatID.Value] : null;
        }

        private async Task SetQuestionsControls()
        {
            _currentQuestionText = _questions?[_currentQuestionIndex!.Value].Question;
            SetCurrentAnswerFormat();
            await PopulateCorrectAnswerDropDown(_questions![_currentQuestionIndex!.Value].Question_ID);
            _currentAnswerChoices =
                CommonServiceMethods.GetAnswerChoicesByQuestionID(_questions![_currentQuestionIndex!.Value].Question_ID!.Value, Database)?.ToList();
        }
    }
}
