using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class CommonServiceMethods
    {
        public static async Task<IEnumerable<SessionInformationModel>?> GetSessionInformation(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<SessionInformationModel>("usp_Training_Questionnaire_GetSessionInformation");

        public static async Task<IEnumerable<QuestionsModel>?> GetQuestionsBySessionID(int sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                ("usp_Training_Questionnaire_GetQuestionsBySessionID", new { Session_ID = sessionID });

        public static async Task<IEnumerable<AnswerChoicesModel>?> GetAnswerChoicesByQuestionID(int questionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<AnswerChoicesModel, object?>
                ("usp_Training_Questionnaire_GetAnswerChoicesByQuestionID", new { Question_ID = questionID });
    }
}
