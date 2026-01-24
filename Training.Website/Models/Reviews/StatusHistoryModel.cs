namespace Training.Website.Models.Reviews
{
    public class StatusHistoryModel
    {
        public int? Review_ID { get; set; }
        public string? Login_ID_Reviewee { get; set; }
        public string? FirstName_Reviewee { get; set; }  // NOT IN [usp_Performance_Review_GetReviewStatusHistoryOneEmployeeByReviewID]
        public string? LastName_Reviewee { get; set; }   // NOT IN [usp_Performance_Review_GetReviewStatusHistoryOneEmployeeByReviewID]
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? FirstName_StatusChangedBy { get; set; }  // NOT IN [usp_Performance_Review_GetReviewStatusHistoryOneEmployeeByReviewID]
        public string? LastName_StatusChangedBy { get; set; }  // NOT IN [usp_Performance_Review_GetReviewStatusHistoryOneEmployeeByReviewID]
        public string? Login_ID_StatusChangedBy { get; set; }
        public DateTime? WhenChanged { get; set; }
    }
}
