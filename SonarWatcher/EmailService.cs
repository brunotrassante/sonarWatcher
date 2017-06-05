using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using SonarWatcher.Entity;

namespace SonarWatcher
{
    public class EmailService
    {
        private readonly Dictionary<string, LinkedResource> resourcesDictionary;
        private readonly MailMessage mail;
        private readonly EmailInfo emailInfo;

        public EmailService(EmailInfo emailInfo)
        {
            this.resourcesDictionary = new Dictionary<string, LinkedResource>();
            this.mail = new MailMessage();
            this.emailInfo = emailInfo;
        }

        public void SendReportEmail()
        {
            AddLinkedResourcesAttachments();
            string html = File.ReadAllText(ConfigurationManager.AppSettings.Get("reportEmailTemplatePath"));
            html = ReplaceAllEmailTagsWithInfo(html);

            CreateAndSendMailForTemplate(html);
        }

        private void CreateAndSendMailForTemplate(string html)
        {
            ConfigureMailGeneralSettings();
            AddDestinationEmails();

            AlternateView avHtml = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
            AddLinkedResource(avHtml);
            mail.AlternateViews.Add(avHtml);

            SmtpClient client = new SmtpClient("cwirelay", 25);
            client.Send(mail);
        }

        public void SendNotRegisteredEmail()
        {
            AddBasicLogoLinkedResourcesAttachments();
            AddDefaultDestinationEmail();

            string html = File.ReadAllText(ConfigurationManager.AppSettings.Get("noRegistryEmailTemplatePath"));
            html = ReplaceBasicEmailTags(html);

            CreateAndSendMailForTemplate(html);
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
            AddChartsLinkedResourcesAttachments();
            AddBasicLogoLinkedResourcesAttachments();
        }

        private void AddBasicLogoLinkedResourcesAttachments()
        {
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.CompanyLogoPath), emailInfo.CompanyLogoPath);
            AddAttachment(emailInfo.CompanyLogoPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.SonarLogoPath), emailInfo.SonarLogoPath);
            AddAttachment(emailInfo.SonarLogoPath, mail);
        }

        private void AddChartsLinkedResourcesAttachments()
        {
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.ComplexityChartPath), emailInfo.ComplexityChartPath);
            AddAttachment(emailInfo.ComplexityChartPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.SeverityChartPath), emailInfo.SeverityChartPath);
            AddAttachment(emailInfo.SeverityChartPath, mail);
            CreateLinkedResourceAndAddToDictionary(nameof(emailInfo.TypeChartPath), emailInfo.TypeChartPath);
            AddAttachment(emailInfo.TypeChartPath, mail);
        }

        private string ReplaceAllEmailTagsWithInfo(string html)
        {
            html = ReplaceBasicEmailTags(html);

            var sonarLinkToProject = string.Format("{0}/dashboard?id={1}", ConfigurationManager.AppSettings.Get("sonarURL"), emailInfo.ProjectKey);

            html = html.Replace("{{sonarLink}}", sonarLinkToProject);
            html = html.Replace("{{typeChartPath}}", this.resourcesDictionary[nameof(emailInfo.TypeChartPath)].ContentId);
            html = html.Replace("{{severityChartPath}}", this.resourcesDictionary[nameof(emailInfo.SeverityChartPath)].ContentId);
            html = html.Replace("{{complexityChartPath}}", this.resourcesDictionary[nameof(emailInfo.ComplexityChartPath)].ContentId);
            html = html.Replace("{{reabilityValue}}", emailInfo.ProjectRating.Reliability.ToClassification());
            html = html.Replace("{{reabilityColor}}", emailInfo.ProjectRating.Reliability.ToColorHexa());
            html = html.Replace("{{securityValue}}", emailInfo.ProjectRating.Security.ToClassification());
            html = html.Replace("{{securityColor}}", emailInfo.ProjectRating.Security.ToColorHexa());
            html = html.Replace("{{manutenibilityValue}}", emailInfo.ProjectRating.Maintainability.ToClassification());
            html = html.Replace("{{manutenibilityColor}}", emailInfo.ProjectRating.Maintainability.ToColorHexa());
            return html;
        }

        private string ReplaceBasicEmailTags(string html)
        {
            html = html.Replace("{{sonarLogo}}", this.resourcesDictionary[nameof(emailInfo.SonarLogoPath)].ContentId);
            html = html.Replace("{{companyLogo}}", this.resourcesDictionary[nameof(emailInfo.CompanyLogoPath)].ContentId);
            html = html.Replace("{{project}}", emailInfo.Project);
            html = html.Replace("{{date}}", DateTime.Now.ToShortDateString());
            html = html.Replace("{{manager}}", emailInfo.Manager);
            html = html.Replace("{{leader}}", emailInfo.Leader);
            return html;
        }

        private void AddDestinationEmails()
        {
            if (emailInfo.DestinataryMails.Count == 0)
                AddDefaultDestinationEmail();
            else
            {
                foreach (var email in emailInfo.DestinataryMails)
                {
                    mail.To.Add(email);
                }
            }
        }

        private void AddDefaultDestinationEmail()
        {
            string defaultReciver = ConfigurationManager.AppSettings["defaultReceiver"];
            mail.To.Add(defaultReciver);
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
    }
}