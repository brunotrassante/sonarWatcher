using System;
using System.Drawing;

namespace SonarWatcher.Entity
{
    public class Rating
    {
        const string SuperGood = "#2ECC40";
        const string Good = "#01FF70";
        const string Meddium = "#FF851B";
        const string Bad = "#EC644B";
        const string SuperBad = "#D91E18";

        private ushort value { get; set; }

        public Rating(ushort value)
        {
            this.value = value;
        }

        public ushort ToUshort()
        {
            return this.value;
        }

        public Color ToColor()
        {
            switch (this.value)
            {
                case 1:
                    return ColorTranslator.FromHtml(SuperGood);
                case 2:
                    return ColorTranslator.FromHtml(Good);
                case 3:
                    return ColorTranslator.FromHtml(Meddium);
                case 4:
                    return ColorTranslator.FromHtml(Bad);
                case 5:
                    return ColorTranslator.FromHtml(SuperBad);
                default:
                    throw new ArgumentOutOfRangeException("Acceptable rating rage: 1-5");
            }
        }

        public string ToClassification()
        {
            switch (this.value)
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
                    throw new ArgumentOutOfRangeException("Acceptable rating rage: 1-5");
            }
        }

        public string ToColorHexa()
        {
            switch (this.value)
            {
                case 1:
                    return SuperGood;
                case 2:
                    return Good;
                case 3:    
                    return Meddium;
                case 4:    
                    return Bad;
                case 5:
                    return SuperBad;
                default:
                    throw new ArgumentOutOfRangeException("Acceptable rating rage: 1-5");
            }
        }

    }
}
