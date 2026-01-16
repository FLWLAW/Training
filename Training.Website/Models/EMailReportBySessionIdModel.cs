namespace Training.Website.Models
{
    public record EMailReportBySessionIdModel
    {
        public int? ID { get; set; }
        public int? Session_ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        
        public int? QuestionnaireNumber { get; set; }       // NEW
        public DateTime? DueDate { get; set; }              // NEW?

        public string? Status { get; set; }
        
        public double? Score { get; set; } // NEW

        
        public string? WhoFirstSent { get; set; }               // First Sent By
        public DateTime? WhenFirstSent { get; set; }            // When First Sent
        public string? WhoLastUpdated { get; set; }             // Last Sent By
        public DateTime? WhenLastUpdated { get; set; }          // Latest Email Sent On
        public DateTime? WhenMustRetakeBy { get; set; }         // NEW - When Must Retake By
        public DateTime? WhenUserLastSubmitted { get; set; }    // When User Last Submitted
        public string? Role { get; set; }
        public string? Title { get; set; }
        public DateTime? WhenLastReminderEmailSent { get; set; }
        public string? WhoLastReminderEmailSent { get; set; }

        // EXTRA
        public int? OPS_Emp_ID { get; set; }
        public int? CMS_User_ID { get; set; }
        public string? LoginID { get; set; }
        public string? Email { get; set; }
    }
}
