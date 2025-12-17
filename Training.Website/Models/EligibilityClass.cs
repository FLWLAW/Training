namespace Training.Website.Models
{
    public class EligibilityClass
    {
        public required string? MessageLine1 { get; set; }
        public required string? MessageLine2 { get; set; }
        public required int Count { get; set; }
        public required bool NoMore { get; set; }
        public required bool WasAssigned { get; set; }
    }
}
