using Microsoft.AspNetCore.Components;

namespace Training.Website.Components.Layout
{
    public partial class MainTitleBar
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion
    }
}
