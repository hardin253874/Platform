// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _ */
(function() {
    'use strict';

    angular.module('app.controls', [
        'ui.router',
        'titleService',
        'app.controls.providers',
        'app.controls.spNumberControl',
        'app.controls.spCurrencyControl',
        'app.controls.spDecimalControl',
        'app.controls.spCheckboxControl',
        'app.controls.spDateControl',
        'app.controls.spDateMobileControl',
        'app.controls.spDateConfigControl',
        'app.controls.spDateAndTimeControl',
        'app.controls.spDateAndTimeMobileControl',
        'app.controls.spDateAndTimeConfigControl',
        'app.controls.spTimeControl',
        'app.controls.spMandatoryIndicator',
        'app.controls.spSearchControl',
        'app.controls.spAutoSizeInput',
        'app.controls.spDefaultContentHeader',
        'app.controls.spIconCheckbox',
        'app.controls.dialog.spEntitySaveAsDialog',
        'app.controls.spNavContainerPicker',
        'app.controls.dialog.spNavContainerPickerDialog'
    ])
        .config(function($stateProvider) {
            $stateProvider.state('controls', {
                url: '/{tenant}/{eid}/controls?path',
                templateUrl: 'controls/controls.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.controlsTestPage = { name: 'Controls', viewType: 'controls' };
        })
        .controller('controlsController', function($scope, titleService) {

            titleService.setTitle('Controls');


            $scope.currentControlUrl = "devViews/fieldProperties/fieldPropertiesTest.tpl.html";

            //developer can set their own control page in the controls array
            var controls =
            [
                { id: "0", title: "Login", url: "login/login.tpl.html" },
                { id: "1", title: "Home", url: "home/home.tpl.html" },
                { id: "2", title: "Dialog", url: "controls/dialog/demoDialog.tpl.html" },
                { id: "3", title: "Summary Validation", url: "controls/summaryValidation/demoSummaryValidation.tpl.html" },
                { id: "4", title: "Test Page Control", url: "controls/dialog/testPage.tpl.html" },
                { id: "5", title: "Entity Pickers", url: "devViews/entityPickers/entityPickersTest.tpl.html" },
                { id: "6", title: "Resource Info", url: "devViews/resourceInfo/resourceInfoTest.tpl.html" },
                { id: "7", title: "Data Grid", url: "devViews/dataGrid/dataGridTest.tpl.html" },
                { id: "8", title: "Entity Request", url: "devViews/entityRequest/entityRequestTest.tpl.html" },
                { id: "9", title: "Map control", url: "controls/maps/mapControl.tpl.html" },
                { id: "10", title: "Report Control", url: "devViews/report/reportTest.tpl.html" },
                { id: "11", title: "Date Time Pickers", url: "devViews/datePickers/datePickersTest.tpl.html" },
                { id: "12", title: "Context Menu", url: "devViews/contextMenu/contextMenuTest.tpl.html" },
                { id: "13", title: "File Upload", url: "devViews/fileUpload/fileUploadTest.tpl.html" },
                { id: "14", title: "Report Builder", url: "controls/reportbuilder/demoReportbuilder.tpl.html" },
                //{ id: "14", title: "Report Builder", url: "reportBuilder/reportBuilder.tpl.html" },
                { id: "15", title: "EditSaveCancel", url: "devViews/editSaveCancel/demoEditSaveCancel.tpl.html" },
                { id: "16", title: "Alerter", url: "devViews/alerter/demoAlerter.tpl.html" },
                { id: "17", title: "Busy Indicator", url: "devViews/busyIndicator/busyIndicatorTest.tpl.html" },
                { id: "18", title: "Charts", url: "devViews/charts/chartTest.tpl.html" },
                { id: "19", title: "Color Picker", url: "devViews/colorPicker/colorPickerTest.tpl.html" },
                { id: "20", title: "Edit Form Controls", url: "devViews/editFormControls/editFormControlsTest.tpl.html" },
                { id: "21", title: "Value Editor", url: "devViews/valueEditor/valueEditorTest.tpl.html" },
                { id: "23", title: "Tabs", url: "devViews/tabs/tabsTest.tpl.html" },
                { id: "24", title: "Refresh Button", url: "devViews/refreshButton/refreshButtonTest.tpl.html" },
                { id: "25", title: "Field Properties", url: "devViews/fieldProperties/fieldPropertiesTest.tpl.html" },
                { id: "26", title: "Applicable Tasks", url: "devViews/applicableTasks/applicableTasksTest.tpl.html" },
                { id: "26", title: "Editable Label", url: "devViews/editableLabel/editableLabelTest.tpl.html" }
            ];
            $scope.controls = _.sortBy(controls, function(c) { return c.title; });

            $scope.openControl = function(control) {
                $scope.currentControlUrl = control.url;
            };
        });
}());