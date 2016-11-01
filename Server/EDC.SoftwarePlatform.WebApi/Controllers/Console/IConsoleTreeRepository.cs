// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
    /// <summary>
    /// A repository for console navigation trees.
    /// </summary>
    public interface IConsoleTreeRepository
    {
        /// <summary>
        /// Build and return the console navigation tree.
        /// </summary>
        /// <returns></returns>
        EntityData GetTree();
    }
}