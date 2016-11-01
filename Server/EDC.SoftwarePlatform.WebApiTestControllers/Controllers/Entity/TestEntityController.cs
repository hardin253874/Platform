// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Web.Http;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApiTestControllers.Controllers.Entity
{
	/// <summary>
	///     Test Controller class.
	/// </summary>
    [RoutePrefix("data/v1/test/entitytest")]
	public class TestEntityController : ApiController
	{
		/// <summary>
		///     Handle a batch of multiple queries.
		/// </summary>
		/// <returns></returns>
		public HttpResponseMessage<int> Get( )
		{
			return new HttpResponseMessage<int>( 1 );
		}

		/// <summary>
		///     Handle a batch of multiple queries.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <returns></returns>
		[Route( "" )]
        [HttpPost]
		public HttpResponseMessage<TestResult> Query( [FromBody] TestRequest batch )
		{
            IEntity entity = EntityNugget.DecodeEntity(batch.MyEntityData);

			var result = new TestResult
			{
				MyOtherData = batch.MyOtherData,
				Success = entity != null
			};

			return new HttpResponseMessage<TestResult>( result );
		}

		/// <summary>
		///     Test report object
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <returns></returns>
		[Route( "report" )]
        [HttpPost]
		public HttpResponseMessage<TestResult> ReportEntity( [FromBody] TestRequest batch )
		{
			Report reportEntity;
			IEntity entity = EntityNugget.DecodeEntity( batch.MyEntityData );
			try
			{
				reportEntity = ReadiNow.Model.Entity.As<Report>( entity );
			}
			catch
			{
				reportEntity = null;
			}
			var result = new TestResult
			{
				MyOtherData = batch.MyOtherData,
				Success = reportEntity != null
			};

			return new HttpResponseMessage<TestResult>( result );
		}

		/// <summary>
		///     Test report column order is changed.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <returns></returns>
		[Route( "updatereportcolumnorder" )]
        [HttpPost]
		public HttpResponseMessage<TestResult> UpdateReportColumnOrder( [FromBody] TestRequest batch )
		{
			int columnOrder = -1;
			int originalColumnOrder = -1;
			IEntity entity = EntityNugget.DecodeEntity( batch.MyEntityData );

			try
			{
				var reportEntity = ReadiNow.Model.Entity.As<Report>( entity );
				var originalReportEntity = ReadiNow.Model.Entity.Get<Report>( reportEntity.Id );
				originalColumnOrder = ( originalReportEntity.ReportColumns != null && originalReportEntity.ReportColumns.Count > 0 ) ? originalReportEntity.ReportColumns.Last( ).ColumnDisplayOrder ?? -1 : -1;
				columnOrder = ( originalReportEntity.ReportColumns != null && originalReportEntity.ReportColumns.Count > 0 ) ? reportEntity.ReportColumns.Last( ).ColumnDisplayOrder ?? -1 : -1;
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to update report column order. " + exc.Message );
			}

			var result = new TestResult
			{
				MyOtherData = batch.MyOtherData,
				Success = columnOrder != originalColumnOrder
			};

			return new HttpResponseMessage<TestResult>( result );
		}

		/// <summary>
		///     Test report related node is added.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <returns></returns>
		[Route( "updatereportrelatednode" )]
        [HttpPost]
		public HttpResponseMessage<TestResult> UpdateReportRelatedNode( [FromBody] TestRequest batch )
		{
			int relatedReportCount = -1;
			int originalRelatedReportCount = -1;
			IEntity entity = EntityNugget.DecodeEntity( batch.MyEntityData );
			try
			{
				var reportEntity = ReadiNow.Model.Entity.As<Report>( entity );
				var originalReportEntity = ReadiNow.Model.Entity.Get<Report>( reportEntity.Id );
				relatedReportCount = ( reportEntity.RootNode != null && reportEntity.RootNode.RelatedReportNodes != null ) ? reportEntity.RootNode.RelatedReportNodes.Count : 0;
				originalRelatedReportCount = ( originalReportEntity.RootNode != null && originalReportEntity.RootNode.RelatedReportNodes != null ) ? originalReportEntity.RootNode.RelatedReportNodes.Count : 0;
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to Update Report Related Node. " + exc.Message );
			}
			var result = new TestResult
			{
				MyOtherData = batch.MyOtherData,
				Success = relatedReportCount != originalRelatedReportCount
			};

			return new HttpResponseMessage<TestResult>( result );
		}

		/// <summary>
		///     Test report related node is added.
		/// </summary>
		/// <param name="batch">The batch.</param>
		/// <returns></returns>
		[Route( "summariseReportRootNode" )]
        [HttpPost]
		public HttpResponseMessage<TestResult> SummariseReportRootNode( [FromBody] TestRequest batch )
		{
			string rootNodeTypeAlias = string.Empty;
			string originalRootNodeTypeAlias = string.Empty;
			IEntity entity = EntityNugget.DecodeEntity( batch.MyEntityData );
			try
			{
				var reportEntity = ReadiNow.Model.Entity.As<Report>( entity );
				var originalReportEntity = ReadiNow.Model.Entity.Get<Report>( reportEntity.Id );
				rootNodeTypeAlias = reportEntity.RootNode.IsOfType[ 0 ].Alias;
				originalRootNodeTypeAlias = originalReportEntity.RootNode.IsOfType[ 0 ].Alias;
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( "Failed to Summarise Report Root Node. " + exc.Message );
			}

			var result = new TestResult
			{
				MyOtherData = batch.MyOtherData,
				Success = rootNodeTypeAlias != originalRootNodeTypeAlias
			};

			return new HttpResponseMessage<TestResult>( result );
		}
	}
}