using Newtonsoft.Json;
using SonarWatcher.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SonarWatcher
{
    class SonarAPI
    {
        public SonarAPI()
        {
            ConfigureClient();
        }

        public void GenerateMetricsChartForAllProjects()
        {
            var sonarProjects = GetAllProjects();
            var chart = new SonarSerieChart();

            sonarProjects.Wait();
            if (sonarProjects.Exception == null && sonarProjects.Result != null)
            {
                foreach (var project in sonarProjects.Result)
                {
                    //TODO: Refatorar a cópia tripla
                    var issuesProjectMetricsTask = GetProjectThreeIsuesMetricsAsync(project.k);
                    issuesProjectMetricsTask.ContinueWith((projectMetrics) =>
                    {
                        if (projectMetrics.Exception == null && projectMetrics.Result != null)
                        {
                            var formatedMetrics = this.FormatMetrics(issuesProjectMetricsTask.Result.First());
                            chart.CreateChartsGrouped(formatedMetrics, project.nm, "Ocorrências por Tipo");
                        }
                    });


                    var severityProjectMetricsTask = GetSeverityProjectMetricsAsync(project.k);
                    severityProjectMetricsTask.ContinueWith((projectMetrics) =>
                    {
                        if (projectMetrics.Exception == null && projectMetrics.Result != null)
                        {
                            var formatedMetrics = this.FormatMetrics(severityProjectMetricsTask.Result.First());
                            chart.CreateChartsGrouped(formatedMetrics, project.nm, "Ocorrências por Severidade");
                        }
                    });


                    var complexityProjectMetricsTask = GetComplexityAndLineNumberProjectMetricsAsync(project.k);
                    complexityProjectMetricsTask.ContinueWith((projectMetrics) =>
                    {
                        if (projectMetrics.Exception == null && projectMetrics.Result != null)
                        {
                            var formatedMetrics = this.FormatMetrics(complexityProjectMetricsTask.Result.First());
                            chart.CreateChartsGrouped(formatedMetrics, project.nm, "Número de linhas x Complexidade ");
                        }
                    });
                }
            }
        }

        private List<MetricSequence> FormatMetrics(SonarMetricsJson projectMetrics)
        {
            List<MetricSequence> metrics = InitializeMetricsList(projectMetrics.cols);

            foreach (var projectMeasureInCertainDay in projectMetrics.cells)
            {
                for (int i = 0; i < projectMeasureInCertainDay.v.Count(); i++)
                {
                    metrics[i].AddMeasure(new DateValue(projectMeasureInCertainDay.d, projectMeasureInCertainDay.v[i]));
                }
            }

            return metrics;
        }

        private List<MetricSequence> InitializeMetricsList(List<Col> cols)
        {
            var metrics = new List<MetricSequence>();

            foreach (var col in cols)
            {
                metrics.Add(new MetricSequence(col.metric));
            }

            return metrics;
        }

        private HttpClient ConfigureClient()
        {
            string username = ConfigurationManager.AppSettings["sonarUserToken"];
            string password = "";
            string encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

            HttpClient serviceClient = new HttpClient();
            serviceClient.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);

            serviceClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["sonarURL"]);
            serviceClient.DefaultRequestHeaders.Accept.Clear();
            serviceClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return serviceClient;
        }

        private async Task<List<SonarMetricsJson>> GetAllProjectMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsAsync(projectKey, "metricsApiTemplateURL");
        }

        private async Task<List<SonarMetricsJson>> GetProjectThreeIsuesMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsAsync(projectKey, "threeIsuesMetricsApiTemplateURL");
        }

        private async Task<List<SonarMetricsJson>> GetSeverityProjectMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsAsync(projectKey, "severityMetricsApiTemplateURL");
        }

        private async Task<List<SonarMetricsJson>> GetComplexityAndLineNumberProjectMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsAsync(projectKey, "complexityAndLineNumberMetricsApiTemplateURL");
        }

        private async Task<List<SonarMetricsJson>> GetProjectMetricsAsync(string projectKey, string apiAppSettingKey)
        {
            List<SonarMetricsJson> sonarMetrics = null;
            string apiURLWithProjectParameter = string.Format(ConfigurationManager.AppSettings[apiAppSettingKey], projectKey);

            using (var serviceClient = this.ConfigureClient())
            {
                HttpResponseMessage response = await serviceClient.GetAsync(apiURLWithProjectParameter);
                if (response.IsSuccessStatusCode)
                {
                    var stringResult = await response.Content.ReadAsStringAsync();
                    sonarMetrics = JsonConvert.DeserializeObject<List<SonarMetricsJson>>(stringResult);
                }
            }

            return sonarMetrics;
        }

        private async Task<List<SonarProjectJsons>> GetAllProjects()
        {
            List<SonarProjectJsons> sonarProjects = null;

            using (var serviceClient = this.ConfigureClient())
            {
                HttpResponseMessage response = await serviceClient.GetAsync(ConfigurationManager.AppSettings["projectsApiURL"]);
                if (response.IsSuccessStatusCode)
                {
                    var stringResult = await response.Content.ReadAsStringAsync();
                    sonarProjects = JsonConvert.DeserializeObject<List<SonarProjectJsons>>(stringResult);
                }
            }

            return sonarProjects;
        }
    }
}
