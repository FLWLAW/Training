namespace Training.Website.Models.Reviews
{
    public class StatusHistoryModel
    {
        public int? Review_ID { get; set; }
        public string? StatusChangedBy { get; set; }
        public string? FirstName { get; set; }  // NOT IN [usp_Performance_Review_GetReviewStatusHistoryByReviewID]
        public string? LastName { get; set; }   // NOT IN [usp_Performance_Review_GetReviewStatusHistoryByReviewID]
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime? WhenChanged { get; set; }
    }
}
