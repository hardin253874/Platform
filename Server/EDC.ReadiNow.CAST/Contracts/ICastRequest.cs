// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Defines the minimum properties of a CAST based request message.
    /// </summary>
    public interface ICastRequest
    {
        /// <summary>
        /// The database identifier of the platform that the request is addressed to.
        /// </summary>
        string DatabaseId { get; set; }

        /// <summary>
        /// The entity id of the workflow run that has initiated the request.
        /// </summary>
        long RunId { get; set; }

        /// <summary>
        /// The step count within the workflow run of when the request was initiated.
        /// </summary>
        int RunStep { get; set; }
    }
}
