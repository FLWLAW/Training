using FLWLAW_Email.Library;
using Microsoft.AspNetCore.Components;
using MimeKit;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Text;
using Telerik.Blazor.Components;
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
        private bool _emailsSentWindowVisible = false;
        private readonly SqlDatabase? _dbCMS = new(Configuration.DatabaseConnectionString_CMS()!);
        private readonly AudienceServiceMethods _service = new();

        private IEnumerable<string>? _sessions = null;
        private string? _selectedSessionString = null;
        private SessionInformationModel? _selectedSession = null;
        private bool _sessionAlreadyExistsInDueDatesTable = false;
        
        private IEnumerable<IdValue<int>?>? _roles = null;
        private List<string> _selectedRoles = [];
        
        private IEnumerable<IdValue<int>?>? _titles = null;
        private List<string> _selectedTitles = [];
        
        private IEnumerable<IdValue<int>?>? _reports = null;
        private List<string> _selectedReports = [];

        private IEnumerable<WorklistGroupsAndReportsModel?>? _worklistGroupsReports = null;
        private IEnumerable<string?>? _worklistGroupsBySelectedReports = [];
        private List<String> _selectedWorklistGroups = [];
        
        private readonly DateTime _minimumDueDate = DateTime.Now.AddDays(1);
        private DateTime? _dueDate = null;
        private AllUsers_CMS_DB?[]? _allUsers_CMS_DB = null;
        private AllUsers_OPS_DB?[]? _allUsers_OPS_DB = null;
        private AllUsers_Assignment?[]? _allUsers_Assignment = null;
        private IEnumerable<AllUsers_Notaries?>? _notaries = null;
        private TelerikGrid<AllUsers_Assignment>? _allUsers_Assignment_ExportGrid;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            IEnumerable<SessionInformationModel>? sessionsInfo = await _service.GetSessionInformation(Database_OPS);

            if (sessionsInfo != null && sessionsInfo.Any() == true)
            {
                _allUsers_CMS_DB = (await _service.GetAllUsers_CMS_DB(_dbCMS))?.ToArray();
                _allUsers_OPS_DB = (await _service.GetAllUsers_OPS_DB(Database_OPS))?.ToArray();
                _roles = await _service.GetAllRoles(true, _dbCMS);
                _titles = await _service.GetAllTitles(_dbCMS);
                _reports = await _service.GetAllReports(_dbCMS);
                _worklistGroupsReports = await _service.GetAllWorklistGroupsWithReports(_dbCMS);
                _notaries = await _service.GetNotaries(_allUsers_CMS_DB, Database_OPS);

                _sessions = Globals.ConcatenateSessionInfoForDropDown(sessionsInfo);
                _selectedSessionString = ApplicationState!.SessionID_String;
                _dueDate = _minimumDueDate;
                if (string.IsNullOrWhiteSpace(_selectedSessionString) == false)
                    await SessionChanged(_selectedSessionString);
            }
        }

        // ================================================================================================================================================================================================================================================================================================

        private List<AllUsers_Assignment>? AddAssignedUsers(IEnumerable<AllUsers_CMS_DB>? users)
        {
            if (users == null || users.Any() == false)
                return null;
            else
            {
                List<AllUsers_Assignment> usersToAssign = [];

                foreach (AllUsers_CMS_DB? user in users)
                {
                    AllUsers_Assignment? assignedUsers = new()
                    {
                        AppUserID = user?.AppUserID,
                        UserName = user?.UserName,
                        EmailAddress = user?.EmailAddress,
                        RoleDesc = _roles?.FirstOrDefault(q => q?.ID == user?.RoleID)?.Value,
                        TitleDesc = _titles?.FirstOrDefault(q => q?.ID == user?.TitleID)?.Value,
                        FirstName = user?.FirstName,
                        LastName = user?.LastName
                    };
                    usersToAssign.Add(assignedUsers);
                }

                return usersToAssign;
            }
        }

        private void AddToLoginIDs(List<string?> loginIDs, WorklistGroupsAndReportsModel worklistGroup_Report)
        {
            if (worklistGroup_Report.AssignedUserList != null)
                loginIDs!.AddRange(worklistGroup_Report.AssignedUserList.Split(';'));

            if (worklistGroup_Report?.TempAssignedUserList != null)
                loginIDs!.AddRange(worklistGroup_Report.TempAssignedUserList.Split(';'));

            loginIDs = [.. loginIDs.Where(q => q != ";").Distinct()];
        }

        private void AddToUsersToAssign(List<AllUsers_Assignment> usersToAssign, in List<string?> loginIDs)
        {
            foreach (string? loginID in loginIDs)
            {
                if (loginID != null)
                {
                    AllUsers_CMS_DB? userCMSDB = _allUsers_CMS_DB?.FirstOrDefault(q => q?.LoginID?.Equals(loginID, StringComparison.InvariantCultureIgnoreCase) == true);

                    if (userCMSDB != null)
                        usersToAssign.AddRange(AddAssignedUsers([userCMSDB])!);
                }
            }
        }

        private void DeSelectAllClicked()
        {
            if (_allUsers_Assignment != null && _allUsers_Assignment.Length > 0)
            {
                foreach (AllUsers_Assignment? assignedUser in _allUsers_Assignment)
                    if (assignedUser != null)
                        assignedUser.Selected = false;

                StateHasChanged();
            }
        }

        private StringBuilder EMailMessage(string? firstName)
        {
#if DEBUG
            const string baseURL = "http://drosenblum-elitedesk:83";
#elif QA
            const string baseURL = "http://drosenblum-elitedesk:8484";
#else
            const string baseURL = "http://training.efwlaw.com";
#endif
            StringBuilder message = new();

            message.Append($"Dear {firstName},<br/><br/>");
            message.Append($"You have been selected to complete the training questionnaire for the training session \"{_selectedSession?.DocTitle}\" (Session ID: {_selectedSession?.Session_ID}).<br/><br/>");
            message.Append("Please click on the link below to access the questionnaire:<br/>");
            message.Append($"<a href='{baseURL}/?SessionID={_selectedSession?.Session_ID}'>Training Questionnaire</a><br/><br/>");
            message.Append("Thank you for your participation!<br/><br/>");
            message.Append("Best regards,<br/>");
            message.Append("Compliance Team");

            return message;
        }

        private void EmailsSentCloseClicked() => _emailsSentWindowVisible = false;

        private void LogEMailingToDB(AllUsers_Assignment? recipient) =>
            _service.UpsertEMailingRecord(recipient, _selectedSession!.Session_ID, ApplicationState!.LoggedOnUser!.UserName, Database_OPS);

        private void OnRowClickHandler_AllUsers(GridRowClickEventArgs args)
        {
            if (args.Item is AllUsers_Assignment detail && args.Field == nameof(AllUsers_Assignment.Selected))
                detail.Selected = !detail.Selected;
        }

        private void RecompileAssignedUsers()
        {
            List<AllUsers_Assignment> usersToAssign_Raw = [];

            usersToAssign_Raw.AddRange(UsersInSelectedRoles());
            usersToAssign_Raw.AddRange(UsersInSelectedTitles());
            usersToAssign_Raw.AddRange(UsersInSelectedWorklistGroupsAndReports());

            List<AllUsers_Assignment?>? allUsers_Assignment = [];
            List<int> usersAsssigned = [];
            
            allUsers_Assignment.Clear();
            foreach (AllUsers_Assignment userToAssign in usersToAssign_Raw)
            {
                int? userID = userToAssign.AppUserID;

                if (userID != null && usersAsssigned.Contains(userID.Value) == false)
                {
                    allUsers_Assignment.Add(userToAssign);
                    usersAsssigned.Add(userID.Value);
                }
                else
                {
                    // IF USER ALREADY HAS BEEN GATHERED, APPEND ROLE INFO IF THEY ARE A NOTARY
                    AllUsers_Assignment? existingRecord = allUsers_Assignment.FirstOrDefault(q => q?.AppUserID == userID!.Value);

                    if (existingRecord != null)
                    {
                        if (existingRecord.RoleDesc != Globals.Notary && userToAssign.RoleDesc == Globals.Notary)
                            existingRecord.RoleDesc = $"{existingRecord.RoleDesc} ({Globals.Notary})";
                        else if (existingRecord.RoleDesc == Globals.Notary && userToAssign.RoleDesc != Globals.Notary)
                            existingRecord.RoleDesc = $"{userToAssign.RoleDesc} ({Globals.Notary})";
                    }
                }
            }

            // IF TITLE IS NULL/BLANK (WHICH CAN HAPPEN WHEN THE "NOTARY" ROLE IS SELECTED), FIX IT HERE
            foreach (AllUsers_Assignment? assignedUser in allUsers_Assignment)
            {
                if (string.IsNullOrWhiteSpace(assignedUser?.TitleDesc) == true)
                {
                    int? titleID = _allUsers_CMS_DB?.FirstOrDefault(q => q?.AppUserID == assignedUser?.AppUserID)?.TitleID;

                    if (titleID != null)
                        assignedUser!.TitleDesc = _titles?.FirstOrDefault(q => q?.ID == titleID)?.Value;
                }
            }

            _allUsers_Assignment = [..allUsers_Assignment.OrderBy(s => s?.UserName)];
        }

        private void ReportsMultiSelectChanged(List<string>? newValues)
        {
            _selectedReports = [];

            if (newValues != null && newValues.Count > 0)
            {
                foreach (string newValue in newValues)
                {
                    IdValue<int>? report = _reports?.FirstOrDefault(q => q?.Value?.Equals(newValue, StringComparison.InvariantCultureIgnoreCase) == true);

                    if (report != null && report.Value != null)
                        _selectedReports.Add(report.Value);
                }
            }

            _worklistGroupsBySelectedReports = WorklistGroupsBySelectedReports();
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private void RolesMultiSelectChanged(List<string>? newValues)
        {
            _selectedRoles = newValues ?? [];
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private void SendEmails()
        {
            IEnumerable<AllUsers_Assignment?>? recipients = _allUsers_Assignment?.Where(u => u != null && u.Selected == true);

            if (recipients != null && recipients.Any() == true)
            {

#if DEBUG || QA
                EMailer email = new();
                StringBuilder testMessageBody = new("HERE ARE WHAT THE EMAILS WOULD LOOK LIKE IN PRODUCTION MODE:");

                testMessageBody.Append("<br /><br />");
                foreach (AllUsers_Assignment? recipient in recipients)
                {
                    StringBuilder message = EMailMessage(recipient?.FirstName);

                    testMessageBody.Append("<br /><br />");
                    testMessageBody.Append("-------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    testMessageBody.Append("<br /><br />");
                    testMessageBody.Append($"From: {email.From?.Name} &lt{email.From?.Address}&gt");
                    testMessageBody.Append("<br />");
                    testMessageBody.Append($"To: {recipient?.UserName} &lt{recipient?.EmailAddress}&gt");
                    testMessageBody.Append("<br />");
                    testMessageBody.Append($"Subject: Training Questionnaire Available for Session #{_selectedSession?.Session_ID}");
                    testMessageBody.Append("<br /><br />");
                    testMessageBody.Append(message);
                    LogEMailingToDB(recipient);
                }

                email.BodyTextFormat = MimeKit.Text.TextFormat.Html;
                email.Subject = $"Training Questionnaire Available for Session #{_selectedSession?.Session_ID}";
                email.Body = testMessageBody;

                AllUsers_CMS_DB? susan = _allUsers_CMS_DB?.FirstOrDefault(q => q.UserName == "Susan Eisenman");
                email.To.Add(new MailboxAddress(susan?.UserName, susan?.EmailAddress));

                email.To.Add(new MailboxAddress("David Rosenblum", "drosenblum@bluetrackdevelopment.com"));

                email.Send();
#else
                foreach (AllUsers_Assignment? recipient in recipients)
                {
                    MailboxAddress address = new(recipient!.UserName ?? string.Empty, recipient!.EmailAddress!);
                    //string body = $"Dear {recipient?.FirstName},<br/><br/>You have been selected to complete the training questionnaire for the training session \"{_selectedSession?.DocTitle}\" (Session ID: {_selectedSession?.Session_ID}).<br/><br/>Please click on the link below to access the questionnaire:<br/><a href='https://yourtrainingwebsite.com/questionnaire?sessionId={_selectedSession?.Session_ID}'>Complete Training Questionnaire</a><br/><br/>Thank you for your participation!<br/><br/>Best regards,<br/>Compliance Team";

                    EMailer email = new()
                    {
                        BodyTextFormat = MimeKit.Text.TextFormat.Html,
                        Subject = $"Training Questionnaire Available for Session #{_selectedSession?.Session_ID}",
                        Body = EMailMessage(recipient?.FirstName),
                        To = [address]
                    };

                    email.Send();
                    LogEMailingToDB(recipient);
                }
#endif
            }
        }

        private void SendEmailsToSelectedAndLogToDB_Main()
        {
            UpsertDueDateToDB();
            SendEmails();
            _emailsSentWindowVisible = true;
            StateHasChanged();
        }

        private async Task SessionChanged(string newValue)
        {
            ApplicationState!.SessionID_String = newValue;
            _selectedSessionString = newValue;
            _selectedSession = Globals.ConvertSessionStringToClass(newValue);
            SessionDueDateModel? sessionDueDateInfo = await _service.GetDueDateBySessionID(_selectedSession!.Session_ID, Database_OPS!);

            if (sessionDueDateInfo != null)
            {
                _dueDate = sessionDueDateInfo.DueDate;
                _sessionAlreadyExistsInDueDatesTable = true;
            }
            else
            {
                _dueDate = _minimumDueDate;
                _sessionAlreadyExistsInDueDatesTable = false;
            }
            StateHasChanged();
        }

        private bool SessionSelected() => _selectedSession != null && _selectedSession.Session_ID != null && _selectedSession.Session_ID > 0;

        private void TitlesMultiSelectChanged(List<string>? newValues)
        {
            _selectedTitles = newValues ?? [];
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private void UpsertDueDateToDB() =>
            _service.UpsertSessionDueDate(_selectedSession!.Session_ID!.Value, _dueDate!.Value, ApplicationState!.LoggedOnUser?.LoginID, _sessionAlreadyExistsInDueDatesTable, Database_OPS!);

        private List<AllUsers_Assignment> UsersInSelectedRoles()
        {
            List<AllUsers_Assignment> usersToAssign = [];

            foreach (string? role in _selectedRoles)
            {
                if (role.Equals(Globals.Notary, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    int? roleID = (_roles?.FirstOrDefault(q => q?.Value == role)?.ID) ?? throw new NullReferenceException($"Unable to find a role description for role {role}.");
                    IEnumerable<AllUsers_CMS_DB>? usersInRole = _allUsers_CMS_DB?.Where(x => x.RoleID == roleID);

                    if (usersInRole != null && usersInRole.Any() == true)
                        usersToAssign.AddRange(AddAssignedUsers(usersInRole)!);
                }
                else
                {
                    foreach(AllUsers_Notaries? notary in _notaries!)
                    {
                        AllUsers_Assignment user = new()
                        {
                            AppUserID = notary.CMS_User_ID,
                            EmailAddress = notary.EMail,
                            FirstName = notary.FirstName,
                            LastName = notary.LastName,
                            UserName = notary.FullName,
                            RoleDesc = Globals.Notary,
                            TitleDesc = null
                        };
                        usersToAssign.Add(user);
                    }
                }
            }

            return usersToAssign;
        }

        private List<AllUsers_Assignment> UsersInSelectedWorklistGroupsAndReports()
        {
            List<AllUsers_Assignment> usersToAssign = [];
            List<string?> loginIDs = [];

            if (_selectedWorklistGroups != null && _selectedWorklistGroups.Count > 0)
            {
                foreach (string? selectedWorklistGroup in _selectedWorklistGroups)
                {
                    if (selectedWorklistGroup != null)
                    {
                        WorklistGroupsAndReportsModel? worklistGroupAndReport = _worklistGroupsReports?.FirstOrDefault(q => q?.StageFullName?.Equals(selectedWorklistGroup) == true);

                        if (worklistGroupAndReport != null)
                        {
                            AddToLoginIDs(loginIDs, worklistGroupAndReport);
                            AddToUsersToAssign(usersToAssign, loginIDs);
                        }
                    }
                }
            }
            else if (_selectedReports != null && _selectedReports.Count > 0)
            {
                foreach (string? selectedReport in _selectedReports)
                {
                    if (selectedReport != null)
                    {
                        IEnumerable<WorklistGroupsAndReportsModel?>? worklistGroupReports =
                            _worklistGroupsReports?.Where(q => q?.ReportName?.Equals(selectedReport, StringComparison.InvariantCultureIgnoreCase) == true);

                        if (worklistGroupReports != null)
                        {
                            foreach (WorklistGroupsAndReportsModel? worklistGroupAndReport in worklistGroupReports)
                                if (worklistGroupAndReport != null)
                                    AddToLoginIDs(loginIDs, worklistGroupAndReport);

                            if (loginIDs.Count > 0)
                                AddToUsersToAssign(usersToAssign, loginIDs);
                        }
                    }
                }
            }

            return usersToAssign;
        }

        private List<AllUsers_Assignment> UsersInSelectedTitles()
        {
            List<AllUsers_Assignment> usersToAssign = [];

            foreach (string? title in _selectedTitles)
            {
                int? titleID = (_titles?.FirstOrDefault(q => q?.Value == title)?.ID) ?? throw new NullReferenceException($"Unable to find a title description for title {title}.");
                IEnumerable<AllUsers_CMS_DB>? usersInTitle = _allUsers_CMS_DB?.Where(x => x.TitleID == titleID);

                usersToAssign.AddRange(AddAssignedUsers(usersInTitle)!);
            }

            return usersToAssign;
        }

        private void WorklistGroupsMultiSelectChanged(List<string>? newValues)
        {
            _selectedWorklistGroups = newValues ?? [];
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private IEnumerable<string?>? WorklistGroupsBySelectedReports()
        {
            List<string?> results = [];

            foreach (string selectedReport in _selectedReports)
            {
                IEnumerable<WorklistGroupsAndReportsModel?>? worklistGroups =
                    _worklistGroupsReports?.Where(q => q?.ReportName?.Equals(selectedReport, StringComparison.InvariantCultureIgnoreCase) == true);

                if (worklistGroups != null)
                    foreach (WorklistGroupsAndReportsModel? worklistGroup in worklistGroups)
                        if (worklistGroup != null && worklistGroup.StageFullName != null)
                            results.Add(worklistGroup.StageFullName);
            }

            return results.Order();
        }
    }
}
