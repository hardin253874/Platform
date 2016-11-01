// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.Marketplace.WebApi.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class VerifyEmail
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "token")]
        public string Token { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class VerifyEmailResult
    {
        /// <summary>
        /// - "tokenerror" unknown/bad token
        /// - "processing" received and processing
        /// - "done" done, ready to log in ... here's the URL..
        /// - other failure modes??
        /// </summary>
        [DataMember(Name = "status")]
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "url")]
        public string PlatformUrl { get; set; }
    }
}