namespace Training.Website.Models.Reviews
{
    public class AnswersByReviewYearOpsReviewerOpsReviewee_Parameters
    {
        public required int ReviewYear { get; set; }
        public required int OPS_User_ID_Reviewer { get; set; }
        public required int OPS_User_ID_Reviewee { get; set; }
    }
}
