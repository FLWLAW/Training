namespace Training.Website.Models.Users
{
    public class AllUsers_Assignment
    {
        public bool Selected { get; set; } = false;
        public required int? CMS_UserID { get; set; }
        public required int? OPS_UserID { get; set; }
        public required string? UserName { get; set; }
        public required string? LastName { get; set; }     // used for email.
        public required string? FirstName { get; set; }     // used for email.
        public required string? EmailAddress { get; set; }
        public required string? RoleDesc { get; set; }
        public required string? TitleDesc { get; set; }
    }
}