using Telerik.Documents.SpreadsheetStreaming;
using Training.Website.Models.Reviews;

namespace Training.Website.Services
{
    public class PerformanceReviewAllEmployeesStatusExcelExport
    {
        public async Task<MemoryStream?> Go(int reviewYear, PerformanceReviewStatusesAllUsersByReviewYearModel?[]? results)
        {
            MemoryStream stream = new();    // CANNOT USE "using" HERE, BECAUSE "using" WILL CLOSE THE STREAM BEFORE IT IS RETURNED

            using (IWorkbookExporter workbook = SpreadExporter.CreateWorkbookExporter(SpreadDocumentFormat.Xlsx, stream))
            {
                using (IWorksheetExporter worksheet = workbook.CreateWorksheetExporter($"All Users Status Year {reviewYear}"))
                {
                    SetColumnWidths(worksheet);
                    SetColumnHeaders(worksheet);
                    SetData(results, worksheet);
                }
            }

            return await Task.FromResult(stream);
        }

        // ===================================================================================================================================================================================================================================================================================================================================================

        private void SetColumnHeaders(IWorksheetExporter worksheet)
        {
            using (IRowExporter row = worksheet.CreateRowExporter())
            {
                SpecialExcelExportClass.SetColumnHeader(row, "ID");
                SpecialExcelExportClass.SetColumnHeader(row, "Review ID");
                SpecialExcelExportClass.SetColumnHeader(row, "Employee First Name");
                SpecialExcelExportClass.SetColumnHeader(row, "Employee Last Name");
                SpecialExcelExportClass.SetColumnHeader(row, "Current Review Status");
                SpecialExcelExportClass.SetColumnHeader(row, "Status Changed By");
                SpecialExcelExportClass.SetColumnHeader(row, "Status Changed Date");
                SpecialExcelExportClass.SetColumnHeader(row, "Review Meeting Held On");

            }
        }

        private void SetColumnWidths(IWorksheetExporter worksheet)
        {
            SpecialExcelExportClass.SetColumnWidth(worksheet, 60);      // ID
            SpecialExcelExportClass.SetColumnWidth(worksheet, 90);      // REVIEW ID
            SpecialExcelExportClass.SetColumnWidth(worksheet, 150);     // EMP. FIRST NAME
            SpecialExcelExportClass.SetColumnWidth(worksheet, 150);     // EMP. LAST NAME
            SpecialExcelExportClass.SetColumnWidth(worksheet, 150);     // CURRENT REVIEW STATUS
            SpecialExcelExportClass.SetColumnWidth(worksheet, 130);     // STATUS CHANGED BY
            SpecialExcelExportClass.SetColumnWidth(worksheet, 160);     // STATUS CHANGED DATE
            SpecialExcelExportClass.SetColumnWidth(worksheet, 170);     // REVIEW MEETING HELD ON
        }

        private void SetData(PerformanceReviewStatusesAllUsersByReviewYearModel?[]? results, IWorksheetExporter worksheet)
        {
            if (results != null)
            {
                foreach (PerformanceReviewStatusesAllUsersByReviewYearModel? result in results)
                {
                    if (result != null)
                    {
                        using (IRowExporter row = worksheet.CreateRowExporter())
                        {
                            SpecialExcelExportClass.SetCellValue(row, result.ID);
                            SpecialExcelExportClass.SetCellValue(row, result.Review_ID);
                            SpecialExcelExportClass.SetCellValue(row, result.FirstName_Reviewee);
                            SpecialExcelExportClass.SetCellValue(row, result.LastName_Reviewee);
                            SpecialExcelExportClass.SetCellValue(row, result.NewStatus);
                            SpecialExcelExportClass.SetCellValue(row, result.FullName_StatusChangedBy);
                            SpecialExcelExportClass.SetCellValue(row, result.WhenChanged);
                            SpecialExcelExportClass.SetCellValue(row, result.ReviewMeetingHeldOn);
                        }
                    }
                }
            }
        }
    }
}
