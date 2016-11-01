// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Abstract base class for iterator instances.
	/// </summary>
	/// <typeparam name="TSource">The type of the source.</typeparam>
	internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
	{
		#region Fields

		/// <summary>
		///     Owning thread id.
		/// </summary>
		private readonly int _threadId;

		/// <summary>
		///     Current iterator value.
		/// </summary>
		protected TSource CurrentValue = default( TSource );

		/// <summary>
		///     Iterator state.
		/// </summary>
		protected IteratorState State = IteratorState.Uninitialized;

		#endregion Fields

		#region Methods

		/// <summary>
		///     Initializes a new instance of the <see cref="Iterator&lt;TSource&gt;" /> class.
		/// </summary>
		protected Iterator( )
		{
			_threadId = Thread.CurrentThread.ManagedThreadId;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TSource> GetEnumerator( )
		{
			if ( ( _threadId == Thread.CurrentThread.ManagedThreadId ) && ( State == IteratorState.Uninitialized ) )
			{
				State = IteratorState.Initialized;
				return this;
			}

			Iterator<TSource> iterator = Clone( );
			iterator.State = IteratorState.Initialized;
			return iterator;
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return GetEnumerator( );
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose( )
		{
			CurrentValue = default( TSource );
			State = IteratorState.Disposed;
		}

		/// <summary>
		///     Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		public abstract bool MoveNext( );

		/// <summary>
		///     Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		void IEnumerator.Reset( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <returns>
		///     A clone of the current iterator instance.
		/// </returns>
		public abstract Iterator<TSource> Clone( );

		#endregion Methods

		#region Properties

		/// <summary>
		///     Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <returns>
		///     The element in the collection at the current position of the enumerator.
		/// </returns>
		public TSource Current
		{
			get
			{
				return CurrentValue;
			}
		}

		/// <summary>
		///     Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <returns>
		///     The element in the collection at the current position of the enumerator.
		/// </returns>
		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		#endregion Properties
	}
}