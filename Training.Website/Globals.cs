using Training.Website.Models;

namespace Training.Website
{
    public class Globals
    {
        public const string YesNo = "Yes/No";
        public const string TrueFalse = "True/False";
        public const string MultipleChoice = "Multiple Choice";

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

        public static IEnumerable<string?>? ItemsForCorrectAnswerDropDown(string? currentAnswerFormat, AnswerChoicesModel?[]? multipleChoiceAnswers_DB)
        {
            switch (currentAnswerFormat)
            {
                case Globals.MultipleChoice: return multipleChoiceAnswers_DB?.Select(q => q?.AnswerLetter.ToString()).Order();
                case Globals.YesNo: return ["Yes", "No"];
                case Globals.TrueFalse: return ["True", "False"];
                case null: return [];
                default: throw new Exception("Invalid current answer format in PopulateCorrectAnswerDropDown()");
            }
        }

        public static int UserID(AppState? appState) =>
            appState?.LoggedOnUser?.AppUserID ?? 0;
    }
}
