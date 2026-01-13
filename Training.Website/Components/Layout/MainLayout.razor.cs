using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SqlServerDatabaseAccessLibrary;
using System.Runtime.CompilerServices;
using Telerik.Blazor.Components;
using Training.Website.Models.Users;
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

        //protected async override Task OnInitializedAsync()
        protected override void OnInitialized()
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

            //await GetLoggedOnUser();
            Task.Run(GetLoggedOnUser).Wait();
        }

        // ================================================================================================================================================================================================================================================================================================

        private async Task GetLoggedOnUser()
        {
            const int msDelay = 1000;
            const int countLimit = 5;
            int count = 1;
            AuthenticationState? authstate;
            SqlDatabase dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
            MainLayoutServiceMethods service = new();

            do
            {
                authstate = await GetAuthenticationStateAsync!.GetAuthenticationStateAsync();
                if (authstate == null)
                {
                    await Task.Delay(msDelay);
                    count++;
                }
            } while (authstate == null && count <= countLimit);

            if (authstate == null)
                throw new NullReferenceException($"Could not authenticate user after {countLimit} attempts with {msDelay}-millisecond delay between attempts.");
            else
            {
                AllUsers_Authentication? loggedOnUser;

                count = 1;

                do
                {
                    loggedOnUser = await service.GetUser(authstate, Database, dbCMS);
                    if (loggedOnUser == null)
                    {
                        await Task.Delay(msDelay);
                        count++;
                    }
                } while (loggedOnUser == null && count <= countLimit);

                if (loggedOnUser != null)
                {
                    ApplicationState!.LoggedOnUser = loggedOnUser;
                    await Task.Delay(2000);
                }
                else
                    throw new NullReferenceException($"Could not get DB information for logged on user after {countLimit} attempts with {msDelay}-millisecond delay between attempts.");
            }
        }
/*
        // OLD CODE - HANG ON TO THIS
        private async Task GetLoggedOnUser()
        {
            AuthenticationState? authstate = await GetAuthenticationStateAsync!.GetAuthenticationStateAsync();
            SqlDatabase dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
            MainLayoutDataService service = new();

            ApplicationState!.LoggedOnUser = await service.GetUser(authstate, Database_OPS, dbCMS);
            await Task.Delay(2000);
        }
*/
    }
}
