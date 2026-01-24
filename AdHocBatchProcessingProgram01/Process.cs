using SqlServerDatabaseAccessLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Training.Website.Models.Users;
using Training.Website.Services;

namespace AdHocBatchProcessingProgram01
{
    internal class Process
    {
#if DEBUG
        private static readonly string _opsDB = Properties.Resources.TrainingDatabaseConnectionString_DEVELOPMENT;
        private static readonly string _cmsDB = Properties.Resources.CmsDatabaseConnectionString_DEVELOPMENT;
#else
        private static readonly string _opsDB = Properties.Resources.TrainingDatabaseConnectionString_PRODUCTION;
        private static readonly string _cmsDB = Properties.Resources.CmsDatabaseConnectionString_PRODUCTION;
#endif

        private static readonly SqlDatabase _sqlDB_OPS = new(_opsDB);
        private static readonly SqlDatabase _sqlDB_CMS = new(_cmsDB);

        private const string _rawFile = "Data.txt";

        public async Task Go()
        {
#if !DEBUG
            Console.WriteLine("WARNING: NOT IN DEBUG MODE. PRESS 'Y' AND [ENTER] TO CONTINUE.");
            string? response = Console.ReadLine();

            if (response != "Y")
            {
                Console.WriteLine("EXITING...");
                Environment.Exit(0);
            }
#endif

            InputFileClass[] rawLines = ReadRawFile();



            CleanAnswerToQuestion5(rawLines);
            await AddToDatabase(rawLines);
        }

        // =========================================================================================================================================================================================================================================================================================================

        private async Task AddToDatabase(InputFileClass[] rawLines)
        {
            CommonServiceMethods commonService = new();

            AllUsers_CMS_DB?[]? allUsers_CMS = (await commonService.GetAllUsers_CMS_DB(_sqlDB_CMS))?.ToArray();

            if (allUsers_CMS == null || allUsers_CMS.Length == 0)
                throw new NoNullAllowedException("allUsers_CMS cannot be null or zero-length.");
            else
            {
                AllUsers_OPS_DB?[]? allUsers_OPS = (await commonService.GetAllUsers_OPS_DB(_sqlDB_OPS))?.ToArray();

                if (allUsers_OPS == null || allUsers_OPS.Length == 0)
                    throw new NoNullAllowedException("allUsers_OPS cannot be null or zero-length.");
                else
                {
                    const int reviewYear = 2025;

                    PerformanceReviewServiceMethods performanceReviewService = new();

                    foreach (InputFileClass rawLine in rawLines)
                    {
                        AllUsers_OPS_DB? opsReviewee = allUsers_OPS?.FirstOrDefault
                            (
                                q => q?.FirstName?.Equals(rawLine.FirstName, StringComparison.InvariantCultureIgnoreCase) == true &&
                                q.LastName?.Equals(rawLine.LastName, StringComparison.InvariantCultureIgnoreCase) == true
                            );

                        if (opsReviewee == null)
                            throw new NoNullAllowedException("opsReviewee cannot be null.");
                        else
                        {
                            AllUsers_CMS_DB? cmsReviewee = allUsers_CMS?.FirstOrDefault
                                (
                                    q => q?.FirstName?.Equals(rawLine.FirstName, StringComparison.InvariantCultureIgnoreCase) == true &&
                                    q?.LastName?.Equals(rawLine.LastName, StringComparison.InvariantCultureIgnoreCase) == true
                                );

                            if (cmsReviewee == null)
                                throw new NoNullAllowedException("cmsReviewee cannot be null.");
                            else
                            {
                                int? space = rawLine.FullSupervisorName?.LastIndexOf(' ');

                                if (space == null || space == -1)
                                    throw new NoNullAllowedException("[space] not found.");
                                else if (rawLine == null)
                                    throw new NoNullAllowedException("[rawLine] cannot be null.");
                                else if (rawLine.FullSupervisorName == null)
                                    throw new NoNullAllowedException("[FullSupervisorName] cannot be null.");
                                else
                                {
                                    string? supervisorFirstName = rawLine.FullSupervisorName?[..space.Value].Trim();
                                    string? supervisorLastName = rawLine.FullSupervisorName?[space.Value..].Trim();

                                    AllUsers_OPS_DB? opsReviewer = allUsers_OPS?.FirstOrDefault
                                        (
                                            q => q?.FirstName?.Equals(supervisorFirstName, StringComparison.InvariantCultureIgnoreCase) == true &&
                                            q?.LastName?.Equals(supervisorLastName, StringComparison.InvariantCultureIgnoreCase) == true
                                        );

                                    if (opsReviewer == null)
                                        throw new NoNullAllowedException("[opsReviewer] cannot be null.");
                                    else
                                    {
                                        AllUsers_CMS_DB? cmsReviewer = allUsers_CMS?.FirstOrDefault
                                            (
                                                q => q?.FirstName?.Equals(supervisorFirstName, StringComparison.InvariantCultureIgnoreCase) == true &&
                                                q?.LastName?.Equals(supervisorLastName, StringComparison.InvariantCultureIgnoreCase) == true
                                            );

                                        if (cmsReviewer == null)
                                            throw new NoNullAllowedException("[cmsReviewer] cannot be null.");
                                        else
                                        {
                                            Console.WriteLine(rawLine.FullName);

                                            if (rawLine.FullName == "Jennifer Grawunder" || rawLine.FullName == "Roberto Cuyan")
                                            {
                                                Console.WriteLine($"NOT INPUTTING {rawLine.FullName} BECAUSE IT IS ALREADY IN DATABASE.");
                                                Console.ReadLine();
                                            }
                                            else
                                            {

                                                int? reviewID = await performanceReviewService.InsertReviewAndFirstStatusChange
                                                    (
                                                        reviewYear,
                                                        opsReviewer.Emp_ID!.Value, opsReviewee!.Emp_ID!.Value,
                                                        cmsReviewer.AppUserID!.Value, cmsReviewee.AppUserID!.Value,
                                                        opsReviewer.UserName!, opsReviewee.UserName!,
                                                        _sqlDB_OPS
                                                    );

                                                if (reviewID == null)
                                                    throw new NoNullAllowedException("[reviewID] cannot be null.");
                                                else
                                                    await performanceReviewService.UpsertPerformanceReviewAnswer_Main
                                                        (
                                                            reviewID.Value, 5, rawLine.AnswerToQuestion5!,
                                                            cmsReviewer.AppUserID!.Value, opsReviewer.Emp_ID!.Value, opsReviewer.UserName, false,
                                                            _sqlDB_OPS
                                                        );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CleanAnswerToQuestion5(InputFileClass[] rawLines)
        {
            int lineIndex = 1;  // MATCH UP WITH EXCEL FILE

            foreach (InputFileClass rawLine in rawLines)
            {
                string? rawAnswer = rawLine.AnswerToQuestion5;

                lineIndex++;
                if (rawAnswer == null)
                    throw new NoNullAllowedException($"Error in line #{lineIndex}. Line in null.");
                else
                { 
                    int indexA = rawAnswer.IndexOf(" A)");
                    int indexB = rawAnswer.IndexOf(" B)");
                    int indexC = rawAnswer.IndexOf(" C)");

                    string beginning = indexA > -1 ?  rawAnswer[..indexA] : rawAnswer;

                    string partA = indexA > -1 && indexB > -1
                        ? rawAnswer[indexA..indexB].Replace("  ", " ").Trim()
                        : indexA > -1
                            ? rawAnswer[indexA..]
                            : string.Empty;

                    string partB;
                    if (indexA > -1 && indexB > -1 && indexC > -1)
                        partB = rawAnswer[indexB..indexC].Replace("  ", " ").Trim();
                    else if (indexA > -1 && indexB > -1)
                        partB = rawAnswer[indexB..];
                    else
                        partB = string.Empty;

                    string partC = indexC > -1 ? rawAnswer[indexC..].Replace("  ", " ").Trim() : string.Empty;

                    rawLine.AnswerToQuestion5 = string.Concat(beginning, "\r\n", partA, "\r\n", partB, "\r\n", partC);
                }
            }
        }

        private InputFileClass[] ReadRawFile()
        {
            if (File.Exists(_rawFile) == false)
                throw new FileNotFoundException(_rawFile);
            else
            {
                string?[]? rawLines = File.ReadAllLines(_rawFile);

                if (rawLines == null || rawLines.Length == 0)
                    throw new Exception($"{_rawFile} is null or empty.");
                else
                {
                    const int ProperFieldCount = 5;

                    int lineIndex = 0;
                    List<InputFileClass> lines = [];

                    foreach (string? line in rawLines)
                    {
                        lineIndex++;
                        if (string.IsNullOrWhiteSpace(line) == false && line.StartsWith("Name", StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            string?[]? parts = line.Split('\t');

                            if (parts == null)
                                throw new Exception($"Error on line #{lineIndex}. parts[] == null");
                            else
                            {
                                if (parts.Length != ProperFieldCount)
                                    throw new Exception($"Error on line #{lineIndex}. [length] = {parts.Length}. Should be: {ProperFieldCount}.");
                                else
                                {
                                    InputFileClass inputFileLine = new()
                                    {
                                        FullName = parts[0],
                                        FirstName = parts[1],
                                        LastName = parts[2],
                                        FullSupervisorName = parts[3],
                                        AnswerToQuestion5 = parts[4]
                                    };
                                    lines.Add(inputFileLine);
                                }
                            }
                        }
                    }

                    return [.. lines];
                }
            }
        }
    }
}
