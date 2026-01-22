namespace Training.Website.Models.Reviews
{
    public class PerformanceReviewByReviewYearAndID_Parameters
    {
        public required int? Review_Year { get; set; }
        public required int? OPS_User_ID_Reviewee { get; set; }
        public required int? CMS_User_ID_Reviewee { get; set; }
        public required string? Login_ID_Reviewee { get; set; }
    }
}
