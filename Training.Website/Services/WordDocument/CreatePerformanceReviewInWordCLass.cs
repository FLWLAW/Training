using SqlServerDatabaseAccessLibrary;
using System.Text;
using Telerik.Documents.Common.Model;
using Telerik.Documents.Media;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Styles;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;

namespace Training.Website.Services.WordDocument
{
    public class CreatePerformanceReviewInWordClass : IWordDocumentCreate
    {
        private readonly Dictionary<int, string>? _answerFormats;
        private readonly int _selectedReviewYear;
        //private readonly string _sheetName;
        private readonly UsersForDropDownModel? _selectedUser;
        private readonly ReviewModel? _selectedReview;
        private readonly EmployeeInformationModel? _headerInfo;
        private readonly PerformanceReviewQuestionModel?[]? _questions;
        private readonly RadioChoiceModel?[]? _allRadioChoices;
        //private readonly AllUsers_OPS_DB?[]? _allUsers_OPS_DB;
        //private readonly AllUsers_CMS_DB?[]? _allUsers_CMS_DB;
        //private readonly PerformanceReviewServiceMethods _service;
        //private readonly IDatabase? _database_OPS;

        public CreatePerformanceReviewInWordClass
            (
                Dictionary<int, string>? answerFormats,
                int selectedReviewYear,
                //string sheetName,
                UsersForDropDownModel? selectedUser,
                ReviewModel? selectedReview,
                EmployeeInformationModel? headerInfo,
                PerformanceReviewQuestionModel?[]? questions,
                RadioChoiceModel?[]? radioChoices
                //AllUsers_OPS_DB?[]? allUsers_OPS_DB,
                //AllUsers_CMS_DB?[]? allUsers_CMS_DB,
                //PerformanceReviewServiceMethods service,
                //IDatabase? database_OPS
            )
        {
            _answerFormats = answerFormats;
            _selectedReviewYear = selectedReviewYear;
            //_sheetName = sheetName;
            _selectedUser = selectedUser;
            _selectedReview = selectedReview;
            _headerInfo = headerInfo;
            _questions = questions;
            _allRadioChoices = radioChoices;
            //_allUsers_CMS_DB = allUsers_CMS_DB;
            //_allUsers_OPS_DB = allUsers_OPS_DB;
            //_service = service;
            //_database_OPS = database_OPS;
        }

        public async Task<RadFlowDocument> Create()
        {
            RadFlowDocument document = new();
            Section? section = document.Sections.AddSection();

            await Task.Run(() =>
            {
                AddTitle(section);
                AddEmployeeHeader(section);
                AddEmployeeInfo(document, section);
                AddQuestionsAndAnswers(section);
            });

            return document;
        }

// =========================================================================================================================================================================================================================================================================================================

        private void AddEmployeeHeader(Section section)
        {
            Paragraph? employeeHeader = section?.Blocks.AddParagraph();

            employeeHeader!.TextAlignment = Alignment.Center;
            employeeHeader.Inlines.AddRun($"Employee: {_selectedUser?.FirstName} {_selectedUser?.LastName}").FontSize = 18;
        }

        private void AddEmployeeInfo(RadFlowDocument document, Section section)
        {
            Table employeeInfo = section.Blocks.AddTable();

            document.StyleRepository.AddBuiltInStyle(BuiltInStyleNames.TableGridStyleId);
            employeeInfo.StyleId = BuiltInStyleNames.TableGridStyleId;
            //ThemableColor cellBackground = new ThemableColor(Colors.White);

            AddRow(employeeInfo, null, null, "ADP File #", _headerInfo?.BadgeNum);
            AddRow(employeeInfo, "Job Title:", _headerInfo?.JobTitleName, "Hire Date", _headerInfo?.HireDate?.ToShortDateString());
            AddRow(employeeInfo, "Location:", _headerInfo?.SiteName, "Employee Type:", _headerInfo?.EmployeeType);
            AddRow(employeeInfo, "Practice Group:", _headerInfo?.PracticeGroupName, "Shift:", _headerInfo?.Shift);
            AddRow(employeeInfo, "Departments:", _headerInfo?.Departments, "Review Status:", Globals.ReviewStatuses[_selectedReview!.Status_ID_Type]);
        }

        private void AddCell(TableRow row, object? value, double width) //, ThemableColor cellBackground)
        {
            TableCell cell = row.Cells.AddTableCell();
            string valueStr = value?.ToString() ?? string.Empty;

            cell.Blocks.AddParagraph().Inlines.AddRun(valueStr);
            //cell.Shading.BackgroundColor = cellBackground;
            cell.PreferredWidth = new TableWidthUnit(width);
        }

        private void AddQuestionsAndAnswers(Section section)
        {
            if (_questions != null)
            {
                section.Blocks.AddParagraph().Inlines.AddRun(); // SKIP LINE

                foreach (PerformanceReviewQuestionModel? question in _questions)
                {
                    if (question != null)
                    {
                        section.Blocks.AddParagraph().Inlines.AddRun($"{question.QuestionNumber})\t{question.Question}");
                        if (_answerFormats![question.AnswerFormat!.Value] == Globals.RadioButtons)
                        {
                            IEnumerable<RadioChoiceModel?>? radioChoicesThisQuestion =
                                _allRadioChoices?.Where(q => q?.ReviewQuestion_ID == question?.Question_ID).OrderBy(s => s?.RadioChoice_Sequence);

                            if (radioChoicesThisQuestion != null)
                            {
                                foreach (RadioChoiceModel? radioChoice in radioChoicesThisQuestion)
                                {
                                    StringBuilder radioChoiceLine = new("\t\t");

                                    if (question.Answer?.Equals(radioChoice?.RadioChoice_Text, StringComparison.InvariantCultureIgnoreCase) == true)
                                        radioChoiceLine.Append($"{Globals.CheckMark}\t");
                                    else
                                        radioChoiceLine.Append('\t');

                                    radioChoiceLine.Append(radioChoice?.RadioChoice_Text);

                                    section.Blocks.AddParagraph().Inlines.AddRun(radioChoiceLine.ToString());
                                }
                            }
                        }
                        else
                        {
                            Paragraph textArea = section.Blocks.AddParagraph();

                            textArea.Borders = new ParagraphBorders(new Border(BorderStyle.Single));
                            textArea.Indentation.LeftIndent = 32;
                            textArea.Indentation.RightIndent = 32;
                            textArea.Inlines.AddRun(question?.Answer);
                        }
                    }
                }
            }
        }

        private void AddRow(Table table, object? value1, object? value2, object? value3, object? value4)
        {
            TableRow row = table.Rows.AddTableRow();

            AddCell(row, value1, 200);
            AddCell(row, value2, 250);
            AddCell(row, value3, 200);
            AddCell(row, value4, 200);
        }

        private void AddTitle(Section section)
        {
            Paragraph title = section.Blocks.AddParagraph();
            title.Shading.BackgroundColor = new ThemableColor(Colors.LightGray);
            title.TextAlignment = Alignment.Center;
            title.Inlines.AddRun($"{_selectedReviewYear} Performance Review").FontSize = 24;
        }
    }
}
