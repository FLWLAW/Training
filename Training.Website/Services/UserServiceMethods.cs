using Dapper;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class UserServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<ScoresAndWhenSubmittedModel>?> GetScoresBySessionIDandUserID(int sessionID, int userID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<ScoresAndWhenSubmittedModel, object?>
                ("usp_Training_Questionnaire_GetScoresBySessionIDandUserID", new { Session_ID = sessionID, User_ID = userID });

        public async Task<int> InsertTestResult(int sessionID, int userID, double score, IDatabase? database)
        {
            int attempts =
                (
                    await database!.QueryByStoredProcedureAsync<int, object?>
                        ("usp_Training_Questionnaire_GetCountOfTestAttemptsBySessionIDandUserID", new { Session_ID = sessionID, User_ID = userID })
                ).First();

            DynamicParameters parameters = new();

            parameters.Add("@Session_ID", value: sessionID, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@User_ID", value: userID, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Attempt", value: (attempts + 1), DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Score", value: score, DbType.Double, direction: ParameterDirection.Input);
            parameters.Add("@Current_ID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            return await database!.NonQueryByStoredProcedureOutputParameterAsync<int>("usp_Training_Questionnaire_InsertTestResult", "@Current_ID", parameters);
        }

        public async Task InsertIndividualAnswer(int testAttemptID, UserAnswersModel userAnswer, IDatabase? database)
        {
            InsertIndividualAnswer_Parameters parameters = new()
            {
                TestAttempt_ID = testAttemptID,
                Question_ID = userAnswer.QuestionID!.Value,
                UserAnswer = userAnswer.UserAnswer!
            };

            await database!.NonQueryByStoredProcedureAsync<InsertIndividualAnswer_Parameters>("usp_Training_Questionnaire_InsertIndividualAnswer", parameters);
        }
    }
}
