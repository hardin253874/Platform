// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ProtoBuf;
using ReadiMon.Shared.Diagnostics.Request;

namespace ReadiMon.Plugin.Redis.Diagnostics
{
    /// <summary>
    ///     Remote exec request
    /// </summary>
    [ProtoContract]
    public class RemoteExecRequest : DiagnosticRequest
    {
        /// <summary>
        /// The code to execute.
        /// </summary>
        [ProtoMember(1)]
        public string Code
        {
            get;
            set;
        }


        /// <summary>
        /// The id of the request.
        /// </summary>
        [ProtoMember(2)]
        public string Id { get; set; }


        /// <summary>
        ///     The name of the target.
        /// </summary>
        [ProtoMember(3)]
        public string Target { get; set; }
    }
}