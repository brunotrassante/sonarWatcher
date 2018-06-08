using SonarWatcher.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonarWatcher
{
    public class CodeQualityService : IMeasurementService<List<CodeQualityMeasurement>>
    {
        private readonly List<CodeQualityMeasurementDto> _joinedMeansureament;

        public CodeQualityService(List<MetricSequence> complexityMetrics)
        {
            this._joinedMeansureament = this.JoinMeasurements(complexityMetrics);
        }

        public List<CodeQualityMeasurement> Calculate()
        {
            CodeQualityMeasurementDto lastMeasurement,
                        oneBeforeTheLastMeasurement,
                        twoBeforeTheLastMeasurement = null,
                        threeBeforeTheLastMeasurement = null,
                        fourBeforeTheLastMeasurement = null;


            lastMeasurement = _joinedMeansureament.Last();

            oneBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(_joinedMeansureament, lastMeasurement.MeasurementDate, 1);

            if (!oneBeforeTheLastMeasurement.HasNoValue())
            {
                twoBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(_joinedMeansureament, oneBeforeTheLastMeasurement.MeasurementDate, 2);

                if (!twoBeforeTheLastMeasurement.HasNoValue())
                {
                    threeBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(_joinedMeansureament, twoBeforeTheLastMeasurement.MeasurementDate, 3);

                    if (!threeBeforeTheLastMeasurement.HasNoValue())
                    {
                        fourBeforeTheLastMeasurement = this.GetLatestMeasurementBasedOnDate(_joinedMeansureament, threeBeforeTheLastMeasurement.MeasurementDate, 4);
                    }
                }
            }

            var lastMeasureCalculatedValue = this.CompareCodeQuality(lastMeasurement, oneBeforeTheLastMeasurement);
            var oneBeforeTheLastMeasureCalculatedValue = this.CompareCodeQuality(oneBeforeTheLastMeasurement, twoBeforeTheLastMeasurement);
            var twoBeforeTheLastMeasureCalculatedValue = this.CompareCodeQuality(twoBeforeTheLastMeasurement, threeBeforeTheLastMeasurement);
            var threeBeforeTheLastMeasureCalculatedValue = this.CompareCodeQuality(threeBeforeTheLastMeasurement, fourBeforeTheLastMeasurement);

            return new List<CodeQualityMeasurement>() {
                lastMeasureCalculatedValue,
                oneBeforeTheLastMeasureCalculatedValue,
                twoBeforeTheLastMeasureCalculatedValue,
                threeBeforeTheLastMeasureCalculatedValue
            };
        }

        private List<CodeQualityMeasurementDto> JoinMeasurements(List<MetricSequence> complexityMetrics)
        {
            var joinedMeasurements = new List<CodeQualityMeasurementDto>();
            var linesMeasurements = complexityMetrics.First().GetMeasures();
            var sqaleIndexMeasurements = complexityMetrics.Last().GetMeasures();

            foreach (var measure in sqaleIndexMeasurements)
            {
                var equivalentLinesMeasurement = linesMeasurements.First(m => m.Date == measure.Date);
                joinedMeasurements.Add(new CodeQualityMeasurementDto(measure, equivalentLinesMeasurement));
            }

            return joinedMeasurements;
        }

        private CodeQualityMeasurement CompareCodeQuality(CodeQualityMeasurementDto referenceMeasurement, CodeQualityMeasurementDto previousMeasurement)
        {
            if (referenceMeasurement == null || previousMeasurement == null || referenceMeasurement.HasNoValue() || previousMeasurement.HasNoValue())
            {
                if (referenceMeasurement != null && !referenceMeasurement.HasNoValue())
                {
                    return new CodeQualityMeasurement(0, referenceMeasurement.MeasurementDate, false);
                }

                return new CodeQualityMeasurement();
            }

            var lineDiff = referenceMeasurement.Lines - previousMeasurement.Lines;
            var difference = referenceMeasurement.SqaleIndex - previousMeasurement.SqaleIndex;

            if (lineDiff < 0)
            {
                var dto = new CodeQualityMeasurement(referenceMeasurement.MeasurementDate);

                return dto;
            }
            else if (difference > 0)
            {
                // 1/10 of the difference between measures taken in days and then transformed to minutes
                var limitTechnicalDebt = ((referenceMeasurement.MeasurementDate - previousMeasurement.MeasurementDate).TotalDays) * 0.1 * 60;

                var calc = (100 - ((difference * 100) / limitTechnicalDebt)) / 10;

                calc = calc < 0 ? 0 : calc;

                var hasLineDifference = referenceMeasurement.Lines != previousMeasurement.Lines;

                return new CodeQualityMeasurement(calc, referenceMeasurement.MeasurementDate, hasLineDifference);
            }
            else if (difference == 0)
            {
                var hasLineDifference = referenceMeasurement.Lines != previousMeasurement.Lines;

                return new CodeQualityMeasurement(10, referenceMeasurement.MeasurementDate, hasLineDifference);
            }
            else
            {
                var hasLineDifference = referenceMeasurement.Lines != previousMeasurement.Lines;

                return new CodeQualityMeasurement(11, referenceMeasurement.MeasurementDate, hasLineDifference);
            }
        }

        private CodeQualityMeasurementDto GetLatestMeasurementBasedOnDate(List<CodeQualityMeasurementDto> measures, DateTime date, int measuresTaken)
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

            return latestMeasure == null ? new CodeQualityMeasurementDto() : latestMeasure;
        }
    }
}
