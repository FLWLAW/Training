namespace Training.Website.Models.Users
{
    public class AllUsers_Notaries
    {
        public int? Emp_ID { get; set; }
        public int? CMS_User_ID { get; set; } // THIS MIGHT NOT BE NECESSARY
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; } // LOGIN ID
        public string? EMail { get; set; }
        //public bool? AuthorizedNotary { get; set; }
        //public string? NotaryNumber { get; set; }
        //public DateTime? NotaryCommencement { get; set; }
        //public DateTime? NotaryExpiration { get; set; }
        //public string? Status { get; set; }
    }
}
