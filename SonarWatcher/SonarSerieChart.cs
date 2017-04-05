using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Drawing;
using System;
using System.Text;
using SonarWatcher.Entity;
using System.Configuration;

namespace SonarWatcher
{
    class SonarSerieChart
    {
        public void CreateCharts(List<MetricSequence> formatedMetrics, string projectName)
        {
            foreach (var metricSequence in formatedMetrics)
            {
                CreateChart(ConvertMetricToDataPoints(metricSequence), projectName);
            }
        }

        private void CreateChart(Series serie, string projectName)
        {
            ChartArea chartArea = InitializeChartArea();
            Chart chart = InitializeChart(chartName: (string.Format("{0} - {1}", projectName, serie.Name)));
            string chartsDirectoryPath = ConfigurationManager.AppSettings["chartsDirectoryPath"];

            chart.ChartAreas.Add(chartArea);
            chart.Series.Add(serie);

            chart.SaveImage(Path.Combine(chartsDirectoryPath, CreateChartName(projectName, serie.Name)), ChartImageFormat.Png);
        }

        private Chart InitializeChart(string chartName)
        {
            var chart = new Chart();
            chart.BackColor = Color.White;
            chart.Width = 800;
            chart.Titles.Add(chartName);
            return chart;
        }

        private ChartArea InitializeChartArea()
        {
            var chartArea = new ChartArea();
            chartArea.AxisX.Title = "Data da Medição";
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.LabelsAngleStep30;
            chartArea.AxisX.IsLabelAutoFit = true;
            chartArea.AxisX.Interval = 1;
            chartArea.AxisY.Title = "Valor";
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            return chartArea;
        }

        private Series ConvertMetricToDataPoints(MetricSequence metricSequence)
        {
            Series serie = InitializeSerie(metricSequence.Name);

            foreach (var measure in metricSequence.GetMeasures())
            {
                serie.Points.AddXY(measure.Date.ToShortDateString(), measure.Value);
            }

            return serie;
        }

        private Series InitializeSerie(string serieName)
        {
            var serie = new Series();
            serie.Name = serieName;
            serie.ChartType = SeriesChartType.Column;
            serie.IsValueShownAsLabel = true;
            return serie;
        }

        private string CreateChartName(string projectName, string metricName)
        {
            projectName = projectName.Replace("/", string.Empty).Replace("*", string.Empty);
            return new StringBuilder("Sonar-").Append(projectName).Append('-').Append(metricName).Append('-').Append(DateTime.Now.ToString("ddMMyyyy-HHmmss")).Append(".png").ToString();
        }
    }
}