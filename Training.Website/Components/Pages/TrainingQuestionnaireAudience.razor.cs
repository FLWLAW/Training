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
        private TelerikGrid<AllUsers_Assignment>? _allUsers_Assignment_ExportGrid;
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
                        FirstName = user?.FirstName,
                        Selected = true
                    };
                    usersToAssign.Add(assignedUsers);
                }
            }
        }

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
            
            List<int> usersAsssigned = [];
            
            _allUsers_Assignment.Clear();
            foreach (AllUsers_Assignment user in usersToAssign_Raw)
            {
                int? userID = user.AppUserID;

                if (userID != null && usersAsssigned.Contains(userID.Value) == false)
                {
                    _allUsers_Assignment.Add(user);
                    usersAsssigned.Add(userID.Value);
                }
            }

            _allUsers_Assignment = [.._allUsers_Assignment.OrderBy(s => s?.UserName)];
        }

        private void RolesMultiSelectChanged(List<string>? newValues)
        {
            _selectedRoles = newValues ?? [];
            RecompileAssignedUsers();
            StateHasChanged();
        }

        private async Task SendEmailsToSelected()
        {
            IEnumerable<AllUsers_Assignment?>? recipients = _allUsers_Assignment.Where(u => u != null && u.Selected == true);

#if DEBUG
            EMailer email = new();
            StringBuilder testMessageBody = new("HERE ARE WHAT THE EMAILS WOULD LOOK LIKE IN PRODUCTION MODE:");

            testMessageBody.Append("<br /><br />");
            foreach (AllUsers_Assignment? recipient in recipients)
            {
                string message = $"Dear {recipient?.FirstName},<br/><br/>You have been selected to complete the training questionnaire for the training session \"{_selectedSession?.DocTitle}\" (Session ID: {_selectedSession?.Session_ID}).<br/><br/>Please click on the link below to access the questionnaire:<br/><a href='https://yourtrainingwebsite.com/questionnaire?sessionId={_selectedSession?.Session_ID}'>Complete Training Questionnaire</a><br/><br/>Thank you for your participation!<br/><br/>Best regards,<br/>Training Team";

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
            }

            email.BodyTextFormat = MimeKit.Text.TextFormat.Html;
            email.Subject = $"Training Questionnaire Available for Session #{_selectedSession?.Session_ID}";
            email.Body = testMessageBody;

            //AllUsers_CMS_DB? susan = _allUsers_DB?.FirstOrDefault(q => q.UserName == "Susan Eisenman");

            email.To.Add(new MailboxAddress("David Rosenblum", "drosenblum@bluetrackdevelopment.com"));
            //email.To.Add(new MailboxAddress(susan?.UserName, susan?.EmailAddress));
            email.Send();
#else
            foreach (AllUsers_Assignment? recipient in recipients)
            {
                MailboxAddress address = new(recipient!.UserName ?? string.Empty, recipient!.EmailAddress!);
                string body = $"Dear {recipient?.FirstName},<br/><br/>You have been selected to complete the training questionnaire for the training session \"{_selectedSession?.DocTitle}\" (Session ID: {_selectedSession?.Session_ID}).<br/><br/>Please click on the link below to access the questionnaire:<br/><a href='https://yourtrainingwebsite.com/questionnaire?sessionId={_selectedSession?.Session_ID}'>Complete Training Questionnaire</a><br/><br/>Thank you for your participation!<br/><br/>Best regards,<br/>Training Team";
                EMailer email = new()
                {
                    BodyTextFormat = MimeKit.Text.TextFormat.Html,
                    Subject = $"Training Questionnaire Available for Session #{_selectedSession?.Session_ID}",
                    Body = new StringBuilder(body),
                    To = [address]
                };
                email.Send();
            }
#endif
            await Task.Delay(1);
        }

        private async Task SessionChanged(string newValue)
        {
            //TODO: MAKE THIS METHOD SYNCHRONOUS??

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
