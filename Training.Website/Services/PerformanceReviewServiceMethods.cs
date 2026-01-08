using SqlServerDatabaseAccessLibrary;
using Training.Website.Models;
using Training.Website.Models.Reviews;

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
    }
}
