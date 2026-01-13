using Microsoft.AspNetCore.Components.Authorization;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class MainLayoutServiceMethods : CommonServiceMethods
    {
        public async Task<AllUsers_Authentication?> GetUser(AuthenticationState? authState, IDatabase? database_OPS, IDatabase? database_CMS)
        {
            int? backSlash = authState?.User?.Identity?.Name?.IndexOf('\\') ?? null;

            if (backSlash != null && backSlash > -1)
            {
                string? loginID = authState?.User?.Identity?.Name?[(backSlash.Value + 1)..];
                AllUsers_CMS_DB? userCMS_DB = await GetCMS_DB_UserInfo(loginID, database_CMS);
                int? OPS_ID = await GetOPS_DB_UserID(loginID, database_OPS);

                return new AllUsers_Authentication()
                {
                    AppUserID = userCMS_DB?.AppUserID,
                    EmpID = OPS_ID,
                    LoginID = loginID,
                    RoleID = userCMS_DB?.RoleID,
                    TitleID = userCMS_DB?.TitleID,
                    UserName = userCMS_DB?.UserName
                };
            }
            else
                return null;
        }

        // ===========================================================================================================================================================================================================================================================================================================================================================

        private async Task<AllUsers_CMS_DB?> GetCMS_DB_UserInfo(string? loginID, IDatabase? database_CMS)
        {
            if (string.IsNullOrEmpty(loginID) == true)
                throw new ArgumentNullException("'loginID' cannot be null.");
            else
            {
                AllUsers_CMS_DB? user = (await GetAllUsers_CMS_DB(database_CMS))?.FirstOrDefault(q => q?.LoginID?.ToLower() == loginID?.ToLower());
#if QA || RELEASE
                if (user != null)
                    return user;
                else if (loginID.Equals("DRosenblum", StringComparison.InvariantCultureIgnoreCase) == false)
                    return null;
                else
                {
                    return new AllUsers_CMS_DB()
                    {
                        AppUserID = 1000000,
                        EmailAddress = "drosenblum@flwlaw.com",
                        FirstName = "David",
                        LastName = "Rosenblum",
                        LoginID = "DRosenblum",
                        RoleID = 1,
                        TitleID = 1,
                        UserName = "David Rosenblum"
                    };
                }
#else
                return user;
#endif
            }
        }
    }
}
