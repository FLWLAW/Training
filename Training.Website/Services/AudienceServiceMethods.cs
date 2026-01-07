using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class AudienceServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<IdValue<int>?>?> GetAllReports(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Report_SA", "ReportID", "ReportDesc");

        public async Task<IEnumerable<int>?> GetAllOpsUserIDsAssignedToTasksBySessionID(int? sessionID, IDatabase? database) =>
            await database!.QueryByStatementAsync<int>($"SELECT DISTINCT Emp_ID FROM [TRAINING Tasks Tbl] WHERE TRAINING_ID = {sessionID} AND (IsDeleted = 0 OR IsDeleted IS NULL)");

        public async Task<IEnumerable<WorklistGroupsAndReportsModel?>?> GetAllWorklistGroupsWithReports(IDatabase? database)
        {
            StagesReportsModel_Parameters parameters = new();   // LEAVE PROPERTIES AS NULL
            IEnumerable<WorklistGroupsAndReportsModel?> results =
                await database!.QueryByStoredProcedureAsync<WorklistGroupsAndReportsModel?, StagesReportsModel_Parameters?>("usp_StageList_SAByStateListReportIDList", parameters);

            return results;
        }

        public void UpsertEMailingRecord(AllUsers_Display? recipient, int? sessionID, string? sendingUser, IDatabase? database)
        {
            UpsertEMailings_Parameters parameters = new()
            {
                CMS_User_ID = recipient!.CMS_UserID!.Value,
                OPS_User_ID = recipient.OPS_UserID!.Value,
                Session_ID = sessionID!.Value,
                SendingUser = sendingUser!,
                EMailedUserLastName = recipient.LastName!,
                EMailedUserFirstName = recipient.FirstName!,
                EMailedUserLogin_ID = recipient.LoginID!
            };

            database!.NonQueryByStoredProcedure("usp_Training_Questionnaire_UpsertEMailings", parameters);
        }

        public void UpsertSessionDueDate(int sessionID, DateTime dueDate, string? user, bool update, IDatabase? database)
        {
            UpsertDueDate_Parameters parameters = new()
            {
                Session_ID = sessionID,
                DueDate = dueDate,
                User = user,
                Update = update
            };

            database!.NonQueryByStoredProcedure("usp_Training_Questionnaire_UpsertDueDate", parameters);
        }
    }
}
