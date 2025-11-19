namespace Training.Website.Models
{
    public class InsertAnswerChoice_Parameters
    {
        public required int Question_ID { get; set; }
        public required char AnswerLetter { get; set; }
        public required string AnswerText { get; set; }
        public required int CreatedBy_ID { get; set; }
    }
}
