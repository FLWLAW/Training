namespace Training.Website.Models
{
    public class StagesReportsModel
    {
        // FOR SP [usp_StageList_SAByStateListReportIDList]

        //public string? State_ID { get; set; }
        public string? ReportName { get; set; }
        //public string? StageName { get; set; }
        public string? AssignedUserList { get; set; }
        public string? TempAssignedUserList { get; set; }
        //public string? RemovedAssignedUserList { get; set; }
        public string? StageFullName { get; set; }
        //public int? StageListID { get; set; }

        // OLD CODE FOR SP [usp_StageList_SA]
        /*
        public string? StageListDesc { get; set; }
        public int? StageListID { get; set; }
        public string? StageListName { get; set; }
        //public string? State_ID { get; set; }
        //public double? StageNumber { get; set; }
        public int? ReportID { get; set; }
        public string? ReportDesc { get; set; }
        */
    }
}
