// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

(function () {
    'use strict';

    angular.module('mod.app.editForm', [
        'ui.router', 'ui.bootstrap', 'titleService', 'mod.common.spEntityService', 'ngGrid', 'mod.app.editFormServices', 'app.editFormModules',
        'mod.app.editForm.designerDirectives', 'mod.common.editForm.editFormDirectives', 'sp.common.fieldValidator',
        'sp.navService', 'sp.common.filters', 'mod.common.alerts', 'mod.common.ui.spBusyIndicator', 'mod.common.spNgUtils',
        'mod.common.ui.spContextMenu', 'mod.app.navigationProviders', 'mod.common.spUserTask', 'sp.common.loginService',
        'mod.common.ui.spMeasureArrange', 'sp.themeService', 'mod.common.spMobile', 'mod.common.ui.spActionsService',
        'mod.featureSwitch', 'sp.consoleIconService', 'mod.common.ui.spDialogService', 'mod.common.ui.rnInfoButton',
        'mod.app.spFormControlVisibilityService']);

    angular.module('mod.app.editForm')
        .config(editFormConfiguration)
        .controller('editFormController', EditFormController)
        .controller('editFormActionPanelController', EditFormActionPanelController);

    /* @ngInject */
    function editFormConfiguration($stateProvider, $compileProvider) {

        var data = {
            showBreadcrumb: false,
            region: {
                'content-header': {templateUrl: 'editForm/editFormActionPanel.tpl.html'},
                'content-footer': {templateUrl: 'editForm/editFormFooter.tpl.html'}
            }
        };

        $stateProvider.state('viewForm', {
            url: '/{tenant}/{eid}/viewForm?path&formId&taskId&forceGenerate&inWizard&test',
            templateUrl: 'editForm/editForm.tpl.html',
            data: data
        });

        $stateProvider.state('editForm', {
            url: '/{tenant}/{eid}/editForm?path&formId&taskId&forceGenerate&inWizard&test',
            templateUrl: 'editForm/editForm.tpl.html',
            data: data
        });


        $stateProvider.state('createForm', {
            url: '/{tenant}/{eid}/createForm?path&formId&forceGenerate&test&returnToParent',
            templateUrl: 'editForm/editForm.tpl.html',
            data: data
        });

        // Allow the standard set of links
        $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|file|tel):/);
    }

    /* @ngInject */
    function EditFormController($scope, $element, $state, $stateParams, spAlertsService,
                                titleService, spEntityService, $q, $timeout, spEditForm, spNavService,
                                spNgUtils, spMobileContext, spNavigationBuilderProvider, spUserTask, spLoginService,
                                spMeasureArrangeService, spThemeService, spActionsService, rnFeatureSwitch, consoleIconService, spDialogService,
                                spFormControlVisibilityService) {

        if (!spLoginService.isSignedIn()) {
            console.log('editFormController: not signed in');
            return;
        }

        if (!spNavService.getCurrentItem()) {
            console.error('editFormController: no current nav item');
            return;
        }

        var navItem = spNavService.getCurrentItem();
        var parentItem = spNavService.getParentItem();
        var firstTime = !(navItem.data && (navItem.data.id || navItem.data.formId));
        var areCreating = $state.current.name === 'createForm';
        var navigationBuilderProvider = spNavigationBuilderProvider($scope);
        var taskPage = { template: 'editForm/applicableTasksMobile.tpl.html', scope: $scope };

        var getFormActionQuickMenuDebounce = _.debounce(getFormActionQuickMenu, 100);

        ///////////////////////////////////////////////////////////////////////
        // Bindings
        //
        $scope.nav = spNavService;
        $scope.isMobile = spMobileContext.isMobile;
        $scope.isTablet = spMobileContext.isTablet;
        $scope.measureArrangeOptions = {id: 'editForm'};
        $scope.fieldValidationMessages = [];
        $scope.pagerOptions = {additionalPages: []};

        // a set of additional request string fragments that formControls can
        // add to to get additional information
        $scope.requestStrings = [];

        // Used to embellish the forms to help in testing
        $scope.isInTestMode = sp.stringToBoolean($stateParams.test);

        $scope.item = navItem;
        $scope.item.isDirty = isEditFormDirty;

        $scope.requestFormData = requestFormData;
        $scope.toEditMode = toEditMode;
        $scope.doSave = doSave;
        $scope.saveIfEditing = saveIfEditing;
        $scope.configMenuModifyEntity = configMenuModifyEntity;
        $scope.configMenuDeleteEntity = configMenuDeleteEntity;

        $scope.allowFlexForm = rnFeatureSwitch.isFeatureOn('flexEditForm');
        $scope.fsShowHideControls = spFormControlVisibilityService.isShowHideFeatureOn();
        $scope.useFlexForm = false; //$scope.allowFlexForm;
        $scope.flexEditFormOptions = {};
        function setFlexEditFormOptions(options) {
            $scope.flexEditFormOptions = _.assign({}, $scope.flexEditFormOptions, options);
        }
        $scope.$watch('model.formMode', mode => {
            setFlexEditFormOptions({editing: mode === 'edit'});
        });
        $scope.$watch('designMode', designing => {
            setFlexEditFormOptions({designing});
        });
        $scope.toggleFlexInline = function () {
            setFlexEditFormOptions({inline: !$scope.flexEditFormOptions.inline});
        };
        $scope.toggleFlexMobile = function () {
            setFlexEditFormOptions({mobile: !$scope.flexEditFormOptions.mobile});
        };
        $scope.toggleFlexForm = function () {
            if (!$scope.allowFlexForm) return;
            $scope.useFlexForm = !$scope.useFlexForm;
            doLayout();
        };
        $scope.showStructureEditor = false;//$scope.allowFlexForm;
        $scope.toggleFormStructureEditor = function () {
            if (!$scope.allowFlexForm) return;
            $scope.showStructureEditor = !$scope.showStructureEditor;
            doLayout();
        };
        $scope.undoFormStructureEdit = function () {
            if (!$scope.allowFlexForm) return;
            if ($scope.model.formControl) {
                $scope.model.formControl.graph.history.undoBookmark();
                doLayout();
            }
        };
        $scope.toggleDesignMode = function () {
            if (!$scope.allowFlexForm) return;
            $scope.designMode = !$scope.designMode;
        };

        $scope.showActionButtons = showActionButtons;

        ///////////////////////////////////////////////////////////////////////
        // Watches and event listeners
        //

        $scope.$watch('nav.getThemes()', function (getThemesCompleted) {
            if (getThemesCompleted === true) {
                $scope.model.titleStyle = spThemeService.getTitleStyle();
            }
        });

        $scope.$watch('model.formTitle', function (value) {
            $scope.item.actionPanelOptions.formTitle = value;
        });

        $scope.$watch('model.formData', formDataChanged);

        $scope.$watch('[$stateParams.formId, $stateParams.taskId, $stateParams.inWizard]', possibleStateChanged);

        $scope.$watch('$scope.model.taskInfo.taskId', function () {
            var taskId = sp.result($scope, 'model.taskInfo.taskId');
            if (taskId) {
                fetchTasks();
            }
        });

        $scope.$on('calcTabsLayout', function (event, callback) {
            event.stopPropagation();
            callback($scope.measureArrangeOptions.id);
        });

        $scope.$on('doLayout', function (event) {
            event.stopPropagation();
            doLayout();
        });

        $scope.$on('measureArrangeComplete', onMeasureArrangeComplete);

        $scope.$on("mobileFormToValidate", function (event, formToValidate) {
            event.stopPropagation();
            if ($scope && $scope.model && formToValidate) {
                $scope.model.formToValidate = formToValidate;
            }
        });

        $scope.$on("addOnReturnFromChildCreate", function (event, onReturnFunction) {
            event.stopPropagation();
            $scope.model.onReturnFromChildCreateActions.push(onReturnFunction);
        });

        $scope.$on("addOnReturnFromChildUpdate", function (event, onReturnFunction) {
            event.stopPropagation();
            $scope.model.onReturnFromChildUpdateActions.push(onReturnFunction);
        });

        $scope.$on("enterEditModeFromDblclick", function (event) {
            event.stopPropagation();

            spUtils.safeApply($scope, function() {
                if (isViewMode()) {
                    toEditMode();
                }
            });
        });

        // A filter source control has changed it's data. Notify any filtered
        // controls to update themselves.
        $scope.$on('filterSourceControlDataChanged', filterSourceControlDataChanged);

        $scope.$on('$destroy', function () {
            spEditForm.clearRelationshipControlFilters($scope.model.formControl);
        });

        ///////////////////////////////////////////////////////////////////////
        // Load data and initialise
        //

        titleService.setTitle(areCreating ? 'Create' : 'Edit');

        if (firstTime || $scope.item.refreshRequired) {
            //console.log('DEBUG: EditFormController ctor: first time visit', $scope.$id);

            $scope.item.refreshRequired = false;
            initialiseModel();
            loadFormDefinition();
        } else {
            //console.log('DEBUG: EditFormController ctor: returning from nav', $scope.$id);

            resyncOnNavReturn();
        }

        if (areCreating && !$scope.model.typeId) {
            // NOTE: Need better error message
            showAlert('Attempting to create a resource without a typeId');
            return;
        }

        updateActionPanel();
        setFocus();
        setupPagerOptions();
        ///////////////////////////////////////////////////////////////////////
        // Implementation functions
        //

        function initialiseModel() {

            $scope.model = {
                id: parseInt($stateParams.eid, 10) || $stateParams.eid,
                typeId: parseInt($stateParams.eid, 10) || $stateParams.eid,
                formId: parseInt($stateParams.formId, 10) || $stateParams.formId,
                taskInfo: {
                    taskList: null,
                    taskId: parseInt($stateParams.taskId, 10) || $stateParams.taskId
                },
                forceGenerate: sp.stringToBoolean($stateParams.forceGenerate),     // force the form to be generated rather than using a default
                formControl: null,
                formData: null,
                onReturnFromChildCreateActions: [],
                onReturnFromChildUpdateActions: [],
                configContextMenu: {},
                inWizard: sp.stringToBoolean($stateParams.inWizard),                // are we part of a wizard?
                haveFetchedTasks: false,                                            // have we already fetched the tasks
                actionButtons: [],
                areCreating: areCreating,
                controlVisibilityModel: {
                    controlsVisibility: {},
                    visibilityCalcDependencies: {}
                }
            };

            switch ($state.current.name) {
                case 'createForm':
                case 'editForm':
                    $scope.model.formMode = spEditForm.formModes.edit;
                    break;
                case 'viewForm':
                    $scope.model.formMode = spEditForm.formModes.view;
                    break;
            }

            $scope.item.data = $scope.model;
        }

        function resyncOnNavReturn() {

            $scope.model = $scope.item.data;

            //
            // Run any pre-load processing (generally as the result of a return from a child navigate where something needs to be done on the parent
            //
            if ($scope.item.performOnReturnFromChildCreateActions === true) {
                _.map($scope.model.onReturnFromChildCreateActions, function (f) {
                    f($scope, $scope.model.formData);
                });
            }

            if ($scope.item.performOnReturnFromChildUpdateActions === true) {
                _.map($scope.model.onReturnFromChildUpdateActions, function (f) {
                    f($scope, $scope.model.formData);
                });
            }

            // This layout call is for when navigateToParent occurs
            if ($scope.model && $scope.model.formControl && $scope.model.formData) {
                doLayout();
            }

            // reset
            $scope.item.performOnReturnFromChildCreateActions = false;
            $scope.item.performOnReturnFromChildUpdateActions = false;
            $scope.model.onReturnFromChildCreateActions = [];
            $scope.model.onReturnFromChildUpdateActions = [];
        }

        // Note - the spinner handling is a bit crazy....


        // Get the definition of the form we are displaying form the server
        function loadFormDefinition() {
            var fetchFormFn;
            var fetchId;

            if ($scope.model.formId) {
                fetchFormFn = spEditForm.getFormDefinition;
                fetchId = $scope.model.formId;
            } else {
                if (areCreating) {
                    fetchFormFn = spEditForm.getFormForDefinition;
                    fetchId = $scope.model.id;
                } else {
                    fetchFormFn = spEditForm.getFormForInstance;
                    fetchId = $scope.model.id;
                }
            }

            showSpinner();

            fetchFormFn(fetchId, $scope.model.forceGenerate)
                .then(loadVisibilityCalculationDependencies)
                .then(loadForm, function (error) {
                    showAlert('An error occurred getting the form: ' + sp.result(error, 'data.Message'));
                    hideSpinner();
                })
                .then(updateTheme)
                .then(setHeaderIconAndStyle);
        }

        function updateVisibilityForAllControls() {
            var controlsWithCalculations = spFormControlVisibilityService.getControlsWithVisibilityCalculations($scope.model.formControl);
            var controlCalculations = {};

            if (!controlsWithCalculations || _.isEmpty(controlsWithCalculations)) {
                return;
            }

            // Get an object mapping the control id to its calculation
            _.forEach(controlsWithCalculations, function(c) {
                controlCalculations[c.id()] = c.visibilityCalculation;
            });

            onUpdateControlVisibility(controlCalculations);
        }

        // Load the visibility calculation dependencies if necessary
        function loadVisibilityCalculationDependencies(formControl) {            
            // Check if the form has any controls with visibility calculations
            var haveVisibilityCalculations = spFormControlVisibilityService.doesFormHaveControlsWithVisibilityCalculations(formControl);
            if (!haveVisibilityCalculations) {
                // No visibility calculation dependencies. Can return here
                return $q.when(formControl);
            }

            // Need to get visibility calculation dependencies
            return spEditForm.getFormVisCalcDependencies(formControl.id()).then(function (visibilityCalcDependencies) {            
                $scope.model.controlVisibilityModel.visibilityCalcDependencies = visibilityCalcDependencies;
                return formControl;
            });
        }

        function loadForm(formControl) {
            // check if user has createType access            
            if (areCreating) {
                var canCreate = hasCanCreateTypeAccess(formControl);

                // update toolbox
                $scope.item.actionPanelOptions.hasCreateAccess = canCreate;

                if (!canCreate) {
                    hideSpinner();
                    // todo : raise error?
                    return;
                }
            }

            $scope.model.formControl = formControl;

            // register the filter source controls along with the filtered controls
            spEditForm.registerRelationshipControlFilters($scope.model.formControl);
            spEditForm.buildRequestStrings($scope.requestStrings, $scope.model.formControl);
            $scope.model.configContextMenu = navigationBuilderProvider.buildConfigureContextMenu($scope.model.formControl);

            if (areCreating) {
                // Create the entity
                createEmptyForm($scope.model.typeId, formControl, parentItem).then(function () {
                    // Give the controls a chance to respond to the updated entity
                    $timeout(function () {
                        updateVisibilityForAllControls();
                        setInitialBookmark();
                        hideSpinner();
                        doLayout();

                        // set focus on first visible input if in edit mode and its not the mobile device
                        if (!$scope.isMobile && !$scope.isTablet && isEditMode()) {
                            spNgUtils.setFocusOnFirstVisibleInput();
                        }
                    }, 0);
                });
            }
            else {
                // fetch data from server
                if ($scope.requestStrings && $scope.requestStrings.length > 0) {
                    $scope.requestFormData($scope.model.id)
                        .then(setInitialBookmark)
                        .then(hideSpinner)
                        .then(doLayout);
                } else {
                    hideSpinner();
                }
            }
        }

        function filterSourceControlDataChanged(event, data) {
            $timeout(function () {
                var sourceControl = data.sourceControl;

                var filteredIds = spEditForm.getFilteredControlIds($scope.model.formControl, sourceControl);

                if (filteredIds && filteredIds.length) {
                    $scope.$broadcast('updateFilteredControlData', {filteredControlIds: filteredIds});
                }
            });
        }        

        function requestFormData(entityId) {
            showSpinner();

            if ($scope.fsShowHideControls) {
                return spEditForm.getFormDataAdvanced(entityId, $scope.requestStrings, $scope.model.formControl.id()).then(function (response) {
                    // Initialise all controls with calcs as initially visible
                    $scope.model.controlVisibilityModel.controlsVisibility = {};
                    var controlsWithCalcs = spFormControlVisibilityService.getControlsWithVisibilityCalculations($scope.model.formControl);
                    _.forEach(controlsWithCalcs, function (c) {
                        if (c) {
                            $scope.model.controlVisibilityModel.controlsVisibility[c.id()] = true;   
                        }                        
                    });
                    
                    // Update visibility for initially hidden controls
                    _.forEach(response.initiallyHiddenControls, function(controlId) {
                        $scope.model.controlVisibilityModel.controlsVisibility[controlId] = false;
                    });
                    var formData = response.formDataEntity;
                    $scope.model.formData = formData;
                    spEditForm.markAutoCardinalityOfAllRelationships($scope.model.formControl, $scope.model.formData);
                    hideSpinner();
                }, function(error) {
                    hideSpinner();
                    console.error('editForm.requestFormData error:', error);
                    if (error === 404) {
                        showAlert('Requested record does not exist or you do not have access to it.');
                    } else {
                        showAlert('An error occurred getting data: ' + error);
                    }

                    throw error;
                }).finally(hideSpinner);
            } else {
                return spEditForm.getFormData(entityId, $scope.requestStrings).then(function(formData) {
                    $scope.model.formData = formData;
                    spEditForm.markAutoCardinalityOfAllRelationships($scope.model.formControl, $scope.model.formData);
                    hideSpinner();
                }, function(error) {
                    hideSpinner();
                    console.error('editForm.requestFormData error:', error);
                    if (error === 404) {
                        showAlert('Requested record does not exist or you do not have access to it.');
                    } else {
                        showAlert('An error occurred getting data: ' + error);
                    }

                    throw error;
                }).finally(hideSpinner);
            }
        }

        function onMeasureArrangeComplete(event) {

            if ($scope && $scope.model && $scope.model.formControl) {
                if ($element) {
                    var titleElement = $element.find('> .form-title');
                    if (titleElement) {

                        var titleHeight = titleElement.outerHeight(true);
                        if (titleHeight && titleHeight > 0) {

                            var contentElement = $element.find('> sp-custom-edit-form');
                            if (contentElement) {

                                contentElement.css('height', 'calc(100% - ' + titleHeight + 'px)');
                            }
                        }
                    }
                }
            }
        }

        function saveIfEditing() {
            if (isEditMode()) {
                showSpinner();
                return doSave().catch(exceptionAlert).finally(hideSpinner);
            } else {
                return $q.when();
            }
        }

        function onUpdateControlVisibility(controlCalculations) {
            if (!$scope.model.formData || !controlCalculations || _.isEmpty(controlCalculations)) {
                return;
            }

            spFormControlVisibilityService.evaluateVisibilityCalculations($scope.model.formData, controlCalculations).then(function (controlVisibility) {
                if (!controlVisibility) {
                    return;
                }

                // Update model
                _.assign($scope.model.controlVisibilityModel.controlsVisibility, controlVisibility);
                // Notify controls
                spFormControlVisibilityService.updateControlsVisibility($scope, $scope.model.controlVisibilityModel.controlsVisibility);
            });
        }

        function formDataChanged() {
            if ($scope.model.formData && $scope.model.formControl) {
                if (!$scope.model.nameFieldEntity) {
                    spEditForm.getNameFieldEntity().then(       // this is here to get the id of name field. the fields in formData can only be identified by id (alias is undefined)
                        function (result) {
                            $scope.model.nameFieldEntity = result;
                            updateControlTitle();
                        },
                        function (error) {
                            console.error(error);
                        });
                }
                else {
                    updateControlTitle();
                }
                
                spFormControlVisibilityService.registerFormDataChangeListener(
                    $scope.model.formControl,
                    $scope.model.formData,
                    $scope.model.controlVisibilityModel.visibilityCalcDependencies,
                    onUpdateControlVisibility);

                updateToolbarAccessControlFieldValues();
                getFormActionQuickMenuDebounce();
            }
        }

        function possibleStateChanged() {

            var stateChange = false;
            var newFormId;
            var newTaskId;
            var newInWizard;

            // has the form id been changed in stateParams?
            if ($scope.model.formId && $stateParams.formId) {
                newFormId = parseInt($stateParams.formId, 10) || $stateParams.formId;
                if ($scope.model.formId !== newFormId) {
                    $scope.model.formId = newFormId;
                    stateChange = true;
                }
            }

            // check on the tasks and task id also
            if ($scope.model.taskInfo && $scope.model.taskInfo.taskId && $stateParams.taskId) {
                newTaskId = parseInt($stateParams.taskId, 10) || $stateParams.taskId;
                if ($scope.model.taskInfo.taskId !== newTaskId) {
                    $scope.model.taskInfo.taskId = newTaskId;
                    stateChange = true;
                }
            }

            // update the inWizard flag too
            if ($scope.model.inWizard && $stateParams.inWizard) {
                newInWizard = sp.stringToBoolean($stateParams.inWizard);
                if ($scope.model.inWizard !== newInWizard) {
                    $scope.model.inWizard = newInWizard;
                    stateChange = true;
                }
            }


            if (stateChange) {
                // must clear the cached form control first
                $scope.model.formControl = null;

                loadFormDefinition();
                fetchTasks();
            }
        }


        function configMenuDeleteEntity() {
            if (!spNavService.isFullEditMode) {
                return;
            }
            navigationBuilderProvider.removeNavItem($scope.model.formControl);
        }

        function configMenuModifyEntity() {
            if (!isViewMode() || !spNavService.isFullEditMode) {
                return;
            }

            //
            // To the designer as a child
            //
            $scope.item.data = null; // clear the form and data so it will be refreshed on the return

            var params = {
                formId: $scope.model.formControl.id(),  // this will probably go away as it is redundant
                mode: 'editForm'
            };

            spNavService.navigateToChildState(
                'formBuilder',
                params.formId, params);
        }

        // on mobile the tasks are rendered as a page, the page is only visible if there are tasks
        function setupPagerOptions() {

            if (!isEditMode()) {
                if (firstTime) {
                    // don't start getting the tasks till the rest of the stuff has processed.
                    $timeout(fetchTasks, 0).then(setupTaskAndActionsPage);
                } else {
                    setupTaskAndActionsPage();
                }
            }

            //TODO - shouldn't be setting page state on the item directly, use the item.data object (or spState.getPageItem())

            //console.log('DEBUG: setting state pagerOptions', JSON.stringify($scope.pagerOptions), $scope);
            $scope.item.pagerOptions = $scope.pagerOptions;

            //temp
            //$scope.$watch('item.pagerOptions.selectedPage', function (n, o) {
            //    console.log('DEBUG: selectedPage changed %o => %o', o, n);
            //});
            //end temp
        }

        //
        // Set up the edit buttons
        //
        function updateActionPanel() {
            $scope.item.actionPanelOptions = {
                areEditing: isEditMode(),
                areCreating: areCreating,
                toEditMode: $scope.toEditMode,
                back: back,
                cancel: cancel,
                save: save,
                savePlus: savePlus,
                formMode: $scope.model.formMode,
                formTitle: $scope.model.formTitle,
                actions: function () {
                } // do nothing for the moment
            };

            updateToolbarAccessControlFieldValues();
        }

        function setFocus() {
            $timeout(function () {
                // set focus on first visible input if in edit mode and its not the mobile device
                if (!$scope.isMobile && !$scope.isTablet && isEditMode()) {
                    spNgUtils.setFocusOnFirstVisibleInput();
                }
            }, 0);
        }

        function showAlert(msg, severity) {
            spAlertsService.addAlert(msg, {severity: severity || 'error', page: $state.current});
        }

        function exceptionAlert(exception) {
            spAlertsService.addAlert(exception.message, {severity: 'error', page: $state.current});
        }

        // Remove any alerts that were generated by this page.
        function clearPageAlerts() {
            spAlertsService.removeAlertsWhere({page: $state.current});
        }

        function getFormDataHistory() {
            if (!$scope.model.formData) {
                console.warn('formData is null');
                return null;
            }
            return $scope.model.formData.graph.history;
        }

        function setInitialBookmark() {
            $scope.model.initialBookmark = getFormDataHistory().addBookmark('editStart');
        }

        function revertToInitialBookmark() {
            getFormDataHistory().undoBookmark($scope.model.initialBookmark);
        }

        function isEditFormDirty() {
            if (isViewMode() || !$scope.model.formData) {
                return false;
            }

            //return $scope.model.formData.hasChangesRecursive();
            return getFormDataHistory().changedSinceBookmark($scope.model.initialBookmark);
        }

        //
        // Busy indicator
        //
        function showSpinner() {
            spNavService.middleLayoutBusy = true;
        }

        function hideSpinner() {
            spNavService.middleLayoutBusy = false;
        }

        //
        // Update the theming
        function updateTheme() {
            $scope.model.titleStyle = spThemeService.getTitleStyle();
        }

        // Update the form header icon and icon background
        function setHeaderIconAndStyle() {
            if (!$scope.model.formControl) {
                return;
            }
            
            var iconInfo = consoleIconService.getNavItemIconUrlAndBackColor($scope.model.formControl);
            if (iconInfo) {
                var iconUrl = iconInfo.iconUrl;
                if (iconUrl) {
                    $scope.model.headerIconUrl = iconUrl;

                    $scope.model.headerIconStyle = {};
                    if (iconInfo.iconBackgroundColor) {
                        $scope.model.headerIconStyle['background-color'] = sp.getCssColorFromARGBString(iconInfo.iconBackgroundColor);
                    }
                }
            }
        }

        //
        // Set up the taskInfo and add an additional page for it)
        //
        function fetchTasks() {
            $scope.model.haveFetchedTasks = true;

            return spUserTask.getPendingTasksForRecord($scope.model.id)
                .then(function (tasks) {
                    if (tasks && tasks.length) {
                        $scope.model.taskInfo.taskList = tasks;
                    }
                });
        }

        //
        // Actions available on this form
        //
        function getFormActionQuickMenu() {

            var formControl = $scope.model.formControl;
            if (!formControl) {
                return;
            }

            var formId = formControl.idP;
            var entityTypeId = sp.result(formControl, 'typeToEditWithForm.idP');
            var hostIds = [formControl.idP];
            var hostTypeIds = _.map(formControl.typesP, 'idP');

            var entityId = -1;
            var entityIds = [];
            var isCreated = sp.result($scope.model, 'formData._dataState') !== spEntity.DataStateEnum.Create;
            if (!areCreating || isCreated) {
                entityId = sp.result($scope.model, 'formData._id._id');
                entityIds.push(entityId);
            }

            $scope.model.actionButtons.length = 0;

            var actionRequest = {
                ids: entityIds,
                lastId: entityId || -1,
                cellId: -1,
                reportId: -1,
                formDataEntityId: -1,
                hostIds: hostIds,
                hostTypeIds: hostTypeIds,
                entityTypeId: entityTypeId,
                data: {},
                display: 'quickmenu',
                formId: formId
            };

            spActionsService.getConsoleActions(actionRequest).then(function (response) {
                if (response && response.actions) {
                    _.each(response.actions, createActionButton);
                }

                if ($scope.isMobile) {
                    setupTaskAndActionsPage();
                }
            }, function (error) {
                console.error('editForm.getFormActionQuickMenu error:', error);
            });
        }

        function createActionButton(action) {

            if (action.isbutton) {
                $scope.model.actionButtons.push({
                    name: action.name,
                    description: action.description,
                    nameshort: spActionsService.getShortName(action.name),
                    multiname: action.multiname,
                    emptyname: action.emptyname,
                    displayname: action.displayname || action.name,
                    displaynameshort: spActionsService.getShortName(action.displayname || action.name),
                    icon: action.icon,
                    order: action.order,
                    action: action,
                    disabled: !action.isenabled,
                    select: action.isselect,
                    multiselect: action.ismultiselect,
                    execute: function () {
                        var a = this.action;
                        var promise = $q.when;

                        // in create mode, if user has not touched any control then item is not marked as dirty
                        if (isEditFormDirty() || ($scope.model.areCreating)) {
                            promise = doSave;
                        }

                        promise().then(function() {
                            var id = sp.result($scope.model, 'formData.idP');
                            var ids = [id];
                            var ctx = getActionExecutionContext(a, ids);
                            spActionsService.executeAction(a, ctx);
                        }).catch(exceptionAlert);
                    }
                });
            }
        }

        function showActionButtons() {
            return !$scope.isMobile;
        }

        function refreshFormData() {
            var entityId = sp.result($scope.model, 'formData.idP');
            requestFormData(entityId)
                .then(setInitialBookmark)
                .then(function() {
                    $scope.$broadcast('refreshFormData');
                });
        }

        function getActionExecutionContext(action, ids) {
            return {
                scope: $scope,
                state: action.state,
                selectionEntityIds: ids,
                isEditMode: false, // board has no edit mode
                refreshDataCallback: refreshFormData
            };
        }

        function setupTaskAndActionsPage() {
            var tasks = sp.result($scope, 'model.taskInfo.taskList');
            var actionButtons = sp.result($scope, 'model.actionButtons');

            if ((tasks && tasks.length) || (actionButtons && actionButtons.length)) {

                if ($scope.pagerOptions.additionalPages.indexOf(taskPage) === -1) {
                    $scope.pagerOptions.additionalPages = [taskPage];
                }

                $scope.pagerOptions.haveTaskPage = true;
            } else {
                $scope.pagerOptions.haveTaskPage = false;
            }
        }

        function isViewMode() {
            return $scope.model.formMode === spEditForm.formModes.view;
        }

        function isEditMode() {
            return $scope.model.formMode === spEditForm.formModes.edit;
        }

        //
        // Flip to edit mode
        //
        function toEditMode() {            
            $scope.model.formMode = spEditForm.formModes.edit;
            updateActionPanel();
            setFocus();
            setInitialBookmark();
            if ($scope.model.formControl) {
                doLayout();
            }
        }

        //
        // Flip to view mode
        //
        function flipToViewMode() {
            $scope.model.formMode = spEditForm.formModes.view;

            updateActionPanel();

            if (!$scope.model.haveFetchedTasks)
                fetchTasks();
        }

        function navigateToViewMode() {
            var id = $scope.model.formData.eid().getNsAliasOrId();
            spNavService.navigateToSibling(
                'viewForm',
                id,
                $stateParams);
        }
        
        function returnToParent() {
            if (!spNavService.navigateToParent()) {
                console.log('editForm.js: unable to navigate back to the parent. Trying to switch to view mode.');

                if ($scope.item.actionPanelOptions.areEditing || $scope.item.actionPanelOptions.areCreating) {
                    navigateToViewMode();
                }
            }
        }

        function preSave(formControls, entity) {
            var promises = _.map(formControls, function (control) {
                if (control.handlePreSave) {
                    return control.handlePreSave(entity);
                } else {
                    return $q.when();
                }
            });

            return $q.all(promises).then(function () {
                return entity;
            });
        }

        function postSave() {
            //
            // default behaviour

            //
            // allow the parent to know what has been created
            //
            if (parentItem && parentItem.data) {
                spEditForm.setCreatedChildEntity(parentItem, $scope.model.formData);

                parentItem.data.updatedChildEntity = $scope.model.formData;
            }

            if (parentItem) {
                parentItem.performOnReturnFromChildCreateActions = true;
                parentItem.performOnReturnFromChildUpdateActions = true;
            }

            
            if (($state.params.returnToParent || $state.current.name === 'editForm') && !$scope.model.inWizard) {
                returnToParent();
            } else if ($state.current.name === 'createForm') {
                navigateToViewMode();
            } else {
                // reload the form data and flip to view mode
                spEditForm.buildRequestStrings($scope.requestStrings, $scope.model.formControl);

                $scope.requestFormData($scope.model.formData.idP)
                    .then(setInitialBookmark) // prevent the confirm dialog
                    .then(flipToViewMode);
            }
        }

        function postSavePlus() {
            //
            // allow the parent to know what has been created
            //
            if (parentItem && parentItem.data) {
                spEditForm.setCreatedChildEntity(parentItem, $scope.model.formData);

                parentItem.performOnReturnFromChildCreateActions = true;
            }
        }

        //
        // Save the form data, returning a promise. If there are validation errors or save errors throw an exception
        //
        function doSave() {
            var formControl = $scope.model.formToValidate || $scope.model.formControl;
            var formControls = spEditForm.getFormControls(formControl);

            // Only validate controls which are visible
            formControls = spFormControlVisibilityService.getVisibleControls(formControls, $scope.model.controlVisibilityModel.controlsVisibility);

            if (spEditForm.validateFormControls(formControls, $scope.model.formData)) {
                return preSave(formControls, $scope.model.formData)
                    .then(spEditForm.saveFormData)
                    .then(setInitialBookmark) // prevent the confirm dialog
                    .then(spNavService.setCacheMarker)
                    .then(clearPageAlerts)
                    .then(invalidateCache)
                    .then(doIconFormStuff);
            } else {
                // return an exception as a promise
                return $q.reject(new Error('Unable to continue, there are validation errors on the form.'));
            }
        }

        /////
        // Invalidate any breadcrumb navigation items that reference the item being saved.
        /////
        function invalidateCache() {
            var breadcrumb = spNavService.getBreadcrumb();

            var id = $scope.model.formData.id().toString();

            if (breadcrumb) {
                _.forEach(breadcrumb, function (navigationItem) {
                    var componentData = navigationItem.componentData;

                    if (componentData) {
                        _.forOwn(componentData, function (value, key) {
                            var keyArray = key.split(':');

                            if (keyArray && _.indexOf(keyArray, id) >= 0) {
                                delete componentData[key];
                            }
                        });
                    }
                });
            }
        }

        function doIconFormStuff() {
            // hack: to force a refreshed form to get latest info of the icon used on form.
            var form = $scope.model.formControl;

            if (!form) {
                return;
            }

            if (form.nsAlias === 'console:iconForm') {
                // remove form from server cache to force a refreshed form
                spEditForm.clearServerFormsCache();

                // clear client side form cache
                spEditForm.clearAllFormCaches();

                // refresh tree
                spNavService.refreshTree(true);
            }
        }

        function doLayout() {
            spFormControlVisibilityService.updateControlsVisibility($scope, $scope.model.controlVisibilityModel.controlsVisibility);

            if (!$scope.useFlexForm) {
                $timeout(function () {                    
                    spMeasureArrangeService.performLayout($scope.measureArrangeOptions.id);                    
                });
            }
        }

        //
        // create an empty form ready for editing
        //
        function createEmptyForm(typeId, formControl, currentParentItem) {
            //$scope.model.formData = spEditForm.createEntityWithDefaults($scope.model.typeId, formControl);
            return spEditForm.createEntityWithDefaults(typeId, formControl).then(function (tempFormData) {

                // in case of a new entity, presume we have modify and delete access.
                // Add these fields to formData so that it can be accessed by individual control if special handling is needed based on the access
                if (tempFormData) {
                    tempFormData.setField('core:canModify', true, spEntity.DataType.Bool);
                    tempFormData.setField('core:canDelete', true, spEntity.DataType.Bool);
                }

                $scope.model.formData = tempFormData;

                // If we are creating new entity in nested edit, autofill relationship value.
                if (currentParentItem && currentParentItem.relationshipId && currentParentItem.relatedEntityId) {
                    return spEditForm.autoFillRelationshipData(currentParentItem, tempFormData,
                        function () {
                            $scope.model.formData = tempFormData;
                        },
                        function (error) {
                            showAlert('An error occurred getting the form data: ' + sp.result(error, 'data.Message'));
                        });
                }
            });
        }

        //
        // Save	the	entity and flip back to view mode
        //
        function save() {
            showSpinner();
            doSave().then(postSave).catch(exceptionAlert).finally(hideSpinner);
        }

        //
        // Save	the new entity and clean the form ready for a new one
        //
        function savePlus() {
            doSave().then(function () {
                postSavePlus();
                createEmptyForm($scope.model.typeId, $scope.model.formControl, parentItem).then(function () {
                    updateVisibilityForAllControls();
                    setInitialBookmark();
                    hideSpinner();
                });
            }).catch(exceptionAlert);
        }

        //
        // User initiated cancel
        //
        function cancel() {
            // if we came in as a create or an edit, return back to the parent.
            if ($state.current.name === 'createForm' || $state.current.name === 'editForm' && !$scope.model.inWizard) {
                _.delay(() => $scope.$apply(returnToParent));
            } else {
                spNavService.navigateInternal().then(function() {
                    clearPageAlerts();
                    revertToInitialBookmark();
                    flipToViewMode();
                });
            }
        }

        //
        // Save	the	entity and flip back to view mode
        //
        function back() {
            returnToParent();
        }

        function hasCanCreateTypeAccess(formControl) {
            if ($scope.model.id && formControl) {
                var typeEntity = formControl.typeToEditWithForm;
                if (typeEntity && typeEntity.canCreateType) {
                    return true;
                }
            }
            return false;
        }

        // hide/show toolbar buttons based on access
        function updateToolbarAccessControlFieldValues() {
            var options = $scope.item.actionPanelOptions;
            // modify access
            options.hasModifyAccess = sp.result($scope.model, 'formData.canModify');

            // delete access
            options.hasDeleteAccess = sp.result($scope.model, 'formData.canDelete');
        }

        function updateControlTitle() {
            var entityName = $scope.model.formData.getField($scope.model.nameFieldEntity.idP);
            if (!sp.isNullOrUndefined(entityName) && entityName !== '') {
                $scope.model.formTitle = entityName;
            }
            else if (areCreating) {
                // if creating, model.id should be 'typeId'

                spEntityService.getEntity($scope.model.id, 'name', {hint: 'efCtrlTitle1', batch: true}).then(
                    function (type) {
                        $scope.model.formTitle = type.name;
                    },
                    function (error) {
                        console.error(error);
                    });
            }
            else {
                // in edit/view mode, model.id should be the id of the instance being edited/viewed

                spEntityService.getEntity($scope.model.id, 'isOfType.name', {hint: 'efCtrlTitle2', batch: true}).then(
                    function (type) {
                        $scope.model.formTitle = type.isOfType[0].name;
                    },
                    function (error) {
                        console.error(error);
                    });
            }
        }
    }

    /* @ngInject */
    function EditFormActionPanelController($scope, spNavService) {
        // we need to use a watch function to access the spNavService via a closure.
        $scope.watchFunction = function () {
            return sp.result(spNavService.getCurrentItem(), 'actionPanelOptions');
        };
        $scope.$watch('watchFunction()', function (options) {
            $scope.actionPanelOptions = options;
        });
    }
}());