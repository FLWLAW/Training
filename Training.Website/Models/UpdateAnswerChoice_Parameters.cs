namespace Training.Website.Models
{
    internal class UpdateAnswerChoice_Parameters
    {
        public required int Answer_ID { get; set; }
        public required char AnswerLetter { get; set; }
        public required string AnswerText { get; set; }
        public required int LastUpdatedBy_ID { get; set; }
    }
}