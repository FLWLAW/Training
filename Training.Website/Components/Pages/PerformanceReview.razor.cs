using Microsoft.AspNetCore.Components;
using SqlServerDatabaseAccessLibrary;
using Training.Website.Models.Reviews;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public partial class PerformanceReview
    {
        #region CASCADING PARAMETERS
        [CascadingParameter]
        private AppState? ApplicationState { get; set; }
        #endregion

        #region DEPENDENCY INJECTION PROPERTIES
        [Inject]
        private IDatabase? Database { get; set; }
        #endregion

        #region PRIVATE FIELDS
        private const int _userID = 296;        // TEMP
        private const int _reviewYear = 2025;   // TEMP
        private EmployeeInformationModel? _headerInfo = null;
        private PerformanceReviewServiceMethods _service = new();


        #endregion



        protected override async Task OnInitializedAsync()
        {
            _headerInfo = await _service.GetEmployeeInformation(_userID, _reviewYear, Database);
        }


    }
}
