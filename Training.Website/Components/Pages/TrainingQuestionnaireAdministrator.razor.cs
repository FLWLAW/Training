using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using System.Runtime.CompilerServices;
using System.Text;
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

        private IEnumerable<QuestionsModel>? _questions = null;

        private IEnumerable<AnswerChoicesModel>? _answerChoices = null;
        
        
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _answerFormats = await CommonServiceMethods.GetAnswerFormats(Database);

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

        private async Task SessionChanged(string newValue)
        {
            _selectedSessionString = newValue;
            _selectedSession = ConvertSessionStringToClass(newValue);
            _questions = await CommonServiceMethods.GetQuestionsBySessionID(_selectedSession!.Session_ID!.Value, Database);

            if (_questions != null)
            {
                List<AnswerChoicesModel> answerChoices = [];

                foreach (QuestionsModel? question in _questions)
                {
                    /*
                    IEnumerable<AnswerChoicesModel>? answerChoicesOneQuestion =
                        GetAnswerChoicesByQuestionID(question!.Question_ID!.Value, Database);

                    if (answerChoicesOneQuestion != null)
                        answerChoices.AddRange(answerChoicesOneQuestion);
                    */
                }

                _answerChoices = answerChoices;
                StateHasChanged();
            }
        }
    }
}
