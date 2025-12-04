using SqlServerDatabaseAccessLibrary;
using System.Threading.Tasks;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class AudienceServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<IdValue<int>?>?> GetAllRoles(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Role_SA", "RoleID", "RoleDesc");

        public async Task<IEnumerable<IdValue<int>?>?> GetAllTitles(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Title_SA", "TitleID", "TitleDesc");

        public async Task<SessionDueDateModel?> GetDueDateBySessionID(int? sessionID, IDatabase database) =>
            (await database!.QueryByStoredProcedureAsync<SessionDueDateModel, object?>("usp_Training_Questionnaire_GetDueDateBySessionID", new { Session_ID = sessionID }))?.FirstOrDefault();

        internal void UpsertEMailingRecord(AllUsers_Assignment? recipient, int? session_ID, string? sendingUser, IDatabase? database)
        {
            UpsertEMailings_Parameters parameters = new()
            {
                CMS_User_ID = recipient!.AppUserID!.Value,
                Session_ID = session_ID!.Value,
                SendingUser = sendingUser!,
                EMailedUserLastName = recipient.LastName!,
                EMailedUserFirstName = recipient.FirstName!,
                EMailedUserLogin_ID = recipient.UserName!
            };

            database!.NonQueryByStoredProcedure("usp_Training_Questionnaire_UpsertEMailings", parameters);
        }

        public void UpsertSessionDueDate(int sessionID, DateTime dueDate, string? user, bool update, IDatabase? database)
        {
            UpsertDueDate_Parameters parameters = new()
            {
                Session_ID = sessionID,
                DueDate = dueDate,
                User = user,
                Update = update
            };

            database!.NonQueryByStoredProcedure("usp_Training_Questionnaire_UpsertDueDate", parameters);
        }

    }
}
