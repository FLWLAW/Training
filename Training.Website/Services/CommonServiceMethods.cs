using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class CommonServiceMethods
    {
        //TODO: SORT THESE METHODS
        
        public static async Task<IEnumerable<SessionInformationModel>?> GetSessionInformation(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<SessionInformationModel>("usp_Training_Questionnaire_GetSessionInformation");

        /*
        public static async Task<IEnumerable<AnswerFormatsModel>?> GetAnswerFormats(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<AnswerFormatsModel>("usp_Training_Questionnaire_GetAnswerFormats");
        */

        public static async Task<Dictionary<int, string>?> GetAnswerFormats(IDatabase? database)
        {
            IEnumerable<AnswerFormatsModel>? data =
                await database!.QueryByStoredProcedureAsync<AnswerFormatsModel>("usp_Training_Questionnaire_GetAnswerFormats");

            if (data == null)
                return null;
            else
            {
                Dictionary<int, string> answerFormats = [];

                foreach (AnswerFormatsModel? row in data)
                    answerFormats.Add(row.Format_ID, row.Name!);

                return answerFormats;
            }
        }

        public static async Task<IEnumerable<QuestionsModel>?> GetQuestionsBySessionID(int sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                ("usp_Training_Questionnaire_GetQuestionsBySessionID", new { Session_ID = sessionID });

        //NOTE: LEAVE AS SYNCHRONOUS
        public static IEnumerable<AnswerChoicesModel>? GetAnswerChoicesByQuestionID(int questionID, IDatabase? database) =>
            database!.QueryByStoredProcedure<AnswerChoicesModel, object?>
                ("usp_Training_Questionnaire_GetAnswerChoicesByQuestionID", new { Question_ID = questionID });
    }
}
