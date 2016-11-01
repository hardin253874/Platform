// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.FileManager
{
	/// <summary>
	///     File Detail.
	/// </summary>
	[DataContract]
	public class FileDetail
	{
		/// <summary>
		///     Gets a value indicating whether this <see cref="FileDetail" /> is exists.
		/// </summary>
		/// <value>
		///     <c>true</c> if exists; otherwise, <c>false</c>.
		/// </value>
		[DataMember]
		public bool Exists
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the full name.
		/// </summary>
		/// <value>
		///     The full name.
		/// </value>
		[DataMember]
		public string FullName
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember]
		public string Name
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the path.
		/// </summary>
		/// <value>
		///     The path.
		/// </value>
		[DataMember]
		public string Path
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the size.
		/// </summary>
		/// <value>
		///     The size.
		/// </value>
		[DataMember]
		public int Size
		{
			get;
			internal set;
		}
	}
}