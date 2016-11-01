// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Interface that monitors for cancellations.
    /// </summary>
    /// <remarks>
    /// It would have been nice to use CancellationToken, but it requires push based cancellations, however
    /// we don't want to do that because we want to maintain state in the database, and poll for cancellation requests.
    /// </remarks>
    public interface ICancellationWatcher
    {
        bool IsCancellationRequested { get; }
    }
}
