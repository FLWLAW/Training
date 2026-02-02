using Dapper;
using SqlServerDatabaseAccessLibrary;
using System.Data;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<AnswersByReviewIdModel?>?> GetAnswersByReviewID(int? reviewID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<AnswersByReviewIdModel?, object?>
                ("usp_Performance_Review_GetAnswersByReviewID", new { Review_ID = reviewID });

        public async Task<string?> GetCurrentReviewStatusByReviewID(int reviewID, IDatabase? database) =>
            (await database!.QueryByStoredProcedureAsync<string?, object?>("usp_Performance_Review_GetCurrentReviewStatusByReviewID", new { Review_ID = reviewID }))?.FirstOrDefault();

        public async Task<PerformanceReviewStatusesAllUsersByReviewYearModel?> GetLatestStatusOneEmployeeByReviewYearAndID(int opsID, int reviewYear, IDatabase? database) =>
            (
                await database!.QueryByStoredProcedureAsync
                    <PerformanceReviewStatusesAllUsersByReviewYearModel?, object?>
                        ("usp_Performance_Review_GetLatestStatusOneEmployeeByReviewYearAndID", new { OPS_Reviewee_ID = opsID, Review_Year = reviewYear })
            )?.FirstOrDefault();


        public async Task<EmployeeInformationModel?> GetEmployeeInformation(int OPS_Emp_ID, int reviewYear, IDatabase? database) =>
            (await database!.QueryByStoredProcedureAsync<EmployeeInformationModel?, object?>
                ("usp_Performance_Review_Employee_Information", new { OPS_Emp_ID = OPS_Emp_ID }))?.FirstOrDefault();

        public async Task<string?> GetLoginIdOfLatestManagerWhoChangedReviewStatusToInReview(int reviewID, IDatabase? database) =>
            (
                await database!.QueryByStoredProcedureAsync
                        <string?, object?>
                            ("usp_Performance_Review_GetLoginIdOfLatestManagerWhoChangedReviewStatusToInReview", new { Review_ID = reviewID })
                )?.FirstOrDefault();

        public async Task<string?> GetManagerForUser_CMS_DB(int cmsUserID_Reviewee, AllUsers_CMS_DB?[]? allUsers_CMS_DB, IDatabase database)
        {
            int? cmsUserID_Manager =
                (
                    await database!.QueryByStatementAsync<int?>
                    ($"SELECT TOP 1 ManagerAppUserID FROM AppUserManager WHERE AppUserID = {cmsUserID_Reviewee} ORDER BY CreateDT DESC")
                )?.FirstOrDefault();

            if (cmsUserID_Manager == null)
                return null;
            else
            {
                AllUsers_CMS_DB? manager = allUsers_CMS_DB?.FirstOrDefault(q => q?.AppUserID == cmsUserID_Manager);

                if (manager == null)
                    return "[NO MANAGER FOUND IN DB]";
                else
                {
                    string managerFirstName = manager.FirstName ?? string.Empty;
                    string managerLastName = manager.LastName ?? string.Empty;
                    bool blankFirstName = string.IsNullOrEmpty(managerFirstName);
                    bool blankLastName = string.IsNullOrEmpty(managerLastName);

                    if (blankFirstName == false && blankLastName == false)
                        return string.Concat(managerFirstName, ' ', managerLastName);
                    else
                    {
                        string managerLoginID = manager.LoginID ?? string.Empty;
                        bool blankLoginID = string.IsNullOrEmpty(managerLoginID);
                        string suffix = $" FOR ID #{cmsUserID_Manager} IN NY.AppUser FOR A MANAGER.";

                        if (blankLastName == true && blankFirstName == true && blankLoginID == true)                                    // 111  (7)
                            return $"[NO FIRST NAME, LAST NAME OR LOGIN ID FOUND{suffix}.]";
                        else if (blankLastName == true && blankFirstName == true && blankLoginID == false)                              // 110  (6)
                            return $"User ID: {managerLoginID} [NO FIRST NAME OR LAST NAME FOUND{suffix}.]";
                        else if (blankLastName == true && blankFirstName == false && blankLoginID == true)                              // 101  (5)
                            return $"First Name: {managerFirstName} [NO FIRST NAME OR LOGIN ID FOUND{suffix}.]";
                        else if (blankLastName == true && blankFirstName == false && blankLoginID == false)                             // 100  (4)
                            return $"First Name: {managerFirstName}, Login ID: {managerLoginID} [NO lAST NAME FOUND{suffix}.]";
                        else if (blankLastName == false && blankFirstName == true && blankLoginID == true)                              // 011  (3)
                            return $"Last Name: {managerLastName}, [NO FIRST NAME OR LOGIN ID FOUND{suffix}.]:";
                        else //if (blankLastName == false && blankFirstName == true && blankLoginID == false)                           // 010  (2)
                            return $"Last Name: {managerLastName}, Login ID: {managerLoginID} [NO FIRST NAME FOUND{suffix}.]";

                        // IF MANAGER FIRST NAME AND MANAGER LAST NAME ARE BOTH NOT BLANK, THAT IS COVERED BEFORE THE "else" AND WE DON'T CARE ABOUT THE LOGIN ID IN THAT CIRCUMSTANCE.
                    }
                }
            }
        }

        public async Task<DateTime?> GetMeetingHeldOnByReviewID(int reviewID, IDatabase? database) =>
            (await database!.QueryByStatementAsync<DateTime?>($"SELECT ReviewMeetingHeldOn FROM [PERFORMANCE Review Main Tbl] WHERE ID = {reviewID}"))?.FirstOrDefault();

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

        public async Task<IEnumerable<PerformanceReviewQuestionModel?>?> GetPerformanceReviewQuestions(int reviewYear, bool isDeleted, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<PerformanceReviewQuestionModel?, object?>
                ("usp_Performance_Review_GetPerformanceReviewQuestionsByReviewYearAndDeletedStatus", new { ReviewYear = reviewYear, IsDeleted = isDeleted });

        public async Task<IEnumerable<PerformanceReviewStatusesAllUsersByReviewYearModel?>?> GetReviewStatusesAllUsersByReviewYear(int reviewYear, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<PerformanceReviewStatusesAllUsersByReviewYearModel?, object?>
                ("usp_Performance_Review_GetReviewStatusesAllUsersByReviewYear", new { Review_Year = reviewYear });

        public async Task<ReviewModel?> GetReviewByReviewID(int reviewID, IDatabase? database) =>
            (await database!.QueryByStoredProcedureAsync<ReviewModel, object?>("usp_Performance_Review_GetReviewByReviewID", new { Review_ID = reviewID }))?.FirstOrDefault();

        public async Task<ReviewModel?> GetReviewByReviewYearAndRevieweeID
            (int? reviewYear, int? opsUserID_Reviewee, int? cms_UserID_Reviewee, string? loginID_Reviewee, IDatabase? database)
        {
            PerformanceReviewByReviewYearAndID_Parameters parameters = new()
            {
                Review_Year = reviewYear,
                OPS_User_ID_Reviewee = opsUserID_Reviewee,
                CMS_User_ID_Reviewee = cms_UserID_Reviewee,
                Login_ID_Reviewee = loginID_Reviewee,
            };

            ReviewModel? result = 
                (
                    await database!.QueryByStoredProcedureAsync
                        <ReviewModel, PerformanceReviewByReviewYearAndID_Parameters>
                            ("usp_Performance_Review_GetReviewByReviewYearAndRevieweeID", parameters)
                )?.FirstOrDefault();

            return result;
        }

        public async Task<IEnumerable<StatusHistoryModel?>?> GetReviewStatusHistoryOneEmployeeByReviewID(int reviewID, IDatabase? database) =>
            await database!.QueryByStoredProcedureAsync<StatusHistoryModel?, object?>("usp_Performance_Review_GetReviewStatusHistoryOneEmployeeByReviewID", new { Review_ID = reviewID });

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

        public async Task<int?> InsertReviewAndFirstStatusChange
            (int reviewYear, int opsReviewerID, int opsRevieweeID, int cmsReviewerID, int cmsRevieweeID, string loginID_Reviewer, string loginID_Reviewee, IDatabase? database)
        {
            try
            {
                DynamicParameters parameters = new();

                parameters.Add("@Review_Year", value: reviewYear, dbType: DbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("@OPS_User_ID_Reviewer", value: opsReviewerID, dbType: DbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("@CMS_User_ID_Reviewer", value: cmsReviewerID, dbType: DbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("@Login_ID_Reviewer", value: loginID_Reviewer, dbType: DbType.String, direction: ParameterDirection.Input);
                parameters.Add("@OPS_User_ID_Reviewee", value: opsRevieweeID, dbType: DbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("@CMS_User_ID_Reviewee", value: cmsRevieweeID, dbType: DbType.Int32, direction: ParameterDirection.Input);
                parameters.Add("@Login_ID_Reviewee", value: loginID_Reviewee, dbType: DbType.String, direction: ParameterDirection.Input);
                parameters.Add("@Review_ID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                int? reviewID = await database!.NonQueryByStoredProcedureOutputParameterAsync<int?>
                        ("usp_Performance_Review_InsertReviewAndFirstStatusChange", "@Review_ID", parameters);

                return reviewID;
            }
            catch
            {
                throw;
            }
        }

        public async Task InsertReviewStatusChangeOnly
            (
                int? reviewID,
                int? opsUserID_Reviewee, int? cmsUserID_Reviewee, string? loginID_Reviewee,
                int? opsUserID_StatusChangedBy, int? cmsUserID_StatusChangedBy, string loginID_StatusChangedBy,
                string oldStatus, string newStatus,
                IDatabase? database
            )
        {
            if (reviewID == null)
                throw new NoNullAllowedException("[reviewID] cannot be null in InsertReviewStatusChangeOnly().");
            else if (opsUserID_Reviewee == null && cmsUserID_Reviewee == null && loginID_Reviewee == null)
                throw new NoNullAllowedException("[opsUserID], [cmsUserID] and [loginID] cannot all be null in InsertReviewStatusChangeOnly(). At least one of these parameters must contain a non-null value.");
            else
            {
                InsertReviewStatusChange_Parameters parameters = new()
                {
                    Review_ID = reviewID,
                    OPS_User_ID_Reviewee = opsUserID_Reviewee,
                    CMS_User_ID_Reviewee = cmsUserID_Reviewee,
                    Login_ID_Reviewee = loginID_Reviewee,
                    OPS_User_ID_StatusChangedBy = opsUserID_StatusChangedBy,
                    CMS_User_ID_StatusChangedBy = cmsUserID_StatusChangedBy,
                    Login_ID_StatusChangedBy = loginID_StatusChangedBy,
                    OldStatus = oldStatus,
                    NewStatus = newStatus
                };

                await database!.NonQueryByStoredProcedureAsync<InsertReviewStatusChange_Parameters>("usp_Performance_Review_InsertStatusChangeOnly", parameters);
            }
        }

        public async Task UpdateWhenReviewMeetingHeldOn(int reviewID, DateTime meetingDate, IDatabase? database) =>
            await database!.NonQueryByStoredProcedureAsync("usp_Performance_Review_Update_ReviewMeetingHeldOn", new { Review_ID = reviewID, Review_DateTime = meetingDate });

        public async Task UpsertPerformanceReviewAnswer_Main
            (int reviewID, int questionID, string answer, int cmsUserID, int opsUserID, string? loginID, bool isAdministrator, IDatabase? database)
        {
            PerformanceReviewAnswerModel_Parameters parameters =
                new()
                {
                    Review_ID = reviewID,
                    Question_ID = questionID,
                    Answer = answer,
                    CMS_ID = cmsUserID,
                    OPS_ID = opsUserID,
                    Login_ID = loginID
                };

            string storedProcedure = isAdministrator == true ? "usp_Performance_Review_UpsertAnswer_Administrator" : "usp_Performance_Review_UpsertAnswer_Manager";

            await database!.NonQueryByStoredProcedureAsync<PerformanceReviewAnswerModel_Parameters>(storedProcedure, parameters);
        }

// =============================================================================================================================================================================================================================================================================================================================================================================================================

        private UsersForDropDownModel ConvertToDropDownClass(AllUsers_CMS_DB? user, AllUsers_OPS_DB?[]? allUsers_OPS_DB)
        {
            int? opsUserID = GetOPS_ID_From_Login_ID(allUsers_OPS_DB, user?.LoginID);
            string? opsLoginID = allUsers_OPS_DB?.FirstOrDefault(q => q?.Emp_ID == opsUserID)?.UserName;

            return new()
            {
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                CMS_UserID = user?.AppUserID,
                OPS_UserID = opsUserID,
                CMS_LoginID = user?.LoginID,
                OPS_LoginID = opsLoginID
            };
        }

        private ReviewModel_Parameters ReviewModelParameters
            (int reviewYear, int opsReviewerID, int opsRevieweeID, int cmsReviewerID, int cmsRevieweeID, string? loginID) =>
                new()
                {
                    ReviewYear = reviewYear,
                    Login_ID_Reviewer = loginID,
                    OPS_User_ID_Reviewer = opsReviewerID,
                    OPS_User_ID_Reviewee = opsRevieweeID,
                    CMS_User_ID_Reviewer = cmsReviewerID,
                    CMS_User_ID_Reviewee = cmsRevieweeID
                };
    }
}
