using Dapper;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using Training.Website.Models;

namespace Training.Website.Services
{
    public class UserServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<UserResponsesModel?>?> GetUserResponses(int testAttemptID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<UserResponsesModel, object?>
                ("usp_Training_Questionnaire_GetUserResponsesByTestAttemptID", new { TestAttempt_ID = testAttemptID });

        public async Task<int> InsertTestResult
            (int sessionID, int userID_CMS, int userID_OPS, double score, int currentAttempt, DateTime? whenMustRetakeTestBy, IDatabase? database)
        {
            DynamicParameters parameters = new();

            parameters.Add("@Session_ID", value: sessionID, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@CMS_User_ID", value: userID_CMS, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@OPS_User_ID", value: userID_OPS, dbType: DbType.Int32 , direction: ParameterDirection.Input);
            parameters.Add("@QuestionnaireNumber", value: currentAttempt, DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Score", value: score, DbType.Double, direction: ParameterDirection.Input);
            parameters.Add("@WhenMustRetakeBy", value: whenMustRetakeTestBy, DbType.DateTime, direction: ParameterDirection.Input);
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

        public async Task<bool> WasUserAssignedQuestionnaire(int sessionID, int CMSUserID, IDatabase? database)
        {
            string[] parameterNames = ["@Session_ID", "@CMS_User_ID"];
            object?[] parameterValues = [sessionID, CMSUserID];
            object? resultRaw =
                await database!.QueryByStoredProcedureAsync_OneRecordOneValueNoFieldName("dbo.usp_Training_Questionnaire_GetEmailedUserBySessionCmsOpsIDs", parameterNames, parameterValues!);

            if (resultRaw == null)
                throw new NoNullAllowedException();
            else if (int.TryParse(resultRaw.ToString(), out int resultRefined) == false)
                throw new InvalidDataException();
            else if (resultRefined != 0)
                return true;
            else
                return false;
        }
    }
}
