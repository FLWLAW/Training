using Microsoft.AspNetCore.Components;

namespace Training.Website.Components.Layout
{
    public partial class NavMenu
    {
        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private NavigationManager? NavManager { get; set; }
        #endregion
    }
}
