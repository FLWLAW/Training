using SqlServerDatabaseAccessLibrary;
using System.Reflection.PortableExecutable;
using System.Text;
using Telerik.Documents.Common.Model;
using Telerik.Documents.Media;
//using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Styles;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;

namespace Training.Website.Services.WordDocument
{
    public class CreatePerformanceReviewDocumentClass
    {
        private readonly Dictionary<int, string>? _answerFormats;
        private readonly int _selectedReviewYear;
        private readonly UsersForDropDownModel? _selectedUser;
        private readonly ReviewModel? _selectedReview;
        private readonly EmployeeInformationModel? _headerInfo;
        private readonly PerformanceReviewQuestionModel?[]? _questions;
        private readonly RadioChoiceModel?[]? _allRadioChoices;

        public CreatePerformanceReviewDocumentClass
            (
                Dictionary<int, string>? answerFormats,
                int selectedReviewYear,
                UsersForDropDownModel? selectedUser,
                ReviewModel? selectedReview,
                EmployeeInformationModel? headerInfo,
                PerformanceReviewQuestionModel?[]? questions,
                RadioChoiceModel?[]? radioChoices
            )
        {
            _answerFormats = answerFormats;
            _selectedReviewYear = selectedReviewYear;
            _selectedUser = selectedUser;
            _selectedReview = selectedReview;
            _headerInfo = headerInfo;
            _questions = questions;
            _allRadioChoices = radioChoices;
        }

        public enum DocumentType
        {
            NOT_SPECIFIED,
            Word,
            Acrobat,
        }

        public async Task<RadFlowDocument> Create(DocumentType documentType)
        {
            RadFlowDocument document = new();
            Section? section = document.Sections.AddSection();

            await Task.Run(() =>
            {
                AddTitle(section);
                AddEmployeeHeader(section);
                AddEmployeeInfo(document, section);
                AddQuestionsAndAnswers(documentType, section);
                if (documentType == DocumentType.Acrobat)
                    AddSignatures(section);
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

        private void AddQuestionsAndAnswers(DocumentType documentType, Section section)
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
                                    {
                                        string mark = (documentType == DocumentType.Acrobat) ? "X" : Globals.CheckMark;     // NOTE: CHECK MARK DOES NOT APPEAR IN ACROBAT
                                        radioChoiceLine.Append($"{mark}\t");
                                    }
                                    else
                                        radioChoiceLine.Append('\t');

                                    radioChoiceLine.Append(radioChoice?.RadioChoice_Text);

                                    section.Blocks.AddParagraph().Inlines.AddRun(radioChoiceLine.ToString());
                                }
                            }
                        }
                        else if (question.Answer != null)
                        {
                            string?[]? lines = question.Answer.Split('\n');

                            foreach (string? line in lines)
                            {
                                Paragraph textArea = section.Blocks.AddParagraph();

                                //textArea.Borders = new ParagraphBorders(new Border(BorderStyle.Single));
                                textArea.Spacing.SpacingBefore = 6;
                                //textArea.Spacing.SpacingAfter = 12;
                                textArea.Indentation.LeftIndent = 64;
                                textArea.Indentation.RightIndent = 64;
                                textArea.Inlines.AddRun(line);
                            }
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

        private void AddSignatures(Section section)
        {
            const string signatureSection = "Signature:\t______________________________\tDate:\t__________";

            section.Blocks.AddParagraph().Inlines.AddRun(string.Empty);
            section.Blocks.AddParagraph().Inlines.AddRun($"Employee {signatureSection}");
            section.Blocks.AddParagraph().Inlines.AddRun(string.Empty);
            section.Blocks.AddParagraph().Inlines.AddRun($"Manager {signatureSection}");
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
