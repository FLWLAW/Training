using SqlServerDatabaseAccessLibrary;

namespace Training.Website.Services
{
    public class NavMenuServiceMethods
    {
        public IEnumerable<string?>? Administrator_LoginIDs(IDatabase? database) =>
            database!.QueryByStoredProcedure<string?>("usp_TRAINING_Questionnaire_GetAdministratorLoginIDs");

        public IEnumerable<string> Testers() =>
            [
                "drosenblum",
                "seisenman",
                "susan.eisenman",
#if DEBUG
                "rcuyan",
#endif
        ];
    }
}
