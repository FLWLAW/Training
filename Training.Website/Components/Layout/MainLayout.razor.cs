using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Services;

namespace Training.Website.Components.Layout
{
    public partial class MainLayout
    {
        #region DEPENDENCY INJECTION PARAMETERS
        [Inject]
        private IDatabase? Database { get; set; }

        [Inject]
        private AuthenticationStateProvider? GetAuthenticationStateAsync { get; set; }

        [Inject]
        private NavigationManager? NavigationManager { get; set; }
        #endregion

        #region STRING QUERY PARAMETERS
        [Parameter]
        [SupplyParameterFromQuery]
        public string? SessionID { get; set; }
        #endregion

        public AppState? ApplicationState { get; set; } = new();

        protected async override Task OnInitializedAsync()
        {
            /*
            ApplicationState!.LoggedOnUser = new()
            {
                AppUserID = 1000000,
                Domain = "EFWLAW",
                LoginID = "DRosenblum",
                UserName = "David Rosenblum"
            };
            
            await base.OnInitializedAsync();
            */

            await GetLoggedOnUser();
        }

        // ================================================================================================================================================================================================================================================================================================

        private async Task GetLoggedOnUser()
        {
            AuthenticationState? authstate = await GetAuthenticationStateAsync!.GetAuthenticationStateAsync();
            SqlDatabase dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
            MainLayoutDataService service = new();

            ApplicationState!.LoggedOnUser = await service.GetUser(authstate, Database, dbCMS);
            await Task.Delay(2000);
        }
    }
}
