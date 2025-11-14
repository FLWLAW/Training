using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;

namespace Training.Website.Components.Layout
{
    //TODO: UNCOMMENT MULTIPLE OBJECTS HERE LATER...
    public partial class MainLayout
    {
        [Inject]
        private IDatabase? Database { get; set; }
/*
        [Inject]
        private AuthenticationStateProvider? GetAuthenticationStateAsync { get; set; }
*/
        public AppState? ApplicationState { get; set; } = new();

        protected async override Task OnInitializedAsync()
        {
            // TODO: CHANGE TO ACTUAL METHOD TO GET LOGGED ON USER
            ApplicationState!.LoggedOnUser = new()
            {
                AppUserID = 1000000,
                Domain = "EFWLAW",
                LoginID = "DRosenblum",
                UserName = "David Rosenblum"
            };

            await base.OnInitializedAsync();

            //await GetLoggedOnUser();
        }

        // ================================================================================================================================================================================================================================================================================================

        /*private async Task GetLoggedOnUser()
        {
            AuthenticationState? authstate = await GetAuthenticationStateAsync!.GetAuthenticationStateAsync();
            MainLayoutDataService service = new();

            ApplicationState!.LoggedOnUser = await service.GetUser(authstate, Database);
            await Task.Delay(2000);
        }
*/
    }
}
