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
        private readonly List<Color> ChartColors;

        public SonarSerieChart()
        {
            this.ChartColors = new List<Color>() {
                Color.FromArgb(255,114,180,212),
                Color.FromArgb(255,22,160,133),
                Color.FromArgb(255,234,149,72),
                Color.FromArgb(255,170,117,193)
            };
        }

        public void CreateChartsIndividually(List<MetricSequence> formatedMetrics, string projectName, string chartName)
        {
            foreach (var metricSequence in formatedMetrics)
            {
                CreateCharts(ConvertMetricToSeries(metricSequence), projectName, chartName);
            }
        }

        public string CreateChartsGrouped(List<MetricSequence> formatedMetrics, string projectName, string chartName)
        {
            return CreateCharts(ConvertMetricToSeries(formatedMetrics), projectName, chartName);
        }

        private string CreateCharts(List<Series> series, string projectName, string chartName)
        {
            ChartArea chartArea = InitializeChartArea();
            Chart chart = InitializeChart(chartName: (string.Format("{0} - {1}", projectName, chartName)));
            string chartsDirectoryPath = GetOrCreateDirectoryForProjectCharts(projectName);

            chart.ChartAreas.Add(chartArea);

            foreach (var serie in series)
            {
                chart.Series.Add(serie);
            }
            string chartFullPath = Path.Combine(chartsDirectoryPath, CreateChartName(projectName, chartName));
            chart.SaveImage(chartFullPath, ChartImageFormat.Jpeg);

            return chartFullPath;
        }

        private string GetOrCreateDirectoryForProjectCharts(string projectName)
        {
            projectName = projectName.Replace("/", string.Empty).Replace("*", string.Empty);

            string chartsDirectoryPath = Path.Combine(ConfigurationManager.AppSettings["chartsDirectoryPath"], projectName);

            if (!Directory.Exists(chartsDirectoryPath))
                Directory.CreateDirectory(chartsDirectoryPath);
            return chartsDirectoryPath;
        }

        private Chart InitializeChart(string chartName)
        {
            var chart = new Chart();
            chart.BackColor = Color.White;
            chart.Width = 1200;
            chart.Titles.Add(chartName);
            chart.Legends.Add("Series");
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

        private List<Series> ConvertMetricToSeries(MetricSequence metricSequence)
        {
            Series serie = InitializeSerie(metricSequence.Name);

            foreach (var measure in metricSequence.GetMeasures())
            {
                serie.Points.AddXY(measure.Date.ToShortDateString(), measure.Value);
            }

            return new List<Series>() { serie };
        }

        private List<Series> ConvertMetricToSeries(List<MetricSequence> metricSequences)
        {
            var series = new List<Series>();
            ushort colorCounter = 0;
            metricSequences = metricSequences == null ? new List<MetricSequence>() : metricSequences;

            foreach (var metricSequence in metricSequences)
            {
                Series serie = InitializeSerie(metricSequence.Name, colorCounter++);

                foreach (var measure in metricSequence.GetMeasures())
                {
                    serie.Points.AddXY(measure.Date.ToShortDateString(), measure.Value);
                }

                series.Add(serie);
            }

            return series;
        }

        private Series InitializeSerie(string serieName, ushort colorCounter = 0)
        {
            var serie = new Series();
            serie.Name = serieName;
            serie.Color = GetNextColor(colorCounter);
            serie.ChartType = SeriesChartType.Column;
            serie.IsValueShownAsLabel = true;
            return serie;
        }

        private Color GetNextColor(ushort colorCounter)
        {
            ushort colorIndex = colorCounter > ChartColors.Count ? (ushort)(ChartColors.Count % colorCounter) : colorCounter;
            return ChartColors[colorIndex];
        }

        private string CreateChartName(string projectName, string metricName)
        {
            projectName = projectName.Replace("/", string.Empty).Replace("*", string.Empty);
            return new StringBuilder(projectName).Append('-').Append(metricName).Append('-').Append(DateTime.Now.ToString("ddMMyyyy-HHmmss")).Append(".jpg").ToString();
        }
    }
}