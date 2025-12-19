using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using Telerik.Blazor.Components;
using Training.Website.Models;
using Training.Website.Models.Users;
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
        private IEnumerable<IdValue<int>?>? _roles = null;
        private IEnumerable<IdValue<int>?>? _titles = null;
        private IEnumerable<EMailReportBySessionIdModel?>? _emailedUsers = null;
        private IEnumerable<AllUsers_CMS_DB?>? _allUsers_CMS = null;
        private AllUsers_Notaries?[]? _notaries = null;
        private IEnumerable<ResultsModel?>? _results = null;
        private readonly EmailReportServiceMethods _service = new();
        private TelerikGrid<EMailReportBySessionIdModel?>? _emailedReports = null;
        
        private readonly SqlDatabase _database_CMS = new(Configuration.DatabaseConnectionString_CMS()!);
        #endregion

        protected override async Task OnInitializedAsync()
        {
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database_OPS);

            _sessions = Globals.ConcatenateSessionInfoForDropDown(sessionInfo);
            _roles = await _service.GetAllRoles(true, _database_CMS);
            _titles = await _service.GetAllTitles(_database_CMS);
            _allUsers_CMS = await _service.GetAllUsers(_database_CMS);
            _notaries = (await _service.GetNotaries(_allUsers_CMS, Database_OPS))?.ToArray();
        }

// ===========================================================================================================================================================================================================================================================================================================================================

        private async Task<IEnumerable<EMailReportBySessionIdModel?>?> GetEMailedUsers()
        {
            DateTime today = DateTime.Today;
            EMailReportBySessionIdModel?[]? emailedUsers = (await _service.GetEMailingsBySessionID(_selectedSession?.Session_ID!.Value, Database_OPS!))?.ToArray();

            foreach(EMailReportBySessionIdModel? user_OPS in emailedUsers!)
            {
                if (user_OPS != null)
                {
                    IEnumerable<ScoresAndWhenSubmittedModel?>? scores =
                        await _service.GetScoresBySessionIDandUserID(_selectedSession!.Session_ID!.Value, user_OPS.CMS_User_ID!.Value, Database_OPS!);

                    bool noAttempts = (scores == null || scores.Any() == false);

                    user_OPS.WhenUserLastSubmitted = (noAttempts == true) ? null : scores!.Max(q => q?.WhenSubmitted);
                    user_OPS.Status = GetStatus(noAttempts, today, user_OPS.WhenUserLastSubmitted, scores);
                    user_OPS.Role = GetRoleName(user_OPS.CMS_User_ID);
                    user_OPS.Title = GetTitleName(user_OPS.CMS_User_ID);
                }
            }

            return emailedUsers;
        }

        private string? GetRoleName(int? cmsUserID)
        {
            if (cmsUserID == null)
                return null;
            else 
            {
                int? roleID = _allUsers_CMS?.FirstOrDefault(q => q?.AppUserID == cmsUserID)?.RoleID;
                string? roleName = (roleID != null) ? _roles?.FirstOrDefault(q => q?.ID == roleID.Value)?.Value : null;

                if (roleName != null && roleName.Contains(Globals.Notary, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    if (_notaries?.Any(q => q?.CMS_User_ID == cmsUserID) == true)
                        roleName = $"{roleName} ({Globals.Notary})";
                }

                return roleName;
            }
        }

        private string? GetTitleName(int? cmsUserID)
        {
            if (cmsUserID == null)
                return null;
            else
            {
                int? titleID = _allUsers_CMS?.FirstOrDefault(q => q?.AppUserID == cmsUserID)?.TitleID;
                return (titleID != null) ? _titles?.FirstOrDefault(q => q?.ID == titleID.Value)?.Value : null;
            }
        }

        private string? GetStatus(bool noAttempts, DateTime today, DateTime? whenUserLastSubmitted, IEnumerable<ScoresAndWhenSubmittedModel?>? scores)
        {
            if (noAttempts == true)
                return (_dueDate < today) ? "Overdue" : "Not Attempted";
            else if (scores!.Any(q => q?.Score >= Globals.TestPassingThreshold))
                return (whenUserLastSubmitted == null) ? "--NULL--" : (whenUserLastSubmitted?.Date > _dueDate) ? "Passed (late)" : "Passed";
            else if (scores!.Count() < Globals.MaximumTestAttemptsPerSession)
                return "Incomplete";
            else
                return "Failed";
        }

        private async Task SessionChanged(string newValue)
        {
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            _dueDate = (await _service.GetDueDateBySessionID(_selectedSession!.Session_ID, Database_OPS!))?.DueDate;
            _emailedUsers = await GetEMailedUsers();
            _results = await _service.GetResultsBySessionID(_selectedSession!.Session_ID!.Value, Database_OPS);

            StateHasChanged();
        }
    }
}
