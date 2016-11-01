// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.Connector.Interfaces
{
    /// <summary>
    /// Interface for an actual worker that processes an import in the worker thread.
    /// </summary>
    public interface IImportRunWorker
    {
        /// <summary>
        /// Start processing an import run.
        /// </summary>
        /// <param name="importRunId">ID of the import run to process.</param>
        void StartImport(long importRunId);
    }
}
