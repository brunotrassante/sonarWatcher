using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using SonarWatcher.Entity;
using System.Drawing;
using System.Windows.Forms;

namespace SonarWatcher
{
    public class EmailService
    {
        private Dictionary<string, LinkedResource> resourcesDictionary;
        MailMessage mail;
        EmailInfo emailInfo;

        public EmailService(EmailInfo emailInfo)
        {
            this.resourcesDictionary = new Dictionary<string, LinkedResource>();
            this.mail = new MailMessage();
            this.emailInfo = emailInfo;
        }

        public void SendEmail()
        {
            ConfigureMailGeneralSettings();
            AddLinkedResourcesAttachments();
            AddDestinationEmails();

            string html = ReplaceEmailTagsWithInfo();
            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
            AddLinkedResource(avHtml);
            mail.AlternateViews.Add(avHtml);

            SmtpClient client = new SmtpClient("cwirelay", 25);
            client.Send(mail);
        }

        private void ConfigureMailGeneralSettings()
        {
            mail.From = new MailAddress("nucleotecnologia@cwi.com.br", "Sonar CWI");
            mail.SubjectEncoding = Encoding.GetEncoding("UTF-8");
            mail.BodyEncoding = Encoding.GetEncoding("UTF-8");
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.Normal;
            mail.Subject = string.Format("Sonar CWI | {0} | {1} ", emailInfo.Project, DateTime.Now.ToShortDateString());
        }

        private void AddLinkedResourcesAttachments()
        {
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.ComplexityChartPath), emailInfo.ComplexityChartPath);
            AddAttachment(emailInfo.ComplexityChartPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.SeverityChartPath), emailInfo.SeverityChartPath);
            AddAttachment(emailInfo.SeverityChartPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.TypeChartPath), emailInfo.TypeChartPath);
            AddAttachment(emailInfo.TypeChartPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.CompanyLogoPath), emailInfo.CompanyLogoPath);
            AddAttachment(emailInfo.CompanyLogoPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.SonarLogoPath), emailInfo.SonarLogoPath);
            AddAttachment(emailInfo.SonarLogoPath, mail);
        }

        private string ReplaceEmailTagsWithInfo()
        {
            var htmlTemplatePath = ConfigurationManager.AppSettings["reportEmailTemplatePath"];
            var sonarLinkToProject = string.Format("{0}/dashboard?id={1}", ConfigurationManager.AppSettings.Get("sonarURL"), emailInfo.ProjectKey);
            string codeQualityDateTag = "{{codeQualityDate{0}}}",
                   codeQualityArrowTag = "{{codeQualityArrow{0}}}";

            var html = File.ReadAllText(htmlTemplatePath);
            html = html.Replace("{{sonarLogo}}", this.resourcesDictionary[nameof(emailInfo.SonarLogoPath)].ContentId);
            html = html.Replace("{{companyLogo}}", this.resourcesDictionary[nameof(emailInfo.CompanyLogoPath)].ContentId);
            html = html.Replace("{{project}}", emailInfo.Project);
            html = html.Replace("{{sonarLink}}", sonarLinkToProject);
            html = html.Replace("{{date}}", DateTime.Now.ToShortDateString());
            html = html.Replace("{{manager}}", emailInfo.Manager);
            html = html.Replace("{{leader}}", emailInfo.Leader);
            html = html.Replace("{{codeHealthPercentage}}", String.Format("{0:0.0}", emailInfo.CodeHealthPercentage));
            html = html.Replace("{{codeQualityColor}}", Rating.GetHexaBasedOnRating(emailInfo.CodeHealthPercentage));
            html = html.Replace("{{typeChartPath}}", this.resourcesDictionary[nameof(emailInfo.TypeChartPath)].ContentId);
            html = html.Replace("{{severityChartPath}}", this.resourcesDictionary[nameof(emailInfo.SeverityChartPath)].ContentId);
            html = html.Replace("{{complexityChartPath}}", this.resourcesDictionary[nameof(emailInfo.ComplexityChartPath)].ContentId);
            html = html.Replace("{{reabilityValue}}", emailInfo.ProjectRating.Reliability.ToClassification());
            html = html.Replace("{{reabilityColor}}", emailInfo.ProjectRating.Reliability.ToColorHexa());
            html = html.Replace("{{securityValue}}", emailInfo.ProjectRating.Security.ToClassification());
            html = html.Replace("{{securityColor}}", emailInfo.ProjectRating.Security.ToColorHexa());
            html = html.Replace("{{manutenibilityValue}}", emailInfo.ProjectRating.Maintainability.ToClassification());
            html = html.Replace("{{manutenibilityColor}}", emailInfo.ProjectRating.Maintainability.ToColorHexa());

            var lastMeasure = emailInfo.CalculatedCodeQuality.First();
            var preLastMeasure = emailInfo.CalculatedCodeQuality.ElementAtOrDefault(1);

            bool noChangeOnLastWeeks = (!lastMeasure.LineAmountHasChanged && !preLastMeasure.LineAmountHasChanged && !preLastMeasure.HasNoValue())
                || lastMeasure.MeasurementDate < DateTime.Now.AddDays(-14);

            bool badCode = lastMeasure.CodeQualityValue < 4 && preLastMeasure.CodeQualityValue < 4 && !preLastMeasure.HasNoValue();

            if (badCode || noChangeOnLastWeeks)
            {

            }

            html = html.Replace("{{projectInactivityWarning}}", noChangeOnLastWeeks ? "display: normal;" : "display: none;");
            html = html.Replace("{{projectCodeQualityWarning}}", badCode ? "display: normal;" : "display: none;");

            for (int i = 0; i < emailInfo.CalculatedCodeQuality.Count; i++)
            {
                var actualMeasure = emailInfo.CalculatedCodeQuality.ElementAt(i);
                var formattedDate = actualMeasure.HasNoValue() ? "-" : actualMeasure.MeasurementDate.ToString("dd/MM/yyyy");
                var arrowContentId = this.SetArrowBasedOnRating(actualMeasure, mail);

                html = html.Replace(String.Format(codeQualityDateTag, i), formattedDate);
                html = html.Replace(String.Format(codeQualityArrowTag, i), arrowContentId);
            }

            return html;
        }

        private void AddDestinationEmails()
        {
            string defaultReceiver = ConfigurationManager.AppSettings["defaultReceiver"];
            if (emailInfo.DestinataryMails.Count == 0)
                emailInfo.AddDestinataryMail(defaultReceiver);

            foreach (var email in emailInfo.DestinataryMails)
            {
                mail.To.Add(email);
            }
        }

        private void AddAttachment(string filepath, MailMessage mail)
        {
            Attachment att = new Attachment(filepath);
            att.ContentDisposition.Inline = true;
            mail.Attachments.Add(att);
        }

        private void AddLinkedResource(AlternateView avHtml)
        {
            foreach (var resource in this.resourcesDictionary)
            {
                avHtml.LinkedResources.Add(resource.Value);
            }
        }

        private void CreateLinkedResourceAndAddToDictionary(string keyName, string resourcePath)
        {
            LinkedResource inline = new LinkedResource(resourcePath, MediaTypeNames.Image.Jpeg);
            inline.ContentId = Guid.NewGuid().ToString();
            this.resourcesDictionary.Add(keyName, inline);
        }

        private string SetArrowBasedOnRating(CodeQualityMeasurementDto rating, MailMessage mailMessage)
        {
            string filePath;
            string key;
            string basePath = Directory.GetParent(Path.GetDirectoryName(Application.ExecutablePath)).Parent.FullName + "\\Images\\";
            var value = rating.CodeQualityValue;

            if (rating.ShowDashInsteadOfValue)
            {
                key = "dash.png";
                filePath = basePath + key;
            }
            else if (value <= 2)
            {
                key = "arrDown.png";
                filePath = basePath + key;
            }
            else if (value <= 4)
            {
                key = "arrDiagDown.png";
                filePath = basePath + key;
            }
            else if (value <= 6)
            {
                key = "arrRight.png";
                filePath = basePath + key;
            }
            else if (value <= 8)
            {
                key = "arrDiagUp.png";
                filePath = basePath + key;
            }
            else
            {
                key = "arrUp.png";
                filePath = basePath + key;
            }


            if (!resourcesDictionary.ContainsKey(key))
            {
                CreateLinkedResourceAndAddToDictionary(key, filePath);
                AddAttachment(filePath, mail);
            }

            return resourcesDictionary[key].ContentId;
        }
    }
}