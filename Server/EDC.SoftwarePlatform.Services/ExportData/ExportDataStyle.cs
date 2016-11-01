// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Drawing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Color = System.Drawing.Color;
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;

namespace EDC.SoftwarePlatform.Services.ExportData
{
	//******* Predefined Number format index for excel sheet ************//
	//ID Format Code
	//0  General
	//1   0
	//2   0.00 
	//3   #,##0
	//4   #,##0.00
	//9   0%
	//10  0.00%
	//11  0.00E+00
	//12  # ?/?
	//13  # ??/??
	//14  mm-dd-yy
	//15  d-mmm-yy
	//16  d-mmm
	//17  mmm-yy
	//18  h:mm AM/PM
	//19  h:mm:ss AM/PM
	//20  h:mm
	//21  h:mm:ss
	//22  m/d/yy h:mm
	//37  #,##0 ;(#,##0)
	//38  #,##0 ;[Red](#,##0)
	//39  #,##0.00;(#,##0.00)
	//40  #,##0.00;[Red](#,##0.00)
	//45  mm:ss
	//46  [h]:mm:ss
	//47  mmss.0
	//48  ##0.0E+0
	//49  @
	//
	/// <summary>
	///     Class supports to create Excel stylesheet.
	/// </summary>
	public class ExportDataStyle
	{
		/// <summary>
		///     Create cell format.
		/// </summary>
		/// <param name="styleSheet">
		///     The stylesheet for the current WorkBook.
		/// </param>
		/// <param name="fontIndex">
		///     Font style index.
		/// </param>
		/// <param name="fillIndex">
		///     Fill index.
		/// </param>
		/// <param name="numberFormatId">
		///     Cell Format Id.
		/// </param>
		/// <returns></returns>
		public static UInt32Value CreateCellFormat(
			Stylesheet styleSheet,
			UInt32Value fontIndex,
			UInt32Value fillIndex,
			UInt32Value numberFormatId )
		{
			var cellFormat = new CellFormat( );

			if ( fontIndex != null )
				cellFormat.FontId = fontIndex;

			if ( fillIndex != null )
				cellFormat.FillId = fillIndex;

			if ( numberFormatId != null )
			{
				cellFormat.NumberFormatId = numberFormatId;
				cellFormat.ApplyNumberFormat = BooleanValue.FromBoolean( true );
			}

			styleSheet.CellFormats.Append( cellFormat );

			UInt32Value result = styleSheet.CellFormats.Count;
			styleSheet.CellFormats.Count++;
			return result;
		}

		/// <summary>
		///     Creates a new Fill object and appends it to the WorkBook's stylesheet.
		/// </summary>
		/// <param name="styleSheet">The stylesheet for the current WorkBook.</param>
		/// <param name="fillColor">The background color for the fill.</param>
		/// <returns></returns>
		public static UInt32Value CreateFill(
			Stylesheet styleSheet,
			Color fillColor )
		{
			var fill = new Fill(
				new PatternFill(
					new ForegroundColor
					{
						Rgb = new HexBinaryValue
						{
							Value =
								ColorTranslator.ToHtml(
									Color.FromArgb(
										fillColor.A,
										fillColor.R,
										fillColor.G,
										fillColor.B ) ).Replace( "#", "" )
						}
					} )
				{
					PatternType = PatternValues.Solid
				}
				);
			styleSheet.Fills.Append( fill );
			UInt32Value result = styleSheet.Fills.Count;
			styleSheet.Fills.Count++;
			return result;
		}

		/// <summary>
		///     Creates a new font and appends it to the workbook's stylesheet
		/// </summary>
		/// <param name="styleSheet">The stylesheet for the current WorkBook</param>
		/// <param name="fontName">The font name.</param>
		/// <param name="fontSize">The font size.</param>
		/// <param name="isBold">Set to true for bold font.</param>
		/// <param name="foreColor">The font color.</param>
		/// <returns>The index of the font.</returns>
		public static UInt32Value CreateFont(
			Stylesheet styleSheet,
			string fontName,
			double? fontSize,
			bool isBold,
			Color foreColor )
		{
			// Fonts fonts = styleSheet.GetFirstChild<Fonts>();

			var font = new Font( );

			if ( !string.IsNullOrEmpty( fontName ) )
			{
				var name = new FontName
				{
					Val = fontName
				};
				font.Append( name );
			}

			if ( fontSize.HasValue )
			{
				var size = new FontSize
				{
					Val = fontSize.Value
				};
				font.Append( size );
			}

			if ( isBold )
			{
				var bold = new Bold( );
				font.Append( bold );
			}

			var color = new DocumentFormat.OpenXml.Spreadsheet.Color
			{
				Rgb = new HexBinaryValue
				{
					Value =
						ColorTranslator.ToHtml(
							Color.FromArgb(
								foreColor.A,
								foreColor.R,
								foreColor.G,
								foreColor.B ) ).Replace( "#", "" )
				}
			};
			font.Append( color );
			
			styleSheet.Fonts.Append( font );
			UInt32Value result = styleSheet.Fonts.Count;
			styleSheet.Fonts.Count++;
			return result;
		}
	}
}