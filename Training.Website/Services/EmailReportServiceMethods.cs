using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class EmailReportServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<EMailReportBySessionIdModel?>?> GetEMailingsBySessionID(int? sessionID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<EMailReportBySessionIdModel, object?>("usp_Training_Questionnaire_GetEmailingsBySessionID", new { Session_ID = sessionID });
    }
}
