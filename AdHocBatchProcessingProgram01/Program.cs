namespace AdHocBatchProcessingProgram01
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Process process = new();

            await process.Go();
        }
    }
}
