// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.flexEditForm')
        .component('spFlexEditForm', {
            bindings: {
                formControl: '=',
                parentControl: '=?',
                formData: '=?',
                formTheme: '=?',
                formMode: '=?',
                actionPanelFile: '=?',
                actionPanelOptions: '=?',
                isInTestMode: '=?',
                isInDesign: '=?',
                isEmbedded: '=?',
                pagerOptions: '=?'
            },
            controller: FlexEditFormController,
            template: `
                <rn-vertical-stack-container-control 
                    form-control="$ctrl.formControl" form-data="$ctrl.formData">
                </rn-vertical-stack-container-control>
                `
        });

    /* @ngInject */
    function FlexEditFormController($scope, spFlexEditFormService) {
        let $ctrl = this; // to match implied controllerAs of ng component
    }

    angular.module('app.flexEditForm').directive('rnFormControl', rnFormControl);

    /* @ngInject */
    function rnFormControl($compile, spFlexEditFormService) {

        let {getDirectiveNameForControl} = spFlexEditFormService;

        return {
            restrict: 'E',
            scope: {
                formControl: '=',
                formData: '=?'
            },
            template: `<div>should be replaced</div>`,
            link: function (scope, element, attrs) {
                let classes = attrs['class'];
                let directiveName = getDirectiveNameForControl(scope.formControl);
                let template = [
                    '<', directiveName,
                    ' class="', classes, '"',
                    ' form-control="formControl"',
                    ' form-data="formData"',
                    ' version2="true"',
                    ' title="{{formControl.debugString}}"',//TODO: remove DEBUG
                    '></', directiveName, '>'
                ].join('');

                 let html = $compile(template)(scope);
                 element.replaceWith(html);
                //$compile(element.html(template).contents())(scope);
            }
        };
    }

    angular.module('app.flexEditForm').directive('rnDefaultFieldControl', rnDefaultFieldControl);

    /* @ngInject */
    function rnDefaultFieldControl() {
        return {
            restrict: 'E',
            scope: {
                formControl: '=',
                formData: '=?'
            },
            bindToController: true,
            controllerAs: 'ctrl',
            templateUrl: 'editForm/flex/defaultControl.tpl.html',
            controller: DefaultFieldControlController
        };

        /* @ngInject */
        function DefaultFieldControlController($scope, spEditForm) {
            let ctrl = this; // to match the controllerAs

            $scope.$watch('ctrl.formControl', (formControl) => {
                // console.log('rnDefaultFieldControl $watch', $scope.$id, (formControl || {}).debugString);
                updateModel();
            });

            $scope.$watch('ctrl.formData', (formData) => {
                // console.log('rnDefaultFieldControl $watch formData', $scope.$id, formData);
                updateModel();
            });

            function updateModel() {
                console.assert(ctrl.formControl);

                let fieldToRender = ctrl.formControl.fieldToRender;

                if (fieldToRender) {
                    ctrl.fieldLabel = fieldToRender.name;
                    if (ctrl.formData) {
                        ctrl.fieldValue = ctrl.formData.getField(fieldToRender.eidP);
                    }
                }

                ctrl.fieldValue = ctrl.fieldValue || ctrl.formControl.typesP[0].nsAlias;

                ctrl.titleModel = spEditForm.createTitleModel(ctrl.formControl);
                ctrl.displayString = ctrl.fieldValue || '';
            }
        }
    }

    // angular.module('app.flexEditForm').directive('rnSingleLineTextControl', rnSingleLineTextControl);

    /* @ngInject */
    function rnSingleLineTextControl() {
        return {
            restrict: 'E',
            scope: {
                formControl: '=',
                formData: '=?'
            },
            bindToController: true,
            controllerAs: 'ctrl',
            templateUrl: 'editForm/flex/singleLineTextControl.tpl.html',
            controller: SingleLineTextControlController
        };

        /* @ngInject */
        function SingleLineTextControlController($scope, spEditForm) {
            let ctrl = this; // to match the controllerAs

            $scope.$watch('ctrl.formControl', updateModel);
            $scope.$watch('ctrl.formData', updateModel);

            function updateModel() {
                console.assert(ctrl.formControl);
                console.assert(ctrl.formControl.fieldToRender);

                let fieldToRender = ctrl.formControl.fieldToRender;

                if (ctrl.formData) {
                    ctrl.fieldValue = ctrl.formData.getField(fieldToRender.eidP);
                }

                ctrl.titleModel = spEditForm.createTitleModel(ctrl.formControl);
                ctrl.displayString = ctrl.fieldValue || '';
            }
        }
    }

    // angular.module('app.flexEditForm').directive('rnChoiceRelationshipRenderControl', rnChoiceRelationshipRenderControl);

    /* @ngInject */
    function rnChoiceRelationshipRenderControl() {
        return {
            restrict: 'E',
            scope: {
                formControl: '=',
                formData: '=?'
            },
            bindToController: true,
            controllerAs: 'ctrl',
            templateUrl: 'editForm/flex/choiceRelationshipRenderControl.tpl.html',
            controller: ChoiceRelationshipRenderControlController
        };

        /* @ngInject */
        function ChoiceRelationshipRenderControlController($scope, spEditForm) {
            let ctrl = this; // to match the controllerAs

            $scope.$watch('ctrl.formControl', updateModel);
            $scope.$watch('ctrl.formData', updateModel);

            function updateModel() {
                console.assert(ctrl.formControl);

                spEditForm.commonRelFormControlInit(ctrl, {validator: validate});

                ctrl.displayString = spEditForm.getDisplayName(getSelectedEntities(ctrl));
            }

            function validate() {
                spEditForm.validateRelationshipControl(ctrl, getSelectedEntities(ctrl));
            }
        }
    }

    // angular.module('app.flexEditForm').directive('rnTabRelationshipRenderControl', rnTabRelationshipRenderControl);

    /* @ngInject */
    function rnTabRelationshipRenderControl() {
        return {
            restrict: 'E',
            scope: {
                formControl: '=',
                formData: '=?'
            },
            bindToController: true,
            controllerAs: 'ctrl',
            templateUrl: 'editForm/flex/tabRelationshipRenderControl.tpl.html',
            controller: TabRelationshipRenderControlController
        };

        /* @ngInject */
        function TabRelationshipRenderControlController($scope, spEditForm) {
            let ctrl = this; // to match the controllerAs

            ctrl.reportOptions = {};

            $scope.$watch('ctrl.formControl', updateModel);
            $scope.$watch('ctrl.formData', updateModel);

            function updateModel() {

                let {formControl, formData, reportOptions} = ctrl;

                console.assert(formControl);

                reportOptions.formControlEntity = formControl;

                if (formData) {
                    reportOptions.formDataEntity = formData;
                    reportOptions.modifyAccessDenied = !formData.canModify;
                    reportOptions.relationDetail = {
                        eid: formData.idP,
                        relid: (formControl.relationshipToRender || {}).idP,
                        // THIS LOOKS WRONG but isn't, the relationship direction on the report filter is reversed
                        // because the report is on the pointed at type not this type.
                        direction: formControl.isReversed ? 'fwd' : 'rev'
                    };

                    // if report or relationship has changed then clear selected items
                    //if ... todo
                    //reportOptions.selectedItems = [];
                }
            }
        }
    }

    function getSelectedEntities({formData, relTypeId}) {
        if (formData && relTypeId) {
            return formData.getRelationship(relTypeId);
        }
        return [];
    }

}());

