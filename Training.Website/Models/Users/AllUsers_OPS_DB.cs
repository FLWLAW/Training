namespace Training.Website.Models.Users
{
    public class AllUsers_OPS_DB
    {
        // NOTE: IF UNCOMMENTING ANY OF THESE PROPERTIES OR ADDING NEW ONES, MODIFY THE SQL STATEMENT IN CommonServiceMethods.GetAllUsers_OPS_DB()
        
        public int? Emp_ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        //public string? FullName { get; set; }
        public string? UserName { get; set; }   // Login ID
        public string? Email { get; set; }
    }
}
