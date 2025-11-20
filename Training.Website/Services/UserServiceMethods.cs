using Microsoft.AspNetCore.Http;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class UserServiceMethods : CommonServiceMethods
    {
        public async Task InsertTestResult(int sessionID, int userID, int statusID, double score, IDatabase? database)
        {
            InsertTestResult_Parameters parameters = new()
            {
                Session_ID = sessionID,
                User_ID = userID,
                Status_ID = statusID,
                Score = score
            };

            await database!.NonQueryByStoredProcedureAsync<InsertTestResult_Parameters>("usp_Training_Questionnaire_InsertTestResult", parameters);
        }
    }
}
