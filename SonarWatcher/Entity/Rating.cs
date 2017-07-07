using System;
using System.Drawing;

namespace SonarWatcher.Entity
{
    public class Rating
    {
        const string Excelent = "#2ECC40";
        const string Good = "#01FF70";
        const string Medium = "#FF851B";
        const string Bad = "#EC644B";
        const string Awful = "#D91E18";

        private ushort Value { get; set; }

        public Rating(ushort value)
        {
            this.Value = value;
        }

        public ushort ToUshort()
        {
            return this.Value;
        }

        public Color ToColor()
        {
            switch (this.Value)
            {
                case 1:
                    return ColorTranslator.FromHtml(Excelent);
                case 2:
                    return ColorTranslator.FromHtml(Good);
                case 3:
                    return ColorTranslator.FromHtml(Medium);
                case 4:
                    return ColorTranslator.FromHtml(Bad);
                case 5:
                    return ColorTranslator.FromHtml(Awful);
                default:
                    throw new ArgumentOutOfRangeException("Acceptable rating range: 1-5");
            }
        }

        public string ToClassification()
        {
            switch (this.Value)
            {
                case 1:
                    return "A";
                case 2:
                    return "B";
                case 3:
                    return "C";
                case 4:
                    return "D";
                case 5:
                    return "E";
                default:
                    throw new ArgumentOutOfRangeException("Acceptable rating range: 1-5");
            }
        }

        public string ToColorHexa()
        {
            switch (this.Value)
            {
                case 1:
                    return Excelent;
                case 2:
                    return Good;
                case 3:    
                    return Medium;
                case 4:    
                    return Bad;
                case 5:
                    return Awful;
                default:
                    throw new ArgumentOutOfRangeException("Acceptable rating range: 1-5");
            }
        }

        public static string GetHexaBasedOnRating(double rating, EmailInfo email)
        {
            if (rating < 2) return Awful;
            if (rating < 4) return Bad;
            if (rating < 6) return Medium;
            if (rating < 8) return Good;
            if (rating <= 11) return Excelent;

            throw new ArgumentOutOfRangeException("Acceptable rating range: 0 to 11");
        }
    }
}
