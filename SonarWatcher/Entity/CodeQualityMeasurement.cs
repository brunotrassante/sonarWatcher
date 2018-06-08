using System;

namespace SonarWatcher.Entity
{
    public class CodeQualityMeasurement
    {
        public double CodeQualityValue { get; set; }
        public DateTime MeasurementDate { get; set; }
        public bool LineAmountHasChanged { get; set; }
        public bool LostLines { get; set; }
        public bool ShowDashInsteadOfValue { get; set; }

        public CodeQualityMeasurement(double sqaleIndex, DateTime measurementDate, bool lineAmountHasChanged)
        {
            this.CodeQualityValue = sqaleIndex;
            this.MeasurementDate = measurementDate;
            this.LineAmountHasChanged = lineAmountHasChanged;
            this.ShowDashInsteadOfValue = !lineAmountHasChanged;
        }

        public CodeQualityMeasurement(DateTime measurementDate)
        {
            this.CodeQualityValue = 0;
            this.MeasurementDate = measurementDate;
            this.LineAmountHasChanged = false;
            this.ShowDashInsteadOfValue = true;
        }

        public CodeQualityMeasurement()
        {
            this.CodeQualityValue = 0;
            this.MeasurementDate = DateTime.MinValue;
            this.LineAmountHasChanged = true;
            this.ShowDashInsteadOfValue = true;
        }

        public bool HasNoValue()
        {
            return Double.Equals(this.CodeQualityValue, 0d) && this.MeasurementDate == DateTime.MinValue;
        }
    }
}
