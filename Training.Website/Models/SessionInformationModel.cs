namespace Training.Website.Models
{
    public class SessionInformationModel
    {
        public required int? Session_ID { get; set; }
        public required string? DocTitle { get; set; }
        public string? DocStatus { get; set; }
        public bool IsActive
        {
            get
            {
                return DocStatus == "ACTIVE";
            }
        }
    }
}
