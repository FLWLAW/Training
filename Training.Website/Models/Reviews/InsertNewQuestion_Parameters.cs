namespace Training.Website.Models.Reviews
{
    public class InsertNewQuestion_Parameters
    {
        public required int Review_Year { get; set; }
        public required int Question_Number { get; set; }
        public required string? Question { get; set; }
        public required int AnswerFormat { get; set; }
    }
}
