﻿// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Messages
{
	/// <summary>
	///     Exit message
	/// </summary>
	[DataContract( Name = "ExitMessage" )]
	public class ExitMessage
	{
		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return Serializer.SerializeObject( this );
		}
	}
}