using SqlServerDatabaseAccessLibrary;
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
    }
}
