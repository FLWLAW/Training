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
        //private TelerikWindow? _radioButtonEditWindow;
        private bool _radioButtonScreenVisible = false;
        private int? _selectedReviewYear = null;
        private string? _selectedAnswerFormatDropDownValue = null;
        //private string? _newRadioButtonText = null;
        private string[]? _reviewYears = null;
        private Dictionary<int, string>? _answerFormats = null;
        private List<PerformanceReviewQuestionModel?>? _activeQuestions = null;     // TODO: MAKE IEnumerable?
        private List<PerformanceReviewQuestionModel?>? _deletedQuestions = null;    // TODO: MAKE IEnumerable?
        private List<RadioChoiceModel?>? _allRadioChoices_Screen = null;
        private List<RadioChoiceModel?>? _allRadioChoices_Original = null;
        //private List<RadioChoiceModel?>? _allRadioChoices_Active_Original = null;
        //private List<RadioChoiceModel?>? _radioChoices_Active_ToEdit = null;
        //private List<RadioChoiceModel?>? _allRadioChoices_Deleted = null;
        private PerformanceReviewQuestionModel? _questionWithRadioButtonsToEdit = null;
        private PerformanceReviewServiceMethods _service = new();
        private TelerikGrid<RadioChoiceModel>? _radioButtonGrid_Active;
        private TelerikGrid<RadioChoiceModel>? _radioButtonGrid_Deleted;
        #endregion


        protected override async Task OnInitializedAsync()
        {
            _reviewYears = ReviewYears();
            _answerFormats = await _service.GetAnswerFormats_PerformanceReview(Database_OPS);
        }

        // =========================================================================================================================================================================================================================================================================================================
        /*
        private void ActiveQuestionCancelAllRadioChangesForOneQuestionHandler(GridCommandEventArgs args)
        {
            if (_allRadioChoices_Active != null)
            {
                PerformanceReviewQuestionModel? questionToDeleteRadioChangesFrom = ConvertToQuestionModel(args);

                if (questionToDeleteRadioChangesFrom != null && _answerFormats![questionToDeleteRadioChangesFrom.AnswerFormat!.Value] == Globals.RadioButtons)
                {
                    int? questionID = questionToDeleteRadioChangesFrom?.Question_ID;

                    if (questionID != null)
                    {
                        foreach (RadioChoiceModel? activeRadioChoice in _allRadioChoices_Active)
                        {
                            if (activeRadioChoice != null && activeRadioChoice.ReviewQuestion_ID == questionID)
                            {
                                RadioChoiceModel? originalRadioChoice = _allRadioChoices_Active_Original?.FirstOrDefault(q => q?.RadioChoice_ID == activeRadioChoice?.RadioChoice_ID);

                                if (originalRadioChoice != null)
                                {
                                    activeRadioChoice.RadioChoice_Text = originalRadioChoice.RadioChoice_Text;
                                    activeRadioChoice.RadioChoice_Sequence = originalRadioChoice.RadioChoice_Sequence;
                                }
                            }
                        }
                    }
                }
            }
        }
        */

        private void ActiveQuestionCancelAllRadioChangesForOneQuestionHandler(GridCommandEventArgs args)
        {
            throw new NotImplementedException();
        }

        private async Task ActiveQuestionCreateHandler(GridCommandEventArgs args)
        {
            if (_selectedAnswerFormatDropDownValue != null)
            {
                string? newQuestionText = (ConvertToQuestionModel(args))?.Question;

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

        private async Task ActiveQuestionDeleteHandler(GridCommandEventArgs args) => await DeleteRestoreQuestionMainHandler(args, true);

        private async Task ActiveQuestionUpdateHandler(GridCommandEventArgs args)
        {
            _questionWithRadioButtonsToEdit = null;

            PerformanceReviewQuestionModel? questionToUpdate = ConvertToQuestionModel(args);

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
            throw new NotImplementedException();
        }

        /*
        private void CloseRadioButtonEditScreen()
        {
            if (_allRadioChoices_Active != null && _radioChoices_Active_ToEdit != null)
            {
                for (int major = 0; major < _allRadioChoices_Active?.Count; major++)
                {
                    for (int minor = 0; minor < _radioChoices_Active_ToEdit?.Count; minor++)
                    {
                        if (_radioChoices_Active_ToEdit[minor]?.RadioChoice_ID == _allRadioChoices_Active[major]?.RadioChoice_ID)
                        {
                            if (_allRadioChoices_Active[major]?.RadioChoice_Text != _radioChoices_Active_ToEdit[minor]?.RadioChoice_Text)
                                _allRadioChoices_Active[major]!.RadioChoice_Text = _radioChoices_Active_ToEdit[minor]?.RadioChoice_Text;

                            if (_allRadioChoices_Active[major]?.RadioChoice_Sequence != _radioChoices_Active_ToEdit[minor]?.RadioChoice_Sequence)
                                _allRadioChoices_Active[major]!.RadioChoice_Sequence = _radioChoices_Active_ToEdit[minor]?.RadioChoice_Sequence;
                        }
                    }
                }
            }

            _radioButtonScreenVisible = false;
            _questionWithRadioButtonsToEdit = null;
            _radioChoices_Active_ToEdit = null;
            StateHasChanged();
        }
        */

        private PerformanceReviewQuestionModel? ConvertToQuestionModel(GridCommandEventArgs args) => (PerformanceReviewQuestionModel?)args.Item;

        private async Task GetAllQuestions()
        {
            if (_selectedReviewYear != null)
            {
                _activeQuestions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, false, Database_OPS))?.ToList();
                _deletedQuestions = (await _service.GetPerformanceReviewQuestions(_selectedReviewYear.Value, true, Database_OPS))?.ToList();
            }
        }

        private async Task DeleteRestoreQuestionMainHandler(GridCommandEventArgs args, bool newDeletionStatus)
        {
            PerformanceReviewQuestionModel? questionToModify = ConvertToQuestionModel(args);

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

        private async Task GetAllRadioChoices_FromDB()
        {
            _allRadioChoices_Original = (await _service.GetAllRadioButtonChoicesByYear(_selectedReviewYear!.Value, Database_OPS))?.ToList();
            _allRadioChoices_Screen = (await _service.GetAllRadioButtonChoicesByYear(_selectedReviewYear.Value, Database_OPS))?.ToList();
        }

        private async Task QuestionMoveDownHandler(GridCommandEventArgs args) => await QuestionMoveHandlerMain(args, _DOWN);

        private async Task QuestionMoveHandlerMain(GridCommandEventArgs args, int moveIncrement)
        {
            if (_activeQuestions == null)
                throw new NoNullAllowedException("[_activeQuestions] cannot be null in MoveHandlerMain().");
            else
            {
                PerformanceReviewQuestionModel? question1 = ConvertToQuestionModel(args);
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

        private IEnumerable<RadioChoiceModel?>? RadioButtonChoicesForScreen(bool deleted) =>
            _allRadioChoices_Screen?.Where(q => q?.ReviewQuestion_ID == _questionWithRadioButtonsToEdit?.Question_ID && q?.IsDeleted == deleted).OrderBy(s => s?.RadioChoice_Sequence);


        private void RadioButtonCreateHandler(GridCommandEventArgs args)
        {
            if (_questionWithRadioButtonsToEdit != null)
            {
                var newRadioButtonItem = (RadioChoiceModel?)args.Item;

                if (newRadioButtonItem != null)
                {
                    _allRadioChoices_Screen ??= [];
                    newRadioButtonItem.ReviewQuestion_ID = _questionWithRadioButtonsToEdit.Question_ID;
                    newRadioButtonItem.RadioChoice_Sequence = _allRadioChoices_Screen.Max(q => q?.RadioChoice_Sequence) + 1;
                    newRadioButtonItem.IsDeleted = false;       // NOT NECESSARY UNLESS PROPERTY DEFAULT IS CHANGED OR IS MADE NULLABLE - SO IT'S GOOD PRACTICVE TO SET IT TO "FALSE" HERE.
                    newRadioButtonItem.HasBeenChangedOnScreen = true;

                    _allRadioChoices_Screen.Add(newRadioButtonItem);
                    ResequenceRadioButtons();
                    _radioButtonGrid_Active?.Rebind();
                }
            }
        }

        private void RadioButtonDeleteHandler(GridCommandEventArgs args)
        {
            var changedItem = (RadioChoiceModel?)args.Item;

            if (changedItem != null)
            {
                RadioChoiceModel? radioButtonToDelete = _allRadioChoices_Screen?.FirstOrDefault(q => q?.RadioChoice_ID == changedItem?.RadioChoice_ID);

                if (radioButtonToDelete != null)
                {
                    // THIS WILL CHANGE THE VALUES IN THE _allRadioChoices_Screen LIST, WHICH IS WHAT THE SCREEN BINDS TO, BUT IT WON'T CHANGE THE VALUES IN THE _allRadioChoices_Original LIST, WHICH IS WHAT WE USE TO COMPARE TO KNOW WHETHER CHANGES HAVE BEEN MADE. THEN, WHEN THE USER CLICKS "SAVE CHANGES", WE CAN LOOP THROUGH THE _allRadioChoices_Screen LIST AND COMPARE TO THE _allRadioChoices_Original LIST TO SEE WHICH RADIO CHOICES HAVE CHANGES THAT NEED TO BE SENT TO THE SERVER.
                    radioButtonToDelete.IsDeleted = true;
                    radioButtonToDelete.HasBeenChangedOnScreen = true;

                    ResequenceRadioButtons();

                    _radioButtonGrid_Active?.Rebind();
                    _radioButtonGrid_Deleted?.Rebind();
                }
            }
        }

        private void RadioButtonMoveDownHandler(GridCommandEventArgs args) => RadioButtonMoveHandlerMain(args, _DOWN);

        private void RadioButtonMoveHandlerMain(GridCommandEventArgs args, int moveIncrement)
        {
            if (_allRadioChoices_Screen != null)
            {
                var radioButton1 = (RadioChoiceModel?)args.Item;
                int highestSequenceNumber = _allRadioChoices_Screen?.Where(q => q?.ReviewQuestion_ID == _questionWithRadioButtonsToEdit?.Question_ID && q?.IsDeleted == false)?.Count() ?? 0;

                if (radioButton1 != null && ((moveIncrement == _UP && radioButton1.RadioChoice_Sequence > 1) || (moveIncrement == _DOWN && radioButton1.RadioChoice_Sequence < highestSequenceNumber)))
                {
                    int secondSequenceNumber = radioButton1.RadioChoice_Sequence.Value + moveIncrement;
                    RadioChoiceModel? radioButton2 = _allRadioChoices_Screen?.FirstOrDefault
                        (q => q?.ReviewQuestion_ID == _questionWithRadioButtonsToEdit?.Question_ID && q?.RadioChoice_Sequence == secondSequenceNumber); // && q?.IsDeleted == false);

                    if (radioButton2 != null)
                    {
                        int firstSequenceNumber = radioButton1.RadioChoice_Sequence.Value;

                        radioButton1.RadioChoice_Sequence = secondSequenceNumber;
                        radioButton1.HasBeenChangedOnScreen = true;
                        radioButton2.RadioChoice_Sequence = firstSequenceNumber;
                        radioButton2.HasBeenChangedOnScreen = true;

                        ResequenceRadioButtons();
                        _radioButtonGrid_Active?.Rebind();
                    }
                }
            }
        }


        private void RadioButtonMoveUpHandler(GridCommandEventArgs args) => RadioButtonMoveHandlerMain(args, _UP);

        /*
        private async Task RadioButtonDeleteHandler(GridCommandEventArgs args)
        {
            await Task.Run(() =>
            {
                var radioButtonToDelete = (RadioChoiceModel?)args.Item;

                _allRadioChoices_Deleted ??= [];

                _allRadioChoices_Active?.Remove(radioButtonToDelete);
                _allRadioChoices_Deleted.Add(radioButtonToDelete);

                _radioButtonGrid_Active?.Rebind();
                _radioButtonEditWindow?.Refresh();
            });
            _radioButtonScreenVisible = false;
            StateHasChanged();
            await Task.Delay(100);
            _radioButtonScreenVisible = true;
            StateHasChanged();
            await Task.Delay(100);
            await Task.Delay(500);
            //TODO: YOU NEED TO DELETE ROWS IN _radioChoices_Active_ToEdit!!!!
        }
        */

        private void RadioButtonRestoreHandler(GridCommandEventArgs args)
        {
            var radioButtonToRestore = (RadioChoiceModel?)args.Item;

            if (radioButtonToRestore != null)
            {
                radioButtonToRestore.IsDeleted = false;
                radioButtonToRestore.HasBeenChangedOnScreen = true;
                
                _radioButtonGrid_Deleted?.Rebind();
                _radioButtonGrid_Active?.Rebind();
            }
        }

        private void RadioButtonUpdateHandler(GridCommandEventArgs args)
        {
            var changedItem = (RadioChoiceModel?)args.Item;

            if (changedItem != null)
            {
                RadioChoiceModel? radioButtonToChange = _allRadioChoices_Screen?.FirstOrDefault(q => q?.RadioChoice_ID == changedItem?.RadioChoice_ID);

                if (radioButtonToChange != null)
                {
                    // THIS WILL CHANGE THE VALUES IN THE _allRadioChoices_Screen LIST, WHICH IS WHAT THE SCREEN BINDS TO, BUT IT WON'T CHANGE THE VALUES IN THE _allRadioChoices_Original LIST, WHICH IS WHAT WE USE TO COMPARE TO KNOW WHETHER CHANGES HAVE BEEN MADE. THEN, WHEN THE USER CLICKS "SAVE CHANGES", WE CAN LOOP THROUGH THE _allRadioChoices_Screen LIST AND COMPARE TO THE _allRadioChoices_Original LIST TO SEE WHICH RADIO CHOICES HAVE CHANGES THAT NEED TO BE SENT TO THE SERVER.
                    radioButtonToChange.RadioChoice_Text = changedItem?.RadioChoice_Text;
                    radioButtonToChange.HasBeenChangedOnScreen = true;
                }
            }
        }

        /*
        private void RadioButtonUpdateHandler(GridCommandEventArgs args)
        {
            var possiblyChangedItem = (RadioChoiceModel?)args.Item;

            if (possiblyChangedItem != null && _radioChoices_Active_ToEdit != null)
            {
                for (int index = 0; index < _radioChoices_Active_ToEdit.Count; index++)
                {
                    if
                        (
                            _radioChoices_Active_ToEdit[index] != null &&
                            _radioChoices_Active_ToEdit[index]!.RadioChoice_ID == possiblyChangedItem.RadioChoice_ID &&
                            _radioChoices_Active_ToEdit[index]!.RadioChoice_Text != possiblyChangedItem.RadioChoice_Text
                        )
                    {
                        _radioChoices_Active_ToEdit[index]!.RadioChoice_Text = possiblyChangedItem.RadioChoice_Text;
                    }
                }
            }
        }
        */

        private void ResequenceRadioButtons()
        {
            if (_allRadioChoices_Screen != null)
            {
                int radioChoiceSequenceShouldBe = 1;

                _allRadioChoices_Screen = _allRadioChoices_Screen
                    .OrderBy(q => q?.ReviewQuestion_ID)
                    .ThenBy(q => q?.RadioChoice_Sequence)
                    .ThenBy(q => q?.RadioChoice_ID)
                    .ToList();

                foreach(RadioChoiceModel? radioChoice in _allRadioChoices_Screen)
                {
                    if
                        (
                            radioChoice != null &&
                            radioChoice.ReviewQuestion_ID == _questionWithRadioButtonsToEdit?.Question_ID &&
                            radioChoice.IsDeleted == false
                        )
                    {
                        if (radioChoice.RadioChoice_Sequence != radioChoiceSequenceShouldBe)
                        {
                            radioChoice.RadioChoice_Sequence = radioChoiceSequenceShouldBe;
                            radioChoice.HasBeenChangedOnScreen = true;
                        }
                        radioChoiceSequenceShouldBe++;
                    }
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

        private async Task RestoreQuestionHandler(GridCommandEventArgs args) => await DeleteRestoreQuestionMainHandler(args, false);

        private async Task ReviewYearChanged(string newValue)
        {
            if (int.TryParse(newValue, out int selectedReviewYear) == true)
            {
                _selectedReviewYear = selectedReviewYear;
                await GetAllQuestions();
                await GetAllRadioChoices_FromDB();
                /*
                foreach (RadioChoiceModel? originalRadioChoice in _allRadioChoices_Active_Original!)
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
                */
                //_allRadioChoices_Deleted = (await _service.GetAllRadioButtonChoicesByYearAndDeletedStatus(_selectedReviewYear.Value, true, Database_OPS))?.ToList();
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

        private void UpdateRadioButtonClicked(GridCommandEventArgs args)
        {
            _questionWithRadioButtonsToEdit = ConvertToQuestionModel(args);     //TODO: MAYBE CHANGE THIS TO int _questionWithRadioButtonsToEdit_ID??
            _radioButtonScreenVisible = true;
            StateHasChanged();
        }

        /*
        private async Task UpdateRadioButtonClicked(GridCommandEventArgs args)
        {
            _questionWithRadioButtonsToEdit = ConvertToQuestionModel(args);
            _radioChoices_Active_ToEdit = _allRadioChoices_Active
               ?.Where(q => q?.ReviewQuestion_ID == _questionWithRadioButtonsToEdit?.Question_ID)
                .OrderBy(q => q?.RadioChoice_Sequence)
                .ToList();

            _radioButtonScreenVisible = true;
            await Task.Delay(1);
            StateHasChanged();
        }
        */
    }
}
