namespace Training.Website.Models.Users
{
    public class AllUsers_Assignment
    {
        public required int? AppUserID { get; set; }
        public required bool Selected { get; set; }
        public required string? UserName { get; set; }
        public required string? EmailAddress { get; set; }
        public required string? RoleDesc { get; set; }
        public required string? TitleDesc { get; set; }
    }
}