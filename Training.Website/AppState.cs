using Training.Website.Models;

namespace Training.Website
{
    public class AppState
    {
        public AllUsers_Name_IDs? LoggedOnUser { get; set; }

        //public int? SessionID { get; set; }
        public string? SessionID_String { get; set; }

    }
}
