namespace Training.Website.Models
{
    public class ResultsModel
    {
        public int? TestAttempt_ID { get; set; }
        public int? OPS_User_ID { get; set; }
        public int? CMS_User_ID { get; set; }
        public int? QuestionnaireNumber { get; set; }
        public double? Score { get; set; }
        public DateTime? WhenSubmitted { get; set; }
        public DateTime? WhenMustRetakeBy { get; set; }
    }
}
