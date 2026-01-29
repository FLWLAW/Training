using SqlServerDatabaseAccessLibrary;
using Telerik.Documents.SpreadsheetStreaming;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;

namespace Training.Website.Services
{
    public class PerformanceReviewOneEmployeeExcelExport
    {
        //private readonly int _selectedReviewYear;
        private readonly string _sheetName;
        //private readonly UsersForDropDownModel? _selectedUser;
        private readonly ReviewModel? _selectedReview;
        private readonly AllUsers_OPS_DB?[]? _allUsers_OPS_DB;
        private readonly AllUsers_CMS_DB?[]? _allUsers_CMS_DB;
        private readonly PerformanceReviewServiceMethods _service;
        private readonly IDatabase? _database_OPS;

        public PerformanceReviewOneEmployeeExcelExport
            (
                //int selectedReviewYear,
                string sheetName,
                //UsersForDropDownModel? selectedUser,
                ReviewModel? selectedReview,
                AllUsers_OPS_DB?[]? allUsers_OPS_DB,
                AllUsers_CMS_DB?[]? allUsers_CMS_DB,
                PerformanceReviewServiceMethods service,
                IDatabase? database_OPS
            )
        {
            //_selectedReviewYear = selectedReviewYear;
            _sheetName = sheetName;
            //_selectedUser = selectedUser;
            _selectedReview = selectedReview;
            _allUsers_CMS_DB = allUsers_CMS_DB;
            _allUsers_OPS_DB = allUsers_OPS_DB;
            _service = service;
            _database_OPS = database_OPS;
        }

        public async Task<MemoryStream?> Go()
        {
            StatusHistoryModel?[]? results = await GetReviewStatusHistoryOneEmployeeByReviewID_Main();

            if (results == null || results.Length == 0)
                return null;
            else
            {
                MemoryStream stream = new();    // CANNOT USE "using" HERE, BECAUSE "using" WILL CLOSE THE STREAM BEFORE IT IS RETURNED

                using (IWorkbookExporter workbook = SpreadExporter.CreateWorkbookExporter(SpreadDocumentFormat.Xlsx, stream))
                {
                    using (IWorksheetExporter worksheet = workbook.CreateWorksheetExporter(_sheetName.Length > 31 ? _sheetName[..31] : _sheetName))
                    {
                        SetColumnWidths(worksheet);
                        SetColumnHeaders(worksheet);
                        SetData(results, worksheet);
                    }
                }

                return await Task.FromResult(stream);
            }
        }

        // =============================================================================================================================================================================================================================================================================================================================================================================================================

        private async Task<StatusHistoryModel?[]?> GetReviewStatusHistoryOneEmployeeByReviewID_Main()
        {
            StatusHistoryModel?[]? results = (await _service.GetReviewStatusHistoryOneEmployeeByReviewID(_selectedReview!.ID!.Value, _database_OPS))?.ToArray();

            if (results != null)
            {
                foreach (StatusHistoryModel? result in results)
                {
                    if (result != null && result.Login_ID_StatusChangedBy != null)
                    {
                        AllUsers_OPS_DB? opsReviewee =
                            _allUsers_OPS_DB?.FirstOrDefault(q => q?.UserName?.Equals(result.Login_ID_Reviewee, StringComparison.InvariantCultureIgnoreCase) == true);
                        
                        if (opsReviewee != null)
                        {
                            result.FirstName_Reviewee = opsReviewee.FirstName;
                            result.LastName_Reviewee = opsReviewee.LastName;

                            AllUsers_OPS_DB? opsStatusChangedBy =
                                _allUsers_OPS_DB?.FirstOrDefault(q => q?.UserName?.Equals(result.Login_ID_StatusChangedBy, StringComparison.InvariantCultureIgnoreCase) == true);
                            
                            result.FirstName_StatusChangedBy = opsStatusChangedBy?.FirstName;
                            result.LastName_StatusChangedBy = opsStatusChangedBy?.LastName;
                        }
                        else
                        {
                            AllUsers_CMS_DB? cmsReviewee =
                                _allUsers_CMS_DB?.FirstOrDefault(q => q?.LoginID?.Equals(result.Login_ID_Reviewee, StringComparison.InvariantCultureIgnoreCase) == true);

                            if (cmsReviewee != null)
                            {
                                result.FirstName_Reviewee = cmsReviewee.FirstName;
                                result.LastName_Reviewee = cmsReviewee.LastName;

                                AllUsers_CMS_DB? cmsStatusChangedBy =
                                    _allUsers_CMS_DB?.FirstOrDefault(q => q?.LoginID?.Equals(result.Login_ID_StatusChangedBy, StringComparison.InvariantCultureIgnoreCase) == true);

                                result.FirstName_StatusChangedBy = cmsStatusChangedBy?.FirstName;
                                result.LastName_StatusChangedBy = cmsStatusChangedBy?.LastName;
                            }
                        }
                    }
                }
            }

            return results;
        }

        private void SetColumnHeaders(IWorksheetExporter worksheet)
        {
            using (IRowExporter row = worksheet.CreateRowExporter())
            {
                SpecialExcelExportClass.SetColumnHeader(row, "ID");
                SpecialExcelExportClass.SetColumnHeader(row, "Employee First Name");
                SpecialExcelExportClass.SetColumnHeader(row, "Employee Last Name");
                SpecialExcelExportClass.SetColumnHeader(row, "Old Review Status");
                SpecialExcelExportClass.SetColumnHeader(row, "New Review Status");
                SpecialExcelExportClass.SetColumnHeader(row, "Status Changed By");
                SpecialExcelExportClass.SetColumnHeader(row, "Status Changed Date");
            }
        }

        private void SetColumnWidths(IWorksheetExporter worksheet)
        {
            SpecialExcelExportClass.SetColumnWidth(worksheet, 60);
            SpecialExcelExportClass.SetColumnWidth(worksheet, 130);
            SpecialExcelExportClass.SetColumnWidth(worksheet, 130);
            SpecialExcelExportClass.SetColumnWidth(worksheet, 130);
            SpecialExcelExportClass.SetColumnWidth(worksheet, 130);
            SpecialExcelExportClass.SetColumnWidth(worksheet, 130);
            SpecialExcelExportClass.SetColumnWidth(worksheet, 160);
        }

        private void SetData(in StatusHistoryModel?[]? results, IWorksheetExporter worksheet)
        {
            foreach (StatusHistoryModel? result in results!)
            {
                if (result != null)
                {
                    using (IRowExporter row = worksheet.CreateRowExporter())
                    {
                        SpecialExcelExportClass.SetCellValue(row, result.Review_ID);
                        SpecialExcelExportClass.SetCellValue(row, result.FirstName_Reviewee);
                        SpecialExcelExportClass.SetCellValue(row, result.LastName_Reviewee);
                        SpecialExcelExportClass.SetCellValue(row, result.OldStatus);
                        SpecialExcelExportClass.SetCellValue(row, result.NewStatus);
                        SpecialExcelExportClass.SetCellValue(row, string.Concat(result.FirstName_StatusChangedBy, ' ', result.LastName_StatusChangedBy));
                        SpecialExcelExportClass.SetCellValue(row, result.WhenChanged);
                    }
                }
            }
        }
    }
}
