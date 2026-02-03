using Dapper;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class AdministratorServiceMethods : CommonServiceMethods
    {
        public async Task<int> CountOfAnswerChoicesByQuestionID(int questionID, IDatabase? database) =>
            (
                await database!.QueryByStoredProcedureAsync<int, object?>
                    ("usp_Training_Questionnaire_CountOfAnswerChoicesByQuestionID", new { Question_ID = questionID })
            ).FirstOrDefault();

        public async Task DeleteAnswerChoicesByQuestionID(int questionID, IDatabase? database) =>
            await database!.NonQueryByStoredProcedureAsync<object?>
                ("usp_Training_Questionnaire_DeleteAnswerChoicesByQuestionID", new { Question_ID = questionID });

        public async Task DeleteQuestionByQuestionID(int questionID, IDatabase? database)
        {
            await DeleteAnswerChoicesByQuestionID(questionID, database);    // DELETE LINKED ANSWER CHOICES (IF ANY) FIRST

            await database!.NonQueryByStoredProcedureAsync<object?>
                ("usp_Training_Questionnaire_DeleteQuestionByQuestionID", new { Question_ID = questionID });
        }

        public async Task<IEnumerable<string>?> GetAnswerLettersByQuestionID(int questionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<string, object?>
                ("usp_Training_Questionnaire_GetAnswerLettersByQuestionID", new { Question_ID = questionID });

        public async Task<QuestionsModel?> GetQuestionByQuestionID(int questionID, IDatabase? database) =>
            (
                await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                    ("usp_Training_Questionnaire_GetQuestionByQuestionID", new { Question_ID = questionID })
            ).FirstOrDefault()!;

        public async Task InsertMultipleChoiceAnswer(int questionID, char answerLetter, string answerText, int createdByID, IDatabase? database)
        {
            InsertAnswerChoice_Parameters parameters = new()
            {
                Question_ID = questionID,
                AnswerLetter = answerLetter,
                AnswerText = answerText,
                CreatedBy_ID = createdByID
            };

            await database!.NonQueryByStoredProcedureAsync<InsertAnswerChoice_Parameters>("usp_Training_Questionnaire_InsertAnswerChoice", parameters);
        }

        public async Task<int> InsertQuestion
            (
                int sessionID, int questionnaireNumber, int questionNumber, string question, int answerFormatID, string? correctAnswer, int createdByID,
                IDatabase? database
            )
        {
            DynamicParameters parameters = new();

            parameters.Add("@Training_SESSION_ID", value: sessionID, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@QuestionnaireNumber", value: questionnaireNumber, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@QuestionNumber", value: questionNumber, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Question", value: question, dbType: DbType.AnsiString, direction: ParameterDirection.Input);
            parameters.Add("@AnswerFormat_ID", value: answerFormatID, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@CorrectAnswer", value: correctAnswer, dbType: DbType.String, direction: ParameterDirection.Input);
            parameters.Add("@CreatedBy_ID", value: createdByID, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Active", value: 1, dbType: DbType.Boolean, direction: ParameterDirection.Input);
            parameters.Add("@Current_ID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            int questionID = await database!.NonQueryByStoredProcedureOutputParameterAsync<int>
                ("usp_Training_Questionnaire_InsertQuestion", "@Current_ID", parameters);

            return questionID;
        }

        public async Task UpdateMultipleChoiceAnswer(int answerID, char answerLetter, string answerText, int updatedByID, IDatabase? database)
        {
            UpdateAnswerChoice_Parameters parameters = new()
            {
                Answer_ID = answerID!,
                AnswerLetter = answerLetter,
                AnswerText = answerText,
                LastUpdatedBy_ID = updatedByID
            };

            await database!.NonQueryByStoredProcedureAsync<UpdateAnswerChoice_Parameters>("usp_Training_Questionnaire_UpdateAnswerChoice", parameters);
        }

        public async Task UpdateQuestion(int questionID, string question, int answerFormat, string correctAnswer, int updatedByID, IDatabase? database)
        {
            UpdateQuestion_Parameters parameters = new()
            {
                Question_ID = questionID!,
                Question = question,
                AnswerFormat = answerFormat,
                CorrectAnswer = correctAnswer,
                UpdatedBy_ID = updatedByID
            };

            await database!.NonQueryByStoredProcedureAsync<UpdateQuestion_Parameters>("usp_Training_Questionnaire_UpdateQuestion", parameters);
        }

        public async Task UpdateQuestion_QuestionNumberOnly(int questionID, int questionNumber, IDatabase? database) =>
            await database!.NonQueryByStoredProcedureAsync<object?>
                ("usp_Training_Questionnaire_UpdateQuestion_QuestionNumberOnly", new { Question_ID = questionID, QuestionNumber = questionNumber });



        /*
        public async Task<IEnumerable<AnswerFormatsModel>?> GetAnswerFormats_TrainingQuestionnaire(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<AnswerFormatsModel>("usp_Training_Questionnaire_GetAnswerFormats");
        */


    }
}
