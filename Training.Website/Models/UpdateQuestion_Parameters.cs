namespace Training.Website.Models
{
    public class UpdateQuestion_Parameters
    {
        public required int Question_ID { get; set; }
        public required string Question { get; set; }
        public required int AnswerFormat { get; set; }
        public required string? CorrectAnswer { get; set; }
        public required int UpdatedBy_ID { get; set; }
    }
}
