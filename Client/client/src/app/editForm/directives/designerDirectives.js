// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console */
(function () {
    'use strict';

    /*
    * Central location for edit form designer directive registration.
    */
    angular.module('mod.app.editForm.designerDirectives', [
        'mod.app.editForm.designerDirectives.spEditForm',
        'mod.app.editForm.designerDirectives.spReplace',
        'mod.app.editForm.designerDirectives.spStructureControlOnFormRepeater',
        'mod.app.editForm.designerDirectives.spHoverAdornment',
        'mod.app.editForm.designerDirectives.spFlowContainer',
        'mod.app.editForm.designerDirectives.spVerticalStackContainerControl',
        'mod.app.editForm.designerDirectives.spControlOnForm',
        'mod.app.editForm.designerDirectives.spNumericKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spCurrencyKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spDecimalKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spColorRenderControl',
        'mod.app.editForm.designerDirectives.spAutoNumberFieldRenderControl',
        'mod.app.editForm.designerDirectives.spCheckboxKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spDateKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spDateAndTimeKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spTimeKFieldRenderControl',
        'mod.app.editForm.designerDirectives.spFieldTitle',
        'mod.app.editForm.designerDirectives.spTitlePlusMarkers',
        'mod.app.editForm.designerDirectives.spWorkflowButtonControl',
        'mod.app.editForm.designerDirectives.spSingleLineTextControl',
        'mod.app.editForm.designerDirectives.spTabContainerControl',
        'mod.app.editForm.designerDirectives.spTabRelationshipRenderControl',
        'mod.app.editForm.designerDirectives.spImageRelationshipRenderControl',
        'mod.app.editForm.designerDirectives.spImageFileNameUploadControl',
        'mod.app.editForm.designerDirectives.spInlineRelationshipRenderControl',
        'mod.app.editForm.designerDirectives.spDropDownRelationshipRenderControl',
        'mod.app.editForm.designerDirectives.spChoiceRelationshipRenderControl',
        'mod.app.editForm.designerDirectives.spMultiChoiceRelationshipRenderControl',
        'mod.app.editForm.designerDirectives.spCustomValidationMessage',
        'mod.app.editForm.designerDirectives.spHorizontalStackContainerControl',
        'mod.app.editForm.designerDirectives.spHeaderColumnContainerControl',
        'mod.app.editForm.designerDirectives.spFileNameUploadControl',
        'mod.app.editForm.designerDirectives.spFormRenderControl',
        'mod.app.editForm.designerDirectives.spChartRenderControl',
        'mod.app.editForm.designerDirectives.spReportRenderControl',
        'mod.app.editForm.designerDirectives.spHeroTextControl',
        'mod.app.editForm.designerDirectives.spPageSelector',
        'mod.app.editForm.designerDirectives.spPageContainer',
        'mod.app.editForm.designerDirectives.spEditFormLayout',
        'mod.app.editForm.spFieldControlProvider',
        'mod.app.editForm.customDirectives'
    ]);

}());