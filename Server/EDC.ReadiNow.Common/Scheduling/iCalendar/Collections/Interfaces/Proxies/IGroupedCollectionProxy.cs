// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     IGroupedCollectionProxy interface.
	/// </summary>
	/// <typeparam name="TGroup">The type of the group.</typeparam>
	/// <typeparam name="TOriginal">The type of the original.</typeparam>
	/// <typeparam name="TNew">The type of the new.</typeparam>
	public interface IGroupedCollectionProxy<TGroup, TOriginal, TNew> : IGroupedCollection<TGroup, TNew>
		where TOriginal : class, IGroupedObject<TGroup>
		where TNew : class, TOriginal
	{
		/// <summary>
		///     Gets the real object.
		/// </summary>
		/// <value>
		///     The real object.
		/// </value>
		IGroupedCollection<TGroup, TOriginal> RealObject
		{
			get;
		}

		/// <summary>
		///     Sets the proxied object.
		/// </summary>
		/// <param name="realObject">The real object.</param>
		void SetProxiedObject( IGroupedCollection<TGroup, TOriginal> realObject );
	}
}