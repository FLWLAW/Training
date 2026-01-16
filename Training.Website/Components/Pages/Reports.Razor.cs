using FLWLAW_Email.Library;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MimeKit;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Data.Common;
using System.Text;
using Telerik.Blazor.Components;
using Telerik.Documents.SpreadsheetStreaming;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Spreadsheet.Model;
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

        [Inject]
        private IJSRuntime? JS { get; set; }
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
        /*
        private void SetCellValue(IRowExporter row, object? value)
        {
            using (ICellExporter cell = row.CreateCellExporter())
            {
                if (value == null)
                    cell.SetValue(string.Empty);
                else if (value is bool boolValue)
                    cell.SetValue(boolValue);
                else if (value is DateTime dateTimeValue)
                {
                    string format = (dateTimeValue.TimeOfDay.TotalSeconds > 0) ? "mm/dd/yyyy hh:mm:ss AM/PM" : "mm/dd/yyyy";

                    cell.SetFormat(new SpreadCellFormat { NumberFormat = format });
                    cell.SetValue(dateTimeValue);
                }
                else if (value is decimal decValue)
                    cell.SetValue((double)decValue);
                else if (value is double dblValue)
                    cell.SetValue(dblValue);
                else if (value is long longValue)
                    cell.SetValue(longValue);
                else if (value is int intValue)
                    cell.SetValue(intValue);
                else if (value is string stringValue)
                    cell.SetValue(stringValue);
                else
                    cell.SetValue(value.ToString()!);
            }
        }

        private void SetDataChildRow(IWorksheetExporter worksheet, ResultsModel? test)
        {
            if (test != null)
            {
                using (IRowExporter childDataRow = worksheet.CreateRowExporter())
                {
                    childDataRow.SkipCells(1);
                    SetCellValue(childDataRow, test.QuestionnaireNumber);
                    childDataRow.SkipCells(1);
                    SetCellValue(childDataRow, test.Score);
                    childDataRow.SkipCells(1);
                    SetCellValue(childDataRow, test.WhenSubmitted);
                    childDataRow.SkipCells(1);
                    SetCellValue(childDataRow, test.WhenMustRetakeBy);
                }
            }
        }

        private void SetDataParentRow(IWorksheetExporter worksheet, EMailReportBySessionIdModel? row)
        {
            if (row != null)
            {
                using (IRowExporter dataRow = worksheet.CreateRowExporter())
                {
                    SetCellValue(dataRow, row.Session_ID);
                    SetCellValue(dataRow, row.FirstName);
                    SetCellValue(dataRow, row.LastName);
                    SetCellValue(dataRow, row.Email);
                    SetCellValue(dataRow, row.Role);
                    SetCellValue(dataRow, row.Title);
                    SetCellValue(dataRow, row.DueDate);
                    SetCellValue(dataRow, row.Status);
                    SetCellValue(dataRow, row.WhoFirstSent);      // First Sent By
                    SetCellValue(dataRow, row.WhenFirstSent);     // When First Sent
                    SetCellValue(dataRow, row.WhoLastUpdated);    // Last Sent By
                    SetCellValue(dataRow, row.WhenLastUpdated);   // Latest Email ASent On
                    SetCellValue(dataRow, row.WhenUserLastSubmitted); // WHen User Last Submitted
                    SetCellValue(dataRow, row.WhenLastReminderEmailSent);     // Reminder Email Sent On
                    SetCellValue(dataRow, row.WhoLastReminderEmailSent);      // Reminder Email Sent By
                }
            }
        }

        private void SetHeader(IRowExporter row, string header) // USED FOR SECTION OR COLUMN HEADERS
        {
            using (ICellExporter cell = row.CreateCellExporter())
            {
                cell.SetFormat(new SpreadCellFormat { IsBold = true, Fill = SpreadPatternFill.CreateSolidFill(new SpreadColor(211, 211, 211)) });
                cell.SetValue(header);
            }
        }

        private void SetMainHeaders(IWorksheetExporter worksheet)
        {
            using (IRowExporter headerRow = worksheet.CreateRowExporter())
            {
                SetHeader(headerRow, "Session ID");
                SetHeader(headerRow, "First Name");
                SetHeader(headerRow, "Last Name");
                SetHeader(headerRow, "Email");
                SetHeader(headerRow, "Role");
                SetHeader(headerRow, "Title");
                SetHeader(headerRow, "Due Date");
                SetHeader(headerRow, "Status");
                SetHeader(headerRow, "First Sent By");
                SetHeader(headerRow, "When First Sent");
                SetHeader(headerRow, "Last Sent By");
                SetHeader(headerRow, "Latest Email Sent On");
                SetHeader(headerRow, "When User Last Submitted");
                SetHeader(headerRow, "Reminder Email Sent On");
                SetHeader(headerRow, "Reminder Email Sent By");
            }
        }



        private void SetSubHeaders(IWorksheetExporter worksheet)
        {
            using (IRowExporter subHeaderRow = worksheet.CreateRowExporter())
            {
                subHeaderRow.SkipCells(1);
                SetHeader(subHeaderRow, "Questionnaire #");
                subHeaderRow.SkipCells(1);
                SetHeader(subHeaderRow, "Score");
                subHeaderRow.SkipCells(1);
                SetHeader(subHeaderRow, "When Submitted");
                subHeaderRow.SkipCells(1);
                SetHeader(subHeaderRow, "When Must Retake By");
            }
        }

        private async Task SpecialExportExcelMain()
        {
            using (MemoryStream stream = await SpecialExportExcelXLSX())
            {
                string filename = $"TestResults_Session_{_selectedSession!.Session_ID}.xlsx";

                stream.Position = 0;    // VERY IMPORTANT!!!!!
                using var streamRef = new DotNetStreamReference(stream: stream);
                await JS!.InvokeVoidAsync("downloadFileFromStream", filename, streamRef);
            }
        }

        private async Task<MemoryStream> SpecialExportExcelXLSX()
        {
            if (_emailedUsers != null)
            {
                MemoryStream stream = new();    // CANNOT USE "using" HERE, BECAUSE "using" WILL CLOSE THE STREAM BEFORE IT IS RETURNED

                using (IWorkbookExporter workbook = SpreadExporter.CreateWorkbookExporter(SpreadDocumentFormat.Xlsx, stream))
                {
                    string name = $"Reports_{_selectedSession!.Session_ID}";

                    using (IWorksheetExporter worksheet = workbook.CreateWorksheetExporter(name))
                    {
                        SetMainHeaders(worksheet);
                        foreach (EMailReportBySessionIdModel? row in _emailedUsers)
                        {
                            if (row != null)
                            {
                                SetDataParentRow(worksheet, row);
                                IOrderedEnumerable<ResultsModel?>? tests = Tests(row);

                                if (tests != null && tests.Any() == true)
                                {
                                    SetSubHeaders(worksheet);
                                    foreach (ResultsModel? test in tests)
                                        SetDataChildRow(worksheet, test);
                                }
                            }
                        }
                    }
                }
                return await Task.FromResult(stream);
            }
            else
                throw new NoNullAllowedException("_emailedUsers cannot be null in SPecialExcportExcelXLSX().");
        }
        
        private async Task SpecialExportExcelCSV()
        {
            if (_emailedUsers != null)
            {
                StringBuilder sheet = new();

                sheet.AppendLine("Session ID,First Name,Last Name,Email,Role,Title,Due Date,Status,First Sent By,When First Sent,Last Sent By,Latest Email Sent On,When User Last Submitted,Reminder Email Sent On,Reminder Email Sent By");
                foreach (EMailReportBySessionIdModel? row in _emailedUsers)
                {
                    if (row != null)
                    {
                        AppendToLine(sheet, row.Session_ID);
                        AppendToLine(sheet, row.FirstName);
                        AppendToLine(sheet, row.LastName);
                        AppendToLine(sheet, row.Email);
                        AppendToLine(sheet, row.Role);
                        AppendToLine(sheet, row.Title);
                        AppendToLine(sheet, row.DueDate);
                        AppendToLine(sheet, row.Status);
                        AppendToLine(sheet, row.WhoFirstSent);      // First Sent By
                        AppendToLine(sheet, row.WhenFirstSent);     // When First Sent
                        AppendToLine(sheet, row.WhoLastUpdated);    // Last Sent By
                        AppendToLine(sheet, row.WhenLastUpdated);   // Latest Email ASent On
                        AppendToLine(sheet, row.WhenUserLastSubmitted); // WHen User Last Submitted
                        AppendToLine(sheet, row.WhenLastReminderEmailSent);     // Reminder Email Sent On
                        AppendToLine(sheet, row.WhoLastReminderEmailSent, true);      // Reminder Email Sent By

                        IOrderedEnumerable<ResultsModel?>? tests = Tests(row);
                        if (tests != null && tests.Any() == true)
                        {
                            sheet.AppendLine(",Questionnaire #,,Score,,When Submitted,,When Must Retake By");
                            foreach (ResultsModel? test in tests)
                                if (test != null)
                                    sheet.AppendLine($",{test.QuestionnaireNumber},,{test.Score},,{test.WhenSubmitted},,{test.WhenMustRetakeBy}");
                        }
                    }
                }

                string csvFilename = $"TestResults_Session_{_selectedSession!.Session_ID}";
                await JS!.InvokeVoidAsync("downloadFile", csvFilename, sheet.ToString());
            }
        }
        */

        private IOrderedEnumerable<ResultsModel?>? Tests(EMailReportBySessionIdModel? row) =>
            _results?.Where(q => q?.OPS_User_ID == row?.OPS_Emp_ID || (row?.OPS_Emp_ID == null && row?.CMS_User_ID == row?.CMS_User_ID)).OrderBy(s => s?.QuestionnaireNumber);
        
        /*
        private void AppendToLine(StringBuilder sheet, object? value, bool lastFieldInLine = false, int separators = 1)
        {
            string? valueStr = value?.ToString();

            if (valueStr != null && (valueStr.Contains(',') || valueStr.Contains("\"") || valueStr.Contains('\n')))
                valueStr = $"\"{valueStr.Replace("\"", "\"\"")}\"";

            if (lastFieldInLine == false)
                sheet.Append($"{valueStr},");
            else
                sheet.AppendLine(valueStr);
        }
        */  

        private string Subject() =>
            $"Subject: Reminder: Training Questionnaire Available for Session #{_selectedSessionString}";
    }
}
