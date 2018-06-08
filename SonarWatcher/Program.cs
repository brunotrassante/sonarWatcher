using SonarWatcher.Entity;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace SonarWatcher
{
    static class Program
    {
        static void Main()
        {
            WriteInLog("Iniciado em " + DateTime.Now.ToString(CultureInfo.GetCultureInfo("pt-BR")));

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
                WriteInLog("Encerrado em " + DateTime.Now.ToString(CultureInfo.GetCultureInfo("pt-BR")));
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
