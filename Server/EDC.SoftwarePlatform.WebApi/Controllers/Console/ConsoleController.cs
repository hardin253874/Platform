// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using ReadiNow.Reporting;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EntityModel = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Model.Client;
using ReadiNow.Reporting.Request;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    [DataContract]
    public class Attachment
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "ext")]
        public string Ext { get; set; }

        [DataMember(Name = "data")]
        public string Data { get; set; }
    }

    [DataContract]
    public class FeedbackData
    {
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "comments")]
        public string Comments { get; set; }

        [DataMember(Name = "messages")]
        public List<string> Messsages { get; set; }

        [DataMember(Name = "attachments")]
        public List<Attachment> Attachments { get; set; }
    }

    [DataContract]
    public class ReadiDeskTicket
    {
        [DataMember(Name = "tenantName")]
        public string TenantName { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "summary")]
        public string Summary { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "attachments")]
        public List<Attachment> Attachments { get; set; }
    }

    /// <summary>
    ///     Controller for performing generic console operations.
    /// </summary>
    [RoutePrefix("data/v1/console")]
    public class ConsoleController : ApiController
    {
        /// <summary>
        /// Create a new <see cref="ConsoleTreeRepository"/>.
        /// </summary>
        public ConsoleController()
        {
            // Ideally, a IConsoleTreeRepository will be passed in via constructor injection.
            ConsoleTreeRepository = Factory.Current.Resolve<IConsoleTreeRepository>();
        }

        /// <summary>
        /// A <see cref="ConsoleTreeRepository"/>.
        /// </summary>
        public IConsoleTreeRepository ConsoleTreeRepository { get; private set; }

        /// <summary>
        ///     Get the session info.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("sessioninfo")]
        public HttpResponseMessage<SessionInfoResult> SessionInfo_Get()
        {
            using (Profiler.Measure("ConsoleController.SessionInfo_Get"))
            {
                var sessionInfoResult = new SessionInfoResult
                {
                    PlatformVersion = SystemInfo.PlatformVersion,
                    BranchName = SystemInfo.BranchName,
                    BookmarkScheme = DatabaseInfoHelper.GetBookmarkScheme()
                };
                return new HttpResponseMessage<SessionInfoResult>(sessionInfoResult);
            }
        }


#if (DEBUG)
        /// <summary>
        ///     Clears the cache. For use by WebApi unit tests.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("clearcache")]
        public HttpResponseMessage ClearCache_Get()
        {
            CacheManager.ClearCaches();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        ///     Explicitly pre-warms the current tenant. For use by WebApi unit tests.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("prewarm")]
        public HttpResponseMessage Prewarm_Get()
        {
            BulkPreloader.TenantWarmup();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
#endif

        /// <summary>
        ///     No operation. Do nothing. Used for performance testing and ADC monitoring.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("noop")]
        [AllowAnonymous]
        public HttpResponseMessage Noop_Get()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        /// <summary>
        /// Write a client error to the log.
        /// </summary>
        [Route("reporterror")]
        [HttpGet]
        public HttpResponseMessage ReportConsoleError(string error)
        {
            ReadiNow.Diagnostics.EventLog.Application.WriteWarning("ClientError: {0}", error);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [Route("tree")]
        [HttpGet]
        public HttpResponseMessage<JsonQueryResult> GetNavigationTree()
        {
            EntityPackage context;

            context = new EntityPackage();
            EntityData tree = ConsoleTreeRepository.GetTree();
            context.AddEntityData(tree, "navItems");
            return new HttpResponseMessage<JsonQueryResult>(context.GetQueryResult());
        }

        /// <summary>
        /// Receive feedback data and save to the file system.
        /// </summary>
        [Route("feedback")]
        [HttpPost]
        public HttpResponseMessage<string> Feedback([FromBody] FeedbackData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var idString = Guid.NewGuid().ToString();

            var text = "";
            // Aggregate for some misguided reason throws if the collection is empty
            if (data.Messsages.Count > 0)
                text = data.Messsages.Aggregate((a, p) => a + (string.IsNullOrEmpty(a) ? "" : "\r\n") + p);

            var context = new RequestContextData(EDC.ReadiNow.IO.RequestContext.GetContext());
            if (context == null || context.Identity == null || context.Tenant == null)
                throw new InvalidOperationException("Failed to determine identity");

            string tenantName = context.Tenant.Name;
            string userName = context.Identity.Name;

            var e = ReadiNow.Model.Entity.Get<UserAccount>(context.Identity.Id);
            if (e == null)
                throw new InvalidOperationException("Failed to determine account holder");

            string personName = e.AccountHolder != null ? e.AccountHolder.Name : context.Identity.Name;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            path = Path.Combine(path, @"ReadiNow\ClientLogs");
            //todo- factor out that clean routine... just can't remember a quick way in C#....
            path = Path.Combine(path, Regex.Replace(tenantName, @"[^A-Za-z0-9_]+", "-"));
            path = Path.Combine(path, Regex.Replace(userName, @"[^A-Za-z0-9_]+", "-"));
            path = Path.Combine(path, idString);

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "attachments"));

            using (var fs = new FileStream(Path.Combine(path, "comments.txt"), FileMode.Create))
            using (var w = new StreamWriter(fs))
            {
                w.Write(data.Comments);
                w.Close();
            }

            using (var fs = new FileStream(Path.Combine(path, "browser.log"), FileMode.Create))
            using (var w = new StreamWriter(fs))
            {
                w.Write(text);
                w.Close();
            }

            if (data.Attachments != null) // am so over null checks.... foreach throws
            {
                foreach (var a in data.Attachments)
                {
                    // strip off things like data:text/plain;base64,
                    if (Regex.IsMatch(a.Data, "^data:.*;base64,"))
                        a.Data = a.Data.Substring(a.Data.IndexOf(',') + 1);

                    using (var fs = new FileStream(Path.Combine(path, "attachments", a.Name + "." + a.Ext), FileMode.Create))
                    using (var bw = new BinaryWriter(fs))
                    {
                        var content = Convert.FromBase64String(a.Data);
                        bw.Write(content);
                        bw.Close();
                    }
                }
            }

            SaveRecentServerLogs(path);

            ReadiNow.Diagnostics.EventLog.Application.WriteInformation("Client feedback saved to {0}", path);

            // if configured to create a ticket in ReadiDesk then do so

            var description = "";

            description += "email: " + data.Email + "\n";
            description += "phone: " + data.Phone + "\n";
            description += "tenant: " + tenantName + "\n";
            description += "account: " + userName + "\n";
            description += "user: " + personName + "\n";
            description += "front end server: " + Dns.GetHostEntry("localhost").HostName + "\n";
            description += "platform version: " + SystemInfo.PlatformVersion + "\n";
            description += "branch name: " + SystemInfo.BranchName + "\n";

            var apps = ReadiNow.Model.Entity.GetInstancesOfType<Solution>();
            var appList = apps.Aggregate("", (a, p) => a + (string.IsNullOrEmpty(a) ? "" : ", ") + 
                p.Name + " (" + p.SolutionVersionString + ")");
            description += "apps: " + appList + "\n";

            description += "\ndetails: " + data.Comments + "\n\n";

            var ticket = new ReadiDeskTicket
            {
                TenantName = tenantName,
                UserName = userName,
                Summary = data.Comments.Split('\n').FirstOrDefault(),
                Description = description,
                Attachments = new List<Attachment>() { 
                    new Attachment {Name = "consolelog", Ext = "txt", Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(text))}
                }
            };

            ticket.Summary = GetSanitizedNameFieldValue(ticket.Summary);    // ticket summary is saved as the name of the ticket
            if ( data?.Attachments != null )
            {
                ticket.Attachments = ticket.Attachments.Concat( data.Attachments ).ToList( );
            }
            idString = ReadiDeskService.RemoteCreateTicket(ticket) ?? idString;

            return new HttpResponseMessage<string>(idString, HttpStatusCode.OK);
        }

        /// <summary>
        /// Receive feedback data and save to the file system.
        /// </summary>
        [Route("createTicket")]
        [HttpPost]
        public HttpResponseMessage<string> CreateTicket([FromBody] ReadiDeskTicket data)
        {
            if (data == null)
                return new HttpResponseMessage<string>(HttpStatusCode.BadRequest);

            var idString = ReadiDeskService.CreateTicket(data);

            return new HttpResponseMessage<string>(idString, HttpStatusCode.OK);
        }

        private void SaveRecentServerLogs(string path)
        {
            var logsPath = SpecialFolder.GetSpecialFolderPath(SpecialMachineFolders.Log);
            var sortedFiles = new DirectoryInfo(logsPath).GetFiles().OrderBy(f => f.LastWriteTime).Reverse().Take(2).ToList();

            foreach (var f in sortedFiles)
                File.Copy(f.FullName, Path.Combine(path, f.Name));
        }

        /// <summary>
        /// Removes angle brackets from the value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Sanitized value</returns>
        private string GetSanitizedNameFieldValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim();
                StringField nameField = ReadiNow.Model.Entity.Get<StringField>("core:name");
                
                if (nameField != null && nameField.MaxLength.HasValue)
                {
                    int maxLength = nameField.MaxLength.Value;

                    if (value.Length > maxLength)
                    {
                        value = value.Substring(0, maxLength);      // truncate
                    }
                }

                string regex = @"\<|\>";            // remove angle brackets
                Regex rgx = new Regex(regex);   
                value = rgx.Replace(value, " ");    // replace with a space
            }

            return value;
        }

        /// <summary>
        /// Gets the documentation settings.
        /// </summary>
        [Route("getDocoSettings")]
        [HttpGet]
        public IHttpActionResult GetDocoSettings()
        {            
            using (new GlobalAdministratorContext())
            {
                var docoSettingEntity = EntityModel.Get<SystemDocumentationSettings>("core:systemDocumentationSettingsInstance");

                if (docoSettingEntity == null)
                {
                    return Ok();
                }

                var passwordSecureId = docoSettingEntity.DocumentationUserPasswordSecureId;

                var docoSettings = new DocoSettingsResult()
                {
                    DocumentationUserName = docoSettingEntity.DocumentationUserName,
                    DocumentationUserPassword = passwordSecureId != null ? Factory.SecuredData.Read(passwordSecureId.Value) : null,
                    DocumentationUrl = docoSettingEntity.DocumentationUrl,
                    ContactSupportUrl = docoSettingEntity.ContactSupportUrl,
                    ReleaseNotesUrl = docoSettingEntity.ReleaseNotesUrl,
                    NavHeaderDocumentationUrl = docoSettingEntity.NavHeaderDocumentationUrl
                };

                return Ok(docoSettings);
            }
        }
    }
}