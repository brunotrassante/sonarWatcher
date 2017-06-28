using SonarWatcher.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarWatcher
{
    public class CodeQualityMeasurementService
    {
        public List<CodeQualityMeasurement> JoinMeasurements(List<MetricSequence> complexityMetrics)
        {
            var joinedMeasurements = new List<CodeQualityMeasurement>();
            var linesMeasurements = complexityMetrics.First().GetMeasures();
            var sqaleIndexMeasurements = complexityMetrics.Last().GetMeasures();

            foreach (var measure in sqaleIndexMeasurements)
            {
                var equivalentLinesMeasurement = linesMeasurements.First(m => m.Date == measure.Date);
                joinedMeasurements.Add(new CodeQualityMeasurement(measure, equivalentLinesMeasurement));
            }

            return joinedMeasurements;
        }

        public List<CodeQualityMeasurementDto> CalculateCodeQuality(List<CodeQualityMeasurement> metrics)
        {
            CodeQualityMeasurement lastMeasurement,
                        oneBeforeTheLastMeasurement,
                        twoBeforeTheLastMeasurement = null,
                        threeBeforeTheLastMeasurement = null,
                        fourBeforeTheLastMeasurement = null;


            lastMeasurement = metrics.Last();

            oneBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(metrics, lastMeasurement.MeasurementDate, 1);

            if (!oneBeforeTheLastMeasurement.HasNoValue())
            {
                twoBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(metrics, oneBeforeTheLastMeasurement.MeasurementDate, 2);

                if (!twoBeforeTheLastMeasurement.HasNoValue())
                {
                    threeBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(metrics, twoBeforeTheLastMeasurement.MeasurementDate, 3);

                    if (!threeBeforeTheLastMeasurement.HasNoValue())
                    {
                        fourBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(metrics, threeBeforeTheLastMeasurement.MeasurementDate, 4);
                    }
                }
            }

            CodeQualityMeasurementDto lastMeasureCalculatedValue = this.CompareCodeQuality(lastMeasurement, oneBeforeTheLastMeasurement),
                                        oneBeforeTheLastMeasureCalculatedValue = this.CompareCodeQuality(oneBeforeTheLastMeasurement, twoBeforeTheLastMeasurement),
                                        twoBeforeTheLastMeasureCalculatedValue = this.CompareCodeQuality(twoBeforeTheLastMeasurement, threeBeforeTheLastMeasurement),
                                        threeBeforeTheLastMeasureCalculatedValue = this.CompareCodeQuality(threeBeforeTheLastMeasurement, fourBeforeTheLastMeasurement);

            return new List<CodeQualityMeasurementDto>() {
                lastMeasureCalculatedValue,
                oneBeforeTheLastMeasureCalculatedValue,
                twoBeforeTheLastMeasureCalculatedValue,
                threeBeforeTheLastMeasureCalculatedValue
            };
        }

        private CodeQualityMeasurementDto CompareCodeQuality(CodeQualityMeasurement referenceMeasurement, CodeQualityMeasurement previousMeasurement)
        {
            if (referenceMeasurement == null || previousMeasurement == null || referenceMeasurement.HasNoValue() || previousMeasurement.HasNoValue())
            {
                if (referenceMeasurement != null && !referenceMeasurement.HasNoValue())
                {
                    return new CodeQualityMeasurementDto(0, referenceMeasurement.MeasurementDate, false);
                }

                return new CodeQualityMeasurementDto();
            }

            var lineDiff = referenceMeasurement.Lines - previousMeasurement.Lines;
            var difference = referenceMeasurement.SqaleIndex - previousMeasurement.SqaleIndex;

            if (lineDiff < 0)
            {
                var dto = new CodeQualityMeasurementDto(referenceMeasurement.MeasurementDate);

                return dto;
            }
            else if (difference > 0)
            {
                // 1/10 of the difference between measures taken in days and then transformed to minutes
                var limitTechnicalDebt = ((referenceMeasurement.MeasurementDate - previousMeasurement.MeasurementDate).TotalDays) * 0.1 * 60;

                var calc = (100 - ((difference * 100) / limitTechnicalDebt)) / 10;

                calc = calc < 0 ? 0 : calc;

                var hasLineDifference = referenceMeasurement.Lines != previousMeasurement.Lines;

                return new CodeQualityMeasurementDto(calc, referenceMeasurement.MeasurementDate, hasLineDifference);
            }
            else if (difference == 0)
            {
                var hasLineDifference = referenceMeasurement.Lines != previousMeasurement.Lines;

                return new CodeQualityMeasurementDto(10, referenceMeasurement.MeasurementDate, hasLineDifference);
            }
            else
            {
                var hasLineDifference = referenceMeasurement.Lines != previousMeasurement.Lines;

                return new CodeQualityMeasurementDto(11, referenceMeasurement.MeasurementDate, hasLineDifference);
            }
        }

        private CodeQualityMeasurement GetLatestMeasurementBasedOnDate(List<CodeQualityMeasurement> measures, DateTime date, int measuresTaken)
        {
            var dayDifference = 6;
            var latestMeasure = measures.FirstOrDefault(m => m.MeasurementDate == date.AddDays(-dayDifference));

            if (latestMeasure == null && measures.Count > measuresTaken)
            {

                while (dayDifference < 60)
                {
                    dayDifference++;
                    latestMeasure = measures.FirstOrDefault(m => m.MeasurementDate.CompareTo(date.AddDays(-dayDifference)) == 0);
                    if (latestMeasure != null)
                    {
                        return latestMeasure;
                    }
                }
            }

            return latestMeasure == null ? new CodeQualityMeasurement() : latestMeasure;
        }
    }
}
