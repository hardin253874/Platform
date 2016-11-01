// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IMutableKey generic interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMutableKey<T>
	{
		/// <summary>
		///     Gets or sets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		T Key
		{
			get;
			set;
		}
	}

	/// <summary>
	///     IMutableIdKey interface.
	/// </summary>
	public interface IMutableIdKey : IMutableKey<long>
	{
	}
}