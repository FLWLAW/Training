namespace Training.Website.Models
{
    public class AllUsers_Name_IDs
    {
        public string? Domain { get; set; }
        public string? UserName { get; set; }
        public string? UserName_Reformatted_For_DB
        {
            get
            {
                return UserName?.ToUpper().Replace(' ', '.');
            }
        }
        public string? LoginID { get; set; }
        public int? AppUserID { get; set; }
        /*
        public int? RoleID { get; set; }
        public int? TitleID { get; set; }
        public string? HomeState { get; set; }
        */
        //TODO: MIGHT NOT NEED
        //public string? TitleDesc { get; set; }
    }
}