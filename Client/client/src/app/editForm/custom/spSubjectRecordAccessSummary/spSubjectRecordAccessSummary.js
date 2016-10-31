// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('mod.app.editForm.customDirectives.spSubjectRecordAccessSummary', [
        'mod.app.accessControl.repository',
        'mod.app.editFormServices',
        'mod.common.spCachingCompile',
        'mod.common.alerts'])
           .directive('spSubjectRecordAccessSummary', function ($q, spEditForm, spCachingCompile, spAccessControlRepository, spAlertsService) {
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
                       isReadOnly: '=?'
                   },
                   link: function (scope, element) {

                       var loaded = false;

                       var templateWithTooltip = '<div class="ngCellText" ng-class="col.colIndex()" title="{{row.getProperty(col.field + \'tooltip\')}}">{{row.getProperty(col.field)}}</div>';

                       /////
                       // Control setup.
                       /////
                       var model = scope.model = {
                           search: { value:'' }, // current search text
                           gridLoaded: false, // grid protected by ng-if to work around issues of ng-grid not appearing at first if loaded inside a tab
                           objectSummaryData: [],
                           objectSummaryGrid: {
                               data: 'model.objectSummaryData',
                               virtualizationThreshold: 10000,      // by default this is 50 .. and if there are more than 50 rows in our grid then we only get to see the first six. sigh.
                               multiSelect: false,
                               enableSorting: true,
                               sortInfo: { fields: ['typename'], directions: ['asc'] },
                               enableColumnResize: true,
                               columnDefs: [
                                   {
                                       field: 'typename',
                                       displayName: 'Object',
                                       sortable: true,
                                       groupable: false,
                                       width: '15%'
                                   },
                                   {
                                       field: 'perms',
                                       displayName: 'Permissions',
                                       sortable: true,
                                       groupable: false,
                                       width: '15%',
                                       cellTemplate: templateWithTooltip
                                   },
                                   {
                                       field: 'scope',
                                       displayName: 'Scope',
                                       sortable: true,
                                       groupable: false,
                                       width: '15%',
                                       cellTemplate: templateWithTooltip
                                   },
                                   {
                                       field: 'subname',
                                       displayName: 'Granted To',
                                       sortable: true,
                                       groupable: false,
                                       width: '15%'
                                   }, 
                                   {
                                       field: 'reason',
                                       displayName: 'Reason',
                                       sortable: true,
                                       groupable: false,
                                       width: '40%',
                                       cellTemplate: templateWithTooltip
                                   }
                               ]
                           },
                           refreshOptions: {
                               refreshCallback: refreshReport,
                               isInDesign: false
                           },
                           busyIndicator: {
                               type: 'spinner',
                               text: 'Calculating...',
                               placement: 'element',
                               isBusy: false,
                               percent: 0
                           }
                       };

                       scope.$watch("model.search.value", applySearch);

                       scope.$watch("formData", function () {
                           if (scope.formData) {
                               model.subject = scope.formData;
                               model.subjectId = scope.formData.idP;
                               model.containerLoaded = loaded;

                               loadReport(model.subjectId);                               
                           }
                       });

                       function refreshReport() {
                           loadReport(model.subjectId);
                       }

                       scope.refreshReport = refreshReport;

                       function loadReport(subjectId) {
                           model.busyIndicator.isBusy = true;
                           scope.loadPromise = spAccessControlRepository.getTypeAccessReport(model.subjectId, model.showAdvanced).
                           then(function (response) {
                               var sorted = _.orderBy(response, 'typename');
                               var formatted = _.forEach(sorted, formatEntry);
                               model.fullData = formatted;
                               applySearch();
                               model.gridLoaded = true;
                               model.busyIndicator.isBusy = false;
                           }, function (errResponse) {
                               model.busyIndicator.isBusy = false;
                               model.objectSummaryData = [];
                               spAlertsService.addAlert('Object summary could not be loaded.' + errResponse, 'error');
                           });
                       }

                       function formatEntry(entry) {
                           entry.reasontooltip = entry.reason;
                           entry.permstooltip = entry.perms;
                           var scopeNames = {
                               'AllInstances':        {text:'All records',       tip:'The access rule grants access to all records of this object.'},
                               'SomeInstances':       {text:'Filtered records',  tip:'The access rule performs some filtering to determine which records access is granted to.'},
                               'PerUser':             {text:'Filtered per user', tip:'The access rule provides access to records related to the logged in user.'},
                               'SecuredRelationship': {text:'Via related',       tip:'Records of this object may be reached via security relationships to a record of another object.'}
                           };
                           if (scopeNames[entry.scope]) {
                               var info = scopeNames[entry.scope];
                               entry.scope = info.text;
                               entry.scopetooltip = info.tip;
                           }
                       }

                       function applySearch() {
                           if (!model.fullData)
                               return;
                           model.objectSummaryData = _.filter(model.fullData, searchPredicate);
                       }

                       function searchPredicate(record) {
                           var search = model.search.value;
                           if (!search)
                               return true;
                           search = search.toLowerCase();
                           var match = function(s) { return s.toLowerCase().indexOf(search) > -1; };
                           return match(record.subname) ||
                                match(record.typename) ||
                                match(record.scope) ||
                                match(record.perms) ||
                                match(record.reason);
                       } 

                       /////
                       // Control sizing and placement.
                       /////
                       scope.$on('gather', function (event, callback) {
                           callback(scope.formControl, scope.parentControl, element);
                       });

                       scope.$on('measureArrangeComplete', function (event) {
                           if (scope.formControl) {
                               loaded = true;
                               if (model.accessOptions) {
                                   model.accessOptions.containerLoaded = loaded;
                               }
                           }
                       });

                       var cachedLinkFunc = spCachingCompile.compile('editForm/custom/spSubjectRecordAccessSummary/spSubjectRecordAccessSummary.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());