namespace Training.Website.Models
{
    public class UserResponsesModel
    {
        public int IndividualAnswer_ID { get; set; }
        public int Question_ID { get; set; }
        public int AnswerFormat { get; set; }
        public int QuestionNumber { get; set; }
        public string? Question { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? CorrectAnswerText { get; set; }
        public string? UserAnswer { get; set; }
        public string? UserAnswerText { get; set; }
    }
}
