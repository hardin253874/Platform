// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.IO;
using ProtoBuf;
using ProtoBuf.Data;

namespace EDC.Serialization.Surrogates
{
	[ProtoContract]
	public class DataTableSurrogate
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="DataTableSurrogate" /> class from being created.
		/// </summary>
		private DataTableSurrogate( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DataTableSurrogate" /> class.
		/// </summary>
		/// <param name="data">The data.</param>
		private DataTableSurrogate( byte[ ] data )
			: this( )
		{
			Data = data;
		}

		/// <summary>
		///     Gets or sets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		[ProtoMember( 1 )]
		public byte[ ] Data
		{
			get;
			set;
		}

		/// <summary>
		///     Performs an implicit conversion from <see cref="DataTable" /> to <see cref="DataTableSurrogate" />.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static implicit operator DataTableSurrogate( DataTable dataTable )
		{
			if ( dataTable == null )
			{
				return null;
			}

			using ( var memoryStream = new MemoryStream( ) )
			using ( IDataReader reader = dataTable.CreateDataReader( ) )
			{
				DataSerializer.Serialize( memoryStream, reader );

				return new DataTableSurrogate( memoryStream.ToArray( ) );
			}
		}

		/// <summary>
		///     Performs an implicit conversion from <see cref="LazySurrogate{T}" /> to <see cref="Lazy{T}" />.
		/// </summary>
		/// <param name="surrogate">The surrogate.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static implicit operator DataTable( DataTableSurrogate surrogate )
		{
			if ( surrogate == null )
			{
				return null;
			}

			var dt = new DataTable( );

			using ( var memoryStream = new MemoryStream( surrogate.Data ) )
			using ( IDataReader reader = DataSerializer.Deserialize( memoryStream ) )
			{
				dt.Load( reader );
			}

			return dt;
		}
	}
}