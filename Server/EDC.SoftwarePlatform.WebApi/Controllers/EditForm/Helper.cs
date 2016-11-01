// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Infrastructure;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
	internal class Helper
	{
		/// <summary>
		///     Given a type, get the default form for it.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <param name="isInDesignMode">if set to <c>true</c> [is in design mode].</param>
		/// <param name="forceGenerate">Force a generated form to be used</param>
		/// <returns>
		///     A response containing entity data for the form
		/// </returns>
		public static HttpResponseMessage<JsonQueryResult> GetDefaultFormForType( EntityType entityType, bool isInDesignMode, bool forceGenerate )
		{
			CustomEditForm formRef = entityType.DefaultEditForm;

			if ( formRef != null && !forceGenerate )
			{
				var entityBatch = new EntityPackage( );

				EntityData formEntityData = EditFormHelper.GetFormAsEntityData( formRef );

				entityBatch.AddEntityData( formEntityData, "formEntity" );

				return new HttpResponseMessage<JsonQueryResult>( entityBatch.GetQueryResult( ) );
			}
			//TODO: return something to indicate that there is no form
			return GetGeneratedFormForType( entityType, isInDesignMode );
		}


		/// <summary>
		///     Given a type, get the generated form for it.
		/// </summary>
		/// <param name="entityType">Type of the entity.</param>
		/// <param name="isInDesignMode">if set to <c>true</c> [is in design mode].</param>
		/// <returns>
		///     A response containing entity data for the form
		/// </returns>
		public static HttpResponseMessage<JsonQueryResult> GetGeneratedFormForType( EntityRef entityType, bool isInDesignMode )
		{
			EntityData formEntityData = EditFormHelper.GenerateDefaultFormForResourceType( entityType, isInDesignMode );

			var entityBatch = new EntityPackage( );

			entityBatch.AddEntityData( formEntityData, "formEntity" );

			return new HttpResponseMessage<JsonQueryResult>( entityBatch.GetQueryResult( ) );
		}
	}
}