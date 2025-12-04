namespace Training.Website.Models
{
    public class UpsertEMailings_Parameters
    {
        public required int CMS_User_ID { get; set; }
        public required int Session_ID { get; set; }
        public required string SendingUser { get; set; }
        public required string EMailedUserLastName { get; set; }
        public required string EMailedUserFirstName { get; set; }
        public required string EMailedUserLogin_ID { get; set; }
    }
}
