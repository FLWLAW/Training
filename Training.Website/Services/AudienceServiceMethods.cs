using SqlServerDatabaseAccessLibrary;
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

        public void SaveSessionDueDate(int? sessionID, DateTime dueDate, string? user, bool update, IDatabase database) =>
            database!.NonQueryByStoredProcedure<object?>("usp_Training_Questionnaire_UpsertDueDate", new { Session_ID = sessionID, DueDate = dueDate, User = user, Update = update });
    }
}
