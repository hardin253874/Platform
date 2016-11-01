﻿// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Redis
{
    /// <summary>
    ///     Remote exec Message
    /// </summary>
    public class RemoteExecMessage : PubSubMessage<RemoteExecData>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RemoteExecMessage" /> class.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="machine">The machine.</param>
        /// <param name="process">The process.</param>
        /// <param name="processId">The process identifier.</param>
        /// <param name="appDomain">The application domain.</param>
        /// <param name="appDomainId">The application domain identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="size">The size.</param>
        /// <param name="hostTenantId">The host tenant identifier.</param>
        /// <param name="message">The message.</param>
        public RemoteExecMessage(string channel, string machine, string process, long processId, string appDomain, long appDomainId, DateTime dateTime, long size, long hostTenantId, RemoteExecData message)
            : base(channel, machine, process, processId, appDomain, appDomainId, dateTime, size, hostTenantId, message)
        {
        }
    }
}