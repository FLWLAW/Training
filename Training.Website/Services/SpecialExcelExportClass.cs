using Microsoft.JSInterop;
using System.Data;
using Telerik.Documents.SpreadsheetStreaming;
using Telerik.SvgIcons;

namespace Training.Website.Services
{
    public class SpecialExcelExportClass
    {
        public static async Task ExportToBrowser(MemoryStream? stream, string? filename, IJSRuntime? js)
        {
            if (stream == null)
                throw new NoNullAllowedException("[stream] cannot ne null in ExportToBrowser().");
            else if (string.IsNullOrWhiteSpace(filename) == true)
                throw new NoNullAllowedException("[filename] cannot be null in ExportToBrowser().");
            else if (filename?.EndsWith(".xlsx") == false && filename?.EndsWith(".xls") == false && filename?.EndsWith(".xlsb") == false)
                throw new ArgumentException("[filename] must have an .xls, .xlsx or .xlsb extension, and that extension must be the final extension.");
            else if (js == null)
                throw new NoNullAllowedException("[js] cannot be null in ExportToBrowser().");
            else
            {
                stream.Position = 0;    // VERY IMPORTANT!!!!!
                using var streamRef = new DotNetStreamReference(stream: stream);
                await js.InvokeVoidAsync("downloadFileFromStream", filename, streamRef);
            }
        }

        public static void SetCellValue(IRowExporter row, object? value)
        {
            using (ICellExporter cell = row.CreateCellExporter())
            {
                if (value == null)
                    cell.SetValue(string.Empty);
                else if (value is bool boolValue)
                    cell.SetValue(boolValue);
                else if (value is DateTime dateTimeValue)
                {
                    string format = (dateTimeValue.TimeOfDay.TotalSeconds > 0) ? "mm/dd/yyyy hh:mm:ss AM/PM" : "mm/dd/yyyy";

                    cell.SetFormat(new SpreadCellFormat { NumberFormat = format });
                    cell.SetValue(dateTimeValue);
                }
                else if (value is decimal decValue)
                    cell.SetValue((double)decValue);
                else if (value is double dblValue)
                    cell.SetValue(dblValue);
                else if (value is long longValue)
                    cell.SetValue(longValue);
                else if (value is int intValue)
                    cell.SetValue(intValue);
                else if (value is string stringValue)
                    cell.SetValue(stringValue);
                else
                    cell.SetValue(value.ToString()!);
            }
        }

        public static void SetColumnHeader(IRowExporter row, string header)
        {
            using (ICellExporter cell = row.CreateCellExporter())
            {
                cell.SetFormat(new SpreadCellFormat { IsBold = true, Fill = SpreadPatternFill.CreateSolidFill(new SpreadColor(211, 211, 211)) });
                cell.SetValue(header);
            }
        }

        public static void SetColumnWidth(IWorksheetExporter worksheet, double width)
        {
            using (IColumnExporter column = worksheet.CreateColumnExporter())
                column.SetWidthInPixels(width);
        }
    }
}
