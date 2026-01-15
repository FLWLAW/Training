namespace Training.Website.Models.Reviews
{
    public class PerformanceReviewAnswerModel_Parameters
    {
        public required int Review_ID { get; set; }
        public required int Question_ID { get; set; }
        public required string Answer { get; set; }
    }
}
