using SonarWatcher.Entity;
using System.Collections.Generic;
using System.Linq;

namespace SonarWatcher
{
    public class CodeHealthService : IMeasurementService<double>
    {
        private readonly List<MetricSequence> _complexityMetrics;

        private readonly List<MetricSequence> _issuesMetrics;

        public CodeHealthService(List<MetricSequence> complexityMetrics, List<MetricSequence> issuesMetrics)
        {
            this._complexityMetrics = complexityMetrics;
            this._issuesMetrics = issuesMetrics;
        }

        public double Calculate()
        {
            var numOfLinesMetric = _complexityMetrics.Find(m => m.Name.Equals("Linhas"));
            var lines = numOfLinesMetric.GetMeasures().Last().Value;

            var bugsMetric = _issuesMetrics.Find(m => m.Name.Equals("bugs"));
            var bugs = bugsMetric.GetMeasures().Last().Value;

            var vulnerabilitiesMetric = _issuesMetrics.Find(m => m.Name.Equals("Vulnerabilidades"));
            var vulnerabilities = vulnerabilitiesMetric.GetMeasures().Last().Value;

            var codeSmellsMetric = _issuesMetrics.Find(m => m.Name.Equals("code_smells"));
            var codeSmells = codeSmellsMetric.GetMeasures().Last().Value;

            double codeHealth = (100 - ((codeSmells + bugs + vulnerabilities) / lines) * 1000) / 10;

            if (codeHealth < 0)
            {
                return 0;
            }

            return codeHealth;
        }
    }
}
