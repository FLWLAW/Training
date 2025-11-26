using SqlServerDatabaseAccessLibrary;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class AudienceServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<IdValue<int>?>?> GetAllRoles(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Role_SA", "RoleID", "RoleDesc");

        public async Task<IEnumerable<IdValue<int>?>?> GetAllTitles(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Title_SA", "TitleID", "TitleDesc");

        public async Task<IEnumerable<AllUsers_CMS_DB>?> GetAllUsers(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<AllUsers_CMS_DB>("usp_AppUser_SA");
    }
}
