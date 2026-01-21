using SqlServerDatabaseAccessLibrary;
using Telerik.Documents.SpreadsheetStreaming;
using Training.Website.Models.Reviews;
using Training.Website.Models.Users;
using Training.Website.Services;

namespace Training.Website.Components.Pages
{
    public class PerformanceReviewExcelExport
    {
        private readonly int _selectedReviewYear;
        private readonly string _sheetName;
        private readonly UsersForDropDownModel? _selectedUser;
        private readonly ReviewModel? _selectedReview;
        private readonly AllUsers_OPS_DB?[]? _allUsers_OPS_DB;
        private readonly AllUsers_CMS_DB?[]? _allUsers_CMS_DB;
        private readonly PerformanceReviewServiceMethods _service;
        private readonly IDatabase? _database_OPS;

        public PerformanceReviewExcelExport
            (
                int selectedReviewYear,
                string sheetName,
                UsersForDropDownModel? selectedUser,
                ReviewModel? selectedReview,
                AllUsers_OPS_DB?[]? allUsers_OPS_DB,
                AllUsers_CMS_DB?[]? allUsers_CMS_DB,
                PerformanceReviewServiceMethods service,
                IDatabase? database_OPS
            )
        {
            _selectedReviewYear = selectedReviewYear;
            _sheetName = sheetName;
            _selectedUser = selectedUser;
            _selectedReview = selectedReview;
            _allUsers_CMS_DB = allUsers_CMS_DB;
            _allUsers_OPS_DB = allUsers_OPS_DB;
            _service = service;
            _database_OPS = database_OPS;
        }

        public async Task<MemoryStream?> Go()
        {
            StatusHistoryModel?[]? results = await GetReviewStatusHistoryByReviewID_Main();

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

        private async Task<StatusHistoryModel?[]?> GetReviewStatusHistoryByReviewID_Main()
        {
            StatusHistoryModel?[]? results = (await _service.GetReviewStatusHistoryByReviewID(_selectedReview!.ID!.Value, _database_OPS))?.ToArray();

            if (results != null)
            {
                foreach (StatusHistoryModel? result in results)
                {
                    if (result != null && result.StatusChangedBy != null)
                    {
                        AllUsers_OPS_DB? opsUser = _allUsers_OPS_DB?.FirstOrDefault(q => q?.UserName?.Equals(result.StatusChangedBy, StringComparison.InvariantCultureIgnoreCase) == true);
                        if (opsUser != null)
                        {
                            result.FirstName = opsUser.FirstName;
                            result.LastName = opsUser.LastName;
                        }
                        else
                        {
                            AllUsers_CMS_DB? cmsUser = _allUsers_CMS_DB?.FirstOrDefault(q => q?.LoginID?.Equals(result.StatusChangedBy, StringComparison.InvariantCultureIgnoreCase) == true);
                            if (cmsUser != null)
                            {
                                result.FirstName = cmsUser.FirstName;
                                result.LastName = cmsUser.LastName;
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
                        SpecialExcelExportClass.SetCellValue(row, result.FirstName);
                        SpecialExcelExportClass.SetCellValue(row, result.LastName);
                        SpecialExcelExportClass.SetCellValue(row, result.OldStatus);
                        SpecialExcelExportClass.SetCellValue(row, result.NewStatus);
                        SpecialExcelExportClass.SetCellValue(row, result.StatusChangedBy);
                        SpecialExcelExportClass.SetCellValue(row, result.WhenChanged);
                    }
                }
            }
        }
    }
}
