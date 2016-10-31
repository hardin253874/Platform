// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spReportEntityQueryManager, spReportEntity */

(function () {
    'use strict';

    /**
    * Module implementing relationship advanced.
    * 
    * @module spReportPropertyController
    * @example
        
    Using the spReportPropertyController:
    p
    spReportPropertyDialog.showModalDialog(options).then(function(result) {
    });
       
    where reportOptions is available on the controller with the following properties:
        - reportId - {long} - the report Id
        - reportModel - {object} - the report object
           
    * 
    */

   
    angular.module('mod.app.reportProperty', [
        'mod.common.ui.spDialogService',
        'mod.common.alerts',
        'mod.common.ui.spEntityComboPicker',
        'mod.common.spEntityService',
        'mod.common.alerts',
        'app.editForm.spInlineRelationshipPicker',
        'sp.navService',
        'sp.app.settings'])

    .controller("spReportPropertyController", function ($scope,  $timeout, $uibModalInstance, options, spEntityService, spAlertsService, spNavService, spAppSettings) {

        $scope.isExistReport = false;
        $scope.dialogName = '';
        $scope.showAll = false;
        $scope.selectedEntityId = 0;
        $scope.disableOkButton = true;
        $scope.nameFieldId = 0;
        $scope.templateReportId = 0;
        $scope.definitionId = 0;
        $scope.typeId = 0;
        $scope.iconFileTypeId = 0;
        $scope.applicationTypeId = 0;
		$scope.applicationPickerReportId = 0;
        $scope.options = options || {};
        $scope.formMap = {};
        $scope.isCollapsed = true;
        ////get option text 
        $scope.getOptionsText = function () {
            if ($scope.isCollapsed === true) {
                $scope.imageUrl = 'assets/images/arrow_down.png';
                return 'Options';
            }
            else {
                $scope.imageUrl = 'assets/images/arrow_up.png';
                return 'Options';
            }
        };
        $scope.canShowAllTypes = spAppSettings.fullConfig;

        $scope.model = {
            reportId : 0,
            reportEntity : null,
            reportName: '',
            reportDescription: '',
            selectedEntityId: 0,
            selectedEntity: null,
            selectedEntities: null,
            navigationElementIconId: 0,
            navigationElementIcon: null,
            navigationElementIcons: null,
            applicationId: 0,
            application: null,
            applications: null,
            showAll: false,
            rootEntityType: null,
            defaultDisplayReport: false,
            defaultPickerReport: false,
            hideActionBar: false,
            hideReportHeader: false,
            visualSettingsOptions: {
                enableOnDesktop: true,
                enableOnTablet: false,
                enableOnMobile: false
            },
            reportForm: {},
            formOptions: { selectedEntityId: -2, selectedEntity: null, entities: [], showSelectOption: false, entityTypeId: -1 },
            styleOptions: {
                selectedEntityId: 0,
                selectedEntity: null,
                showSelectOption: false,
                entityTypeId: 'core:reportStyleEnum'
            },
            errors: [],
            showAdvanced: spAppSettings.fullConfig, // only full admin can see advanced properties (esp 'in app', and 'custom form')
            formatTabActiveByDefault: !spAppSettings.fullConfig
        };

        $scope.model.formOptions = { selectedEntityId: -2, selectedEntity: null, entities: [], showSelectOption: false };

        $scope.pickerOptions = {            
            selectedEntityId: $scope.model.selectedEntityId,
            selectedEntity: $scope.model.selectedEntity,
            selectedEntities: $scope.model.selectedEntities,
            multiSelect: false,
            pickerReportId: $scope.templateReportId,
            entityTypeId: $scope.definitionId,
            isDisabled: false
        };

        $scope.iconPickerOptions = {
            selectedEntityId: $scope.model.navigationElementIconId,
            selectedEntity: $scope.model.navigationElementIcon,
            selectedEntities: $scope.model.navigationElementIcons,
            multiSelect: false,
            pickerReportId: $scope.templateReportId,
            entityTypeId: $scope.iconFileTypeId > 0 ? $scope.iconFileTypeId : 'core:iconFileType'
        };

        $scope.applicationPickerOptions = {
            selectedEntityId: $scope.model.applicationId,
            selectedEntity: $scope.model.application,
            selectedEntities: $scope.model.applications,
            multiSelect: true,
            pickerReportId: $scope.applicationPickerReportId > 0 ? $scope.applicationPickerReportId : 'core:applicationsPickerReport',
            entityTypeId: $scope.applicationTypeId > 0 ? $scope.applicationTypeId : 'core:solution'
        };

        

        $scope.$watch('options', function() {
            
            if ($scope.options.reportId && $scope.options.reportEntity) {
                //load current report    
                $scope.model.reportId = $scope.options.reportId;
                $scope.model.reportEntity = $scope.options.reportEntity;

                $scope.model.reportName = $scope.model.reportEntity.getName();
                $scope.model.reportDescription = $scope.model.reportEntity.getDescription();
                $scope.model.hideActionBar = $scope.model.reportEntity.getEntity().hideActionBar;
                $scope.model.hideReportHeader = $scope.model.reportEntity.getEntity().hideReportHeader;
                $scope.model.visualSettingsOptions.enableOnDesktop = !$scope.model.reportEntity.getEntity().hideOnDesktop;
                $scope.model.visualSettingsOptions.enableOnTablet = !$scope.model.reportEntity.getEntity().hideOnTablet;
                $scope.model.visualSettingsOptions.enableOnMobile = !$scope.model.reportEntity.getEntity().hideOnMobile;
                $scope.model.reportForm = $scope.model.reportEntity.getEntity().resourceViewerConsoleForm ? $scope.model.reportEntity.getEntity().resourceViewerConsoleForm : $scope.getDefaultOption();
                $scope.model.selectedEntityId = $scope.model.reportEntity.getRootNode().getResourceReportNodeType().id();
                $scope.model.selectedEntity = $scope.model.reportEntity.getRootNode().getResourceReportNodeType();
                $scope.model.selectedEntities = [$scope.model.reportEntity.getRootNode().getResourceReportNodeType()];
                $scope.model.defaultDisplayReport = $scope.model.reportEntity.getEntity().isDefaultDisplayReportForTypes && $scope.model.reportEntity.getEntity().isDefaultDisplayReportForTypes.length > 0 ? true : false;
                $scope.model.defaultPickerReport = $scope.model.reportEntity.getEntity().isDefaultPickerReportForTypes && $scope.model.reportEntity.getEntity().isDefaultPickerReportForTypes.length > 0 ? true : false;
                $scope.model.navigationElementIconId = $scope.model.reportEntity.getEntity().navigationElementIcon ? $scope.model.reportEntity.getEntity().navigationElementIcon.id() : 0;
                $scope.model.navigationElementIcon = $scope.model.reportEntity.getEntity().navigationElementIcon ? $scope.model.reportEntity.getEntity().navigationElementIcon : null;
                $scope.model.navigationElementIcons = $scope.model.reportEntity.getEntity().navigationElementIcon ? [$scope.model.reportEntity.getEntity().navigationElementIcon] : null;
                $scope.model.applicationId = $scope.model.reportEntity.getEntity().inSolution ? $scope.model.reportEntity.getEntity().inSolution.id() : 0;
                $scope.model.applications = $scope.model.reportEntity.getEntity().inSolution ? [$scope.model.reportEntity.getEntity().inSolution] : null;
                $scope.model.styleOptions.selectedEntityId = $scope.model.reportEntity.getEntity().reportStyle ? $scope.model.reportEntity.getEntity().reportStyle.id() : 0;
                $scope.model.styleOptions.selectedEntity = $scope.model.reportEntity.getEntity().reportStyle ? $scope.model.reportEntity.getEntity().reportStyle : null;

                $scope.buildReportFormOptions($scope.model.reportForm);
                $scope.isExistReport = true;
                
            } else {
                $scope.model.reportName = "New Report";
                $scope.model.applicationId = $scope.options.solution && $scope.options.solution > 0 ? $scope.options.solution : spNavService.getCurrentApplicationId();
                spEntityService.getEntity($scope.model.applicationId, 'name,description').then(function (entity) {
                    if (entity) {                       
                        $scope.model.applications = [entity];
                        
                        $scope.applicationPickerOptions = {
                            selectedEntityId: $scope.model.applicationId,
                            selectedEntities: $scope.model.applications,
                            multiSelect: true,
                            pickerReportId: $scope.applicationPickerReportId,
                            entityTypeId: $scope.applicationTypeId
                        };
                    }
                });

                $scope.model.reportForm = $scope.getDefaultOption();
                
            }



            if ($scope.nameFieldId === 0 || $scope.templateReportId === 0) {


                var ids = ['core:name', 'core:templateReport', 'core:definition', 'core:type', 'core:iconFileType', 'core:solution', 'core:applicationsPickerReport'];

                spEntityService.getEntities(ids, 'name').then(function(entities) {
                    if (entities) {
                        $scope.nameFieldId = entities[0].id();
                        $scope.templateReportId = entities[1].id();
                        $scope.definitionId = entities[2].id();
                        $scope.typeId = entities[3].id();
                        $scope.iconFileTypeId = entities[4].id();
                        $scope.applicationTypeId = entities[5].id();
						$scope.applicationPickerReportId = entities[6] ? entities[6].id() : $scope.templateReportId;

                        $scope.pickerOptions = {                           
                            selectedEntityId: $scope.model.selectedEntityId,
                            selectedEntity: $scope.model.selectedEntity,
                            selectedEntities: $scope.model.selectedEntities,
                            multiSelect: false,
                            pickerReportId: null,
                            entityTypeId: $scope.definitionId,
                            isDisabled: $scope.isExistReport
                        };
                        
                        $scope.iconPickerOptions = {
                            selectedEntityId: $scope.model.navigationElementIconId,
                            selectedEntity: $scope.model.navigationElementIcon,
                            selectedEntities: $scope.model.navigationElementIcons,
                            multiSelect: false,
                            pickerReportId: $scope.templateReportId,
                            entityTypeId: $scope.iconFileTypeId
                        };
                        
                        $scope.applicationPickerOptions = {
                            selectedEntityId: $scope.model.applicationId,                            
                            selectedEntities: $scope.model.applications,
                            multiSelect: true,
                            pickerReportId: $scope.applicationPickerReportId,
                            entityTypeId: $scope.applicationTypeId
                        };
                    }
                });               
            }

            if ($scope.options.typeId) {                
                $scope.model.selectedEntityId = $scope.options.typeId;
                $scope.buildReportFormOptions($scope.model.reportForm);
            }

        });

        

        $scope.$watch('model.showAll', function () {
            if ($scope.model.showAll) {                
                $scope.pickerOptions = {
                   
                    selectedEntityId: $scope.model.selectedEntityId,
                    selectedEntity: $scope.model.selectedEntity,
                    selectedEntities: $scope.model.selectedEntities,
                    multiSelect: false,
                    pickerReportId: $scope.templateReportId,
                    entityTypeId: $scope.typeId > 0 ? $scope.typeId : 'core:type',
                    isDisabled: $scope.isExistReport
                };


            } else {                
                $scope.pickerOptions = {
                    
                    selectedEntityId: $scope.model.selectedEntityId,
                    selectedEntity: $scope.model.selectedEntity,
                    selectedEntities: $scope.model.selectedEntities,
                    multiSelect: false,
                    pickerReportId: $scope.templateReportId,
                    entityTypeId: $scope.definitionId > 0 ? $scope.definitionId : 'core:definition',
                    isDisabled: $scope.isExistReport
                };
            }
        });

        $scope.$watch('isExistReport', function() {
            $scope.dialogName = $scope.isExistReport === true ? "Report Properties" : "New Report";
        });

        $scope.$watch('rootEntityPickerOptions.selectedEntity', function () {
            if ($scope.rootEntityPickerOptions && $scope.rootEntityPickerOptions.selectedEntity) {
                $scope.model.selectedEntityId = $scope.rootEntityPickerOptions.selectedEntity.id();
            }
        });
               
        $scope.$watch('pickerOptions.selectedEntity', function () {
            if ($scope.pickerOptions && $scope.pickerOptions.selectedEntity) {
                $scope.model.selectedEntityId = $scope.pickerOptions.selectedEntity.id();
            }
        });
        
        $scope.$watch('iconPickerOptions.selectedEntity', function () {
            if ($scope.iconPickerOptions && $scope.iconPickerOptions.selectedEntity) {
                $scope.model.navigationElementIconId = $scope.iconPickerOptions.selectedEntity.id();
            }
        });
        

        $scope.$watch('model.reportName', function () {
            $scope.DisableOkButton();
        });

        $scope.$watch('model.selectedEntityId', function () {
            $scope.DisableOkButton();
            $scope.buildReportFormOptions($scope.model.reportForm);
        });

        $scope.buildReportStyleOptions = function() {
            
        };

        $scope.buildReportFormOptions = function (reportForm) {
            var defaultOption = $scope.getDefaultOption();
            if ($scope.model.selectedEntityId > 0) {

               
                var rq = 'name, {k:formsToEditType,defaultDisplayReport,defaultPickerReport,definitionUsedByReport}.{name, alias, description}';
                spEntityService.getEntity($scope.model.selectedEntityId, rq).then(function (type) {
                    var entities = [];
                    $scope.model.rootEntityType = type;
                    //the first report for an object that report should be marked as the default picker report and default display report automatically
                    if (type.definitionUsedByReport.length === 0) {
                        $scope.model.defaultDisplayReport = true;
                        $scope.model.defaultPickerReport = true;
                    } else if ($scope.model.reportEntity && $scope.model.reportEntity.getEntity()) {
                        if (type.defaultPickerReport && type.defaultPickerReport.id() === $scope.model.reportEntity.getEntity().id()) {
                            $scope.model.defaultPickerReport = true;
                        } else {
                            $scope.model.defaultPickerReport = false;
                        }

                        if (type.defaultDisplayReport && type.defaultDisplayReport.id() === $scope.model.reportEntity.getEntity().id()) {
                            $scope.model.defaultDisplayReport = true;
                        } else {
                            $scope.model.defaultDisplayReport = false;
                        }
                    } else {
                        $scope.model.defaultDisplayReport = false;
                        $scope.model.defaultPickerReport = false;
                    }


                    if (type.getFormsToEditType() && type.getFormsToEditType().length > 0) {
                        entities = type.getFormsToEditType();                      
                    }
                   
                    entities.splice(0, 0, defaultOption);
                                        
                    $scope.model.formOptions.selectedEntity = reportForm;
                    $scope.model.formOptions.selectedEntityId = reportForm.id();
                    $scope.model.formOptions.entities = entities;
                    $scope.pickerOptions.selectedEntities = [type];
                   
                });
            }
            else {
                $scope.model.formOptions = { selectedEntityId: -2, selectedEntity: null, entities: [defaultOption], showSelectOption: false };                
            }
        };
        
        $scope.$watch('model.formOptions.selectedEntityId', function () {
            if ($scope.model.formOptions.selectedEntity) {
                $scope.model.reportForm = $scope.model.formOptions.selectedEntity;
            }
        });

        
        $scope.$watch('pickerOptions.selectedEntities', function () {
            if ($scope.pickerOptions.selectedEntities) {
                $scope.model.selectedEntities = $scope.pickerOptions.selectedEntities;
                $scope.model.selectedEntity = $scope.pickerOptions.selectedEntities[0];
                $scope.model.selectedEntityId = $scope.model.selectedEntity.id();                
            }
        });
        
        $scope.$watch('applicationPickerOptions.selectedEntities', function () {

            if ($scope.applicationPickerOptions.selectedEntities && $scope.applicationPickerOptions.selectedEntities.length > 0) {
                $scope.model.applicationId = $scope.applicationPickerOptions.selectedEntities[0].id();
                $scope.model.applications = $scope.applicationPickerOptions.selectedEntities;
            } 
        });
        
        $scope.$watch('iconPickerOptions.selectedEntities', function () {
            if ($scope.iconPickerOptions.selectedEntities && $scope.iconPickerOptions.selectedEntities.length > 0) {
                $scope.model.navigationElementIconId = $scope.iconPickerOptions.selectedEntities[0].id();
                $scope.model.navigationElementIcon = $scope.iconPickerOptions.selectedEntities[0];
            } 
        });
        
        $scope.$watch('model.selectedEntityId', function () {
            $scope.DisableOkButton();
        });

        $scope.$watch('model.retReportId', function () {
            $scope.DisableOkButton();
        });

        $scope.DisableOkButton = function() {
            if ($scope.model.reportName && $scope.model.reportName.length > 0 && $scope.model.selectedEntityId && $scope.model.selectedEntityId > 0) {
                $scope.disableOkButton = false;
            } else {
                $scope.disableOkButton = true;
            }
        };

        // click ok button to return selected relationships
        $scope.ok = function () {
            $scope.model.errors = [];
            //disable the OK button to avoid double click ok button error
            $scope.disableOkButton = true;
            if ($scope.isExistReport && $scope.model.reportEntity.getEntity())
            {
                $scope.model.reportEntity.getEntity().name = $scope.model.reportName;
                $scope.model.reportEntity.getEntity().description = $scope.model.reportDescription;
                $scope.model.reportEntity.getEntity().hideActionBar = $scope.model.hideActionBar;
                $scope.model.reportEntity.getEntity().hideReportHeader = $scope.model.hideReportHeader;
                $scope.model.reportEntity.getEntity().hideOnDesktop = !$scope.model.visualSettingsOptions.enableOnDesktop;
                $scope.model.reportEntity.getEntity().hideOnTablet = !$scope.model.visualSettingsOptions.enableOnTablet;
                $scope.model.reportEntity.getEntity().hideOnMobile = !$scope.model.visualSettingsOptions.enableOnMobile;
                
                if ($scope.model.reportForm && $scope.model.reportForm.id() > 0) {
                    $scope.model.reportEntity.getEntity().resourceViewerConsoleForm = $scope.model.reportForm;
                } else {
                    $scope.model.reportEntity.getEntity().resourceViewerConsoleForm = null;
                }
                            
                //Set Applications
                var applications = sp.result($scope, 'applicationPickerOptions.selectedEntities');

                if (applications != null && applications.length > 0) {
                    $scope.model.reportEntity.getEntity().inSolution = applications[0];
                } else {
                    $scope.model.reportEntity.getEntity().inSolution = null;
                }
                

                if ($scope.model.styleOptions.selectedEntity && $scope.model.styleOptions.selectedEntity.id() > 0) {
                    $scope.model.reportEntity.getEntity().reportStyle = $scope.model.styleOptions.selectedEntity;
                } else {
                    $scope.model.reportEntity.getEntity().reportStyle = null;
                }
              
                //Set Navigation Element Icon
                var iconEntity = sp.result($scope, 'iconPickerOptions.selectedEntities.0');
                $scope.model.reportEntity.getEntity().navigationElementIcon = iconEntity || null;

               
                // Set default display and picker reports (admin only)
                if (spAppSettings.fullConfig && $scope.model.rootEntityType) {                    
                    if ($scope.model.defaultDisplayReport) {
                        //set the defaultDisplayReport property to current report                        
                        if (!$scope.model.reportEntity.getEntity().isDefaultDisplayReportForTypes) {
                            $scope.model.reportEntity.getEntity().registerRelationship('core:isDefaultDisplayReportForTypes');
                        }
                        $scope.model.reportEntity.getEntity().isDefaultDisplayReportForTypes.add($scope.model.rootEntityType.id());
                        
                    } else {
                        if ($scope.model.rootEntityType.defaultDisplayReport && $scope.model.rootEntityType.defaultDisplayReport.id() === $scope.model.reportEntity.getEntity().id()) {
                            //remove the defaultDisplayReport to null                           
                            $scope.model.reportEntity.getEntity().isDefaultDisplayReportForTypes = null;
                        }                        
                    }

                    if ($scope.model.defaultPickerReport) {
                        //set the defaultPickerReport property to current report                        
                        if (!$scope.model.reportEntity.getEntity().isDefaultDisplayReportForTypes) {
                            $scope.model.reportEntity.getEntity().registerRelationship('core:isDefaultDisplayReportForTypes');
                        }
                        $scope.model.reportEntity.getEntity().isDefaultPickerReportForTypes.add($scope.model.rootEntityType.id());
                       
                    } else {
                        if ($scope.model.rootEntityType.defaultPickerReport && $scope.model.rootEntityType.defaultPickerReport.id() === $scope.model.reportEntity.getEntity().id()) {                           
                            //remove the defaultPickerReport to null
                            $scope.model.reportEntity.getEntity().isDefaultPickerReportForTypes = null;                           
                        }
                    } 
                }
                
                var retResult = { reportId: $scope.model.reportId, reportEntity: $scope.model.reportEntity };
                $uibModalInstance.close(retResult);
                


            } else {
                $scope.createReportEntity();
            }       
        };

        // click cancel to return report builder
        $scope.cancel = function () {
            $uibModalInstance.close(null);
        };

        $scope.getFormOption = function (resourceViewerConsoleForm) {
            var idFunction = function () { return resourceViewerConsoleForm.id(); };
            var aliasFunction = function () { return resourceViewerConsoleForm.alias(); };
            var nameFunction = function () { return resourceViewerConsoleForm.getName(); };
            var descriptionFunction = function () { return resourceViewerConsoleForm.description(); };
            var entityFunction = function () { return null; };
            var formOption = { id: idFunction, alias: aliasFunction, getName: nameFunction, description: descriptionFunction, entity: entityFunction };
            return formOption;
        };

        $scope.getDefaultOption = function () {
            var idFunction = function () { return -1; };
            var aliasFunction = function () { return "default"; };
            var nameFunction = function () { return "[Default]"; };
            var descriptionFunction = function () { return "description"; };
            var entityFunction = function () { return null; };
            var defaultOption = { id: idFunction, alias: aliasFunction, getName: nameFunction, description: descriptionFunction, entity: entityFunction };
            return defaultOption;
        };
        
        $scope.createReportEntity = function () {
            var typeId = $scope.model.selectedEntityId;           
            var selectedEntityName = $scope.model.selectedEntity ? $scope.model.selectedEntity.name : 'Name';
            var rootEntity = spReportEntityQueryManager.createRootEntity(typeId);
            var reportColumns = spReportEntityQueryManager.createInitializeReportColumns(rootEntity, $scope.nameFieldId, selectedEntityName);
            var reportConditions = spReportEntityQueryManager.createInitializeReportCondition(rootEntity, $scope.nameFieldId, selectedEntityName);
            //Set Applications
            var applications = sp.result($scope, 'applicationPickerOptions.selectedEntities');
           
            //Set Navigation Element Icon
            var iconEntity = sp.result($scope, 'iconPickerOptions.selectedEntities.0');

            var report = spEntity.fromJSON({
                typeId: 'report',
                name: jsonString($scope.model.reportName),
                description: jsonString($scope.model.reportDescription),

                // Structure
                reportUsesDefinition: jsonLookup(typeId),
                rootNode: jsonLookup(rootEntity),
                reportColumns: jsonRelationship(reportColumns),
                hasConditions: jsonRelationship(reportConditions),

                // Appearance
                hideActionBar: $scope.model.hideActionBar,
                hideReportHeader: $scope.model.hideReportHeader,
                rollupOptionLabels: true,
                reportStyle: $scope.model.styleOptions && $scope.model.styleOptions.selectedEntity ? jsonLookup($scope.model.styleOptions.selectedEntity) : jsonLookup(),
                resourceViewerConsoleForm: ($scope.model.reportForm && $scope.model.reportForm.id && $scope.model.reportForm.id() > 0) ? jsonLookup($scope.model.reportForm) : jsonLookup(),

                // Presence
                'console:resourceInFolder': $scope.options.folder ? jsonRelationship([$scope.options.folder]) : jsonRelationship(),
                'core:inSolution': applications && applications.length > 0 ? jsonLookup(applications[0]) : jsonLookup(),
                'console:navigationElementIcon': iconEntity ? jsonLookup(iconEntity) : jsonLookup(),
                hideOnDesktop: !$scope.model.visualSettingsOptions.enableOnDesktop,
                hideOnTablet: !$scope.model.visualSettingsOptions.enableOnTablet,
                hideOnMobile: !$scope.model.visualSettingsOptions.enableOnMobile,
                isDefaultDisplayReportForTypes: jsonRelationship(),
                isDefaultPickerReportForTypes: jsonRelationship(),
                isPrivatelyOwned: !spAppSettings.publicByDefault
            });            

            if (!$scope.isExistReport) {                
                spEntityService.putEntity(report).then(function (id) {
                    var retResult = { reportId: id, report: report };

                    //after create current new report, update defaultDisplayReport and defaultPickerReport relationships later for security purpose
                    //User maybe does not have permission to update the root type. use try and catch to handle this error
                    var updateReport = false;
                    if ($scope.model.defaultDisplayReport || $scope.model.defaultPickerReport) {
                        updateReport = true;
                    }
                   
                    //set default display and picker report settings (admin only)
                    //only update report when the defaultDisplayReport and defaultPickerReport are checked.
                    if (updateReport && spAppSettings.fullConfig) {
                        
                        //get report entity
                        var rq = spReportEntity.makeReportRequest();
                        spEntityService.getEntity(id, rq).then(function (newReportEntity) {
                           
                            if ($scope.model.defaultDisplayReport) {
                                newReportEntity.isDefaultDisplayReportForTypes.add(typeId);                                
                            }
                            if ($scope.model.defaultPickerReport) {
                                newReportEntity.isDefaultPickerReportForTypes.add(typeId);                                
                            }
                           
                                                 
                            spEntityService.putEntity(newReportEntity).then(function (newReportId) {
                                retResult.reportId = newReportId;
                                retResult.report = newReportEntity;
                            }, function (error) {
                                var errorMessage = 'An error occurred update report defaultDisplayReport and defaultPickerReport relationships';
                                if (error && error.data) {
                                    errorMessage += ', ' + (error.data.ExceptionMessage || error.data.Message);
                                }
                                console.log('update report fail', errorMessage);
                            });


                        }, function (error) {
                            
                            var errorMessage = 'An error occurred getting the report';
                            if (error && error.data) {
                                errorMessage += ', ' + (error.data.ExceptionMessage || error.data.Message);
                            }
                            console.log('update report fail', errorMessage);                            
                        }).finally(function () {
                            $uibModalInstance.close(retResult);
                        });

                        
                    } else {
                        $uibModalInstance.close(retResult);
                    }
                                                     
                }, function (error) {
                    console.log(error);
                    $scope.disableOkButton = false;
                    addError('Fail to create report, ' + (error.data.ExceptionMessage || error.data.Message));                    
                })
                .finally(function() {

                });
            }

        };
        
        $scope.createRootEntity = function () {
            var rootEntity = spEntity.fromJSON({
                typeId: 'resourceReportNode',
                exactType: false,
                targetMustExist: false,
                resourceReportNodeType: jsonLookup($scope.model.selectedEntityId)
            });
            return rootEntity;
        };



        $scope.createReportColumns = function (rootEntity) {
            var idField = spEntity.fromJSON({
                typeId: 'reportColumn',
                columnDisplayOrder: 0,
                columnIsHidden: true,
                columnExpression: jsonLookup($scope.createIdExpression(rootEntity))
            });

            var nameField = spEntity.fromJSON({
                typeId: 'reportColumn',
                columnDisplayOrder: 1,
                columnIsHidden: false,
                columnExpression: jsonLookup($scope.CreateNameExpression(rootEntity))
            });

            return [idField, nameField];
        };

        $scope.createIdExpression = function (rootEntity) {
            return spEntity.fromJSON({
                typeId: 'idExpression',
                sourceNode: jsonLookup(rootEntity)
            }
            );
        };

        $scope.CreateNameExpression = function (rootEntity) {
            return spEntity.fromJSON({
                typeId: 'fieldExpression',
                sourceNode: jsonLookup(rootEntity),
                fieldExpressionField: jsonLookup($scope.nameFieldId)
            }
            );
        };

        $scope.createReportConditions = function (rootEntity) {
            return [spEntity.fromJSON({
                typeId: 'reportCondition',
                conditionDisplayOrder: 0,
                conditionIsHidden: false,
                conditionIsLocked: false,
                conditionExpression: jsonLookup($scope.CreateNameExpression(rootEntity))
            })];
        };

        // Handle onkey press events
        $scope.onKeyPress = function (event) {
            
            if (!event) {
                return;
            }

            var tagName = event.target && event.target.tagName ? event.target.tagName.toLowerCase() : '';
            //check the enter key
            //also the disableOkButton flag should be off and the target tag is not textarea.
            if (!$scope.disableOkButton && event.keyCode === 13 && tagName !== 'textarea') {
                $scope.ok();
            }
        };


        // Add an error
        function addError(errorMsg) {
            $scope.model.errors.push({ type: 'error', msg: errorMsg });
        }


    })
    .factory('spReportPropertyDialog', function (spDialogService) {
        // setup the dialog
        var exports = {
            
            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Report Properties',
                    keyboard: true,
                    backdropClick: true,
                    windowClass: 'modal reportpropertydialog-view',
                    templateUrl: 'reportBuilder/dialogs/reportProperty/reportProperty.tpl.html',
                    controller: 'spReportPropertyController',
                    resolve: {
                        options: function () {
                            return options;
                        }
                    }
                };

                if (defaultOverrides) {
                    angular.extend(dialogDefaults, defaultOverrides);
                }

                return spDialogService.showModalDialog(dialogDefaults);
            }          
        };

        return exports;
    });
}());