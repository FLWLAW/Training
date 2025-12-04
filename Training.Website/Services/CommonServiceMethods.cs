using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class CommonServiceMethods
    {
        public async Task<IEnumerable<AllUsers_CMS_DB>?> GetAllUsers(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<AllUsers_CMS_DB>("usp_AppUser_SA");

        public async Task<Dictionary<int, string>?> GetAnswerFormats(IDatabase? database)
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

        //NOTE: LEAVE AS SYNCHRONOUS
        public IEnumerable<AnswerChoicesModel>? GetAnswerChoicesByQuestionID(int questionID, IDatabase? database) =>
            database!.QueryByStoredProcedure<AnswerChoicesModel, object?>
                ("usp_Training_Questionnaire_GetAnswerChoicesByQuestionID", new { Question_ID = questionID });

        /*
        public async Task<IEnumerable<QuestionsModel>?> GetQuestionsBySessionID(int sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                ("usp_Training_Questionnaire_GetQuestionsBySessionID", new { Session_ID = sessionID });
        */

        public async Task<IEnumerable<QuestionsModel>?> GetQuestionsBySessionIDandQuestionnaireNumber(int sessionID, int questionnaireNumber, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                ("usp_Training_Questionnaire_GetQuestionsBySessionIDandQuestionnaireNumber", new { Session_ID = sessionID, QuestionnaireNumber = questionnaireNumber });

        public async Task<IEnumerable<SessionInformationModel>?> GetSessionInformation(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<SessionInformationModel>("usp_Training_Questionnaire_GetSessionInformation");
    }
}
