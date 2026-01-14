using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class PerformanceReviewServiceMethods : CommonServiceMethods
    {
        public async Task<IEnumerable<RadioChoiceModel?>?> GetAllRadioButtonChoicesByYear(int reviewYear, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<RadioChoiceModel?, object?>
                ("usp_Performance_Review_GetAllRadioButtonChoicesByYear", new { ReviewYear = reviewYear });

        public async Task<IEnumerable<UsersForDropDownModel?>?> GetAllUsersExceptUserLoggedOn(AllUsers_CMS_DB?[]? allUsers_CMS_DB, AllUsers_OPS_DB?[]? allUsers_OPS_DB, int? loggedOnUser_CMS_ID)
        {
            // TODO: DO WE WANT TO EXCLUDE THE USER WHO IS LOGGED ON?

            List<UsersForDropDownModel?>? result = null;

            await Task.Run(() =>
            {
                if (allUsers_CMS_DB != null && allUsers_CMS_DB.Length > 0)
                {
                    result = [];
                    foreach (AllUsers_CMS_DB? user in allUsers_CMS_DB)
                        if (user != null && user.AppUserID != loggedOnUser_CMS_ID)
                            result.Add(ConvertToDropDownClass(user, allUsers_OPS_DB));
                }
            });

            return result;
        }

        public async Task<IEnumerable<AnswersByReviewYearOpsReviewerOpsRevieweeModel?>?> GetAnswersByReviewYearOpsReviewerOpsReviewee
            (int reviewYear, int opsReviewerID, int opsRevieweeID, IDatabase? database)
        {
            AnswersByReviewYearOpsReviewerOpsReviewee_Parameters parameters = new()
            {
                ReviewYear = reviewYear,
                OPS_User_ID_Reviewer = opsReviewerID,
                OPS_User_ID_Reviewee = opsRevieweeID
            };

            IEnumerable<AnswersByReviewYearOpsReviewerOpsRevieweeModel?>? result =
                await database!.QueryByStoredProcedureAsync<AnswersByReviewYearOpsReviewerOpsRevieweeModel?, AnswersByReviewYearOpsReviewerOpsReviewee_Parameters>
                    ("usp_Performance_Review_GetAnswersByReviewYearOpsReviewerOpsReviewee", parameters);

            return result;
        }

        public async Task<EmployeeInformationModel?> GetEmployeeInformation(int OPS_Emp_ID, int reviewYear, IDatabase? database)
        {
            EmployeeInformationModel? employeeInfo =
                (
                    await database!.QueryByStoredProcedureAsync<EmployeeInformationModel?, object?>("usp_Performance_Review_Employee_Information", new { OPS_Emp_ID = OPS_Emp_ID })
                )?.FirstOrDefault();

            if (employeeInfo != null)
            {
                ReviewStatusModel? reviewAndStatus =
                    (
                        await database!.QueryByStoredProcedureAsync<ReviewStatusModel?, object?>
                            ("usp_Performance_Review_GetStatusAndID", new { OPS_Emp_ID = OPS_Emp_ID, ReviewYear = reviewYear })
                    )?.FirstOrDefault();

                if (reviewAndStatus != null)
                {
                    employeeInfo.Status = reviewAndStatus.Status;
                    employeeInfo.Review_ID = reviewAndStatus.Review_ID;
                }
            }

            return employeeInfo;
        }

        public async Task<Dictionary<int, string>?> GetPerformanceReviewAnswerFormats(IDatabase? database)
        {
            IEnumerable<AnswerFormatsModel>? data =
                await database!.QueryByStoredProcedureAsync<AnswerFormatsModel>("usp_Performance_Review_GetAnswerFormats");

            if (data == null)
                return null;
            else
            {
                Dictionary<int, string> answerFormats = [];

                foreach (AnswerFormatsModel? row in data)
                    answerFormats.Add(row.Format_ID, row.Name!);

                return answerFormats;
            }
        }

        public async Task<IEnumerable<PerformanceReviewQuestionModel?>?> GetPerformanceReviewQuestions(int reviewYear, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<PerformanceReviewQuestionModel?, object?>
                ("usp_Performance_Review_GetPerformanceReviewQuestionsByReviewYear", new { ReviewYear = reviewYear });

        // KEEP ASYNCHRONOUS
        public IEnumerable<RadioChoiceModel?> GetRadioButtonChoicesByID(int reviewQuestionID, IDatabase? database) =>
            database!.QueryByStoredProcedure<RadioChoiceModel?, object?>
                ("usp_Performance_Review_GetRadioButtonChoicesByID", new { ReviewQuestion_ID = reviewQuestionID });

        public async Task<IEnumerable<UsersForDropDownModel?>?> GetUsersForManager_CMS_DB
            (AllUsers_CMS_DB?[]? allUsers_CMS_DB, AllUsers_OPS_DB?[]? allUsers_OPS_DB, int? cmsDB_ManagerID, IDatabase? database)
        {
            if (allUsers_CMS_DB == null || cmsDB_ManagerID == null)
                return null;
            else
            {
                // RETURNS ALL USERS FOR MANAGER, INCLUDING INACTIVE ONES. INACTIVE USERS WILL BE FILTERED OUT BELOW.
                IEnumerable<int>? userIDsForManager_Raw = await database!.QueryByStatementAsync<int>($"SELECT AppUserID FROM AppUserManager WHERE ManagerAppUserID = {cmsDB_ManagerID}");

                if (userIDsForManager_Raw == null || userIDsForManager_Raw.Count() == 0)
                    return null;
                else
                {
                    List<UsersForDropDownModel> usersForManager_Refined = [];

                    foreach (int id in userIDsForManager_Raw)
                    {
                        AllUsers_CMS_DB? user = allUsers_CMS_DB.FirstOrDefault(q => q?.AppUserID == id);    // USER WON'T BE IN ARRAY IF NOT ACTIVE. WE DON'T WANT INACTIVE USERS.

                        if (user != null && user.AppUserID != null)
                        {
                            UsersForDropDownModel userForManager = ConvertToDropDownClass(user, allUsers_OPS_DB);
                            usersForManager_Refined.Add(userForManager);
                        }
                    }

                    return usersForManager_Refined;
                }
            }
        }

        public async Task InsertPerformanceReviewAnswer
            (int questionID, int opsUserID_Reviewer, int opsUserID_Reviewee, string answer, int? cmsUserID_Reviewer, int? cmsUserID_Reviewee, IDatabase? database)
        {
            PerformanceReviewAnswerModel_Parameters parameters =
                new()
                {
                    Question_ID = questionID,
                    OPS_User_ID_Reviewer = opsUserID_Reviewer,
                    OPS_User_ID_Reviewee = opsUserID_Reviewee,
                    Answer = answer,
                    CMS_User_ID_Reviewer = cmsUserID_Reviewer,
                    CMS_User_ID_Reviewee = cmsUserID_Reviewee
                };

            await database!.NonQueryByStoredProcedureAsync<PerformanceReviewAnswerModel_Parameters>
                ("usp_Performance_Review_InsertAnswer", parameters);
        }

        // =============================================================================================================================================================================================================================================================================================================================================================================================================

        private UsersForDropDownModel ConvertToDropDownClass(AllUsers_CMS_DB? user, AllUsers_OPS_DB?[]? allUsers_OPS_DB) =>
            new()
            {
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                CMS_UserID = user?.AppUserID,
                OPS_UserID = GetOPS_ID_From_Login_ID(allUsers_OPS_DB, user?.LoginID)
            };

    }
}
