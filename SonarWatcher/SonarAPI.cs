using Newtonsoft.Json;
using SonarWatcher.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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

        private JsonSerializerSettings GetDeserializationSettings()
        {
            return new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        private List<MetricSequence> FormatMetrics(SonarMetricsJson projectMetrics)
        {
            List<MetricSequence> metrics = InitializeMetricsList(projectMetrics.Cols);

            foreach (var projectMeasureInCertainDay in projectMetrics.Cells)
            {
                for (int i = 0; i < projectMeasureInCertainDay.V.Count; i++)
                {
                    metrics[i].AddMeasure(new DateValue(projectMeasureInCertainDay.D, projectMeasureInCertainDay.V[i]));
                }
            }

            return metrics;
        }

        private List<MetricSequence> InitializeMetricsList(List<Col> colunas)
        {
            var metrics = new List<MetricSequence>();

            foreach (var coluna in colunas)
            {
                metrics.Add(new MetricSequence(coluna.Metric));
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

        public async Task<List<MetricSequence>> GetAllProjectMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsFormatedAsync(projectKey, "metricsApiTemplateURL");
        }

        public async Task<List<MetricSequence>> GetProjectThreeIsuesMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsFormatedAsync(projectKey, "threeIsuesMetricsApiTemplateURL");
        }

        public async Task<List<MetricSequence>> GetSeverityProjectMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsFormatedAsync(projectKey, "severityMetricsApiTemplateURL");
        }

        public async Task<List<MetricSequence>> GetComplexityAndLineNumberAndCodeQualityProjectMetricsAsync(string projectKey)
        {
            return await GetProjectMetricsFormatedAsync(projectKey, "complexityAndLineNumberAndCodeQualityMetricsApiTemplateURL");
        }

        private async Task<List<MetricSequence>> GetProjectMetricsFormatedAsync(string projectKey, string valor)
        {
            List<MetricSequence> metrics = null;
            var issuesProjectMetricsTask = GetProjectMetricsAsync(projectKey, valor);
            await issuesProjectMetricsTask.ContinueWith((projectMetrics) =>
            {
                if (projectMetrics.Exception == null && projectMetrics.Result != null)
                {
                    metrics = FormatMetrics(issuesProjectMetricsTask.Result.First());
                }
            });
            return metrics;
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

                    sonarMetrics = JsonConvert.DeserializeObject<List<SonarMetricsJson>>(stringResult, GetDeserializationSettings());
                }
            }

            return sonarMetrics;
        }

        public async Task<List<SonarProjectJsons>> GetAllProjects()
        {
            List<SonarProjectJsons> sonarProjects = null;

            using (var serviceClient = this.ConfigureClient())
            {
                HttpResponseMessage response = await serviceClient.GetAsync(ConfigurationManager.AppSettings["projectsApiURL"]);
                if (response.IsSuccessStatusCode)
                {
                    var stringResult = await response.Content.ReadAsStringAsync();
                    sonarProjects = JsonConvert.DeserializeObject<List<SonarProjectJsons>>(stringResult, GetDeserializationSettings());
                }
            }

            return sonarProjects;
        }

        public async Task<ProjectRating> GetProjectRatings(string projectKey)
        {
            ProjectRating projectRatings = null;

            string apiURLWithProjectParameter = string.Format(ConfigurationManager.AppSettings["ratingsApiTemplateURL"], projectKey);

            using (var serviceClient = this.ConfigureClient())
            {
                HttpResponseMessage response = await serviceClient.GetAsync(apiURLWithProjectParameter);
                if (response.IsSuccessStatusCode)
                {
                    var stringResult = await response.Content.ReadAsStringAsync();
                    var projectRatingsJson = JsonConvert.DeserializeObject<SonarRatingJson>(stringResult, GetDeserializationSettings());

                    projectRatings = CreateProjectRatingPopulated(projectRatingsJson);
                }
            }

            return projectRatings;
        }

        private static ProjectRating CreateProjectRatingPopulated(SonarRatingJson projectRatingsJson)
        {
            ushort reabilityRating = 0;
            ushort securityRating = 0;
            ushort manutenibilityRating = 0;

            foreach (var measure in projectRatingsJson.Component.Measures)
            {
                switch (measure.Metric)
                {
                    case "security_rating":
                        securityRating = (ushort)decimal.Parse(measure.Value, CultureInfo.InvariantCulture);
                        break;
                    case "sqale_rating":
                        manutenibilityRating = (ushort)decimal.Parse(measure.Value, CultureInfo.InvariantCulture);
                        break;
                    case "reliability_rating":
                        reabilityRating = (ushort)decimal.Parse(measure.Value, CultureInfo.InvariantCulture);
                        break;
                    default:
                        break;
                }
            }

            return new ProjectRating(reabilityRating, securityRating, manutenibilityRating);
        }
    }
}
