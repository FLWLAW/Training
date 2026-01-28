using System.Security.Cryptography.Pkcs;

namespace Training.Website.Models.Reviews
{
    public class PerformanceReviewStatusesAllUsersByReviewYearModel
    {
        public int? ID { get; set; }
        public int? Review_ID { get; set; }
        public int? OPS_User_ID_Reviewee { get; set; }
        public int? CMS_User_ID_Reviewee { get; set; }
        public string? Login_ID_Reviewee { get; set; }
        public string? LastName_Reviewee { get; set; }  // NOT IN usp_Performance_Review_GetReviewStatusesAllUsersByReviewYear
        public string? FirstName_Reviewee { get; set; } // NOT IN usp_Performance_Review_GetReviewStatusesAllUsersByReviewYear
        public string? Login_ID_StatusChangedBy { get; set; }
        public string? FullName_StatusChangedBy { get; set; } // NOT IN usp_Performance_Review_GetReviewStatusesAllUsersByReviewYear
        public string? NewStatus { get; set; }
        public DateTime? WhenChanged { get; set; }
        public DateTime? ReviewMeetingHeldOn { get; set; }
    }
}
