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
            List<Task> AllTasksFromAllProjects = new List<Task>();

            sonarProjects.Wait();
            if (sonarProjects.Exception == null && sonarProjects.Result != null)
            {
                foreach (var project in sonarProjects.Result)
                {
                    if (!string.IsNullOrEmpty(project.K))
                    {
                        var issuesMetricsTask = sonarAPI.GetProjectThreeIsuesMetricsAsync(project.K);
                        var severityProjectMetricsTask = sonarAPI.GetSeverityProjectMetricsAsync(project.K);
                        var complexityProjectMetricsTask = sonarAPI.GetComplexityAndLineNumberProjectMetricsAsync(project.K);
                        var ratingTask = sonarAPI.GetProjectRatings(project.K);

                        AllTasksFromAllProjects.Add(Task.WhenAll(issuesMetricsTask, severityProjectMetricsTask, complexityProjectMetricsTask, ratingTask).ContinueWith(
                            tasksResults =>
                            {
                                string typeChartPath = chart.CreateChartsGrouped(issuesMetricsTask.Result, project.Nm, "Ocorrências por Tipo");
                                string severityChartPath = chart.CreateChartsGrouped(severityProjectMetricsTask.Result, project.Nm, "Ocorrências por Severidade");
                                string complexityChartPath = chart.CreateChartsGrouped(complexityProjectMetricsTask.Result, project.Nm, "Número de linhas x Complexidade");
                                IEnumerable<Person> projectMembers = personRepository.FindAllByProjectKey(project.K);
                                string managerName = projectMembers.SingleOrDefault(m => m.Role == Role.Manager)?.Name ?? "Não cadastrado";
                                string headName = projectMembers.SingleOrDefault(m => m.Role == Role.Head)?.Name ?? "Não cadastrado";
                                ProjectRating rating = ratingTask.Result;

                                IEnumerable<string> projecMemberEmails = projectMembers.Select(m => m.Email);
                                EmailInfo email = new EmailInfo(project.Nm, project.K, managerName, headName, projecMemberEmails, complexityChartPath, typeChartPath, severityChartPath, rating);

                                var emailService = new EmailService(email);
                                emailService.SendReportEmail();
                            }));
                    }
                }
                // Aguarda todas as atividades assincronas terminarem para seguir
                Task.WaitAll(AllTasksFromAllProjects.ToArray());
            }
        }

        public void SendNoRegistryMailsForProjectHeads()
        {
            var projectRepository = new ProjectRepository();
            var personRepository = new PersonRepository();
            var projectsWithNoSonarKey = projectRepository.FindAllWithNoProjectKey();

            foreach (var project in projectsWithNoSonarKey)
            {
                IEnumerable<Person> projectMembers = personRepository.FindAllByProjectId(project.Id);
                string managerName = projectMembers.SingleOrDefault(m => m.Role == Role.Manager)?.Name ?? "Não cadastrado";
                string headName = projectMembers.SingleOrDefault(m => m.Role == Role.Head)?.Name ?? "Não cadastrado";
                IEnumerable<string> projecMemberEmails = projectMembers.Select(m => m.Email);

                EmailInfo email = new EmailInfo(project.Name, managerName, headName, projecMemberEmails);

                var emailService = new EmailService(email);
                emailService.SendNotRegisteredEmail();
            }
        }
    }
}
