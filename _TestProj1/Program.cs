using Dapper;
using SqlServerDatabaseAccessLibrary;
using System.Data;

namespace _TestProj1
{
    internal class Program
    {
        private const string _connStr = "Server=btdb;Database=FLW_OP;Integrated Security=SSPI;TrustServerCertificate=true;MultipleActiveResultSets=true;";
        
        static async Task Main(string[] args)
        {
            await Method2();
        }

        /*
        static async Task Method1()
        {
            SqlDatabase db = new(_connStr);
            InsertQuestion_Parameters parameters = new()
            {
                Training_SESSION_ID = 1,
                Question = "What is your favorite color?",
                AnswerFormat_ID = 2,
                CreatedBy_ID = 1
            };
            CurrentIdClass[] result = 
                (
                    await db.QueryByStoredProcedureAsync<CurrentIdClass, InsertQuestion_Parameters>
                    ("usp_Training_Questionnaire_InsertQuestion", parameters)
                ).ToArray();
            //int id = await db.NonQueryByStoredProcedureAsync("usp_Training_Questionnaire_InsertQuestion", parameters);

            Console.WriteLine($"Inserted Question ID: {result[0].CurrentID}");
            Console.ReadLine();
        }
        */

        static async Task Method2()
        {
            SqlDatabase db = new(_connStr);
            DynamicParameters parameters = new();

            parameters.Add("@Training_SESSION_ID", 1, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@QuestionNumber", 1000000, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Question", "What is your favorite color?", dbType: DbType.String, direction: ParameterDirection.Input);
            parameters.Add("@AnswerFormat_ID", 3, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@CreatedBy_ID", 4, dbType: DbType.Int32, direction: ParameterDirection.Input);
            parameters.Add("@Active", true, dbType: DbType.Boolean, direction: ParameterDirection.Input);
            parameters.Add("@Current_ID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            int currentID = await db.NonQueryByStoredProcedureOutputParameterAsync<int>
                ("usp_Training_Questionnaire_InsertQuestion", "@Current_ID", parameters);

            Console.WriteLine($"Inserted Question ID: {currentID}");
        }
    }
}
