using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class PerformanceReviewServiceMethods : CommonServiceMethods
    {
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

        public async Task<IEnumerable<UsersForManagerModel?>?> GetUsersForManager_CMS_DB
            (AllUsers_CMS_DB?[]? allUsers_CMS_DB, AllUsers_OPS_DB?[]? allUsers_OPS_DB, int? cmsDB_ManagerID, IDatabase? database)
        {
            if (allUsers_CMS_DB == null || cmsDB_ManagerID == null)
                return null;
            else
            {
                // RETURNS ALL USERS FOR MANAGER, INCLUDING INACTIVE ONES
                IEnumerable<int>? userIDsForManager_Raw = await database!.QueryByStatementAsync<int>($"SELECT AppUserID FROM AppUserManager WHERE ManagerAppUserID = {cmsDB_ManagerID}");

                if (userIDsForManager_Raw == null || userIDsForManager_Raw.Count() == 0)
                    return null;
                else
                {
                    List<UsersForManagerModel> usersForManager_Refined = [];

                    foreach (int id in userIDsForManager_Raw)
                    {
                        AllUsers_CMS_DB? user = allUsers_CMS_DB.FirstOrDefault(q => q?.AppUserID == id);    // USER WON'T BE IN ARRAY IN NOT ACTIVE. WE DON'T WANT INACTIVE USERS

                        if (user != null && user.AppUserID != null)
                        {
                            UsersForManagerModel userForManager = new()
                            {
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                CMS_UserID = user.AppUserID,
                                OPS_UserID = allUsers_OPS_DB?.FirstOrDefault(q => q?.UserName?.Equals(user.LoginID, StringComparison.InvariantCultureIgnoreCase) == true)?.Emp_ID
                            };
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
    }
}
