using System;


namespace SonarWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var sonarAPI = new SonarAPI();
            sonarAPI.GenerateMetricsChartForAllProjects();
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
