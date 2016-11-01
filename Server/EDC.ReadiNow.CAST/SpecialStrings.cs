// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST
{
	/// <summary>
    /// Defines system and/or reserved strings.
	/// </summary>
    public static class SpecialStrings
    {
        /// <summary>
        /// CAST User. Built-in account for CAST management tasks.
        /// </summary>
        public static readonly string CastUserAlias = "cast:castUser";

        /// <summary>
        /// The well known name of the CAST heartbeat queue.
        /// </summary>
	    public static readonly string CastHeartbeatKey = "CAST.Heartbeat";

        /// <summary>
        /// The name of the exchange used to broadcast a "phone home" request to clients from the CAST server.
        /// </summary>
	    public static readonly string CastHeartbeatDemandKey = "CAST.Heartbeat-Demand";

        /// <summary>
        /// This string is prepended to the name of queues used for client communication by the CAST server.
        /// </summary>
	    public static readonly string CastClientKeyPrefix = "CAST.";
    }
}
