using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class ReportServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<EMailReportBySessionIdModel?>?> GetEMailingsBySessionID(int? sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<EMailReportBySessionIdModel, object?>("usp_Training_Questionnaire_GetEmailingsBySessionID", new { Session_ID = sessionID });

        public async Task<IEnumerable<ResultsModel?>?> GetResultsBySessionID(int? sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<ResultsModel?, object?>("usp_Training_Questionnaire_GetResultsBySessionID", new { Session_ID = sessionID });
    }
}
