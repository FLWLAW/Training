using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using Training.Website.Models;
using Training.Website.Models.Users;
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
        private IEnumerable<IdValue<int>?>? _roles = null;
        private List<string> _selectedRoles = [];
        private IEnumerable<IdValue<int>?>? _titles = null;
        private List<string> _selectedTitles = [];
        private AllUsers_CMS_DB[]? _allUsers_DB = null;
        private List<AllUsers_Assignment?> _allUsers_Assignment = [];
        #endregion

        protected override async Task OnInitializedAsync()
        {
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database_OPS);

            if (sessionInfo != null && sessionInfo.Any() == true)
            {
                _allUsers_DB = (await _service.GetAllUsers(_dbCMS))?.ToArray();
                _roles = await _service.GetAllRoles(_dbCMS);
                _titles = await _service.GetAllTitles(_dbCMS);

                _sessions = Globals.ConcatenateSessionInfoForDropDown(sessionInfo);
                _selectedSessionString = ApplicationState!.SessionID_String;
                if (string.IsNullOrWhiteSpace(_selectedSessionString) == false)
                    await SessionChanged(_selectedSessionString);
            }
        }

        // ================================================================================================================================================================================================================================================================================================

        private void AddAssignedUsers(IEnumerable<AllUsers_CMS_DB>? users, List<AllUsers_Assignment> usersToAssign)
        {
            if (users != null)
            {
                foreach (AllUsers_CMS_DB? user in users)
                {
                    AllUsers_Assignment? assignedUsers = new()
                    {
                        AppUserID = user?.AppUserID,
                        UserName = user?.UserName,
                        EmailAddress = user?.EmailAddress,
                        RoleDesc = _roles?.FirstOrDefault(q => q?.ID == user?.RoleID)?.Value,
                        TitleDesc = _titles?.FirstOrDefault(q => q?.ID == user?.TitleID)?.Value,
                        Selected = true
                    };
                    usersToAssign.Add(assignedUsers);
                }
            }
        }

        private void RecompileAssignedUsers()
        {
            List<AllUsers_Assignment> usersToAssign_Raw = [];

            usersToAssign_Raw.AddRange(UsersInSelectedRoles());
            usersToAssign_Raw.AddRange(UsersInSelectedTitles());
            
            IEnumerable<int?> distinctIDs = usersToAssign_Raw.Select(q => q.AppUserID).Distinct();

            _allUsers_Assignment = [];
            foreach(int? id in distinctIDs)
            {
                if (id != null)
                {
                    IEnumerable<AllUsers_Assignment>? users = usersToAssign_Raw.Where(q => q.AppUserID == id);
                    _allUsers_Assignment.AddRange(users);
                }
            }

            _allUsers_Assignment = [.. _allUsers_Assignment.OrderBy(s => s?.UserName)];
        }

        private void RolesMultiSelectChanged(List<string>? newValues)
        {
            _selectedRoles = newValues ?? [];
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private async Task SessionChanged(string newValue)
        {
            await Task.Delay(1);
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            StateHasChanged();
        }

        private void TitlesMultiSelectChanged(List<string>? newValues)
        {
            _selectedTitles = newValues ?? [];
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private List<AllUsers_Assignment> UsersInSelectedRoles()
        {
            List<AllUsers_Assignment> usersToAssign = [];

            foreach (string? role in _selectedRoles)
            {
                int? roleID = (_roles?.FirstOrDefault(q => q?.Value == role)?.ID) ?? throw new NullReferenceException($"Unable to find a role description for role {role}.");
                IEnumerable<AllUsers_CMS_DB>? usersInRole = _allUsers_DB?.Where(x => x.RoleID == roleID);

                AddAssignedUsers(usersInRole, usersToAssign);
            }

            return usersToAssign;
        }

        private List<AllUsers_Assignment> UsersInSelectedTitles()
        {
            List<AllUsers_Assignment> usersToAssign = [];

            foreach (string? title in _selectedTitles)
            {
                int? titleID = (_titles?.FirstOrDefault(q => q?.Value == title)?.ID) ?? throw new NullReferenceException($"Unable to find a title description for title {title}.");
                IEnumerable<AllUsers_CMS_DB>? usersInTitle = _allUsers_DB?.Where(x => x.TitleID == titleID);

                AddAssignedUsers(usersInTitle, usersToAssign);
            }

            return usersToAssign;
        }
    }
}
