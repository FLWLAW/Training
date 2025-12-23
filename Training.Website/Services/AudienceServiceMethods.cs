using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class AudienceServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<IdValue<int>?>?> GetAllReports(IDatabase? database) =>
            await database!.QueryByStoredProcedureForDropDownControlAsync<int>("usp_Report_SA", "ReportID", "ReportDesc");

        public async Task<IEnumerable<StagesReportsModel?>?> GetAllStages(IDatabase? database)
        {
            StagesReportsModel_Parameters parameters = new();   // LEAVE PROPERTIES AS NULL

            IEnumerable<StagesReportsModel?> results = await database!.QueryByStoredProcedureAsync<StagesReportsModel?, StagesReportsModel_Parameters?>("usp_StageList_SAByStateListReportIDList", parameters);
            //await database!.QueryByStoredProcedureAsync<StageModel?>("usp_StageList_SA");

            return results;
        }
        /*
        public async Task<IEnumerable<Reports_Username_ReportDesc_StageName_Model?>?> GetReportsUsersReportDescriptionsStageNames(IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<Reports_Username_ReportDesc_StageName_Model?>("usp_ReportSelectionStage_SA");
        */
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
