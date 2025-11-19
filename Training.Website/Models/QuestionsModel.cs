namespace Training.Website.Models
{
    public class QuestionsModel
    {
        public int? Question_ID { get; set; }
        public int? Training_SESSION_ID { get; set; }
        public string? Question { get; set; }
        public int? QuestionNumber { get; set; }
        public int? AnswerFormat { get; set; }
        public string? CorrectAnswer { get; set; }
        //public bool AnythingBesidesAnswerFormatUpdated { get; set; } = false;
        //public bool AnswerFormatUpdated { get; set; } = false;
    }
}
