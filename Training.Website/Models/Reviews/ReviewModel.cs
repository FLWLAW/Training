namespace Training.Website.Models.Reviews
{
    public class ReviewModel
    {
        public int? ID { get; set; }
        public int? Review_Year { get; set; }
        public int? Status_ID { get; set; }
        public Globals.ReviewStatusType Status_ID_Type
        {
            get
            {
                return (Status_ID != null) ? (Globals.ReviewStatusType)Status_ID : Globals.ReviewStatusType.ERROR;
            }
        }
        public DateTime? When_Started { get; set; }
        public DateTime? ReviewMeetingHeldOn { get; set; }
    }
}
