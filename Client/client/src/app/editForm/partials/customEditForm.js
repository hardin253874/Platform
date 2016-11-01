// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('app.editForm.customEditFormController', [
        'mod.app.editForm',
        'mod.app.resourceScopeService',
        'mod.app.editFormMobileServices',
        'sp.navService',
        'mod.app.spFormControlVisibilityService'
    ]);

    angular.module('app.editForm.customEditFormController')
        .controller('customEditFormController', CustomEditFormController)
        .controller('customEditFormMobileController', CustomEditFormMobileController);

    /* @ngInject*/
    function CustomEditFormController($scope, spEditForm) {

        $scope.$watch("formMode", function () {
            $scope.isReadOnly = $scope.formMode !== spEditForm.formModes.edit;
        });

        $scope.isFirstStructureControl = true; // used for determining the title class
    }

    /* @ngInject*/
    function CustomEditFormMobileController($scope, spEditForm, spEditFormMobile, spState, spFormControlVisibilityService) {

        // If some wrapping control has defined a pager we will we will use that.
        // If not we will need to create a pager with one page containing a vertical stack of controls.

        var pagifyFn;

        //console.log('DEBUG: CustomEditFormMobileController: ', $scope.$id, spState.getPageState());

        if ($scope.pagerOptions) {
            pagifyFn = spEditFormMobile.pagifyForm;
        } else {
            $scope.pagerOptions = {pages: [], additionalPages: []};
            pagifyFn = spEditFormMobile.dummyPagifyForm;
        }

        $scope.isFirstStructureControl = true; // used for determining the title class

        $scope.$watch("formMode", function () {
            $scope.isReadOnly = $scope.formMode !== spEditForm.formModes.edit;
        });

        $scope.$watch('formControl', function (formControl) {
            // console.log('DEBUG: watch formControl', $scope.$id, formControl);
            createPages(formControl, $scope.pagerOptions, pagifyFn);
        });

        $scope.$watch('pagerOptions.additionalPages', function (value) {
            // console.log('DEBUG: watch pagerOptions', $scope.$id, value);
            createPages($scope.formControl, $scope.pagerOptions, pagifyFn);
        });

        $scope.$watch('pagerOptions.selectedPage', function (selectedPage) {
            //console.log('DEBUG: watch selectedPage', selectedPage, $scope.id);
            if (!_.isUndefined(selectedPage)){
                spState.getPageState().lastSelectedMobilePage = selectedPage;
            }
        });

        function createPages(formControl, pagerOptions, pagifyFn) {
            if (formControl) {
                configurePager(pagifyFn(formControl), pagerOptions);
                $scope.$emit('dolayout');

                // #28305: Mobile: Able to save a form with null mandatory fields. (*a validator function is attached to the leaf control of cloned form Entity not the original form entity.)
                // a leaf control attaches a validator function to the leaf control (containedControls relationship instances of form Entity). These controls are then asked to validate on save.
                // in the case of mobile, we use a clone of original form entity and the validator functions gets attached to cloned form entity controls instead of the original form entity.
                var formToValidate = sp.result(pagerOptions, 'pages[0].scope.pageFormControl');
                if (formToValidate) {
                    $scope.$emit('mobileFormToValidate', pagerOptions.pages[0].scope.pageFormControl);
                } else {
                    console.error('Invalid form to validate.');
                }
            }
        }

        function controlVisibilityHandler(controlId, isControlVisible) {
            if (!$scope.pagerOptions || !$scope.pagerOptions.pages || !controlId) {
                return;
            }

            // Find the page that the control is on
            var page = _.find($scope.pagerOptions.pages, function (p) {
                var pageFormControl = sp.result(p, "scope.pageFormControl");
                if (!pageFormControl) {
                    return false;
                }
                
                return _.some(spEditForm.getFormControls(pageFormControl), function(c) {
                    return c && c.id() === controlId;
                });
            });

            if (page &&
                page.isHidden === isControlVisible) {
                page.isHidden = !isControlVisible;
            }
        }

        function registerVisibilityHandler(formControls) {
            if (!formControls || !spFormControlVisibilityService.isShowHideFeatureOn()) {
                return;
            }

            // Get paged controls except for the details page            
            _.forEach(_.drop(formControls), function(control) {
                var controls = spFormControlVisibilityService.getControlsWithVisibilityCalculations(control);
                if (!controls || _.isEmpty(controls)) {
                    return true;
                }

                _.forEach(controls, function(c) {
                    spFormControlVisibilityService.registerControlVisibilityHandler($scope, c.id(), controlVisibilityHandler);
                });

                return true;
            });
        }

        function configurePager(formControls, pagerOptions) {
            //console.log('DEBUG: configurePager', $scope.$id, pagerOptions, spState.navItem);
            //console.log('DEBUG: configurePager pages before: ', sp.result(pagerOptions.pages, 'length'), pagerOptions.selectedPage);

            registerVisibilityHandler(formControls);

            var pages = _.map(formControls, createPageObject);
            pagerOptions.pages = pages.concat(pagerOptions.additionalPages);

            setSelectedPage(pagerOptions);

            //console.log('DEBUG: configurePager pages after: ', pagerOptions.pages.length, pagerOptions.selectedPage);
        }

        function setSelectedPage(pagerOptions) {
            pagerOptions.selectedPage  = spState.getPageState().lastSelectedMobilePage || 0;
            pagerOptions.selectedPage = Math.min(pagerOptions.selectedPage, pagerOptions.pages.length - 1);
            spState.getPageState().lastSelectedMobilePage = pagerOptions.selectedPage;
        }

        function createPageObject(pageFormControl) {
            // create a new child scope with the formControl set to the pages form control
            var newScope = $scope.$new(false);
            newScope.pageFormControl = pageFormControl;

            return {
                template: 'editForm/partials/customEditFormMobilePage.tpl.html',
                scope: newScope,
                isHidden: false
            };
        }
    }
}());
