// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
    static class CustomEditFormHelper
    {
        /// <summary>
        ///     Defines info that we want to load whenever fetching a report for use by a form.
        /// </summary>
        public static string GetRelationshipReportQueryString =
            "name, alias, description, resourceViewerConsoleForm.id";

        

        /// <summary>
        /// The form query for a html form
        /// </summary>
        /// <param name="isDesignMode">Is the forms to be used in design mode?</param>
        /// <returns></returns>
        public static string GetHtmlFormQuery(bool isDesignMode = false)
        {

            var ndTag = new string[] {"_ND_", " name, description, alias "};
            var iotTag = new string[] { "_IOT_", " isOfType.{id, alias _RF_} " }; // ** warning: on client, 'cloneFormEntity' will create new entity if isOfType information is available 
            var iotControlTag = new string[] { "_IOTControl_", "isOfType.{ name, alias, k:minWidth, k:minWidthTablet, k:minWidthMobile, k:minHeight, k:minHeightTablet, k:minHeightMobile, k:pagerSupportMobile _RF_} " };
            var imgSizeTag = new string[] { "_ImgSize_", " k:thumbnailSizeSetting.{ alias, k:thumbnailWidth, k:thumbnailHeight } " };
            var typeBehaviorTag = new string[] { "_TypeBehavior_", " k:typeConsoleBehavior.{ k:treeIcon.{ name, imageBackgroundColor} } " };

            const string relatedFormSearch = "_RF_";

            const string fieldValidationQueryFragment = @"
isOfType.{id, alias},
isRequired, 
allowMultiLines, 
pattern.{regex, regexDescription}, 
minLength, maxLength, 
minInt, maxInt, 
minDecimal, maxDecimal, 
minDate, maxDate, 
minTime, maxTime, 
minDateTime, maxDateTime ";

            string baseQuery =(@"
_ND_, _IOTControl_, 
cacheChangeMarker,
isPrivatelyOwned,
isOfType.{id, alias, _TypeBehavior_},
k:typeToEditWithForm.{name, canCreateType, k:typeConsoleBehavior.{ k:treeIcon.{ name, imageBackgroundColor} } }, 
k:resourceConsoleBehavior.{id, name, alias, k:suppressActionsForType},
k:resourceInFolder.{ id, name, alias},            
allowSelectMultiTypes,
k:hideLabel,
k:showFormHelpText,
k:renderingHorizontalResizeMode.{alias},
k:renderingVerticalResizeMode.{alias},
k:containedControlsOnForm*.{ 
    _ND_, _IOTControl_, 
    k:renderingOrdinal, k:renderingHeight, k:renderingWidth, k:renderingBackgroundColor,k:hideLabel,k:renderingHorizontalResizeMode.{alias},k:renderingVerticalResizeMode.{alias},
    k:mandatoryControl, k:readOnlyControl,k:showControlHelpText, k:visibilityCalculation,
    _ImgSize_, k:thumbnailScalingSetting.alias,
    k:relationshipControlFilters.{
        _IOT_,
        k:relationshipControlFilterOrdinal,
        k:relationshipControlFilter.id,
        k:relationshipFilter.id,
        k:relationshipDirectionFilter.{alias}
    },
	k:controlRelatedEntityDataPathNodes.{
		k:dataPathNodeOrdinal, k:dataPathNodeRelationshipDirection.alias, k:dataPathNodeRelationship.{ 
			reverseAlias, relType.alias,
			_ND_, toName, fromName, 
			fromType.{name, alias, isAbstract, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
			fromType.inherits*.{name, alias, k:defaultEditForm.name},
			toType.{name, alias, isAbstract, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
			toType.inherits*.{name, alias, k:defaultEditForm.name},
			toTypeDefaultValue.{name, description}, fromTypeDefaultValue.{name, description},
			defaultFromUseCurrent, defaultToUseCurrent, 
			cardinality.{_ND_}, 
			relationshipIsMandatory, revRelationshipIsMandatory
		}
	},
    resourceViewerConsoleForm.name, canCreate, canCreateDerivedTypes, k:pickerReport.name, k:relationshipDisplayReport.name,
    k:wbcWorkflowToRun.{name}, k:wbcResourceInputParameter.{name}
}, 
k:fieldToRender.{
    _ND_,
    isOfType.{id, alias}, decimalPlaces, fieldRepresents.{id, alias}, defaultValue, autoNumberDisplayPattern, fieldWatermark, isFieldReadOnly,
    isCalculatedField,
    " + fieldValidationQueryFragment + @"
}, 
k:readOnlyControl,
k:isReversed,
hideOnDesktop,hideOnTablet,hideOnMobile,inSolution.name,k:navigationElementIcon.{alias, name, imageBackgroundColor}, k:navElementTreeIconBackgroundColor,
k:relationshipToRender.{ 
    reverseAlias, relType.alias,
    _ND_, toName, fromName, 
    enumValueFormattingType.{_ND_},
    fromType.{name, alias, isAbstract, canCreateType, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
    fromType.inherits*.{name, alias, canCreateType, k:defaultEditForm.name},
    toType.{name, alias, isAbstract, canCreateType, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
    toType.inherits*.{name, alias, canCreateType, k:defaultEditForm.name},
    toTypeDefaultValue.{name, description}, fromTypeDefaultValue.{name, description},
    defaultFromUseCurrent, defaultToUseCurrent, 
    cardinality.{_ND_}, showRelationshipHelpText,
    relationshipIsMandatory, revRelationshipIsMandatory
},
k:receiveContextFrom.id,
k:sendContextTo.id, 
k:reportToRender.{_ND_}, 
heroTextReport.{_ND_}, 
k:chartToRender.{_ND_,chartReport.name}, 
k:formToRender*.{_ND_ }"
)
                    .Replace(ndTag[0], ndTag[1])
                    .Replace(iotTag[0], iotTag[1])
                    .Replace(iotControlTag[0], iotControlTag[1])
                    .Replace(typeBehaviorTag[0], typeBehaviorTag[1])
                    .Replace(imgSizeTag[0], imgSizeTag[1]);

            var viewEditQuery = baseQuery.Replace(relatedFormSearch, string.Empty);

            if (!isDesignMode)
            {
                return viewEditQuery;
            }
            else
            {
                // when in design mode we also return any of the related configration forms.
                var designQuery = baseQuery
                    .Replace(relatedFormSearch, ", k:defaultEditForm.{ " + viewEditQuery + '}')
                    .Replace("isCalculatedField", "isCalculatedField, fieldScriptName, fieldCalculation")
                    .Replace("toName", "toName, toScriptName")
                    .Replace("fromName", "fromName, fromScriptName")
                    + ", typeScriptName";
                return designQuery;
            }

        }
    }
}