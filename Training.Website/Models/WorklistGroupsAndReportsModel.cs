namespace Training.Website.Models
{
    public class WorklistGroupsAndReportsModel
    {
        // FOR SP [usp_StageList_SAByStateListReportIDList]

        public string? ReportName { get; set; }
        public string? AssignedUserList { get; set; }
        public string? TempAssignedUserList { get; set; }
        public string? StageFullName { get; set; }
    }
}
