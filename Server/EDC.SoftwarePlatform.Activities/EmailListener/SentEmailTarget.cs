// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
	/// <summary>
	///     Updates mail boxes on the provider whenever the mail box is saved.
	/// </summary>
	public class SentEmailTarget : IEntityEventSave
	{
		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			foreach ( IEntity entity in entities )
			{
				var message = entity.Cast<SentEmailMessage>( );

				if ( message.SemSequenceNumber == null )
				{
					message.SemSequenceNumber = EmailHelper.GenerateMessageIdLocalPart( );
				}
			}

			return false;
		}

		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			// do nothing
		}
	}
}