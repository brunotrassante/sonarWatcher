using SonarWatcher.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarWatcher
{
    public class WatcherApplication
    {
        public void SendMetricMailsToProjectHeads()
        {
            var sonarAPI = new SonarAPI();
            var sonarProjects = sonarAPI.GetAllProjects();
            var chart = new SonarSerieChart();

            sonarProjects.Wait();
            if (sonarProjects.Exception == null && sonarProjects.Result != null)
            {
                foreach (var project in sonarProjects.Result)
                {
                    var issuesMetricsTask = sonarAPI.GetProjectThreeIsuesMetricsAsync(project.k);
                    var severityProjectMetricsTask = sonarAPI.GetSeverityProjectMetricsAsync(project.k);
                    var complexityProjectMetricsTask = sonarAPI.GetComplexityAndLineNumberProjectMetricsAsync(project.k);
                    var ratingTask = sonarAPI.GetProjectRatings(project.k);

                    Task.WhenAll(issuesMetricsTask, severityProjectMetricsTask, complexityProjectMetricsTask, ratingTask).ContinueWith(
                        tasksResults =>
                        {
                            var typeChartPath = chart.CreateChartsGrouped(issuesMetricsTask.Result, project.nm, "Ocorrências por Tipo");
                            var severityChartPath = chart.CreateChartsGrouped(severityProjectMetricsTask.Result, project.nm, "Ocorrências por Severidade");
                            var complexityChartPath = chart.CreateChartsGrouped(complexityProjectMetricsTask.Result, project.nm, "Número de linhas x Complexidade");
                            var rating = ratingTask.Result;
                            EmailInfo email = new EmailInfo(project.nm, project.k, "André Hoffmann", "John Voloski", "bruno@cwi.com.br", complexityChartPath, typeChartPath, severityChartPath, rating);

                            var emailService = new EmailService();
                            emailService.SendEmail(email);
                        });
                }
            }
        }
    }
}
