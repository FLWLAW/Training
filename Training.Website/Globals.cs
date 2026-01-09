using SqlServerDatabaseAccessLibrary;
using System.Text;
using Telerik.Blazor.Components;
using Telerik.Windows.Documents.Flow.Model;
using Training.Website.Models;

namespace Training.Website
{
    public class Globals
    {
        public const int MaximumTestAttemptsPerSession = 2;
        public const int RetakeTestDeadlineDays = 5;
        public const double TestPassingThreshold = 80;

        public const string YesNo = "Yes/No";
        public const string TrueFalse = "True/False";
        public const string MultipleChoice = "Multiple Choice";
        public const string RadioButtons = "Radio Buttons";
        public const string TextArea = "Text Area";
        public const string CurrentAnswerFormatError = "Invalid current answer format in PopulateCorrectAnswerDropDown()";
        public static readonly string[] YesNo_Choices = ["Yes", "No"];
        public static readonly string[] TrueFalse_Choices = ["True", "False"];

        public const string SelectAll_Verbiage = "-- Select All --";
        public const string Notary = "Notary";


        public static int CMS_UserID(AppState? appState) =>
            appState?.LoggedOnUser?.AppUserID ?? 0;

        public static string? ConcatenateSessionInfo(SessionInformationModel? session) => $"{session?.Session_ID} ({session?.DocTitle})";

        public static IEnumerable<string>? ConcatenateSessionInfoForDropDown(IEnumerable<SessionInformationModel>? sessionInfo)
        {
            if (sessionInfo == null)
                return null;
            else
            {
                List<string>? sessions = [];

                foreach (SessionInformationModel? session in sessionInfo)
                {
                    string? item = ConcatenateSessionInfo(session);

                    if (string.IsNullOrWhiteSpace(item) == false)
                        sessions.Add(item);
                }

                return sessions;
            }
        }

        public static SessionInformationModel? ConvertSessionStringToClass(string newValue)
        {
            SessionInformationModel? result = null;
            int openParenthesis = newValue.IndexOf('(');

            if (openParenthesis > -1)
            {
                int closeParenthesis = newValue.LastIndexOf(')');

                if (closeParenthesis > -1 && (int.TryParse(newValue[..openParenthesis].Trim(), out int sessionId) == true))
                    result = new SessionInformationModel()
                    {
                        Session_ID = sessionId,
                        DocTitle = newValue.Substring(openParenthesis + 1, closeParenthesis - openParenthesis - 1).Trim()
                    };
            }

            return result;
        }

        public static string? CurrentAnswerFormat(in Dictionary<int, string>? answerFormats, in QuestionsModel? question)
        {
            int? answerFormatID = question!.AnswerFormat;
            string? currentAnswerFormat = (answerFormatID != null) ? answerFormats?[answerFormatID.Value] : null;

            return currentAnswerFormat;
        }

        public static string ExportFilename(string filenameRaw, params object?[] additionalFilenameSegments)
        {
            StringBuilder filenameRefined = new(filenameRaw);

            if (additionalFilenameSegments != null && additionalFilenameSegments.Any() == true)
                foreach(object? segment in additionalFilenameSegments)
                    if (segment != null)
                        filenameRefined.Append($"_{segment.ToString()}");

            return filenameRefined.ToString();
        }

        public static async Task ExportToExcelFile<T>(TelerikGrid<T>? grid) => await grid!.SaveAsExcelFileAsync();

        public static IdValue<string> SelectAll(string id) =>
            new()
            {
                ID = id,
                Value = SelectAll_Verbiage
            };

        public static int OPS_UserID(AppState? appState) =>
            appState?.LoggedOnUser?.EmpID ?? 0;

    }
}
