namespace Training.Website
{
    public class Globals
    {
        public static int UserID(AppState? appState) =>
            appState?.LoggedOnUser?.AppUserID ?? 0;
    }
}
