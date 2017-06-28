using System;

namespace SonarWatcher.Entity
{
    public class CodeQualityMeasurement
    {
        public double SqaleIndex { get; private set; }
        public double Lines { get; private set; }
        public DateTime MeasurementDate { get; private set; }
        

        public CodeQualityMeasurement()
        {
            this.SqaleIndex = 0;
            this.Lines = 0;
            this.MeasurementDate = DateTime.MinValue;
        }

        public CodeQualityMeasurement(DateValue sqaleIndexMeasurement, DateValue linesMeasurement)
        {
            this.SqaleIndex = sqaleIndexMeasurement.Value;
            this.Lines = linesMeasurement.Value;
            this.MeasurementDate = sqaleIndexMeasurement.Date;
        }

        public bool HasNoValue()
        {
            return Double.Equals(this.SqaleIndex, 0d) && Double.Equals(this.Lines, 0d) && this.MeasurementDate == DateTime.MinValue;
        }
    }
}
