namespace Training.Website.Models
{
    public class EligibilityClass
    {
        public required string? Message { get; set; }
        public required int Count { get; set; }
        public required bool Finished { get; set; }
        public required bool WasAssigned { get; set; }
    }
}
