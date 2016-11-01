// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using EDC.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Utc;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;
using EDC.ReadiNow.Core;

// TODO - refactor out the common stuff

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEditor
{
	/// <summary>
	///     Calculation Editor controller.
	/// </summary>
	[RoutePrefix( "data/v1/calceditor" )]
	public class CalcEditorController : ApiController
	{
		/// <summary>
		///     Inners the eval.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		private static EvalResult InnerEval( EvalRequest request )
		{
            try
			{
				var settings = new BuilderSettings( );

				if ( !string.IsNullOrEmpty( request.Context ) )
				{
					long typeId;
					EntityRef contextType = long.TryParse( request.Context, out typeId )
						? new EntityRef( typeId )
						: new EntityRef( request.Context );
					settings.RootContextType = ExprTypeHelper.EntityOfType( contextType );
				}
				if ( !string.IsNullOrEmpty( request.Host ) )
				{
					settings.ScriptHost = ( ScriptHostType ) Enum.Parse( typeof ( ScriptHostType ), request.Host );
				}


				var paramTypes = new Dictionary<string, ExprType>( );
				var paramValues = new Dictionary<string, object>( );

				if ( request.Parameters != null )
				{
					foreach ( ExpressionParameter p in request.Parameters )
					{
						var type = ( DataType ) Enum.Parse( typeof ( DataType ), p.TypeName );
						if ( type == DataType.Entity )
						{
							long typeId;
							paramTypes[ p.Name ] = long.TryParse( p.EntityTypeId, out typeId )
								? ExprTypeHelper.EntityOfType( new EntityRef( typeId ) )
								: ExprTypeHelper.EntityOfType( new EntityRef( p.EntityTypeId ) );
						}
						else
						{
							paramTypes[ p.Name ] = new ExprType( type );
						}
						paramTypes[ p.Name ].IsList = p.IsList;
						paramValues[ p.Name ] = DataTypeHelpers.StringToObject( p.StringValue, type );
					}
					settings.ParameterNames = paramTypes.Keys.ToList( );
					settings.StaticParameterResolver = paramName =>
					{
						ExprType result2;
						return paramTypes.TryGetValue( paramName, out result2 ) ? result2 : null;
					};
				}

				// Compile
				IExpression expression = Factory.ExpressionCompiler.Compile( request.Expression, settings );

				// used for runtime parameters
                EvaluationSettings evalSettings = new EvaluationSettings
				{
					TimeZoneName = TimeZoneHelper.SydneyTimeZoneName,
					ParameterResolver = paramName =>
					{
						object result1;
						return paramValues.TryGetValue( paramName, out result1 ) ? result1 : null;
					}
				};
				object result = Factory.ExpressionRunner.Run( expression, evalSettings ).Value;

				// Return success result
				return new EvalResult
				{
					Value = result.ToString( ),
					ResultType = expression.ResultType.Type,
					IsList = expression.ResultType.IsList,
					EntityTypeId = expression.ResultType.EntityTypeId
				};
			}
			catch ( ParseException ex )
			{
				// Return script error result
				return new EvalResult
				{
					ErrorMessage = ex.Message
				};
			}
		}

		/// <summary>
		///     Inners the compile.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		private static CompileResult InnerCompile( EvalRequest request )
		{
			try
			{
				var settings = new BuilderSettings( );

				if ( !string.IsNullOrEmpty( request.Context ) )
				{
					EntityRef contextType = WebApiHelpers.GetId( request.Context );
					settings.RootContextType = ExprTypeHelper.EntityOfType( contextType );
				}
				if ( !string.IsNullOrEmpty( request.Host ) )
				{
					settings.ScriptHost = ( ScriptHostType ) Enum.Parse( typeof ( ScriptHostType ), request.Host );
				}

                if ( request.ExpectedResultType != null )
                {
                    settings.ExpectedResultType = request.ExpectedResultType.ToExprType();
                }

				var paramTypes = new Dictionary<string, ExprType>( );

				if ( request.Parameters != null )
				{
					foreach ( ExpressionParameter p in request.Parameters )
					{
						var type = ( DataType ) Enum.Parse( typeof ( DataType ), p.TypeName );
						if ( type == DataType.Entity )
						{
							long typeId;
							paramTypes[ p.Name ] = long.TryParse( p.EntityTypeId, out typeId )
								? ExprTypeHelper.EntityOfType( new EntityRef( typeId ) )
								: ExprTypeHelper.EntityOfType( new EntityRef( p.EntityTypeId ) );
						}
						else
						{
							paramTypes[ p.Name ] = new ExprType( type );
						}
						paramTypes[ p.Name ].IsList = p.IsList;
					}
					settings.ParameterNames = paramTypes.Keys.ToList( );
					settings.StaticParameterResolver = paramName =>
					{
						ExprType result2;
						return paramTypes.TryGetValue( paramName, out result2 ) ? result2 : null;
					};
				}

				// Compile
				IExpression expression = Factory.ExpressionCompiler.Compile( request.Expression, settings );

				// Return success result
				return new CompileResult
				{
					ResultType = expression.ResultType.Type,
					IsList = expression.ResultType.IsList,
					EntityTypeId = expression.ResultType.EntityTypeId
				};
			}
			catch ( ParseException ex )
			{
				// Return script error result
				return new CompileResult
				{
					ErrorMessage = ex.Message
				};
			}
		}

		/// <summary>
		///     Evaluates the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		[Route( "eval" )]
        [HttpPost]
		public HttpResponseMessage<EvalResult> Eval( [FromBody] EvalRequest request )
		{
            if (request == null)
                throw new WebArgumentNullException("request");

            if (request.Parameters == null)
                request.Parameters = new List<ExpressionParameter>();

            try
            {
				using ( Profiler.Measure( "CalcEditorController.Eval" ) )
				{
					List<ExpressionParameter> paramsToEval = request.Parameters.Where( p => p.StringValue == null && !string.IsNullOrEmpty( p.Expr ) ).ToList( );

					paramsToEval.Sort( ( x, y ) => y.Expr.IndexOf( x.Name, StringComparison.Ordinal ) >= 0 ? -1 : +1 );

					int retries = paramsToEval.Count( );
					var innerRequest = new EvalRequest
					{
						Context = request.Context,
                        Host = request.Host,
                        ExpectedResultType = request.ExpectedResultType
                    };

					while ( retries-- > 0 && paramsToEval.Count > 0 )
					{
						foreach ( ExpressionParameter paramToEval in paramsToEval )
						{
							innerRequest.Expression = paramToEval.Expr;
							innerRequest.Parameters = request.Parameters.Where( p => p.StringValue != null ).ToList( );

							EvalResult evalResult = InnerEval( innerRequest );

							if ( evalResult.Value != null && string.IsNullOrEmpty( evalResult.ErrorMessage ) )
							{
								paramToEval.StringValue = evalResult.Value;
							}
						}

						paramsToEval = request.Parameters.Where( p => p.StringValue == null && !string.IsNullOrEmpty( p.Expr ) ).ToList( );
					}

					return new HttpResponseMessage<EvalResult>( InnerEval( request ) );
				}
			}
			catch ( Exception ex )
			{
                throw new Exception( string.Format("Exception evaluating expression {0}", request.Expression ), ex);
            }
		}

		/// <summary>
		///     Compiles the specified request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		[Route( "compile" )]
        [HttpPost]
		public HttpResponseMessage<CompileResult> Compile( [FromBody] EvalRequest request )
		{
            if (request == null)
                throw new WebArgumentNullException("request");

            if (request.Parameters == null)
                request.Parameters = new List<ExpressionParameter>();

            try
			{
				using ( Profiler.Measure( "CalcEditorController.Compile" ) )
				{
					List<ExpressionParameter> paramsToCompile = request.Parameters.Where( p => p.TypeName == "None" && !string.IsNullOrEmpty( p.Expr ) ).ToList( );

					paramsToCompile.Sort( ( x, y ) => y.Expr.IndexOf( x.Name, StringComparison.Ordinal ) >= 0 ? -1 : +1 );

					int retries = paramsToCompile.Count( );
					var innerRequest = new EvalRequest
					{
						Context = request.Context,
                        Host = request.Host,
                        ExpectedResultType = request.ExpectedResultType
					};

					while ( retries-- > 0 && paramsToCompile.Count > 0 )
					{
						foreach ( ExpressionParameter paramToCompile in paramsToCompile )
						{
							innerRequest.Expression = paramToCompile.Expr;
							innerRequest.Parameters = request.Parameters.Where( p => p.TypeName != "None" ).ToList( );

							CompileResult compileResult = InnerCompile( innerRequest );

							if ( compileResult.ResultType != DataType.None && string.IsNullOrEmpty( compileResult.ErrorMessage ) )
							{
								paramToCompile.TypeName = compileResult.ResultType.ToString( );
							}
						}

						paramsToCompile = request.Parameters.Where( p => p.TypeName == "None" && !string.IsNullOrEmpty( p.Expr ) ).ToList( );
					}

					return new HttpResponseMessage<CompileResult>( InnerCompile( request ) );
				}
			}
			catch ( Exception ex )
			{
                throw new Exception(string.Format("Exception evaluating expression {0}", request.Expression), ex);
			}
		}

	}
}