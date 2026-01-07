using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class CommonServiceMethods
    {
        public async Task<IEnumerable<IdValue<int>?>?> GetAllRoles(bool addNotary, IDatabase? database)
        {
            List<IdValue<int>?> roles = (await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Role_SA", "RoleID", "RoleDesc"))?.ToList() ?? [];

            if (addNotary == true)
                roles.Add(new IdValue<int>() { ID = 0, Value = Globals.Notary });

            return roles.OrderBy(q => q?.Value);
        }

        public async Task<IEnumerable<IdValue<int>?>?> GetAllTitles(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Title_SA", "TitleID", "TitleDesc");

        public async Task<IEnumerable<AllUsers_CMS_DB>?> GetAllUsers_CMS_DB(IDatabase? database) =>
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

        public async Task<IEnumerable<AllUsers_OPS_DB?>?> GetAllUsers_OPS_DB(IDatabase? database) =>
            await database!.QueryByStatementAsync<AllUsers_OPS_DB?>("SELECT Emp_ID, UserName FROM [Employees Tbl]");

        //NOTE: LEAVE AS SYNCHRONOUS
        public IEnumerable<AnswerChoicesModel>? GetAnswerChoicesByQuestionID(int questionID, IDatabase? database) =>
            database!.QueryByStoredProcedure<AnswerChoicesModel, object?>
                ("usp_Training_Questionnaire_GetAnswerChoicesByQuestionID", new { Question_ID = questionID });

        public async Task<SessionDueDateModel?> GetDueDateBySessionID(int? sessionID, IDatabase database) =>
            (await database!.QueryByStoredProcedureAsync<SessionDueDateModel, object?>("usp_Training_Questionnaire_GetDueDateBySessionID", new { Session_ID = sessionID }))?.FirstOrDefault();

        /*
        public async Task<IEnumerable<QuestionsModel>?> GetQuestionsBySessionID(int sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                ("usp_Training_Questionnaire_GetQuestionsBySessionID", new { Session_ID = sessionID });
        */

        public async Task<IEnumerable<AllUsers_Notaries?>?> GetNotaries(IEnumerable<AllUsers_CMS_DB?>? allUsers_CMS_DB, IDatabase? database)
        {
            AllUsers_Notaries?[]? notaries = (await database!.QueryByStoredProcedureAsync<AllUsers_Notaries?>("usp_Training_Questionnaire_GetNotaries"))?.ToArray();

            if (notaries == null || notaries.Length == 0)
                return null;
            else
            {
                AllUsers_CMS_DB?[]? allCMS_DB_Users_Array = allUsers_CMS_DB?.ToArray();

                if (allCMS_DB_Users_Array != null && allCMS_DB_Users_Array.Length > 0)
                {
                    foreach (AllUsers_Notaries? notary in notaries)
                    {
                        if (notary != null)
                        {
                            int? CMS_ID = allCMS_DB_Users_Array.FirstOrDefault(u => (u?.LoginID?.Equals(notary!.UserName, StringComparison.InvariantCultureIgnoreCase) == true))?.AppUserID;
                            notary.CMS_User_ID = CMS_ID;
                        }
                    }
                }

                return notaries;
            }
        }

        public async Task<int?> GetOPS_DB_UserID(string? loginID, IDatabase? database_OPS) =>
            (await database_OPS!.QueryByStatementAsync<int?>($"SELECT TOP 1 Emp_ID FROM [Employees Tbl] WHERE UserName = '{loginID}'"))?.FirstOrDefault();

        public async Task<IEnumerable<QuestionsModel>?> GetQuestionsBySessionIDandQuestionnaireNumber(int sessionID, int questionnaireNumber, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<QuestionsModel, object?>
                ("usp_Training_Questionnaire_GetQuestionsBySessionIDandQuestionnaireNumber", new { Session_ID = sessionID, QuestionnaireNumber = questionnaireNumber });

        public async Task<IEnumerable<ScoresAndWhenSubmittedModel>?> GetScoresBySessionIDandUserID(int sessionID, int userID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<ScoresAndWhenSubmittedModel, object?>
                ("usp_Training_Questionnaire_GetScoresBySessionIDandUserID", new { Session_ID = sessionID, CMS_User_ID = userID });

        public async Task<IEnumerable<SessionInformationModel>?> GetSessionInformation(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<SessionInformationModel>("usp_Training_Questionnaire_GetSessionInformation");
    }
}
