using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class EmailReports
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
        private IEnumerable<string>? _sessions = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;
        private DateTime? _dueDate = null;
        private IEnumerable<EMailReportBySessionIdModel?>? _emailedUsers = null;
        private readonly EmailReportServiceMethods _service = new();
        #endregion

        protected override async Task OnInitializedAsync()
        {
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database_OPS);
            _sessions = Globals.ConcatenateSessionInfoForDropDown(sessionInfo);
        }

        private async Task SessionChanged(string newValue)
        {
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            _dueDate = (await _service.GetDueDateBySessionID(_selectedSession!.Session_ID, Database_OPS!))?.DueDate;
            _emailedUsers = await _service.GetEMailingsBySessionID(_selectedSession?.Session_ID!.Value, Database_OPS!);
            StateHasChanged();
        }
    }
}
