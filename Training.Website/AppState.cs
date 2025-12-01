using Training.Website.Models.Users;

namespace Training.Website
{
    public class AppState
    {
        public AllUsers_Authentication? LoggedOnUser { get; set; }

        //public int? SessionID { get; set; }
        public string? SessionID_String { get; set; }

    }
}
