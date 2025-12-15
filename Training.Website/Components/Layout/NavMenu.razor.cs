using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Services;

namespace Training.Website.Components.Layout
{
    public partial class NavMenu
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        public AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database_OPS { get; set; }

        /*
        [Inject]
        private NavigationManager? NavManager { get; set; }
        */
        #endregion

        #region PRIVATE FIELDS
        private readonly NavMenuServiceMethods _service = new();
        private IEnumerable<string>? _testers = null;
        private IEnumerable<string?>? _administrators = null;
        #endregion

        protected override void OnInitialized()
        {
            _testers = _service.Testers();
            _administrators =_service.Administrator_LoginIDs(Database_OPS);
        }

        private bool IsAdministrator() => ApplicationState != null && ApplicationState.LoggedOnUser != null && _administrators != null
            ? _administrators!.Contains(ApplicationState!.LoggedOnUser!.LoginID?.ToLower())
            : false;

        private bool IsTester() => 
            #if DEBUG 
                ApplicationState != null && ApplicationState.LoggedOnUser != null && ApplicationState.LoggedOnUser.LoginID != null && _testers != null
                    ? _testers!.Contains(ApplicationState!.LoggedOnUser!.LoginID!.ToLower())
                    : false;
            #else
                false;
            #endif
    }
}
