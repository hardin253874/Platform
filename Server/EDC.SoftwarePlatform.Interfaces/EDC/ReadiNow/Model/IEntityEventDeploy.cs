// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Interface for specifying deploy events.
	/// </summary>
	public interface IEntityEventDeploy : IEntityEvent
	{
		/// <summary>
		///     Called after deploying an application.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state.</param>
		void OnAfterDeploy( IEnumerable<IEntity> entities, IDictionary<string, object> state );

        /// <summary>
        /// Called if a failure occurs deploying an application
        /// </summary>
        /// <param name="solutions">The solutions.</param>
        /// <param name="state">The state.</param>
        void OnDeployFailed(IEnumerable<ISolutionDetails> solutions, IDictionary<string, object> state);
	}
}