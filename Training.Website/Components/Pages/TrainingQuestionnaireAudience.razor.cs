using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class TrainingQuestionnaireAudience
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database_OPS { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private AudienceServiceMethods _service = new();
        private IEnumerable<string>? _sessions = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            // GET ALL SESSIONS
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database_OPS);

            if (sessionInfo != null && sessionInfo.Any() == true)
            {
                _sessions = Globals.ConcatenateSessionInfoForDropDown(sessionInfo);
                _selectedSessionString = ApplicationState!.SessionID_String;
                if (string.IsNullOrWhiteSpace(_selectedSessionString) == false)
                    await SessionChanged(_selectedSessionString);
            }
        }

        // ================================================================================================================================================================================================================================================================================================

        private async Task SessionChanged(string newValue)
        {
            await Task.Delay(1);
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            StateHasChanged();
        }
    }
}
