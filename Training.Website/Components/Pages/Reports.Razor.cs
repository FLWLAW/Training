using FLWLAW_Email.Library;
using Microsoft.AspNetCore.Components;
using MimeKit;
using SqlServerDatabaseAccessLibrary;
using System.Text;
using Telerik.Blazor.Components;
using Training.Website.Models;
using Training.Website.Models.Users;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class Reports
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
        private bool _reminderEmailsSentWindowVisible = false;
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
        private readonly ReportServiceMethods _service = new();
        private TelerikGrid<EMailReportBySessionIdModel?>? _emailedReports = null;
        
        private readonly SqlDatabase _database_CMS = new(Configuration.DatabaseConnectionString_CMS()!);
        #endregion

        #region PRIVATE CONSTANTS
        private const string _failed = "Failed";
        private const string _passed = "Passed";
        #endregion

        protected override async Task OnInitializedAsync()
        {
            IEnumerable<SessionInformationModel>? sessionInfo = await _service.GetSessionInformation(Database_OPS);

            _sessions = Globals.ConcatenateSessionInfoForDropDown(sessionInfo);
            _roles = await _service.GetAllRoles(true, _database_CMS);
            _titles = await _service.GetAllTitles(_database_CMS);
            _allUsers_CMS = await _service.GetAllUsers_CMS_DB(_database_CMS);
            _notaries = (await _service.GetNotaries(_allUsers_CMS, Database_OPS))?.ToArray();
        }

        // ===========================================================================================================================================================================================================================================================================================================================================

        private StringBuilder EMailMessage()
        {
            StringBuilder message = new();

            message.Append($"<b>REMINDER:</b> You have been selected to complete the training questionnaire for the session {_selectedSession?.DocTitle}.<br/>");
            message.Append($"Session ID: {_selectedSession?.Session_ID}<br/>");
            message.Append($"Due Date: {_dueDate?.ToShortDateString()}");   //   ToString("MMMM dd, yyyy")}<br/><br/>");
            message.Append("<br/><br/>");
            message.Append("Please click on the link below to access the questionnaire:<br/>");
            message.Append($"<a href='{Globals.BaseURL}/?SessionID={_selectedSession?.Session_ID}'>Training Questionnaire</a><br/><br/>");
            message.Append("<br/><br/>");
            message.Append("Thank you,<br/>");
            message.Append("Compliance Team");

            return message;
        }

        private void EmailsSentCloseClicked() => _reminderEmailsSentWindowVisible = false;

        private string FullName(EMailReportBySessionIdModel? recipient) => $"{recipient?.FirstName?.Trim() ?? string.Empty} {recipient?.LastName?.Trim() ?? string.Empty}";

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
                    user_OPS.DueDate = _dueDate;
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
            else if (scores!.Any(q => q?.Score >= Globals.TestPassingThreshold) == true)
                return (whenUserLastSubmitted == null) ? "--NULL--" : (whenUserLastSubmitted?.Date > _dueDate) ? $"{_passed} (late)" : _passed;
            /*
            else if (scores!.Count() < Globals.MaximumTestAttemptsPerSession)
                return "Incomplete";
            */
            else
                return _failed;
        }

        private void LogEMailingToDB(int? id, string? username) => _service.UpdateReminderEmailing(id, username, Database_OPS);

        private bool ReminderEmailEligible(EMailReportBySessionIdModel? recipient) =>
            recipient != null && recipient.Email != null && recipient.Status != null && recipient.Status.Contains(_passed) == false && recipient.Status.Contains(_failed) == false;

        private void SendReminderEmails()
        {
            if (_emailedUsers != null && _emailedUsers.Any() == true)
            {
#if DEBUG || QA
                EMailer email = new();
                StringBuilder message = EMailMessage();
                StringBuilder testMessageBody = new("HERE ARE WHAT THE REMINDER EMAILS WILL LOOK LIKE IN PRODUCTION MODE:");

                foreach (EMailReportBySessionIdModel? recipient in _emailedUsers)
                {
                    if (ReminderEmailEligible(recipient) == true)
                    {
                        testMessageBody.Append("<br /><br />");
                        testMessageBody.Append("-------------------------------------------------------------------------------------------------------------------------------------------------------------");
                        testMessageBody.Append("<br /><br />");
                        testMessageBody.Append($"From: {email.From?.Name} &lt{email.From?.Address}&gt");
                        testMessageBody.Append("<br />");
                        testMessageBody.Append($"To: {FullName(recipient)} &lt{recipient?.Email}&gt");
                        testMessageBody.Append("<br />");
                        testMessageBody.Append($"Subject: {Subject()}");
                        testMessageBody.Append("<br /><br />");
                        testMessageBody.Append(message);
                        LogEMailingToDB(recipient?.ID, ApplicationState!.LoggedOnUser!.UserName);
                    }
                }

                email.BodyTextFormat = MimeKit.Text.TextFormat.Html;
                email.Subject = $"Reminder: Training Questionnaire Available for Session #{_selectedSessionString}";
                email.Body = testMessageBody;
#if QA
                AllUsers_CMS_DB? susan = _allUsers_CMS?.FirstOrDefault(q => q?.UserName == "Susan Eisenman");
                email.To.Add(new MailboxAddress(susan?.UserName, susan?.EmailAddress));
#endif
                email.To.Add(new MailboxAddress("David Rosenblum", "drosenblum@bluetrackdevelopment.com"));
                email.Send();
                _reminderEmailsSentWindowVisible = true;
#else
                IEnumerable<EMailReportBySessionIdModel?>? usersToRemind = _emailedUsers.Where(q => ReminderEmailEligible(q) == true);

                if (usersToRemind != null && usersToRemind.Any() == true)
                {
                    foreach (EMailReportBySessionIdModel? recipient in usersToRemind)
                    {
                        MailboxAddress address = new(FullName(recipient), recipient!.Email);

                        EMailer email = new()
                        {
                            BodyTextFormat = MimeKit.Text.TextFormat.Html,
                            Subject = Subject(),
                            Body = EMailMessage(),
                            To = [address]
                        };

                        email.Send();
                        LogEMailingToDB(recipient.ID, ApplicationState!.LoggedOnUser!.UserName);
                    }

                    _reminderEmailsSentWindowVisible = true;
                    StateHasChanged();
                }
#endif
            }
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

        private string Subject() =>
            $"Subject: Reminder: Training Questionnaire Available for Session #{_selectedSessionString}";
    }
}
