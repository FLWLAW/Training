namespace Training.Website.Models
{
    public class UpsertDueDate_Parameters
    {
        public required int Session_ID { get; set; }
        public required DateTime DueDate { get; set; }
        public required string? User { get; set; }
        public required bool Update { get; set; }
    }
}
