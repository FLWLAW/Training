using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models.Users;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class ImpersonationScreen
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        /*
        [Inject]
        private IDatabase? Database { get; set; }
        */

        [Inject]
        private NavigationManager? NavManager { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private readonly CommonServiceMethods _service = new();
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private IEnumerable<AllUsers_CMS_DB>? _allUsers = null;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            _allUsers = await _service.GetAllUsers(_dbCMS);
        }

        private void  UserSingleSelectValueChanged(string? newValue)
        {
            AllUsers_CMS_DB? user = _allUsers?.FirstOrDefault(q => q?.UserName?.Equals(newValue, StringComparison.InvariantCultureIgnoreCase) == true);

            ApplicationState!.LoggedOnUser = new()
            {
                AppUserID = user?.AppUserID,
                RoleID = user?.RoleID,
                TitleID = user?.TitleID,
                UserName = user?.UserName,
                LoginID = user?.LoginID
            };
            NavManager?.NavigateTo("/");
        }

    }
}
