using Microsoft.VisualBasic;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using Telerik.Windows.Documents.Model.Drawing.Charts;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class ReportServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<EMailReportBySessionIdModel?>?> GetEMailingsBySessionIDAndTestResultsByUserID
            (int? sessionID, IEnumerable<AllUsers_CMS_DB?>? allUsers_CMS_DB, IEnumerable<IdValue<int>?>? roles, IEnumerable<IdValue<int>?>? titles, AllUsers_Notaries?[]? notaries, IDatabase? database)
        {
            if (allUsers_CMS_DB == null || allUsers_CMS_DB.Any() == false)
                throw new NoNullAllowedException("allUsers_CMS_DB cannot be null or empty in GetEMailingsBySessionID().");
            else
            {
                // GET ALL EMAILINGS (BUT NOT TEST RESULTS - THAT'S DOWN BELOW)
                IEnumerable<EMailReportBySessionIdModel?>? emailings =
                    (
                        await database!.QueryByStoredProcedureAsync
                            <EMailReportBySessionIdModel, object?>
                                ("usp_Training_Questionnaire_GetEmailingsBySessionID", new { Session_ID = sessionID })
                    );

                if (emailings == null || emailings.Any() == false)
                    return null;
                else
                {
                    DateTime? dueDate = (await GetDueDateBySessionID(sessionID, database!))?.DueDate;
                    DateTime today = DateTime.Today;

                    // GET STATUSES, ROLES AND TITLES. DON'T REFACTOR THIS CODE
                    foreach (EMailReportBySessionIdModel? user_OPS in emailings!)
                    {
                        if (user_OPS != null)
                        {
                            IEnumerable<ScoresAndWhenSubmittedModel?>? scores =
                                await GetScoresBySessionIDandUserID(sessionID!.Value, user_OPS.CMS_User_ID!.Value, database!);

                            bool noAttempts = (scores == null || scores.Any() == false);

                            user_OPS.WhenUserLastSubmitted = (noAttempts == true) ? null : scores!.Max(q => q?.WhenSubmitted);
                            user_OPS.Status = GetStatus(noAttempts, dueDate, today, user_OPS.WhenUserLastSubmitted, scores);
                            user_OPS.Role = GetRoleName(user_OPS.CMS_User_ID, allUsers_CMS_DB, roles, notaries);
                            user_OPS.Title = GetTitleName(user_OPS.CMS_User_ID, allUsers_CMS_DB, titles);
                            user_OPS.DueDate = dueDate;
                        }
                    }
                    
                    // GET RAW TEST RESULTS, ADD THEM AS ADDITIONAL RECORDS (IF ANY).
                    ResultsModel?[]? testResults_Raw =
                        (
                            await database!.QueryByStoredProcedureAsync
                                <ResultsModel?, object?>
                                    ("usp_Training_Questionnaire_GetResultsBySessionID", new { Session_ID = sessionID })
                        )?.ToArray();

                    List<EMailReportBySessionIdModel> results = [];

                    if (testResults_Raw == null)   // IF NO TEST RESULTS, JUST RETURN WHAT WE HAVE SO FAR
                        results.AddRange(emailings!);
                    else
                    {
                        // ADD TEST RESULTS TO THE RECORDS. THIS MAY CREATE MULTIPLE RECORDS PER USER (ONE PER TEST TAKEN)
                        foreach (EMailReportBySessionIdModel? emailing in emailings)
                        {
                            if (emailing != null)
                            {
                                emailing.DueDate = dueDate;
                                IEnumerable<ResultsModel?>? tests =
                                    testResults_Raw.Where(q => q?.OPS_User_ID == emailing.OPS_Emp_ID || q?.CMS_User_ID == emailing.CMS_User_ID);

                                if (tests == null || tests.Any() == false)
                                    results.Add(emailing);
                                else
                                {
                                    foreach (ResultsModel? test in tests)
                                    {
                                        if (test != null)
                                        {
                                            EMailReportBySessionIdModel recordToAdd = emailing; // THIS IS WHY WE USE A RECORD AND NOT A CLASS

                                            recordToAdd.QuestionnaireNumber = test.QuestionnaireNumber;
                                            recordToAdd.Score = test.Score;
                                            recordToAdd.WhenUserLastSubmitted = test.WhenSubmitted;
                                            recordToAdd.WhenMustRetakeBy = test.WhenMustRetakeBy;
                                            results.Add(recordToAdd);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return results.OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ThenBy(s => s.QuestionnaireNumber);
                }
            }
        }

        public void UpdateReminderEmailing(int? emailingID, string? senderName, IDatabase? database) =>
            database!.NonQueryByStoredProcedure<object?>("usp_Training_Questionnaire_UpdateReminderEmailing", new { ID = emailingID, SenderName = senderName });

// =============================================================================================================================================================================================================================================================================================================================================================================================================

        private string? GetRoleName(int? cmsUserID, IEnumerable<AllUsers_CMS_DB?>? allUsers_CMS, IEnumerable<IdValue<int>?>? roles, AllUsers_Notaries?[]? notaries)
        {
            if (cmsUserID == null)
                return null;
            else
            {
                int? roleID = allUsers_CMS?.FirstOrDefault(q => q?.AppUserID == cmsUserID)?.RoleID;
                string? roleName = (roleID != null) ? roles?.FirstOrDefault(q => q?.ID == roleID.Value)?.Value : null;

                if (roleName != null && roleName.Contains(Globals.Notary, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    if (notaries?.Any(q => q?.CMS_User_ID == cmsUserID) == true)
                        roleName = $"{roleName} ({Globals.Notary})";
                }

                return roleName;
            }
        }

        private string? GetStatus(bool noAttempts, DateTime? dueDate, DateTime today, DateTime? whenUserLastSubmitted, IEnumerable<ScoresAndWhenSubmittedModel?>? scores)
        {
            if (noAttempts == true)
                return (dueDate < today) ? "Overdue" : "Not Attempted";
            else if (scores!.Any(q => q?.Score >= Globals.TestPassingThreshold) == true)
                return (whenUserLastSubmitted == null) ? "--NULL--" : (whenUserLastSubmitted?.Date > dueDate) ? $"{Globals.Passed} (late)" : Globals.Passed;
            /*
            else if (scores!.Count() < Globals.MaximumTestAttemptsPerSession)
                return "Incomplete";
            */
            else
                return Globals.Failed;
        }

        private string? GetTitleName(int? cmsUserID, IEnumerable<AllUsers_CMS_DB?>? allUsers_CMS, IEnumerable<IdValue<int>?>? titles)
        {
            if (cmsUserID == null)
                return null;
            else
            {
                int? titleID = allUsers_CMS?.FirstOrDefault(q => q?.AppUserID == cmsUserID)?.TitleID;
                return (titleID != null) ? titles?.FirstOrDefault(q => q?.ID == titleID.Value)?.Value : null;
            }
        }
    }
}
