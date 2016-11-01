// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Xml;
using System.Xml.Linq;
using EDC.SoftwarePlatform.Migration.Processing;

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure
{
	/// <summary>
	///     Xml Translation failure
	/// </summary>
	public class XmlFailure : EntityDataFailure
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="XmlFailure" /> class.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="source">The source.</param>
		/// <param name="value">The value.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="reason">The reason.</param>
		/// <param name="level">The level.</param>
		public XmlFailure( Guid entityUpgradeId, Guid fieldUpgradeId, XElement source, object value, XmlConversionMode mode, XmlFailureReason reason, FailureLevel level )
			: base( entityUpgradeId, fieldUpgradeId, "Xml", level )
		{
			Mode = mode;
			Source = source;
			Reason = reason;
			Value = value;
		}

		/// <summary>
		///     Gets the mode.
		/// </summary>
		/// <value>
		///     The mode.
		/// </value>
		public XmlConversionMode Mode
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the reason.
		/// </summary>
		/// <value>
		///     The reason.
		/// </value>
		public XmlFailureReason Reason
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the source.
		/// </summary>
		/// <value>
		///     The source.
		/// </value>
		public XElement Source
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public object Value
		{
			get;
			private set;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			IXmlLineInfo lineInfo = Source;

			return string.Format( @"  Entity Id: {0}
  Field Id: {1}
  Mode: {2}
  Reason: {3}
  Value: {4}
  Node: {5}
  Line: {6}
  Position: {7}", EntityUpgradeId.ToString( "B" ), FieldUpgradeId.ToString( "B" ), StatisticsHelper.Capitalize( Mode.ToString( ) ), StatisticsHelper.Capitalize( Reason.ToString( ) ), Value, Source, lineInfo.HasLineInfo( ) ? lineInfo.LineNumber.ToString( ) : "N/A", lineInfo.HasLineInfo( ) ? lineInfo.LinePosition.ToString( ) : "N/A" );
		}
	}
}