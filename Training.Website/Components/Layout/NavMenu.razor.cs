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
        #endregion

        #region PRIVATE FIELDS
        private bool _performanceReviewMode = false;
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private readonly NavMenuServiceMethods _service = new();
        private IEnumerable<string>? _testers = null;
        private IEnumerable<string?>? _trainingQuestionnaire_Administrators = null;
        private IEnumerable<int?>? _managerIDs = null;
        #endregion

        protected override void OnInitialized()
        {
            _testers = _service.Testers();
            _trainingQuestionnaire_Administrators =_service.TrainingQuestionnaire_Administrator_LoginIDs(Database_OPS);
            _managerIDs = _service.Managers(_dbCMS);
        }

        private bool IsTrainingQuestionnaire_Administrator() => ApplicationState != null && ApplicationState.LoggedOnUser != null && _trainingQuestionnaire_Administrators != null
            ? _trainingQuestionnaire_Administrators!.Contains(ApplicationState!.LoggedOnUser!.LoginID?.ToLower())
            : false;

        private bool IsManager() => ApplicationState != null && ApplicationState.LoggedOnUser != null &&_managerIDs != null
            ? _managerIDs!.Contains(ApplicationState!.LoggedOnUser!.AppUserID)
            : false;

        private bool IsTester() =>
            ApplicationState != null && ApplicationState.LoggedOnUser != null && ApplicationState.LoggedOnUser.LoginID != null && _testers != null
                ? _testers!.Contains(ApplicationState!.LoggedOnUser!.LoginID!.ToLower())
                : false;

        private void SetPerformanceViewModeToTrue()
        {
            //NOTE: IF THIS EVENT IS FIRED IMMEDIATELY AFTER THE PAGE IS RENDERED, ITS INTENDED EFFECT OF MAKING THE "questionnaire" LINKS DISAPPEAR DOESN'T WORK.
            //      THE USER NEEDS TO WAIT A FEW SECONDS AFTER THE PAGE RENDERS.
            //      USING AN "await Task.Delay(x)" DIDN'T WORK.

            _performanceReviewMode = true;
            StateHasChanged();
        }
    }
}
