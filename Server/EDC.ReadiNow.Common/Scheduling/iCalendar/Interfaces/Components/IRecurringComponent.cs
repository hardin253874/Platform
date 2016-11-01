// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IRecurringComponent interface.
	/// </summary>
	public interface IRecurringComponent : IUniqueComponent, IRecurrable, IAlarmContainer
	{
		/// <summary>
		///     Gets or sets the attachments.
		/// </summary>
		/// <value>
		///     The attachments.
		/// </value>
		IList<IAttachment> Attachments
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the categories.
		/// </summary>
		/// <value>
		///     The categories.
		/// </value>
		IList<string> Categories
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the class.
		/// </summary>
		/// <value>
		///     The class.
		/// </value>
		string Class
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the contacts.
		/// </summary>
		/// <value>
		///     The contacts.
		/// </value>
		IList<string> Contacts
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the created.
		/// </summary>
		/// <value>
		///     The created.
		/// </value>
		IDateTime Created
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the last modified.
		/// </summary>
		/// <value>
		///     The last modified.
		/// </value>
		IDateTime LastModified
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the priority.
		/// </summary>
		/// <value>
		///     The priority.
		/// </value>
		int Priority
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the related components.
		/// </summary>
		/// <value>
		///     The related components.
		/// </value>
		IList<string> RelatedComponents
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the sequence.
		/// </summary>
		/// <value>
		///     The sequence.
		/// </value>
		int Sequence
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the summary.
		/// </summary>
		/// <value>
		///     The summary.
		/// </value>
		string Summary
		{
			get;
			set;
		}
	}
}