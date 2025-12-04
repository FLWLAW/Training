namespace Training.Website.Models
{
    public class InsertTestResult_Parameters
    {
        public required int Session_ID { get; set; }
        public required int QuestionnaireNumber { get; set; }
        public required int User_ID { get; set; }
        public required double Score { get; set; }
    }
}
