// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.EntityRequests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.EntityGraph
{
    /// <summary>
    /// Interface for a parser of graph (entity member / entity-info-service) style requests.
    /// </summary>
    public interface IRequestParser
    {
        /// <summary>
        /// Parser of graph (entity member / entity-info-service) style requests.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="settings">Parse settings.</param>
        /// <returns>Request object model.</returns>
        EntityMemberRequest ParseRequestQuery(string request, RequestParserSettings settings = null);
    }


    /// <summary>
    /// Parse settings.
    /// </summary>
    public class RequestParserSettings
    {
        /// <summary>
        /// Default settings, which validate fields and relationships.
        /// </summary>
        public static readonly RequestParserSettings Default = new RequestParserSettings(true);

        /// <summary>
        /// Default settings, which validate fields and relationships.
        /// </summary>
        public static readonly RequestParserSettings NoVerify = new RequestParserSettings(false);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="validate">Validate that fields and relationships actually are valid fields and relationships.</param>
        public RequestParserSettings(bool validate)
        {
            Validate = validate;
        }

        /// <summary>
        /// Validate that fields and relationships actually are valid fields and relationships.
        /// </summary>
        public bool Validate { get; set; }
    }
}
