using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Threading.Tasks;
using Telerik.Blazor.Components;
using Training.Website.Models;
using Training.Website.Models.Reviews;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class PerformanceReviewAdministratorPage
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database_OPS { get; set; }
        /*
        [Inject]
        private IJSRuntime? JS { get; set; }
        */
        #endregion

        #region PRIVATE CONSTANTS
        private const int _UP = -1;
        private const int _DOWN = 1;
        #endregion

        #region PRIVATE FIELDS
        private int? _selectedReviewYear = null;
        private string? _selectedAnswerFormatDropDownValue = null;
        private string[]? _reviewYears = null;
        private Dictionary<int, string>? _answerFormats = null;
        private List<PerformanceReviewQuestionModel?>? _activeQuestions = null;
        private List<PerformanceReviewQuestionModel?>? _deletedQuestions = null;
        private PerformanceReviewServiceMethods _service = new();
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _reviewYears = ReviewYears();
            _answerFormats = await _service.GetAnswerFormats_PerformanceReview(Database_OPS);
        }

        // =========================================================================================================================================================================================================================================================================================================

        private async Task ActiveQuestionCreateHandler(GridCommandEventArgs args)
        {
            if (_selectedAnswerFormatDropDownValue != null)
            {
                string? newQuestionText = (ConvertToModel(args))?.Question;

                if (newQuestionText != null)
                {
                    int? newAnswerFormat = null;
                    foreach (KeyValuePair<int, string> kvp in _answerFormats!)
                    {
                        if (kvp.Value == _selectedAnswerFormatDropDownValue)
                        {
                            newAnswerFormat = kvp.Key;
                            break;
                        }
                    }

                    if (newAnswerFormat == null)
                        throw new NoNullAllowedException("[newAnswerFormat] cannot be NULL in ActiveQuestionCreateHandler().");
                    else
                    {
                        _activeQuestions ??= [];
                        int? newQuestionNumber = (_activeQuestions.Count == 0) ? 1 : _activeQuestions.Max(q => q?.QuestionNumber) + 1;
                        await _service.InsertNewQuestion(_selectedReviewYear!.Value, newQuestionNumber!.Value, newQuestionText.Trim(), newAnswerFormat.Value, Database_OPS);
                        await GetAllQuestions();
                        StateHasChanged();
                    }
                }
            }
        }

        private async Task ActiveQuestionDeleteHandler(GridCommandEventArgs args) => await DeleteRestoreMainHandler(args, true);

        private async Task ActiveQuestionUpdateHandler(GridCommandEventArgs args)
        {
            PerformanceReviewQuestionModel? updatedQuestion = ConvertToModel(args);

            if (updatedQuestion != null)
            {
                bool questionChanged = await UpdateQuestion_IfChanged(updatedQuestion);
                bool answerFormatChanged = await UpdateAnswerFormat_IfChanged(updatedQuestion);

                if (questionChanged == true || answerFormatChanged == true)
                {
                    await GetAllQuestions();
                    StateHasChanged();
                }
            }
        }

        private PerformanceReviewQuestionModel? ConvertToModel(GridCommandEventArgs args) => (PerformanceReviewQuestionModel?)args.Item;

        private async Task GetAllQuestions()
        {
            if (_selectedReviewYear != null)
            {
                _activeQuestions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, false, Database_OPS))?.ToList();
                _deletedQuestions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, true, Database_OPS))?.ToList();
            }
        }

        private async Task DeleteRestoreMainHandler(GridCommandEventArgs args, bool newDeletionStatus)
        {
            PerformanceReviewQuestionModel? questionToModify = ConvertToModel(args);

            if (questionToModify == null)
                throw new NoNullAllowedException("[questionToModify] not allowed in DeleteRestoreMainHandler().");
            else
            {
                await _service.UpdateDeletedStatus(questionToModify.Question_ID!.Value, newDeletionStatus, Database_OPS);
                await GetAllQuestions();
                await ResequenceQuestions();
                await GetAllQuestions();

                StateHasChanged();
            }
        }

        private async Task MoveDownHandler(GridCommandEventArgs args) => await MoveHandlerMain(args, _DOWN);

        private async Task MoveHandlerMain(GridCommandEventArgs args, int moveIncrement)
        {
            if (_activeQuestions == null)
                throw new NoNullAllowedException("[_activeQuestions] cannot be null in MoveHandlerMain().");
            else
            {
                PerformanceReviewQuestionModel? question1 = ConvertToModel(args);
                int highestQuestionNumber = _activeQuestions.Count;

                if (question1 != null && ((moveIncrement == _UP && question1.QuestionNumber > 1) || (moveIncrement == _DOWN && question1.QuestionNumber < highestQuestionNumber)))
                {
                    int secondQuestionNumber = question1.QuestionNumber.Value + moveIncrement;
                    PerformanceReviewQuestionModel? question2 = _activeQuestions.FirstOrDefault(q => q?.QuestionNumber == secondQuestionNumber);

                    if (question2 != null)
                    {
                        int firstQuestionNumber = question1.QuestionNumber.Value;

                        await _service.UpdateQuestionNumber(question1.Question_ID!.Value, secondQuestionNumber, Database_OPS);
                        await _service.UpdateQuestionNumber(question2.Question_ID!.Value, firstQuestionNumber, Database_OPS);
                        await GetAllQuestions();

                        StateHasChanged();
                    }
                }
            }
        }
        private async Task MoveUpHandler(GridCommandEventArgs args) => await MoveHandlerMain(args, _UP);

        private async Task ResequenceQuestions()
        {
            if (_activeQuestions == null)
                throw new NoNullAllowedException("[_activeQuestions] cannot be NULL in ResequenceQuestions().");
            else
            {
                int questionNumberShouldBe = 1;

                _activeQuestions = _activeQuestions?.OrderBy(q => q?.QuestionNumber).ThenBy(q => q?.Question_ID).ToList();

                foreach (PerformanceReviewQuestionModel? activeQuestion in _activeQuestions!)
                {
                    if (activeQuestion != null && activeQuestion!.QuestionNumber != questionNumberShouldBe)
                        await _service.UpdateQuestionNumber(activeQuestion.Question_ID!.Value, questionNumberShouldBe, Database_OPS);

                    questionNumberShouldBe++;
                }
            }
        }

        private async Task RestoreHandler(GridCommandEventArgs args) => await DeleteRestoreMainHandler(args, false);

        private async Task ReviewYearChanged(string newValue)
        {
            if (int.TryParse(newValue, out int selectedReviewYear) == true)
            {
                _selectedReviewYear = selectedReviewYear;
                await GetAllQuestions();
                /*
                _allRadioChoices = (await _service.GetAllRadioButtonChoicesByYear(_selectedReviewYear.Value, Database_OPS))?.ToArray();
                _selectedUser = null;
                _headerInfo = null;
                _answers = null;
                _showChangeStatusReminder = false;
                _showMustClickSubmitReviewReminder = false;
                */
                StateHasChanged();
            }
        }

        private string[]? ReviewYears()
        {
            // FOR POPULATING "REVIEW YEAR" DROPDOWN

            List<string> reviewYears = [];

            for (int year = Globals.FirstReviewYear; year <= DateTime.Now.Year; year++)
                reviewYears.Add(year.ToString());

            return [.. reviewYears];
        }

        private async Task<bool> UpdateAnswerFormat_IfChanged(PerformanceReviewQuestionModel? updatedQuestion)
        {
            bool dataHasChanged = false;

            if (string.IsNullOrWhiteSpace(_selectedAnswerFormatDropDownValue) == false)     // WILL BE null IF UNCHANGED
            {
                foreach (KeyValuePair<int, string> kvp in _answerFormats!)
                {
                    if (kvp.Value == _selectedAnswerFormatDropDownValue)
                    {
                        // TODO: ADD ANSWER FORMAT QUESTIONS - THAT WILL TAKE A WHILE
                        await _service.UpdateAnswerFormat(updatedQuestion!.Question_ID!.Value, kvp.Key, Database_OPS);
                        dataHasChanged = true;
                        break;
                    }
                }
                _selectedAnswerFormatDropDownValue = null;
            }

            return dataHasChanged;
        }

        private async Task<bool> UpdateQuestion_IfChanged(PerformanceReviewQuestionModel? updatedQuestion)
        {
            int? updatedQuestionID = updatedQuestion?.Question_ID;

            if (updatedQuestionID == null)
                throw new NoNullAllowedException("[updatedQuestionID] cannot be NULL in UpdateQuestion()");
            else
            {
                string? oldQuestion = _activeQuestions!.FirstOrDefault(q => q?.Question_ID == updatedQuestionID)?.Question?.Trim();
                string? newQuestion = updatedQuestion!.Question?.Trim();
                bool dataHasChanged = string.IsNullOrWhiteSpace(newQuestion) == false && oldQuestion!.Equals(newQuestion, StringComparison.InvariantCultureIgnoreCase) == false;

                if (dataHasChanged == true)
                    await _service.UpdateQuestion(updatedQuestionID.Value, newQuestion, Database_OPS);

                return dataHasChanged;
            }
        }
    }
}
