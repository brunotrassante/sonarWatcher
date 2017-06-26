using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SonarWatcher.Entity
{
    public class MetricSequence
    {
        public string Name { get; private set; }

        private List<DateValue> Measures { get; set; }

        public MetricSequence(string metricName)
        {
            switch(metricName)
            {
                case "ncloc":
                    this.Name = "Linhas";
                    break;
                case "complexity":
                    this.Name = "Complexidade";
                    break;
                case "sqale_index":
                    this.Name = "Débito Técnico (min)";
                    break;
                case "vulnerabilities":
                    this.Name = "Vulnerabilidades";
                    break;
                default:
                    this.Name = metricName;
                    break;
            }
            Measures = new List<DateValue>();
        }

        public IEnumerable<DateValue> GetMeasures()
        {
            return Measures;
        }

        public void AddMeasure(DateValue dateValue)
        {
            if (!AlreadyHasAMeasureInTheSameWeek(dateValue))
                Measures.Add(dateValue);

            if (Measures.Count > 16)
                Measures.RemoveAt(0);
        }

        private bool AlreadyHasAMeasureInTheSameWeek(DateValue dateValue)
        {
            GregorianCalendar calendar = new GregorianCalendar();
            return this.Measures.Any(measure => calendar.GetWeekOfYear(measure.Date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday) == calendar.GetWeekOfYear(dateValue.Date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday) && measure.Date.Year == dateValue.Date.Year);
        }
    }
}
