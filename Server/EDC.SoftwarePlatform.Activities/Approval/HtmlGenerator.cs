// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EDC.SoftwarePlatform.Activities.Approval
{
    /// <summary>
    /// Generate Html for approval to put in emails etc
    /// </summary>
    public class HtmlGenerator
    {
        public string GenerateRejectionPage()
        {
            return @"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <link rel='stylesheet' type='text/css' href='assets/SoftwarePlatform.client.css' /></head>
<body style='font-size:100%'>
The task you are attempting to load has been completed or no longer exists. 
</body>
";
        }

        /// <summary>
        /// Given a token and the options to present, generate an appropriate html page
        /// </summary>
        /// <param name="task">The task to render</param>
        /// <param name="token">The task token</param>
        /// <returns></returns>
        public string GenerateSelectionPage(DisplayFormUserTask task)
        {
            var taskToken = task.DfutLinkToken;
            var options = task.AvailableTransitions.Select(t => t.FromExitPoint.Name);

            var sb = new StringBuilder();

            GeneratePageStart(sb, task, "User action required for:");

            sb.Append("<p><ul>");

            foreach (var option in options)
            {
                sb.Append("<li>");
                sb.Append(GenerateSelectionAnchor(taskToken, option));
                sb.Append("</li>");
            }

            GeneratePageEnd(sb);

            return sb.ToString();
        }


     

        /// <summary>
        /// Generate a page to show on completing a task.
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public string GenerateCompletedPage(DisplayFormUserTask task)
        {
            var exitPoint = task.UserResponse.FromExitPoint;

            var sb = new StringBuilder();
            GeneratePageStart(sb, task, "Completed:");

            var display = exitPoint.ExitPointActionSummary ?? exitPoint.Name;

            sb.Append("<p>");
            sb.Append(display);
            sb.Append("<p>");

            GeneratePageEnd(sb);

            return sb.ToString();

        }

        private static void GeneratePageStart(StringBuilder sb, DisplayFormUserTask task, string intro)
        {
            sb.Append(@"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <link rel='stylesheet' type='text/css' href='assets/SoftwarePlatform.client.css' /></head>
<body style='font-size:100%'>
");
            sb.Append(intro);
            sb.Append("<p>");

            sb.Append(HttpUtility.HtmlEncode(task.Name));

            if (task.RecordToPresent != null)
            {
                sb.Append("<p>");
                sb.Append(task.RecordToPresent.Name);
            }
        }



        private static void GeneratePageEnd(StringBuilder sb)
        {
            sb.Append(@"
</ul>
<p>
<a href='wibble.com'>Open task in browser</a>
</body>
</html>
");
        }

        string GenerateSelectionAnchor(string taskToken, string option)
        {
            var encodedOption = HttpUtility.HtmlEncode(option);
            var uri = GenerateSelectionUrl(taskToken, option);
           return $"<a href='{uri}'>{HttpUtility.HtmlEncode(encodedOption)}</a>";             
        }

        public string GenerateSelectionUrl(string taskToken, string option)
        {
            var escapedOption = Uri.EscapeUriString(option);

            return $"{GenerateSelectionPageUrl(taskToken)}&select={escapedOption}";

        }

        public string GenerateSelectionPageUrl(string taskToken)
        {
            var baseAddress = ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address;
            var escapedTenantName = Uri.EscapeUriString(RequestContext.GetContext().Tenant.Name);
            var escapedTaskToken = Uri.EscapeDataString(taskToken);

            return $"https://{baseAddress}/spapi/approval/{escapedTenantName}/approve?token={escapedTaskToken}";
        }
    }
        
}
