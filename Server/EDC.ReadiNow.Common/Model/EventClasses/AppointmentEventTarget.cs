// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Model.EventClasses
{
	using iCalEvent = Scheduling.iCalendar.Event;

	/// <summary>
	///     EventEmail event target.
	/// </summary>
    public class AppointmentEventTarget : FilteredTarget
	{

        protected override HashSet<long> GetWatchedFields()
        {
            return new HashSet<long>
            {
                Appointment.Name_Field.Id,
                Appointment.Description_Field.Id,
                Appointment.EventStart_Field.Id,
                Appointment.EventEnd_Field.Id,
                Appointment.EventIsAllDay_Field.Id,
                Appointment.EventLocation_Field.Id,
                Appointment.EventEmailPriority_Field.Id,
                Appointment.ApptSendEmail_Field.Id,

			};
        }

        protected override HashSet<long> GetWatchedForwardRels()
        {
            return new HashSet<long>
            {
                Appointment.EventEmailAttendees_Field.Id,
                Appointment.EventStatus_Field.Id,
            };
        }

        protected override HashSet<long> GetWatchedReverseRels()
        {
            return new HashSet<long>();
        }


		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		protected override void FilteredOnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			if ( entities == null )
			{
				return;
			}

			foreach ( IEntity entity in entities )
			{
				var appointment = entity.As<Appointment>( );

                if (appointment != null)
				{
                    AppointmentHelper.GenerateAndSendICalAppointment(appointment);
                }
			}
		}

		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		protected override bool FilteredOnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			return false;
		}
    }
}