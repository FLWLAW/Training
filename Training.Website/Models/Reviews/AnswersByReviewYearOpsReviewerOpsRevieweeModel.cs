namespace Training.Website.Models.Reviews
{
    public class AnswersByReviewYearOpsReviewerOpsRevieweeModel
    {
        public int? Answer_ID { get; set; }

        public int? Question_ID { get; set; }

        public int? OPS_User_ID_Reviewer { get; set; }

        public int? OPS_User_ID_Reviewee { get; set; }

        public string? Answer { get; set; }
        /*
        public int? CMS_User_ID_Reviewer { get; set; }

        public int? CMS_User_ID_Reviewee { get; set; }
        */
        public int? Year { get; set; }
    }
}
