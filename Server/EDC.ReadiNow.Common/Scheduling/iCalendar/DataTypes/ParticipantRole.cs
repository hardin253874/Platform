// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     Participant Role.
	/// </summary>
	public enum ParticipantRole
	{
		/// <summary>
		///     Unknown participant.
		/// </summary>
		Unknown,

		/// <summary>
		///     Chair participant.
		/// </summary>
		Chair,

		/// <summary>
		///     Optional participant.
		/// </summary>
		OptionalParticipant,

		/// <summary>
		///     Required participant.
		/// </summary>
		RequiredParticipant,

		/// <summary>
		///     Non-Participant.
		/// </summary>
		NonParticipant
	}
}