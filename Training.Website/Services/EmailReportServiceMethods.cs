using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class EmailReportServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<EMailReportBySessionIdModel?>?> GetEMailingsBySessionID(int? sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<EMailReportBySessionIdModel, object?>("usp_Training_Questionnaire_GetEmailingsBySessionID", new { Session_ID = sessionID });

        public async Task<IEnumerable<ResultsModel?>?> GetResultsBySessionID(int? sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<ResultsModel?, object?>("usp_Training_Questionnaire_GetResultsBySessionID", new { Session_ID = sessionID });

        /*
        public async Task<IEnumerable<ResultsModel?>?> GetResults(int? sessionID, int? opsUserID, int? cmsUserID, IDatabase? database)
        {
            ResultsModel_Parameters parameters = new()
            {
                Session_ID = sessionID,
                OPS_User_ID = opsUserID,
                CMS_User_ID = cmsUserID
            };

            IEnumerable<ResultsModel?>? results =
                await database!.QueryByStoredProcedureAsync<ResultsModel, ResultsModel_Parameters>("usp_Training_Questionnaire_GetResultsBySessionIDandUserIDs", parameters);

            return results;
        }
        */
    }
}
