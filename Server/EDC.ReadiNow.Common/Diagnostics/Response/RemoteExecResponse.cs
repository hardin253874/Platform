// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics.Response
{
    /// <summary>
    ///     Remote exec response.
    /// </summary>
    [ProtoContract]
    public class RemoteExecResponse : DiagnosticResponse
    {
        /// <summary>
        ///     The response data.
        /// </summary>
        [ProtoMember(1)]
        public List<Tuple<string, string>> Data { get; set; }


        /// <summary>
        ///     The id of the response.
        /// </summary>
        [ProtoMember(2)]
        public string Id { get; set; }
    }
}