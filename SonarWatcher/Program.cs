using SonarWatcher.Entity;
using System;


namespace SonarWatcher
{
    class Program
    {
        static void Main(string[] args)
        {        
            var watcherApplication = new WatcherApplication();
            watcherApplication.SendMetricMailsToProjectHeads();

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
