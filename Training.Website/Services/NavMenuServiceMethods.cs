using SqlServerDatabaseAccessLibrary;

namespace Training.Website.Services
{
    public class NavMenuServiceMethods
    {
        public IEnumerable<int?>? Managers(IDatabase? database) =>
            database!.QueryByStatement<int?>("SELECT ManagerAppUserID FROM AppUserManager");

        public IEnumerable<string> Testers() =>
            [
                "drosenblum",
                "seisenman",
                "susan.eisenman",
#if DEBUG
                "rcuyan",
#endif
            ];

        public IEnumerable<string?>? TrainingQuestionnaire_Administrator_LoginIDs(IDatabase? database) =>
            database!.QueryByStoredProcedure<string?>("usp_TRAINING_Questionnaire_GetAdministratorLoginIDs");


    }
}
