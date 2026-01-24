namespace Training.Website.Models.Reviews
{
    public class InsertReviewStatusChange_Parameters
    {
        public required int? Review_ID { get; set; }
        public int? OPS_User_ID_Reviewee { get; set; }
        public int? CMS_User_ID_Reviewee { get; set; }
        public required string? Login_ID_Reviewee { get; set; }
        public int? OPS_User_ID_StatusChangedBy { get; set; }
        public int? CMS_User_ID_StatusChangedBy { get; set; }
        public required string? Login_ID_StatusChangedBy { get; set; }
        public required string? OldStatus { get; set; }
        public required string? NewStatus { get; set; }
    }
}
