namespace Training.Website.Models.Reviews
{
    public class EmployeeInformationModel
    {
        public int? Review_ID { get; set; }
        //public string? Status { get; set; }
        public int? Emp_ID { get; set; }
        public string? FullName { get; set; }
        public string? BadgeNum { get; set; }
        public string? EmployeeType { get; set; }
        public string? Shift { get; set; }
        public DateTime? HireDate { get; set; }
        public string? SiteName { get; set; }
        public string? PracticeGroupName { get; set; }
        public string? Departments { get; set; }
        public string? JobTitleName { get; set; }
    }
}
