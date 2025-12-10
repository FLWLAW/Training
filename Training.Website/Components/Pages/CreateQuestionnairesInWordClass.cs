using Microsoft.JSInterop;
using SqlServerDatabaseAccessLibrary;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Flow.FormatProviders.Docx;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Styles;
using Training.Website.Models;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public class CreateQuestionnairesInWordClass
    {
        private readonly AdministratorServiceMethods _service;
        private readonly SessionInformationModel? _selectedSession;
        private readonly string? _selectedSessionString;
        private readonly IDatabase? _database;

        public CreateQuestionnairesInWordClass(AdministratorServiceMethods service, SessionInformationModel? selectedSession, string? selectedSessionString, IDatabase? database)
        {
            _service = service;
            _selectedSession = selectedSession;
            _selectedSessionString = selectedSessionString;
            _database = database;
        }

        public async Task<RadFlowDocument> Create()
        {
            RadFlowDocument document = new();
            Section? section = document.Sections.AddSection();

            for (int questionnaireNumber = 1; questionnaireNumber <= Globals.MaximumTestAttemptsPerSession; questionnaireNumber++)
            {
                IEnumerable<QuestionsModel?>? questions = await _service.GetQuestionsBySessionIDandQuestionnaireNumber(_selectedSession!.Session_ID!.Value, questionnaireNumber, _database);

                await Task.Run(() =>
                {
                    if (questions != null)
                    {
                        AddTitle(section);
                        AddCaption(section, questionnaireNumber);
                        AddQuestions(section, questions);
                    }
                });
            }

            return document;
        }

        // =========================================================================================================================================================================================================================================================================================================

        private void AddCaption(Section? section, int questionnaireNumber)
        {
            Paragraph? caption = section?.Blocks.AddParagraph();
            caption!.TextAlignment = Alignment.Center;

            Run qn = caption.Inlines.AddRun($"Questionnaire #{questionnaireNumber}");
            qn.FontSize = 16;
            qn.Underline.Pattern = UnderlinePattern.Single;
        }

        private void AddQuestions(Section? section, IEnumerable<QuestionsModel?>? questions)
        {
            foreach (QuestionsModel? question in questions!)
            {
                section!.Blocks.AddParagraph().Inlines.AddRun($"{question!.QuestionNumber}.\t{question.Question}");

                IEnumerable<AnswerChoicesModel?>? answerChoices = _service.GetAnswerChoicesByQuestionID(question.Question_ID!.Value, _database);

                if (answerChoices != null && answerChoices.Any() == true)
                    foreach (AnswerChoicesModel? answerChoice in answerChoices)
                        section.Blocks.AddParagraph().Inlines.AddRun($"\t{answerChoice!.AnswerLetter}.\t{answerChoice.AnswerText}");

                section.Blocks.AddParagraph();
                section.Blocks.AddParagraph().Inlines.AddRun($"Answer: \t{question.CorrectAnswer}");
                section.Blocks.AddParagraph().Inlines.AddRun("----------------------------------------------------------------------------------------------------------------------------");
            }

            section!.Blocks.AddParagraph().Inlines.AddRun("=====================================================================================");
        }

        private void AddTitle(Section? section)
        {
            Paragraph? title = section?.Blocks.AddParagraph();

            title!.TextAlignment = Alignment.Center;
            title.Inlines.AddRun($"SESSION #{_selectedSession?.Session_ID} {_selectedSessionString}").FontSize = 18;
        }
    }
}
