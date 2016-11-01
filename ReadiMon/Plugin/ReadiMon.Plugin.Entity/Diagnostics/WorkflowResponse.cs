// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using ProtoBuf;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Entity.Diagnostics
{
	/// <summary>
	///     Workflow response.
    /// </summary>
    [ProtoContract]
    public class WorkflowResponse : DiagnosticResponse
    {
        /// <summary>
        ///     Gets or sets the date.
        /// </summary>
        /// <value>
        ///     The date.
        /// </value>
        [ProtoMember(5)]
        public DateTime Date
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        [ProtoMember(1)]
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        [ProtoMember(4)]
        public string Status
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the task identifier.
        /// </summary>
        /// <value>
        ///     The task identifier.
        /// </value>
        [ProtoMember(7)]
        public string TaskId
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the triggered by.
        /// </summary>
        /// <value>
        ///     The triggered by.
        /// </value>
        [ProtoMember(6)]
        public string TriggeredBy
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the name of the workflow.
        /// </summary>
        /// <value>
        ///     The name of the workflow.
        /// </value>
        [ProtoMember(2)]
        public string WorkflowName
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the name of the workflow run.
        /// </summary>
        /// <value>
        ///     The name of the workflow run.
        /// </value>
        [ProtoMember(3)]
        public string WorkflowRunName
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the name of the server the event occurred on.
        /// </summary>
        /// <value>
        ///     The name of the workflow run.
        /// </value>
        [ProtoMember(8)]
        public string Server
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the name of process the event occurred on.
        /// </summary>
        /// <value>
        ///     The name of the workflow run.
        /// </value>
        [ProtoMember(9)]
        public string Process
        {
            get;
            set;
        }
    }
}