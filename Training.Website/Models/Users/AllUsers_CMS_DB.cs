namespace Training.Website.Models.Users
{
    public class AllUsers_CMS_DB
    {
        public int? AppUserID { get; set; }
        public int? RoleID { get; set; }
        public int? TitleID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName) == false && string.IsNullOrWhiteSpace(LastName) == false)
                    return string.Concat(FirstName, ' ', LastName);
                else if (string.IsNullOrWhiteSpace(FirstName) == true)
                    return LastName;
                else
                    return FirstName;
            }
        }
        public string? EmailAddress { get; set; }
        public string? LoginID { get; set; }        // LAN ACCOUNT ID
    }
}