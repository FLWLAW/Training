namespace Training.Website.Models.Reviews
{
    public class PerformanceReviewQuestionModel
    {
        public int? Question_ID { get; set; }
        public int? QuestionNumber { get; set; }
        public string? Question { get; set; }
        public int? AnswerFormat { get; set; }
        public string? Answer { get; set; }
        public int? RadioChoice_ID { get; set; }
    }
}
