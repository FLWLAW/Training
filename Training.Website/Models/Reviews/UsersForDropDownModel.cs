namespace Training.Website.Models.Reviews
{
    public class UsersForDropDownModel
    {
        public required string? LastName { get; set; }
        public required string? FirstName { get; set; }
        
        public string FullName
        {
            get
            {
                return string.Concat(FirstName, ' ', LastName);
            }
        }
        
        public required int? CMS_UserID { get; set; }
        public required int? OPS_UserID { get; set; }
        public required string? CMS_LoginID { get; set; }
        public required string? OPS_LoginID { get; set; }
    }
}
