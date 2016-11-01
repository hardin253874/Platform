// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor
{
	/// <summary>
	///     DataType helpers.
	/// </summary>
	public static class DataTypeHelpers
	{
		/// <summary>
		///     Strings to object.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static object StringToObject( string value, DataType type )
		{
			//todo - fill out with other type... or ask around.. surely this has been done before

			try
			{
				switch ( type )
				{
                    case DataType.String:
                        return value;

                    case DataType.Int32:
						return Int32.Parse( value );

					case DataType.Decimal:
					case DataType.Currency:
						return Decimal.Parse( value );

					case DataType.DateTime:
					case DataType.Date:
					case DataType.Time:
						return DateTime.Parse( value );

                    case DataType.Bool:
                        return bool.Parse( value );

                    case DataType.Entity:
						return new EntityRef( value ).Entity;

					default:
						return value;
				}
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError(
					"CalcEditor WebService: exception parsing parameter value {0} and type {1}. Ex=", value, type,
					ex.Message );
				return null;
			}
		}
	}
}