// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Contracts
{
    /// <summary>
    /// Defines the minimum properties of a CAST based request message.
    /// </summary>
    public interface ICastResponse
    {
        /// <summary>
        /// Flag indicating an error has occurred from the request.
        /// </summary>
        bool IsError { get; set; }

        /// <summary>
        /// Provides information about any error that may have occurred.
        /// </summary>
        string Error { get; set; }
    }
}
