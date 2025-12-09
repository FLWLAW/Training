using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using System.IO;
using Telerik.Blazor.Components;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Flow.FormatProviders.Docx;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Styles;
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
        private IJSRuntime? JS { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private const string _windowWidth = "80%";
        private const string _windowLeft = "20%";
        private const string _topWindowTop = "7%";
        private const string _middleWindowTop = "27%";
        private const string _bottomWindowTop = "74%";

        private Dictionary<int, string>? _answerFormats = null;
        private IEnumerable<string>? _sessions_FullText = null;
        private IEnumerable<string?>? _sessions_IDs = null;
        private string? _selectedSessionString = null;
        private string? _keypressedSessionID = null;
        private SessionInformationModel? _selectedSession = null;

        private int? _currentQuestionnaireNumber = 1;
        private int? _newQuestionNumber = null;
        private int? _currentQuestionIndex = null;
        private string? _currentQuestionText = null;
        private string? _originalAnswerFormat = null;
        private string? _currentAnswerFormat = null;
        private bool _sessionHasQuestions;
        private List<QuestionsModel>? _questions = null;

        private bool _addMode = false;
        private bool _editMode = false;

        private bool _changingSession = false;

        private const int _maxMultipleChoices = 4;
        private AnswerChoicesModel?[]? _currentMultipleChoiceAnswers_DB = null;
        private IEnumerable<string?>? _currentAnswerChoices_DropDown = null;
        private string? _changedMultipleChoiceLetter = null;
        private string? _changedMultipleChoiceAnswer = null;
        private List<AnswerChoicesModel?> _changedMultipleChoiceAnswers = [];
        private string? _currentSelectedCorrectAnswer = null;

        private readonly AdministratorServiceMethods _service = new();

        private TelerikAutoComplete<string?> _sessionIdAutoComplete = new();

        #endregion

        protected override async Task OnInitializedAsync()
        {
            _answerFormats = await _service.GetAnswerFormats(Database);

            // GET ALL SESSIONS
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database);

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

        private bool AddOrEditMode() => _addMode == true || _editMode == true;

        private void AddQuestionClicked()
        {
            _addMode = true;
            _editMode = false;
            _newQuestionNumber = (_sessionHasQuestions == true) ? _questions?.Count + 1 : 1;
            _currentQuestionIndex = null;
            _currentQuestionText = null;
            _currentAnswerFormat = null;
            _currentMultipleChoiceAnswers_DB = [];
            _currentAnswerChoices_DropDown = [];
            _currentSelectedCorrectAnswer = null;

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

        private async Task CancelButtonClicked() => await RefreshPageFromBeginning();

        private async Task ChangedMultipleChoiceAnswerChanged()
        {
            await Task.Run(() =>
            {
                AnswerChoicesModel? changedQuestion = _currentMultipleChoiceAnswers_DB?.FirstOrDefault(q => q?.AnswerLetter.ToString() == _changedMultipleChoiceLetter);

                if (changedQuestion != null)
                {
                    char? changedMultipleChoiceLetter = _changedMultipleChoiceLetter?[0];

                    AnswerChoicesModel? remove = _changedMultipleChoiceAnswers.FirstOrDefault(q => q?.AnswerLetter == changedMultipleChoiceLetter);

                    if (remove != null)
                        _changedMultipleChoiceAnswers.Remove(remove);

                    AnswerChoicesModel multipleAnswerChoice = new()
                    {
                        Answer_ID = changedQuestion?.Answer_ID,
                        AnswerLetter = changedMultipleChoiceLetter,
                        AnswerText = _changedMultipleChoiceAnswer,
                        Question_ID = changedQuestion?.Question_ID
                    };

                    _changedMultipleChoiceAnswers.Add(multipleAnswerChoice);
                }
            });
        }

        private void ChangedMultipleChoiceLetterChanged(object newValue)
        {
            _changedMultipleChoiceLetter = newValue.ToString();
            _changedMultipleChoiceAnswer = null;
        }

        private void CorrectAnswerChanged(string newValue)
        {
            _currentSelectedCorrectAnswer = newValue;
            StateHasChanged();
        }

        private async Task CurrentQuestionnaireNumberChanged(object newValue)
        {
            _currentQuestionnaireNumber = (int?)newValue;
            await SessionChanged(_selectedSessionString!);
        }

        private void EditQuestionClicked()
        {
            _addMode = false;
            _editMode = true;
         
            StateHasChanged();
        }

        private async Task ExportToWordClicked()
        {
            // https://www.google.com/search?q=telerik+blazor+export+to+word&rlz=1C1GCEU_enUS1109US1109&oq=telerik+blazor+export+to+word&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIHCAEQIRigATIHCAIQIRigATIHCAMQIRigATIHCAQQIRigATIHCAUQIRigATIHCAYQIRirAjIHCAcQIRirAjIHCAgQIRifBTIHCAkQIRifBdIBCjEwNjI0ajBqMTWoAgiwAgHxBa1fxYxW5ZVS&sourceid=chrome&ie=UTF-8

            RadFlowDocument document = new();
            Section? section = document.Sections.AddSection();

            for (int questionnaireNumber = 1; questionnaireNumber <= Globals.MaximumTestAttemptsPerSession; questionnaireNumber++)
            {
                IEnumerable<QuestionsModel?>? questions = await _service.GetQuestionsBySessionIDandQuestionnaireNumber(_selectedSession!.Session_ID!.Value, questionnaireNumber, Database);

                if (questions != null)
                {
                    Paragraph? title = section.Blocks.AddParagraph();
                    title.TextAlignment = Alignment.Center;
                    title.Inlines.AddRun($"SESSION #{_selectedSession.Session_ID} {_selectedSessionString}").FontSize = 18;
                    
                    Paragraph? subtitle = section.Blocks.AddParagraph();
                    subtitle.TextAlignment = Alignment.Center;
                    Run qn = subtitle.Inlines.AddRun($"Questionnaire #{questionnaireNumber}");
                    qn.FontSize = 16;
                    qn.Underline.Pattern = UnderlinePattern.Single;

                    foreach (QuestionsModel? question in questions)
                    {
                        section.Blocks.AddParagraph().Inlines.AddRun($"{question!.QuestionNumber}.\t{question.Question}");
                        IEnumerable<AnswerChoicesModel?>? answerChoices = _service.GetAnswerChoicesByQuestionID(question.Question_ID!.Value, Database);
                    
                        if (answerChoices != null && answerChoices.Any() == true)
                            foreach (AnswerChoicesModel? answerChoice in answerChoices)
                                section.Blocks.AddParagraph().Inlines.AddRun($"\t{answerChoice!.AnswerLetter}.\t{answerChoice.AnswerText}");
                        section.Blocks.AddParagraph();
                        section.Blocks.AddParagraph().Inlines.AddRun($"Answer: \t{question.CorrectAnswer}");
                        section.Blocks.AddParagraph().Inlines.AddRun("----------------------------------------------------------------------------------------------------------------------------");
                    }
                    section.Blocks.AddParagraph().Inlines.AddRun("=====================================================================================");
                }
            }

            DocxFormatProvider formatProvider = new();

            using (MemoryStream ms = new ())
            {
                string filename = $"Questionnaire Export Session {_selectedSession!.Session_ID!.Value}.docx";

                formatProvider.Export(document, ms, null);

                ms.Position = 0;    // VERY IMPORTANT!!!!!
                using var streamRef = new DotNetStreamReference(stream: ms);
                await JS!.InvokeVoidAsync("downloadFileFromStream", filename, streamRef);
            }
        }

        private async Task<List<QuestionsModel>?> GetQuestionsBySessionIDandQuestionnaireNumber_Main() =>
            (await _service.GetQuestionsBySessionIDandQuestionnaireNumber(_selectedSession!.Session_ID!.Value, _currentQuestionnaireNumber!.Value, Database))?.ToList();


        private async Task InsertMultipleChoiceAnswers(QuestionsModel? question)
        {
            for (int index = 0; index < _maxMultipleChoices; index++)
            {
                AnswerChoicesModel? currentChoice = _currentMultipleChoiceAnswers_DB![index];

                if (currentChoice != null && string.IsNullOrWhiteSpace(currentChoice?.AnswerText) == false)
                {
                    await _service.InsertMultipleChoiceAnswer
                    (
                        question!.Question_ID!.Value,
                        currentChoice.AnswerLetter!.Value,
                        currentChoice.AnswerText!,
                        Globals.UserID(ApplicationState),
                        Database
                    );
                }
            }
        }

        private void InitializeMultipleChoiceCurrentAnswerTextAndLetters()
        {
            _currentMultipleChoiceAnswers_DB = new AnswerChoicesModel?[_maxMultipleChoices];
            _currentAnswerChoices_DropDown = [];

            for (int index = 0; index < _maxMultipleChoices; index++)
            {
                char letter = (char)('a' + index);

                _currentMultipleChoiceAnswers_DB[index] = new AnswerChoicesModel
                {
                    AnswerLetter = letter,
                    AnswerText = string.Empty
                };
                _currentAnswerChoices_DropDown = _currentAnswerChoices_DropDown!.Append(letter.ToString());
            }
        }

        private async Task<int> InsertNewQuestion(int answerFormatKey)
        {
            int questionID = await _service.InsertQuestion
            (
                _selectedSession!.Session_ID!.Value,
                _currentQuestionnaireNumber!.Value,
                _newQuestionNumber!.Value,
                _currentQuestionText!,
                answerFormatKey,
                _currentSelectedCorrectAnswer,
                Globals.UserID(ApplicationState),
                Database
            );

            return questionID;
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

        private bool OkToSave()
        {
            bool ok = string.IsNullOrWhiteSpace(_currentQuestionText) == false &&
                      string.IsNullOrWhiteSpace(_currentAnswerFormat) == false &&
                      string.IsNullOrWhiteSpace(_currentSelectedCorrectAnswer) == false;

            if (ok == true && _currentAnswerFormat == Globals.MultipleChoice)
                ok = _currentMultipleChoiceAnswers_DB != null && _currentMultipleChoiceAnswers_DB.All(q => string.IsNullOrWhiteSpace(q?.AnswerText) == false);

            return ok;
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

        private async Task PopulateCorrectAnswerDropDown(int? questionID)
        {
            _currentAnswerChoices_DropDown = [];

            switch(_currentAnswerFormat)
            {
                case Globals.MultipleChoice:
                    _currentAnswerChoices_DropDown = await _service.GetAnswerLettersByQuestionID(questionID!.Value, Database); // ?? [];
                    break;
                case Globals.YesNo:
                    _currentAnswerChoices_DropDown = Globals.YesNo_Choices;
                    break;
                case Globals.TrueFalse:
                    _currentAnswerChoices_DropDown = Globals.TrueFalse_Choices;
                    break;
                case null:
                    _currentAnswerChoices_DropDown = [];
                    break;
                default:
                    throw new Exception(Globals.CurrentAnswerFormatError);
            }

            _currentSelectedCorrectAnswer = (_addMode == false)
                ? _questions?.FirstOrDefault(q => q.Question_ID == questionID)?.CorrectAnswer
                : null;
        }

        private async Task RefreshPageFromBeginning()
        {
            string sessionID_String = ApplicationState!.SessionID_String ?? string.Empty;   // should never be null
            await SessionChanged(sessionID_String); // this is just a roundabout way of refreshing the page, since NavManager wasn't working ocrrectly.
        }

        private async Task RemoveQuestionClicked()
        {
            int? id = _questions?[_currentQuestionIndex!.Value].Question_ID;

            if (id != null)
            {
                await _service.DeleteQuestionByQuestionID(id.Value, Database);
                await RenumberQuestions();
                await RefreshPageFromBeginning();
            }
        }

        private async Task RenumberQuestions()
        {
            IEnumerable<QuestionsModel>? questions = await GetQuestionsBySessionIDandQuestionnaireNumber_Main();

            if (questions != null)
            {
                int correctQuestionNumber = 1;

                foreach (QuestionsModel? question in questions)
                {
                    if (question?.QuestionNumber != correctQuestionNumber)
                        await _service.UpdateQuestion_QuestionNumberOnly(question!.Question_ID!.Value, correctQuestionNumber, Database);

                    correctQuestionNumber++;
                }

                _questions = await GetQuestionsBySessionIDandQuestionnaireNumber_Main();
            }
        }

        private async Task SaveButtonClicked()
        {
            KeyValuePair<int, string> selectedAnswerFormat =
                _answerFormats!.FirstOrDefault(x => x.Value == _currentAnswerFormat);

            if (_addMode == true)
            {
                int questionID = await InsertNewQuestion(selectedAnswerFormat.Key);
                QuestionsModel? newestQuestion = await _service.GetQuestionByQuestionID(questionID, Database);

                if (newestQuestion != null)
                {
                    _questions!.Add(newestQuestion!);
                    _sessionHasQuestions = true;
                    _newQuestionNumber = newestQuestion.QuestionNumber + 1;

                    if (selectedAnswerFormat.Value == Globals.MultipleChoice)
                        await InsertMultipleChoiceAnswers(newestQuestion);
                }
            }
            else if (_editMode == true)
            {
                int index = _currentQuestionIndex ?? -1;

                if (index >= 0)
                {
                    QuestionsModel? question = _questions![index];

                    if (question != null)
                    {
                        bool questionChanged = question.Question != _currentQuestionText ||
                                               _originalAnswerFormat != _currentAnswerFormat ||
                                               string.IsNullOrWhiteSpace(_changedMultipleChoiceLetter) == true ||
                                               question.CorrectAnswer != _currentSelectedCorrectAnswer;

                        bool multipleChoiceAnswersChanged = _changedMultipleChoiceAnswers.Count > 0;
                        bool formerlyMultipleChoice = _originalAnswerFormat == Globals.MultipleChoice && _currentAnswerFormat != Globals.MultipleChoice;

                        if (questionChanged == true)
                            await _service.UpdateQuestion
                                (question.Question_ID!.Value, _currentQuestionText!, selectedAnswerFormat.Key, _currentSelectedCorrectAnswer!, Globals.UserID(ApplicationState), Database);

                        if (formerlyMultipleChoice == true)
                            await _service.DeleteAnswerChoicesByQuestionID(question.Question_ID!.Value, Database);
                        else if (multipleChoiceAnswersChanged == true)
                            foreach (AnswerChoicesModel? newAnswer in _changedMultipleChoiceAnswers)
                                if (newAnswer != null)
                                    await _service.UpdateMultipleChoiceAnswer(newAnswer.Answer_ID!.Value, newAnswer.AnswerLetter!.Value, newAnswer.AnswerText!, Globals.UserID(ApplicationState), Database);

                        if (questionChanged == true || formerlyMultipleChoice == true || multipleChoiceAnswersChanged == true)
                        {
                            question.Question = _currentQuestionText;
                            question.AnswerFormat = selectedAnswerFormat.Key;
                            question.CorrectAnswer = _currentSelectedCorrectAnswer;
                        }
                    }
                }
            }

            _currentQuestionText = null;
            _currentAnswerFormat = null;
            _currentAnswerChoices_DropDown = [];
            _currentMultipleChoiceAnswers_DB = [];
            _currentSelectedCorrectAnswer = null;
            _changedMultipleChoiceAnswers = [];

            StateHasChanged();
        }

        private async Task SessionChanged(string newValue)
        {
            _changingSession = true;
            ApplicationState!.SessionID_String = newValue;
            _addMode = false;
            _editMode = false;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            _sessionHasQuestions = false;   // THIS WILL PREVENT ERRORS IN THE NEXT STATEMENT, BECAUSE THE SCREEN WILL RENDER BEFORE THE AWAIT COMPLETES.
            _questions = await GetQuestionsBySessionIDandQuestionnaireNumber_Main();
            _sessionHasQuestions = SessionHasQuestions();

            if (_sessionHasQuestions == true)
            {
                _currentQuestionIndex = 0;
                _currentQuestionText = _questions![_currentQuestionIndex.Value].Question;
                _currentAnswerFormat = Globals.CurrentAnswerFormat(_answerFormats, _questions[_currentQuestionIndex!.Value]);
                _originalAnswerFormat = _currentAnswerFormat;
                await PopulateCorrectAnswerDropDown(_questions![_currentQuestionIndex.Value].Question_ID);
            }
            else
            {
                _currentQuestionIndex = null;
                _currentQuestionText = null;
                _currentAnswerFormat = null;
                _originalAnswerFormat = null;
            }

            if (_keypressedSessionID != _selectedSession?.Session_ID.ToString())
                _keypressedSessionID = _selectedSession?.Session_ID.ToString();

            _changedMultipleChoiceLetter = null;
            _changedMultipleChoiceAnswer = null;
            _changedMultipleChoiceAnswers = [];

            _changingSession = false;
            StateHasChanged();
        }

        private bool SessionHasQuestions() => (_questions != null && _questions.Count > 0);

        private void SessionIdAutoCompleteValueChanged(string newValue) => _keypressedSessionID = newValue;

        private async Task SetQuestionsControls()
        {
            int index = _currentQuestionIndex ?? -1;

            if (index >= 0)
            {
                _currentQuestionText = _questions?[index].Question;
                _currentAnswerFormat = Globals.CurrentAnswerFormat(_answerFormats, _questions?[_currentQuestionIndex!.Value]);
                _originalAnswerFormat = _currentAnswerFormat;
                await PopulateCorrectAnswerDropDown(_questions![index].Question_ID);
                _currentMultipleChoiceAnswers_DB =
                    _service.GetAnswerChoicesByQuestionID(_questions![index].Question_ID!.Value, Database)?.ToArray();
            }
        }
    }
}
