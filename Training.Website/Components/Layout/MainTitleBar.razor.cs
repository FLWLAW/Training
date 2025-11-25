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
        private const string _TITLE = "Training Questionnaire (** DEBUG MODE **)";
#else
        private const string _TITLE = "Training Questionnaire";
#endif
    }
}
