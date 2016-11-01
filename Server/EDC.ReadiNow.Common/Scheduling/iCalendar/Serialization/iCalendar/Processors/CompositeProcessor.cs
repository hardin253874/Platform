// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     CompositeProcessor class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CompositeProcessor<T> : List<ISerializationProcessor<T>>, ISerializationProcessor<T>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CompositeProcessor{T}" /> class.
		/// </summary>
		public CompositeProcessor( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CompositeProcessor{T}" /> class.
		/// </summary>
		/// <param name="processors">The processors.</param>
		public CompositeProcessor( IEnumerable<ISerializationProcessor<T>> processors )
		{
			AddRange( processors );
		}

		/// <summary>
		///     Pres the serialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PreSerialization( T obj )
		{
			foreach ( var p in this )
			{
				p.PreSerialization( obj );
			}
		}

		/// <summary>
		///     Posts the serialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PostSerialization( T obj )
		{
			foreach ( var p in this )
			{
				p.PostSerialization( obj );
			}
		}

		/// <summary>
		///     Pres the deserialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PreDeserialization( T obj )
		{
			foreach ( var p in this )
			{
				p.PreDeserialization( obj );
			}
		}

		/// <summary>
		///     Posts the deserialization.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void PostDeserialization( T obj )
		{
			foreach ( var p in this )
			{
				p.PostDeserialization( obj );
			}
		}
	}
}