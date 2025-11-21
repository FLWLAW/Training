namespace Training.Website.Models
{
    public class InsertTestResult_Parameters
    {
        public required int Session_ID { get; set; }
        public required int Attempt { get; set; }
        public required int User_ID { get; set; }
        //public required int Status_ID { get; set; }
        public required double Score { get; set; }
    }
}
