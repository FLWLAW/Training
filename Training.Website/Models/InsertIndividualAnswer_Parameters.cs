namespace Training.Website.Models
{
    public class InsertIndividualAnswer_Parameters
    {
        public required int TestAttempt_ID { get; set; }
        public required int Question_ID { get; set; }
        public required string UserAnswer { get; set; }
    }
}
