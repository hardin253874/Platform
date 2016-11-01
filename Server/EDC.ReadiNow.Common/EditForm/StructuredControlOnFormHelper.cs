// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model
{
	public static class StructureControlOnFormHelper
	{
		/// <summary>
		///     Get all the contained field controls
		/// </summary>
		/// <param name="structureControl">The structure control.</param>
		/// <returns></returns>
		public static IEnumerable<FieldControlOnForm> GetAllFieldControlOnForms( StructureControlOnForm structureControl )
		{
			IEnumerable<FieldControlOnForm> fieldControls = from sc in GetAllStrutureControls( structureControl )
			                                                from fc in sc.ContainedControlsOnForm
			                                                let fcAsFieldControlOnForm = fc.As<FieldControlOnForm>( )
			                                                where fcAsFieldControlOnForm != null
			                                                select fcAsFieldControlOnForm;

			return fieldControls;
		}

		/// <summary>
		///     Get all the contained structure controls including the given one (if it is one)
		/// </summary>
		/// <param name="structureControl">The structure control.</param>
		/// <returns></returns>
		private static IEnumerable<StructureControlOnForm> GetAllStrutureControls( StructureControlOnForm structureControl )
		{
			IEnumerable<StructureControlOnForm> structureControls = from c in structureControl.ContainedControlsOnForm
			                                                        let cAsStructureControlOnForm = c.As<StructureControlOnForm>( )
			                                                        where cAsStructureControlOnForm != null
			                                                        select cAsStructureControlOnForm;

			return new List<StructureControlOnForm>
				{
					structureControl
				}.Concat( structureControls );
		}

	}
}