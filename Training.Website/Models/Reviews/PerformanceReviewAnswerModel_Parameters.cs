namespace Training.Website.Models.Reviews
{
    public class PerformanceReviewAnswerModel_Parameters
    {
        public required int Question_ID { get; set; }
        public required int OPS_User_ID_Reviewer { get; set; }
        public required int OPS_User_ID_Reviewee { get; set; }
        public required string Answer { get; set; }
        public required int? CMS_User_ID_Reviewer { get; set; }
        public required int? CMS_User_ID_Reviewee { get; set; }
    }
}
