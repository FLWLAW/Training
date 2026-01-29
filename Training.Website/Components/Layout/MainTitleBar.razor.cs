using Microsoft.AspNetCore.Components;

namespace Training.Website.Components.Layout
{
    public partial class MainTitleBar
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

#if DEBUG
        private const string _TITLE = "** DEBUG MODE **";
#elif QA
        private const string _TITLE = "** QA MODE **";
#else
        private const string _TITLE = "";
#endif
    }
}
