namespace Training.Website.Models.Reviews
{
    public class RadioChoiceModel
    {
        public int? RadioChoice_ID { get; set; }
        public int? ReviewQuestion_ID { get; set; }
        public int? RadioChoice_Sequence { get; set; }
        public string? RadioChoice_Text { get; set; }
        public bool Selected { get; set; } = false;
    }
}
