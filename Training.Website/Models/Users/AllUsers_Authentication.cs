namespace Training.Website.Models.Users
{
    public class AllUsers_Authentication
    {
        //public string? Domain { get; set; }
        public required string? UserName { get; set; }
        /*
        public string? UserName_Reformatted_For_DB
        {
            get
            {
                return UserName?.ToUpper().Replace(' ', '.');
            }
        }
        */
        public required string? LoginID { get; set; }
        public required int? AppUserID { get; set; } // CMS DB
        public required int? EmpID { get; set; } // OPS DB
        public required int? RoleID { get; set; }
        public required int? TitleID { get; set; }
        //public string? HomeState { get; set; }
        public required bool IsPerformanceReviewAdministrator { get; set; }
        public required bool IsPerformanceReviewSuperAdministrator { get; set; }
    }
}