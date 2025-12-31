using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class AudienceServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<IdValue<int>?>?> GetAllReports(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Report_SA", "ReportID", "ReportDesc");

        public async Task<IEnumerable<WorklistGroupsAndReportsModel?>?> GetAllWorklistGroupsWithReports(IDatabase? database)
        {
            StagesReportsModel_Parameters parameters = new();   // LEAVE PROPERTIES AS NULL
            IEnumerable<WorklistGroupsAndReportsModel?> results =
                await database!.QueryByStoredProcedureAsync<WorklistGroupsAndReportsModel?, StagesReportsModel_Parameters?>("usp_StageList_SAByStateListReportIDList", parameters);

            return results;
        }

        // MAY NEED TO MOVE TO CommonMethods
        public async Task<IEnumerable<AllUsers_OPS_DB?>?> GetAllUsers_OPS_DB(IDatabase? database) =>
            await database!.QueryByStatementAsync<AllUsers_OPS_DB?>("SELECT Emp_ID, UserName FROM [Employees Tbl]");

        public void UpsertEMailingRecord(AllUsers_Assignment? recipient, int? session_ID, string? sendingUser, IDatabase? database)
        {
            UpsertEMailings_Parameters parameters = new()
            {
                CMS_User_ID = recipient!.AppUserID!.Value,
                Session_ID = session_ID!.Value,
                SendingUser = sendingUser!,
                EMailedUserLastName = recipient.LastName!,
                EMailedUserFirstName = recipient.FirstName!,
                EMailedUserLogin_ID = recipient.UserName!
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
