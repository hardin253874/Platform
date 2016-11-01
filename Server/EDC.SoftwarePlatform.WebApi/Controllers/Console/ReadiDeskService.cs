// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Configuration;
using System.Net;
using System.IO;
using System.Text;
using Jil;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    static class ReadiDeskService
    {
        /// <summary>
        /// Create a ReadiDesk Incident record, but only if the ReadiDesk application is installed.
        /// </summary>
        public static string CreateTicket(ReadiDeskTicket ticketData)
        {
            IEntity ticketEntity = EDC.ReadiNow.Model.Entity.Create("readidesk:incident");
            ticketEntity.SetField("core:name", ticketData.Summary);
            ticketEntity.SetField("readidesk:description", ticketData.Description);
            ticketEntity.Save();

            foreach (var at in ticketData.Attachments)
                CreateAttachment(ticketEntity, at.Name, at.Ext, at.Data);

            Info("TDB: Created ticket in ReadiDesk: {0} {1}, for user {2} and tenant {3}",
                ticketEntity.Id, ticketData.Summary, ticketData.UserName, ticketData.TenantName);

            return ticketEntity.Id.ToString();
        }

        /// <summary>
        /// Create a ReadiDesk Incident record in the configured remote ReadiDesk system.
        /// </summary>
        public static string RemoteCreateTicket(ReadiDeskTicket ticket)
        {
            //todo - create a ticket in readidesk ... getting connection details from config

            var cookies = Login();
            var request = BuildRequest(@"data/v1/console/createTicket", WebRequestMethods.Http.Post, cookies);

            if (cookies == null || request == null)
                return null;

            PopulateBodyString(request, JSON.Serialize(ticket));

            var response = request.GetResponse();
            string result;
            using (var s = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream()))
                result = s.ReadToEnd();

            Info("Ticket created in remote ReadiDesk: {0} => {1}", ticket.Summary, result);
            return result;
        }

        private static Document CreateAttachment(IEntity incidentEntity, string fileName, string docExt, string content)
        {
            //todo - work out how to get the right doc type, although I can't see what to use for images
            //todo - at the moment this assumes the content is base64... does that make sense??

            var docType = ReadiNow.Model.Entity.Get<DocumentType>(new EntityRef("textDocumentDocumentType"));
            string hash;
            using (var stream = new MemoryStream(Convert.FromBase64String(content)))
            {
                hash = FileRepositoryHelper.AddTemporaryFile(stream);
            }
            
            var doc = ReadiNow.Model.Entity.Create<Document>();
            doc.FileDataHash = hash;
            doc.Name = fileName;
            doc.FileExtension = docExt;
            doc.DocumentFileType = docType;
            doc.SetRelationships("readidesk:attachmentIncident", new EntityRelationshipCollection<IEntity> { incidentEntity }, Direction.Reverse);
            doc.Save();

            return doc;
        }

        private static CookieCollection Login()
        {
            var request = BuildRequest(@"data/v1/login/spsignin", WebRequestMethods.Http.Post);

            if (request == null)
                return null;

            // todo - get details from config
            var username = ReadiDeskConfig("username", "");
            var password = ReadiDeskConfig("password", "");
            var tenant = ReadiDeskConfig("tenant", "");
            // assuming quotes are not legal in these....?!
            PopulateBodyString(request, string.Format(@"{{""username"":""{0}"", ""password"":""{1}"", ""tenant"":""{2}""}}",
                username, password, tenant));

            var response = (HttpWebResponse)request.GetResponse();
            var xsrfToken = (string)null;

            // Note - not using the xsrfToken yet, but this is how to get it if needed
            foreach (Cookie cookie in response.Cookies)
            {
                //Info("cookie: {0} = {1}", cookie.Name, cookie.Value);
                if (cookie.Name == "XSRF-TOKEN")
                {
                    xsrfToken = cookie.Value;
                    break;
                }
            }
            //Info("xsrf-token: {0}", xsrfToken);

            return response.Cookies;
        }

        private static HttpWebRequest BuildRequest(string apiPath, string method, CookieCollection cookies = null)
        {
            var readiDeskHost = ReadiDeskConfig("hostName", null);
            if (string.IsNullOrEmpty(readiDeskHost))
                return null;

            if (readiDeskHost.ToLower() == "localhost")
                readiDeskHost = Dns.GetHostEntry("localhost").HostName;

            var baseUrl = string.Format("https://{0}", readiDeskHost);
            var requestUrl = string.Concat(baseUrl, "/SpApi/", apiPath);

            var request = WebRequest.CreateHttp(requestUrl);

            request.CookieContainer = new CookieContainer();

            if (cookies != null)
            {
                if (cookies[".ASPXAUTH"] == null || cookies["XSRF-TOKEN"] == null)
                    throw new InvalidOperationException("missing expected cookies");

                var uri = new Uri(baseUrl);
                request.CookieContainer.Add(uri, cookies[".ASPXAUTH"]);
                request.CookieContainer.Add(uri, cookies["XSRF-TOKEN"]);
                request.Headers.Add("X-XSRF-TOKEN", cookies["XSRF-TOKEN"].Value);
            }

            request.Method = method;
            request.Accept = "application/json";

            return request;
        }

        private static void PopulateBodyString(HttpWebRequest request, string data)
        {
            var byteArray = Encoding.UTF8.GetBytes(data);
            request.ContentType = @"application/json; charset=utf-8";
            request.ContentLength = byteArray.Length;
            using (var dataStream = request.GetRequestStream())
                dataStream.Write(byteArray, 0, byteArray.Length);
        }

        private static string ReadiDeskConfig(string key, string defaultValue = "")
        {
            var config = EDC.ReadiNow.Configuration.ConfigurationSettings.GetConfigurationSettings();
            var section = config != null ? (AppSettingsSection)config.GetSection("readiDesk") : null;
            var setting = section != null ? section.Settings[key] : null;
            return setting != null ? setting.Value : defaultValue;
        }

        private static void Info(string fmt, params object[] args)
        {
            if (args.Length == 0) ReadiNow.Diagnostics.EventLog.Application.WriteInformation(fmt);
            else ReadiNow.Diagnostics.EventLog.Application.WriteInformation(fmt, args);
        }
    }
}
