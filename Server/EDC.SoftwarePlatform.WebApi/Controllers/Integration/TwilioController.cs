// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Web.Http;
using EDC.Common;
using EDC.Exceptions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using ReadiNow.Connector;
using ReadiNow.Integration.Sms;
using System.Net;
using ERM = EDC.ReadiNow.Model;
using System.Web;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Integration
{
    /// <summary>
    ///     Controller for APIs used by twilio integrations.
    /// </summary>
    [RoutePrefix("integration")]
    public class TwilioController : ApiController
    {

        ITwilioSmsReceiver _receiver;

        /// <summary>
        /// Constructor
        /// </summary>
        public TwilioController()
        {
            // Resolve services
            _receiver = Factory.Current.Resolve<ITwilioSmsReceiver>();
        }

        /// <summary>
        /// Handle GET verb
        /// Note that the following parameters sent by twilio are currently ignored:
        ///      FromCity	The city of the sender
        ///      FromState The state or province of the sender.
        ///      FromZip The postal code of the called sender.
        ///      FromCountry The country of the called sender.
        ///      ToCity The city of the recipient.
        ///      ToState The state or province of the recipient.
        ///      ToZip The postal code of the recipient.
        ///      ToCountry The country of the recipient.
        /// </summary>
        [Route("twilio/sms/{tenant}/{notifierId}")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(
            [FromUri] string tenant,                // Our stuff
            [FromUri] string notifierId,
            [FromUri] TwilioSms sms)
        {
            // Run service
            using (Profiler.Measure("TwilioController.Get"))
            {
                try
                {
                    long notifierIdNum;

                    try
                    {
                        notifierIdNum = Int64.Parse(notifierId);
                    }
                    catch
                    {
                        throw new UnmatchedNotifierException();
                    }

                    using (WebApiHelpers.GetTenantContext(tenant))
                    {
                        _receiver.HandleRequest(notifierIdNum, sms);
                    }

                    return new HttpResponseMessage(HttpStatusCode.Accepted);
                }
                catch (TwilioValidationException)
                {
                    EventLog.Application.WriteError($"Request failed validation. URL:{System.Web.HttpContext.Current.Request.Url}");
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
        }


        /// <summary>
        /// Note that the following parameters sent by twilio are currently ignored:
        ///      FromCity	The city of the sender
        ///      FromState The state or province of the sender.
        ///      FromZip The postal code of the called sender.
        ///      FromCountry The country of the called sender.
        ///      ToCity The city of the recipient.
        ///      ToState The state or province of the recipient.
        ///      ToZip The postal code of the recipient.
        ///      ToCountry The country of the recipient.
        /// </summary>
        [Route("twilio/sms/sendstatus/{tenant}/{notifierId}")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage UpdateStatus( [FromUri] string tenant, [FromUri] string notifierId, SmsStatus request)
        {
            string messageSid = request.SmsSid;
            string messageStatus = request.MessageStatus;
            string errorCode = request.ErrorCode;

            // Run service
            using (Profiler.Measure("TwilioController.UpdateStatus"))
            {
                try
                {
                    long notifierIdNum;

                    try
                    {
                        notifierIdNum = Int64.Parse(notifierId);
                    }
                    catch
                    {
                        throw new UnmatchedNotifierException();
                    }

                    bool handled = false;

                    using (WebApiHelpers.GetTenantContext(tenant))
                    {
                        handled = _receiver.HandleStatusUpdate(notifierIdNum, messageSid, messageStatus, errorCode);
                    }

                    return new HttpResponseMessage(handled ? HttpStatusCode.Accepted : HttpStatusCode.NotFound);
                }
                catch (TwilioValidationException)
                {
                    EventLog.Application.WriteError($"Request failed validation. URL:{System.Web.HttpContext.Current.Request.Url}");
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
        }


    }

    public class SmsStatus
    {
        /// <summary>
        /// Your Twilio account id. It is 34 characters long, and always starts with the letters AC
        /// </summary>
        public string AccountSid { get; set; }

        /// <summary>
        /// The phone number or client identifier of the party that initiated the call
        /// </summary>
        /// <remarks>
        /// Phone numbers are formatted with a '+' and country code, e.g. +16175551212 (E.164 format). Client identifiers begin with the client: URI scheme; for example, for a call from a client named 'tommy', the From parameter will be client:tommy.
        /// </remarks>
        public string From { get; set; }

        /// <summary>
        /// The phone number or client identifier of the called party
        /// </summary>
        /// <remarks>
        /// Phone numbers are formatted with a '+' and country code, e.g. +16175551212 (E.164 format). Client identifiers begin with the client: URI scheme; for example, for a call to a client named 'jenny', the To parameter will be client:jenny.
        /// </remarks>
        public string To { get; set; }

        /// <summary>
        /// A 34 character unique identifier for the message. May be used to later retrieve this message from the REST API
        /// </summary>
        public string SmsSid { get; set; }

        /// <summary>
        /// The status of the message
        /// </summary>
        public string MessageStatus { get; set; }

        /// <summary>
        /// The status of the message
        /// </summary>
        public string ErrorCode { get; set; }
    }
}