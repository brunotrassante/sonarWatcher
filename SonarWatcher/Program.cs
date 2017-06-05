using SonarWatcher.Entity;
using System;
using System.IO;
using System.Reflection;

namespace SonarWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteInLog("Iniciado em " + DateTime.Now.ToString());

            try
            {
                var watcherApplication = new WatcherApplication();
                watcherApplication.SendMetricMailsToProjectHeads();
                watcherApplication.SendNoRegistryMailsForProjectHeads();
                WriteInLog("Sucesso");
            }
            catch (Exception ex)
            {
                WriteInLog(ex.ToString());
            }
            finally
            {
                WriteInLog("Encerrado em " + DateTime.Now.ToString());
                WriteInLog("-------------------------------");
            }
        }

        static void WriteInLog(string message)
        {
            using (StreamWriter sw = File.AppendText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log.txt")))
            {
                sw.WriteLine(message);
            }
        }
    }
}
