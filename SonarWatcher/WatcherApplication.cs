using SonarWatcher.Entity;
using SonarWatcher.Repository;
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
            var personRepository = new PersonRepository();
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
                            //TODO: Tratamento de erros
                            string typeChartPath = chart.CreateChartsGrouped(issuesMetricsTask.Result, project.nm, "Ocorrências por Tipo");
                            string severityChartPath = chart.CreateChartsGrouped(severityProjectMetricsTask.Result, project.nm, "Ocorrências por Severidade");
                            string complexityChartPath = chart.CreateChartsGrouped(complexityProjectMetricsTask.Result, project.nm, "Número de linhas x Complexidade");
                            ProjectRating rating = ratingTask.Result;

                            var projectMembers = personRepository.FindAllByProjectKey(project.k);
                            string managerName = projectMembers.SingleOrDefault(m => m.Role == RoleEnum.Manager)?.Name ?? "Não cadastrado";
                            string headName = projectMembers.SingleOrDefault(m => m.Role == RoleEnum.Head)?.Name ?? "Não cadastrado";
                            IEnumerable<string> projecMemberEmails = projectMembers.Select(m => m.Email);
                            EmailInfo email = new EmailInfo(project.nm, project.k, managerName, headName, projecMemberEmails, complexityChartPath, typeChartPath, severityChartPath, rating);

                            var emailService = new EmailService(email);
                            emailService.SendEmail();
                        });
                }
            }
        }
    }
}
