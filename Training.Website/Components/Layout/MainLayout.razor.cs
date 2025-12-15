using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Services;

namespace Training.Website.Components.Layout
{
    //TODO: UNCOMMENT MULTIPLE OBJECTS HERE LATER...
    public partial class MainLayout
    {
        [Inject]
        private IDatabase? Database { get; set; }

        [Inject]
        private AuthenticationStateProvider? GetAuthenticationStateAsync { get; set; }

        public AppState? ApplicationState { get; set; } = new();

        protected async override Task OnInitializedAsync()
        {
            // TODO: CHANGE TO ACTUAL METHOD TO GET LOGGED ON USER
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
