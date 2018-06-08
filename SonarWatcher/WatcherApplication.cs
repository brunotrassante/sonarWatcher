﻿using SonarWatcher.Entity;
using SonarWatcher.Repository;
using System.Collections.Generic;
using System.Linq;
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
                var projects = sonarProjects.Result;

                foreach (var project in projects)
                {
                    if (!string.IsNullOrEmpty(project.Key))
                    {
                        var issuesMetricsTask = sonarAPI.GetProjectThreeIssuesMetricsAsync(project.Key);
                        var severityProjectMetricsTask = sonarAPI.GetSeverityProjectMetricsAsync(project.Key);
                        var complexityProjectMetricsTask = sonarAPI.GetComplexityAndLineNumberAndCodeQualityProjectMetricsAsync(project.Key);
                        var ratingTask = sonarAPI.GetProjectRatings(project.Key);

                        AllTasksFromAllProjects.Add(Task.WhenAll(issuesMetricsTask, severityProjectMetricsTask, complexityProjectMetricsTask, ratingTask).ContinueWith(
                            tasksResults =>
                            {
                                string typeChartPath = chart.CreateChartsGrouped(issuesMetricsTask.Result, project.Name, "Ocorrências por Tipo");
                                string severityChartPath = chart.CreateChartsGrouped(severityProjectMetricsTask.Result, project.Name, "Ocorrências por Severidade");
                                string complexityChartPath = chart.CreateChartsGrouped(complexityProjectMetricsTask.Result, project.Name, "Número de linhas x Complexidade");
                                ProjectRating rating = ratingTask.Result;

                                var projectMembers = personRepository.FindAllByProjectKey(project.Key);
                                string managerName = projectMembers.SingleOrDefault(m => m.Role == Role.Manager)?.Name ?? "Não cadastrado";
                                string headName = projectMembers.SingleOrDefault(m => m.Role == Role.Head)?.Name ?? "Não cadastrado";
                                IEnumerable<string> projecMemberEmails = projectMembers.Select(m => m.Email);
                                double codeHealthPercentage = new CodeHealthService(complexityProjectMetricsTask.Result, issuesMetricsTask.Result).Calculate();
                                List<CodeQualityMeasurement> calculatedCodeQuality = new CodeQualityService(complexityProjectMetricsTask.Result).Calculate();
                                EmailInfo email = new EmailInfo(project.Name, project.Key, managerName, headName, projecMemberEmails, complexityChartPath, typeChartPath, severityChartPath, codeHealthPercentage, calculatedCodeQuality, rating);

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
