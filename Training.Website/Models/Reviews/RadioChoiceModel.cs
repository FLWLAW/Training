namespace Training.Website.Models.Reviews
{
    public class RadioChoiceModel
    {
        public int? RadioChoice_ID { get; set; }
        public int? ReviewQuestion_ID { get; set; }
        public int? RadioChoice_Sequence { get; set; }
        public string? RadioChoice_Text { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasBeenChangedOnScreen { get; set; }    // This is used to track whether the user has made a change to this radio choice on the screen, so that we know whether to include it in the list of changes to be sent to the server when the user clicks "Save Changes"
        public bool IsNewAndNotAddedToDatabaseYet { get; set; }    // This is used to track whether this radio choice is a new one that the user has added on the screen but has not yet been saved to the database, so that we know to include it in the list of changes to be sent to the server when the user clicks "Save Changes"
        //public bool Selected { get; set; } = false;
    }
}
