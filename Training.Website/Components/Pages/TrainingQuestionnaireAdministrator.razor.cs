using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Runtime.CompilerServices;
using System.Text;
using Telerik.Blazor.Components;
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
        #endregion

        #region PRIVATE FIELDS
        private const string _yesNo = "Yes/No";
        private const string _trueFalse = "True/False";
        private const string _multipleChoice = "Multiple Choice";

        private Dictionary<int, string>? _answerFormats = null;
        private IEnumerable<string>? _sessions = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;

        private bool _currentQuestionControlsEnabled = false;
        private int? _currentQuestionIndex = null;
        private string? _currentQuestionText = null;
        private string? _currentAnswerFormat = null;
        private List<QuestionsModel>? _questions = null;

        private bool _addOrEditModeEnabled = false;

        private IEnumerable<AnswerChoicesModel>? _currentAnswerChoices = null;
        private IEnumerable<string>? _currentAnswerChoiceItems = null;
        private string? _currentCorrectAnswerChoiceItem = null;

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
            }
        }

        // ================================================================================================================================================================================================================================================================================================

        private async Task AnswerFormatChanged(string newValue)
        {
            await Task.Run(() => _currentAnswerFormat = newValue);
        }

        private bool AnswerFormatMatch(string? answerFormat, string answerType) =>
            answerFormat?.Equals(answerType, StringComparison.InvariantCultureIgnoreCase) ?? false;

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

        private async Task PopulateCorrectAnswerDropDown(int? questionID)
        {
            switch(_currentAnswerFormat)
            {
                case _multipleChoice:
                    _currentAnswerChoiceItems = await CommonServiceMethods.GetAnswerLettersByQuestionID(questionID!.Value, Database);
                    break;
                case _yesNo:
                    _currentAnswerChoiceItems = ["Yes", "No"];
                    break;
                case _trueFalse:
                    _currentAnswerChoiceItems = ["True", "False"];
                    break;
            }

            _currentCorrectAnswerChoiceItem = _questions?.FirstOrDefault(q => q.Question_ID == questionID)?.CorrectAnswer;
            if (_currentCorrectAnswerChoiceItem != null && _currentAnswerFormat == _multipleChoice)
                _currentCorrectAnswerChoiceItem = _currentCorrectAnswerChoiceItem.ToLower();
        }

        private async Task SessionChanged(string newValue)
        {
            _selectedSessionString = newValue;
            _selectedSession = ConvertSessionStringToClass(newValue);
            _questions = (await CommonServiceMethods.GetQuestionsBySessionID(_selectedSession!.Session_ID!.Value, Database))?.ToList();

            if (_questions != null && _questions.Count > 0)
            {
                _currentQuestionIndex = 0;
                _currentQuestionText = _questions![_currentQuestionIndex.Value].Question;
                SetCurrentAnswerFormat();
                await PopulateCorrectAnswerDropDown(_questions![_currentQuestionIndex.Value].Question_ID);
                StateHasChanged();
            }
            else
            {
                _currentQuestionIndex = null;
                _currentQuestionText = null;
                _currentAnswerFormat = null;
            }
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
            await PopulateCorrectAnswerDropDown(_questions![_currentQuestionIndex.Value].Question_ID);
            _currentAnswerChoices =
                CommonServiceMethods.GetAnswerChoicesByQuestionID(_questions![_currentQuestionIndex!.Value].Question_ID!.Value, Database);
        }
    }
}
