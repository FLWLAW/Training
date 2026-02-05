using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Threading.Tasks;
using Telerik.Blazor.Components;
using Training.Website.Models;
using Training.Website.Models.Reviews;
using Training.Website.Services;

// TODO: IMPORTANT - IN PRODUCTION, REMOVE CURRENT PK FROM [PERFORMANCE Review Radio Button Choices Tbl] (ReviewQuestion_ID, RadioChoice_Sequence), AND MAKE NEW PK (RadioChoice_ID) ONLY. UPDATE ALL CODE ACCORDINGLY.


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
        private bool _radioButtonScreenVisible = false;
        private int? _selectedReviewYear = null;
        private string? _selectedAnswerFormatDropDownValue = null;
        private string? _newRadioButtonText = null;
        private string[]? _reviewYears = null;
        private Dictionary<int, string>? _answerFormats = null;
        private List<PerformanceReviewQuestionModel?>? _activeQuestions = null;     // TODO: MAKE IEnumerable?
        private List<PerformanceReviewQuestionModel?>? _deletedQuestions = null;    // TODO: MAKE IEnumerable?
        private List<RadioChoiceModel?>? _allRadioChoices_Active = null;
        private List<RadioChoiceModel?>? _allRadioChoices_Active_Original = null;
        private List<RadioChoiceModel?>? _allRadioChoices_Active_ToEdit = null;
        private IEnumerable<RadioChoiceModel?>? _allRadioChoices_Deleted = null;
        private PerformanceReviewQuestionModel? _questionWithRadioButtonsToEdit = null;
        private PerformanceReviewServiceMethods _service = new();
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _reviewYears = ReviewYears();
            _answerFormats = await _service.GetAnswerFormats_PerformanceReview(Database_OPS);
        }

        // =========================================================================================================================================================================================================================================================================================================

        private void ActiveQuestionCancelAllChangesForOneQuestionHandler(GridCommandEventArgs args)
        {
            if (_allRadioChoices_Active != null)
            {
                int? questionToCancelID = ConvertToModel(args)?.Question_ID;

                foreach (RadioChoiceModel? activeRadioChoice in _allRadioChoices_Active)
                {
                    // TODO: FINISH CODING HERE
                }
            }

        }

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
                        //TODO: CODE FOR RADIO BUTTONS HERE
                        StateHasChanged();
                    }
                }
            }
        }

        private async Task ActiveQuestionDeleteHandler(GridCommandEventArgs args) => await DeleteRestoreMainHandler(args, true);

        private async Task ActiveQuestionUpdateHandler(GridCommandEventArgs args)
        {
            _questionWithRadioButtonsToEdit = null;

            PerformanceReviewQuestionModel? questionToUpdate = ConvertToModel(args);

            if (questionToUpdate != null)
            {
                bool questionTextChanged = await UpdateQuestion_IfChanged(questionToUpdate);
                int? newAnswerFormat = await UpdateAnswerFormat_IfChanged(questionToUpdate);
                //bool wasOldAnswerFormatRadioButtons = _answerFormats?[questionToUpdate.AnswerFormat!.Value] == Globals.RadioButtons;
                //bool isNewAnswerFormatRadioButtons = newAnswerFormat != null && _answerFormats?[newAnswerFormat.Value] == Globals.RadioButtons;

                if (questionTextChanged == true || newAnswerFormat != null)
                {
                    await GetAllQuestions();
                    //_radioButtonScreenVisible = wasOldAnswerFormatRadioButtons == true || isNewAnswerFormatRadioButtons == true;
                    //_questionWithRadioButtonsToEdit = _radioButtonScreenVisible == true ? _activeQuestions!.FirstOrDefault(q => q?.Question_ID == questionToUpdate.Question_ID) : null;
                    StateHasChanged();
                }
            }
        }

        private void CloseRadioButtonEditScreen()
        {
            if (_allRadioChoices_Active != null && _allRadioChoices_Active_ToEdit != null)
            {
                for (int major = 0; major < _allRadioChoices_Active?.Count; major++)
                {
                    for (int minor = 0; minor < _allRadioChoices_Active_ToEdit?.Count; minor++)
                    {
                        if (_allRadioChoices_Active_ToEdit[minor]?.RadioChoice_ID == _allRadioChoices_Active[major]?.RadioChoice_ID)
                        {
                            if (_allRadioChoices_Active[major]?.RadioChoice_Text != _allRadioChoices_Active_ToEdit[minor]?.RadioChoice_Text)
                                _allRadioChoices_Active[major]!.RadioChoice_Text = _allRadioChoices_Active_ToEdit[minor]?.RadioChoice_Text;

                            if (_allRadioChoices_Active[major]?.RadioChoice_Sequence != _allRadioChoices_Active_ToEdit[minor]?.RadioChoice_Sequence)
                                _allRadioChoices_Active[major]!.RadioChoice_Sequence = _allRadioChoices_Active_ToEdit[minor]?.RadioChoice_Sequence;
                        }
                    }
                }
            }

            _radioButtonScreenVisible = false;
            _questionWithRadioButtonsToEdit = null;
            _allRadioChoices_Active_ToEdit = null;
            StateHasChanged();
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

        private bool EnableEditRadioButton(object context)
        {
            if (_answerFormats == null)
                return false;
            else
            {
                // TODO: CHANGE STATEMENTS WITH KEYVALUEPAIR TO USE .ContainsKey() FUNCTION??

                var dataItem = (PerformanceReviewQuestionModel?)context;

                if (dataItem == null || dataItem.AnswerFormat != 1)
                    return false;
                else
                    return true;
            }
        }

        private async Task QuestionMoveDownHandler(GridCommandEventArgs args) => await QuestionMoveHandlerMain(args, _DOWN);

        private async Task QuestionMoveHandlerMain(GridCommandEventArgs args, int moveIncrement)
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
        private async Task QuestionMoveUpHandler(GridCommandEventArgs args) => await QuestionMoveHandlerMain(args, _UP);

        /*
        private void RadioButtonTextBlur(object? args, RadioChoiceModel? possiblyChangedItem)
        {
            // IF TAB IS PRESSED OR ANOTHER CONTROL IS CLICKED ON FIRST, THIS METHOD FIRES FIRST.
            // IF ENTER IS PRESSED, RadioButtonUpdateHandler FIRES FIRST, THEN THIS FIRES.

            if (possiblyChangedItem != null && _allRadioChoices_Active_ToEdit != null)
            {
                for (int index = 0; index < _allRadioChoices_Active_ToEdit.Count; index++)
                {
                    if
                        (
                            _allRadioChoices_Active_ToEdit[index] != null &&
                            _allRadioChoices_Active_ToEdit[index]!.RadioChoice_ID == possiblyChangedItem.RadioChoice_ID &&
                            _allRadioChoices_Active_ToEdit[index]!.RadioChoice_Text != possiblyChangedItem.RadioChoice_Text
                        )
                    {
                        _allRadioChoices_Active_ToEdit[index]!.RadioChoice_Text = possiblyChangedItem.RadioChoice_Text;
                        //_allRadioChoices_Active_ToEdit[index]!.Changed = true;
                    }
                    //else
                        //_allRadioChoices_Active_ToEdit[index]!.Changed = false;
                }
            }
        }
        */

        private void RadioButtonTextChanged(string? newValue, RadioChoiceModel? item)
        {
            if (string.IsNullOrWhiteSpace(newValue) == false && item != null)
            {
                item.RadioChoice_Text = newValue;
                StateHasChanged();
            }
        }

        private async Task RadioButtonMoveDownHandler(GridCommandEventArgs args)
        {
            await Task.Delay(1);
            //throw new NotImplementedException();
        }

        private async Task RadioButtonMoveUpHandler(GridCommandEventArgs args)
        {
            await Task.Delay(1);
            //throw new NotImplementedException();
        }

        private async Task RadioButtonDeleteHandler(GridCommandEventArgs args)
        {
            await Task.Delay(1);
            //throw new NotImplementedException();
        }

        private void RadioButtonUpdateHandler(GridCommandEventArgs args)
        {
            var possiblyChangedItem = (RadioChoiceModel?)args.Item;

            if (possiblyChangedItem != null && _allRadioChoices_Active_ToEdit != null)
            {
                for (int index = 0; index < _allRadioChoices_Active_ToEdit.Count; index++)
                {
                    if
                        (
                            _allRadioChoices_Active_ToEdit[index] != null &&
                            _allRadioChoices_Active_ToEdit[index]!.RadioChoice_ID == possiblyChangedItem.RadioChoice_ID &&
                            _allRadioChoices_Active_ToEdit[index]!.RadioChoice_Text != possiblyChangedItem.RadioChoice_Text
                        )
                    {
                        _allRadioChoices_Active_ToEdit[index]!.RadioChoice_Text = possiblyChangedItem.RadioChoice_Text;
                        //_allRadioChoices_Active_ToEdit[index]!.Changed = true;
                    }
                    //else
                    //_allRadioChoices_Active_ToEdit[index]!.Changed = false;
                }
            }
        }

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
                _allRadioChoices_Active_Original = (await _service.GetAllRadioButtonChoicesByYearAndDeletedStatus(_selectedReviewYear.Value, false, Database_OPS))?.ToList();
                _allRadioChoices_Active = [];
                foreach(RadioChoiceModel? originalRadioChoice in _allRadioChoices_Active_Original!)
                {
                    RadioChoiceModel radioChoice = new()
                    {
                        RadioChoice_ID = originalRadioChoice?.RadioChoice_ID,
                        ReviewQuestion_ID = originalRadioChoice?.ReviewQuestion_ID,
                        RadioChoice_Sequence = originalRadioChoice?.RadioChoice_Sequence,
                        RadioChoice_Text = originalRadioChoice?.RadioChoice_Text,
                        Selected = originalRadioChoice!.Selected
                    };
                    _allRadioChoices_Active.Add(radioChoice);
                }
                _allRadioChoices_Deleted = (await _service.GetAllRadioButtonChoicesByYearAndDeletedStatus(_selectedReviewYear.Value, true, Database_OPS))?.ToList();
                /*
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

        private async Task<int?> UpdateAnswerFormat_IfChanged(PerformanceReviewQuestionModel? updatedQuestion)
        {
            int? newAnswerFormat = null;

            if (string.IsNullOrWhiteSpace(_selectedAnswerFormatDropDownValue) == false)     // WILL BE null IF UNCHANGED
            {
                foreach (KeyValuePair<int, string> kvp in _answerFormats!)
                {
                    if (kvp.Value == _selectedAnswerFormatDropDownValue)
                    {
                        // TODO: ADD ANSWER FORMAT QUESTIONS - THAT WILL TAKE A WHILE
                        await _service.UpdateAnswerFormat(updatedQuestion!.Question_ID!.Value, kvp.Key, Database_OPS);
                        newAnswerFormat = kvp.Key;
                        break;
                    }
                }
                _selectedAnswerFormatDropDownValue = null;
            }

            return newAnswerFormat;
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

        private async Task UpdateRadioButtonClicked(GridCommandEventArgs args)
        {
            _questionWithRadioButtonsToEdit = ConvertToModel(args);
            _allRadioChoices_Active_ToEdit = _allRadioChoices_Active
               ?.Where(q => q?.ReviewQuestion_ID == _questionWithRadioButtonsToEdit?.Question_ID)
                .OrderBy(q => q?.RadioChoice_Sequence)
                .ToList();

            _radioButtonScreenVisible = true;
            await Task.Delay(1);
            StateHasChanged();
        }
    }
}
