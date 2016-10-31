// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives', [
        'mod.app.editForm.customDirectives.spTargetCampaignSurveyTakerPicker',
        'mod.app.editForm.customDirectives.spTargetCampaignTargetsEditor',
        'mod.app.editForm.customDirectives.spSurveySectionsEditor',
        'mod.app.editForm.customDirectives.spChoiceQuestionOptionsEditor',
        'mod.app.editForm.customDirectives.spSubjectNavigationAccessEditor',
        'mod.app.editForm.customDirectives.spSubjectRecordAccessEditor',
        'mod.app.editForm.customDirectives.spSubjectRecordAccessSummary'
    ]);
}());