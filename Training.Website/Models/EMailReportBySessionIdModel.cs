namespace Training.Website.Models
{
    public class EMailReportBySessionIdModel
    {
        public int? ID { get; set; }
        public int? OPS_Emp_ID { get; set; }
        public int? CMS_User_ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? WhoFirstSent { get; set; }
        public DateTime? WhenFirstSent { get; set; }
        public string? WhoLastUpdated { get; set; }
        public DateTime? WhenLastUpdated { get; set; }
        /*
        public string? Role { get; set; }
        public string? Title { get; set; }
        */
    }
}
