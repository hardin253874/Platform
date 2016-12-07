// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spTargetCampaignTargetsEditor', [
        'mod.common.spMobile',
        'mod.common.spEntityService',
        'mod.common.alerts',
        'mod.app.editFormServices',
        'mod.common.ui.spEditFormDialog',
        'sp.common.fieldValidator',
        'mod.app.formBuilder.services.spFormBuilderService',
        'spApps.reportServices',
        'mod.app.navigationProviders',
        'mod.common.spCachingCompile'])
           .directive('spTargetCampaignTargetsEditor', function ($q, $parse, $timeout, $location, $anchorScroll, $templateCache, spMobileContext, spEntityService, spAlertsService, spDialogService, spEditForm, spEditFormDialog, spFieldValidator, spFormBuilderService, spReportService, spNavService, spCachingCompile) {
               return {
                   restrict: 'AE',
                   replace: false,
                   transclude: false,
                   scope: {
                       formControl: '=',
                       parentControl: '=?',
                       formData: '=',
                       formMode: '=?',
                       isInTestMode: '=?',
                       isReadOnly: '=?',
                       isInDesign: '=?'
                   },
                   link: function (scope, element) {

                       var saving = false;
                       var reverse = null;
                       var surveyTakerCellTemplate = $templateCache.get('editForm/custom/spTargetCampaignTargetsEditor/targetsEditorSurveyTakerCell.tpl.html');

                       scope.isMobile = spMobileContext.isMobile;
                       scope.canModify = false;
                       scope.canAdd = false;
                       scope.canRemove = false;
                       scope.gridLoaded = false;
                       scope.containerLoaded = false;
                       scope.relationshipToRender = null;
                       scope.relTargetType = { id: -1, isReverse: false };
                       scope.relSurveyTaker = { id: -1, isReverse: false };
                       scope.relSurveyTakerDirection = { id: -1, isReverse: false };

                       scope.model = {
                           busy: {
                               type: 'spinner',
                               placement: 'element',
                               isBusy: false
                           },
                           search: {
                               value: '',
                               onSearchValueChanged: function() {
                                   scope.model.filteredRows = filter(scope.model.rows, scope.model.search.value || '');
                               }
                           },
                           rows: [],
                           filteredRows: [],
                           targetType: null,
                           surveyTaker: null,
                           surveyTakerDirection: null,
                           gridOptions: {
                               data: 'model.filteredRows',
                               targetvirtualizationThreshold: 10000,
                               multiSelect: true,
                               enableSorting: true,
                               sortInfo: { fields: ['name'], directions: ['asc'] },
                               enableColumnResize: true,
                               selectedItems: [],
                               columnDefs: [
                                   {
                                       field: 'name',
                                       displayName: 'Object',
                                       sortable: true,
                                       groupable: false,
                                       enableCellEdit: false
                                   },
                                   {
                                       field: 'description',
                                       displayName: 'Description',
                                       sortable: false,
                                       groupable: false,
                                       enableCellEdit: false
                                   },
                                   {
                                       displayName: 'Survey taker',
                                       sortable: false,
                                       groupable: false,
                                       enableCellEdit: false,
                                       cellTemplate: surveyTakerCellTemplate
                                   }
                               ],
                               afterSelectionChange: function (row) {
                                   scope.canRemove = row && scope.canModify && scope.model.gridOptions.selectedItems.length;
                               }
                           }
                       };

                       scope.$watch("formControl", function () {
                           if (scope.formControl) {
                               scope.relationshipToRender = scope.formControl.relationshipToRender;
                               loadRelationships().then(watchFormData);
                           }
                       });

                       var loadDebounced = _.debounce(load, 100);
                       scope.$watch("formData", function () {
                           if (scope.formData) {
                               scope.canModify = scope.formData.canModify || false;

                               if (scope.formData.dataState === spEntity.DataStateEnum.Create) {
                                   var relContainer = scope.formData.registerRelationship(relId());
                                   relContainer.autoCardinality();
                                   setRelationship([]);
                                   scope.model.rows = getRelationship();
                                   loadGrid();
                               } else {
                                   loadDebounced();
                               }
                           }
                       });

                       scope.$watch("model.rows", function() {
                           scope.model.filteredRows = filter(scope.model.rows, scope.model.search.value || '');
                           //safeApply(scope, function () {
                           //    var grid = sp.result(scope.model.gridOptions, 'ngGrid');
                           //    if (grid && grid.refreshRowData) {
                           //        grid.refreshRowData();
                           //    }
                           //});
                       });

                       scope.add = function() {
                           if (scope.canModify && !saving) {
                               if (scope.model.targetType && scope.model.targetType.idP > 0) {
                                   spEntityService.getEntity(scope.model.targetType.idP, 'defaultPickerReport.id', {
                                       batch: 'true',
                                       hint: 'targets-defaultPickerReport'
                                   }).then(function(type) {
                                       var reportId = sp.result(type, 'defaultPickerReport.id');
                                       if (reportId) {
                                           return reportId;
                                       } else {
                                           return spEntityService.getEntity('core:templateReport', 'name', {
                                               hint: 'targets-templateReportId',
                                               batch: true
                                           }).then(function (report) {
                                               return sp.result(report, 'idP');
                                           });
                                       }
                                   }).then(openPickerDialog);
                               }
                           }
                       };

                       scope.remove = function () {
                           if (scope.canModify && !saving) {
                               var campaignTargets = getRelationship();

                               _.each(scope.model.gridOptions.selectedItems, function (item) {
                                   campaignTargets.remove(item);
                               });

                               scope.model.gridOptions.selectedItems.length = 0;
                               scope.model.rows = getRelationship();
                               scope.canRemove = false;

                               saveFormDataInViewMode();
                           }
                       };

                       function safeApply(s, fn) {
                           if (!s.$root.$$phase) { s.$apply(fn); } else { fn(); }
                       }

                       function relId() {
                           return { id: scope.relationshipToRender.eid(), isReverse: false };
                       }

                       function getRelationship() {
                           var rel = null;
                           if (scope.relationshipToRender) {
                               rel = scope.formData.getRelationship(relId());
                           }
                           return rel;
                       }

                       function setRelationship(value) {
                           if (scope.relationshipToRender) {
                               scope.formData.setRelationship(relId(), value);
                           }
                       }

                       function getSurveyTakerQuery() {
                           var qd = (scope.model.surveyTakerDirection && (scope.model.surveyTakerDirection.idP === reverse.idP)) ? '-' : '';
                           var qr = (scope.model.surveyTaker && (scope.model.surveyTaker.idP > 0)) ? ', ' + qd + '#' + scope.model.surveyTaker.idP + '.{name}' : '';
                           return qr;
                       }
                       
                       function loadGrid() {
                           // ng-grid doesn't go nicely with show/hide from tabs. but it's fine with ng-if.
                           $timeout(function () {
                               scope.gridLoaded = true;
                           }, 0);
                       }

                       function loadRelationships() {
                           var promises = [];

                           // load the reverse direction enum for comparisons
                           if (!reverse) {
                               promises.push(spEntityService.getEntity('core:reverse', 'name, alias').then(function (r) { reverse = r; }));
                           }

                           var fn = function (relObject, alias, handleResult) {
                               if (!relObject || relObject.id < 0) {
                                   return spEntityService.getEntity(alias, 'name').then(function(rel) {
                                       if (rel) {
                                           handleResult({ id: rel.idP, isReverse: false });
                                       }
                                   });
                               }
                               return $q.when();
                           };

                           promises.push(fn(scope.relTargetType, "core:campaignTarget", function (r) { scope.relTargetType = r; }));
                           promises.push(fn(scope.relSurveyTaker, "core:campaignTargetRelationship", function (r) { scope.relSurveyTaker = r; }));
                           promises.push(fn(scope.relSurveyTakerDirection, "core:campaignTargetRelationshipDirection", function (r) { scope.relSurveyTakerDirection = r; }));

                           return $q.all(promises);
                       }

                       function load() {
                           var preq = $q.when;
                           
                           // direction may not be loaded if in view mode
                           if (!scope.model.surveyTakerDirection) {
                               preq = _.partial(spEntityService.getEntity, scope.formData.idP, 'name, campaignTargetRelationshipDirection.alias');
                           }

                           return preq().then(function (d) {
                               if (d) {
                                   scope.model.surveyTakerDirection = d.campaignTargetRelationshipDirection;
                               }

                               var qr = getSurveyTakerQuery();
                               var qf = 'name, campaignTargetTargets.{name, description, isOfType.alias' + qr + '}';

                               scope.canRemove = false;
                               scope.model.busy.isBusy = true;
                               scope.model.rows = [];

                               return spEntityService.getEntity(scope.formData.idP, qf).then(function (t) {

                                   var incoming = _.filter(t.campaignTargetTargets, function (target) {
                                       if (!scope.model.targetType) {
                                           return false;
                                       }
                                       return _.includes(_.map(target.isOfType, 'idP'), scope.model.targetType.idP);
                                   });

                                   // attach to the correct graph on these entities for history and change management
                                   _.each(incoming, function (target) {
                                       target._setGraph(scope.formData._graph);
                                   });

                                   setRelationship([]);
                                   setRelationship(incoming);

                                   scope.model.rows = getRelationship();
                               }).finally(function () {
                                   scope.model.busy.isBusy = false;
                                   loadGrid();
                               });
                           });
                       }

                       function watchFormData() {
                           var str = function(relObjectName) { return 'formData && ' + relObjectName + ' && (formData.getRelationshipContainer(' + relObjectName + ').changeId + "|" + formData.id())'; };

                           scope.$watch(str('relTargetType'), function () {
                               var data = [];
                               if (scope.formData && scope.relTargetType && scope.relTargetType.id > 0) {
                                   data = scope.formData.getRelationship(scope.relTargetType);
                               }
                               scope.model.targetType = _.first(data) || null;

                               scope.canAdd = scope.model.targetType && scope.model.surveyTaker;
                               safeApply(scope, function () {
                                   scope.model.gridOptions.columnDefs[0].displayName = sp.result(scope.model.targetType, 'name') || 'Object';
                                   var grid = sp.result(scope.model.gridOptions, 'ngGrid');
                                   if (grid && grid.buildColumns) {
                                       grid.buildColumns();
                                   }
                               });
                           });

                           scope.$watch(str('relSurveyTaker'), function () {
                               var data = [];
                               if (scope.formData && scope.relSurveyTaker && scope.relSurveyTaker.id > 0) {
                                   data = scope.formData.getRelationship(scope.relSurveyTaker);
                               }
                               scope.model.surveyTaker = _.first(data) || null;

                               scope.canAdd = scope.model.targetType && scope.model.surveyTaker;
                               loadDebounced();
                           });

                           scope.$watch(str('relSurveyTakerDirection'), function() {
                               var data = [];
                               if (scope.formData && scope.relSurveyTakerDirection && scope.relSurveyTakerDirection.id > 0) {
                                   data = scope.formData.getRelationship(scope.relSurveyTakerDirection);
                               }
                               scope.model.surveyTakerDirection = _.first(data) || null;

                               loadDebounced();
                           });
                       }

                       function saveFormDataInViewMode() {
                           if ((scope.formMode === spEditForm.formModes.view) && (scope.isReadOnly === true) && (scope.canModify === true)) {
                               saving = true;
                               spEditForm.saveFormData(scope.formData).then(load, function (error) {
                                   spAlertsService.addAlert(spEditForm.formatSaveErrorMessage(error), { severity: spAlertsService.sev.Error });
                               }).finally(function () {
                                   saving = false;
                               });
                           }
                       }

                       function filter(rows, search) {
                           var lowerSearch = search.toLowerCase().trim();
                           var filtered = rows;

                           if (lowerSearch.length) {
                               // filter by survey taker name
                               filtered = _.filter(filtered, function (item) {
                                   var takerName = scope.getSurveyTakerName(item);
                                   return _.includes(item.name.toLowerCase(), lowerSearch) ||
                                       _.includes(item.description.toLowerCase(), lowerSearch) ||
                                       _.includes(takerName.toLowerCase(), lowerSearch);
                               });
                           }

                           return filtered;
                       }

                       function openPickerDialog(reportId) {
                           var modalInstanceCtrl = ['$scope', '$uibModalInstance', 'outerOptions', function ($scope, $uibModalInstance, outerOptions) {
                               $scope.model = {
                                   reportOptions: outerOptions
                               };
                               $scope.ok = function () {
                                   $scope.isModalOpened = false;
                                   $uibModalInstance.close($scope.model.reportOptions);
                               };
                               $scope.$on('spReportEventGridDoubleClicked', function (event) {
                                   event.stopPropagation();
                                   $scope.ok();
                               });
                               $scope.cancel = function () {
                                   $scope.isModalOpened = false;
                                   $uibModalInstance.dismiss('cancel');
                               };
                               $scope.model.reportOptions.cancelDialog = $scope.cancel;
                               $scope.isModalOpened = true;
                           }];

                           var existing = scope.model.rows;

                           var targetPickerOptions = {
                               reportId: reportId,
                               entityTypeId: sp.result(scope.model, 'targetType.idP'),
                               multiSelect: true,
                               isEditMode: false,
                               newButtonInfo: {},
                               isInPicker: true,
                               isMobile: scope.isMobile,
                               fastRun: true
                           };

                           var defaults = {
                               templateUrl: 'entityPickers/entityCompositePicker/spEntityCompositePickerModal.tpl.html',
                               controller: modalInstanceCtrl,
                               windowClass: 'modal inlineRelationPickerDialog',
                               resolve: {
                                   outerOptions: function () {
                                       return targetPickerOptions;
                                   }
                               }
                           };

                           var options = {};

                           var qr = getSurveyTakerQuery();
                           var qf = 'name, description, isOfType.alias' + qr ;

                           spDialogService.showDialog(defaults, options).then(function (result) {
                               if (targetPickerOptions.selectedItems) {
                                   // discover the newly added
                                   var added = _(targetPickerOptions.selectedItems).differenceWith(existing, function (s, e) {
                                       return s.eid === e.idP;
                                   }).map('eid').value();

                                   // insert all the added items
                                   if (added.length) {
                                       spEntityService.getEntities(added, qf).then(function (addedEntities) {
                                           var campaignTargets = getRelationship();

                                           _.each(addedEntities, function (item) {
                                               campaignTargets.add(item);
                                           });

                                           scope.model.gridOptions.selectedItems.length = 0;
                                           scope.model.rows = getRelationship();
                                           scope.canRemove = false;
                                           
                                           saveFormDataInViewMode();
                                       });
                                   }
                               }
                           });
                       }

                       scope.getSurveyTakerName = function (target) {
                           var name = '';

                           if (target && scope.model.surveyTaker) {
                               var isReverse = (scope.model.surveyTakerDirection && (scope.model.surveyTakerDirection.idP === reverse.idP)) === true;
                               var rel = { id: scope.model.surveyTaker.eid(), isReverse: isReverse };
                               var e = target.getRelationship(rel);
                               if (e) {
                                   if (e.length > 1) {
                                       name = _.compact(_.map(e, 'name')).join(', ');
                                   } else {
                                       var taker = _.first(e) || null;
                                       if (taker) {
                                           name = taker.name;
                                       }
                                   }
                               }
                           }

                           return name;
                       };

                       /////
                       // Control sizing and placement.
                       /////
                       scope.$on('gather', function (event, callback) {
                           callback(scope.formControl, scope.parentControl, element);
                       });

                       scope.$on('measureArrangeComplete', function (event) {
                           scope.containerLoaded = true;
                       });

                       var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spTargetCampaignTargetsEditor/spTargetCampaignTargetsEditor.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());