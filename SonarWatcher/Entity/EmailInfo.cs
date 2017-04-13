using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarWatcher.Entity
{
    public class EmailInfo
    {
        public string Project { get; private set; }
        public string ProjectKey { get; private set; }
        public string Manager { get; private set; }
        public string Leader { get; private set; }
        public string ComplexityChartPath { get; set; }
        public string TypeChartPath { get; set; }
        public string SeverityChartPath { get; set; }
        public string MailTo { get; private set; }
        public ProjectRating ProjectRating { get; private set; }


        public EmailInfo(string project, string projectKey, string manager, string leader, string mailTo, string complexityChartPath, string typeChartPath, string severityChartPath, ProjectRating projectRating)
        {
            this.Project = project;
            this.ProjectKey = projectKey;
            this.Manager = manager;
            this.Leader = leader;
            this.MailTo = mailTo;
            string defaultErrorImagePath = ConfigurationManager.AppSettings["defaultErrorImage"];
            this.ComplexityChartPath = complexityChartPath;
            this.TypeChartPath = typeChartPath;
            this.SeverityChartPath = severityChartPath;
            this.ProjectRating = projectRating;
        }
    }
}
