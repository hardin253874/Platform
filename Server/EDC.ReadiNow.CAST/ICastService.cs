// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.CAST
{
    /// <summary>
    /// Describes CAST specific operations.
    /// </summary>
    public interface ICastService
    {
        /// <summary>
        /// Checks if CAST has been enabled and remote communication has been configured for this platform.
        /// </summary>
        /// <returns>True if CAST communication should take place.</returns>
        bool GetIsCastConfigured();

        /// <summary>
        /// Checks if this platform has the CAST application installed in any tenancy and thus should operate as a CAST Server.
        /// </summary>
        /// <returns>True if this rig is a CAST Server.</returns>
        bool GetIsCastServer();

        /// <summary>
        /// Returns the key that uniquely defines this instance of the ReadiNow Platform for communication purposes.
        /// </summary>
        /// <returns>A key that can identify this platform for CAST/Client communication.</returns>
        string GetClientCommunicationKey();

        /// <summary>
        /// Constructs a context for specific internal CAST based operations.
        /// </summary>
        /// <returns>The disposable CAST context object.</returns>
        ContextBlock GetCastContext();

        /// <summary>
        /// Sets the user to be the well-known CAST user account for internal CAST based operations.
        /// </summary>
        /// <returns>The disposable user context object.</returns>
        SetUser GetCastUser();

        /// <summary>
        /// Sends a heartbeat payload with detailed platform information for processing by a listening CAST Server.
        /// </summary>
        void SendHeartbeat();

        /// <summary>
        /// Publishes a request, as a CAST Server, for any listening platforms to send their heartbeat info asap.
        /// </summary>
        void RequestHeartbeat();
    }
}
