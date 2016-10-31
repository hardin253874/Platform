// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    'use strict';
    
    angular.module('mod.app.formBuilder.directives.spFormBuilderToolboxCustomControls', [
        'mod.app.formBuilder.services.spFormBuilderService',
        'mod.app.formBuilder.factories.FieldContainer',
        'mod.common.spEntityService',
        'mod.common.spCachingCompile'])
           .directive('spFormBuilderToolboxCustomControls', function ($q, $timeout, spFormBuilderService, spEntityService, FieldContainer, spCachingCompile) {
               return {
                   restrict: 'AE',
                   replace: false,
                   transclude: false,
                   scope: {
                   },
                   link: function (scope, element) {
                       scope.model = {};
                       scope.customControlsLoading = true;
                       scope.customControlsGroups = [];

                       scope.spFormBuilderService = spFormBuilderService;

                       function dragStart() {
                           spFormBuilderService.isDragging = true;
                       }
                       
                       function dragEnd() {
                           spFormBuilderService.isDragging = false;
                           spFormBuilderService.destroyInsertIndicator();
                       }

                       function getControl(state) {
                           var json = {
                               typeId: state.type,
                               name: state.name
                           };

                           if (state.relationship) {
                               json['console:relationshipToRender'] = state.relationship;
                               if (state.relationship.name) {
                                   json.name = state.relationship.name;
                               }
                           }

                           if (state.verticalMode) {
                               json['console:renderingVerticalResizeMode'] = state.verticalMode;
                           }

                           if (state.horizontalMode) {
                               json['console:renderingHorizontalResizeMode'] = state.horizontalMode;
                           }

                           return $q.when(spEntity.fromJSON(json));
                       }

                       function load() {
                           var baseId = sp.result(spFormBuilderService.definition, 'idP');
                           if (baseId) {
                               scope.customControlsLoading = true;
                               scope.customControlsGroups.length = 0;

                               spEntityService.getEntity(baseId, 'name, inherits*.{name}').then(function (baseType) {
                                   var types = spUtils.walkGraph(function (t) { return t.inherits; }, baseType);
                                   var baseTypeIds = _.map(types, 'idP');

                                   var renderControlQuery = 'alias,name,description,console:customRenderControlIcon,' +
                                       'console:customRenderControlForType.{alias,name,description},' +
                                       'console:customRenderControlForRelationship.{alias,name,description,cardinality.{alias,name}},' +
                                       'console:customRenderControlDefaultHorizontalResizeMode.{alias,name},' +
                                       'console:customRenderControlDefaultVerticalResizeMode.{alias,name}';

                                   var renderControlFilter = '';
                                   _.forEach(baseTypeIds, function(id) {
                                       renderControlFilter += 'id([For type])=' + id + ' or ';
                                   });

                                   renderControlFilter = _.trimEnd(renderControlFilter, ' or ');

                                   spEntityService.getEntitiesOfType('console:customRenderControlType', renderControlQuery, { filter: renderControlFilter }).then(function (response) {
                                       if (response) {

                                           var groups = _.toPairs(_.groupBy(response, function (entity) {
                                               return sp.result(entity.customRenderControlForType, 'name');
                                           }));

                                           scope.customControlsGroups = _.map(groups, function (group) {
                                               return {
                                                   name: group[0] || 'Default',
                                                   hidden: true,
                                                   customControls: getCustomControls(group[1])
                                               };
                                           });
                                       }
                                   }).finally(function () {
                                       scope.customControlsLoading = false;
                                       scope.$emit('toolboxGroupVisibilityChanged', { groupName: 'Custom', hidden: scope.customControlsGroups.length <= 0 });
                                   });
                               });
                           }
                       }

                       scope.$watch('spFormBuilderService.form', function (newVal, oldVal) {
                           if (newVal === oldVal || !spFormBuilderService.definition) {
                               return;
                           }

                           load();
                       });

                       function getCustomControls(controls) {
                           return _.map(controls, function (entity) {

                               var fieldContainer = null;

                               try {
                                   var renderForType = new spResource.Type(entity.customRenderControlForType);

                                   if (entity.customRenderControlForRelationship) {
                                       var renderForRelationship = new spResource.Relationship(entity.customRenderControlForRelationship, renderForType, false);
                                       fieldContainer = new FieldContainer(renderForRelationship._relEntity.idP, renderForRelationship, FieldContainer.containerType.relationship);
                                   }
                               } finally {
                                   if (!fieldContainer) {
                                       fieldContainer = {};
                                   }

                                   fieldContainer.customInfo = {
                                       name: entity.name,
                                       description: entity.description,
                                       type: entity.nsAliasOrId,
                                       icon: entity.customRenderControlIcon,
                                       relationship: entity.customRenderControlForRelationship,
                                       verticalMode: entity.customRenderControlDefaultVerticalResizeMode,
                                       horizontalMode: entity.customRenderControlDefaultHorizontalResizeMode
                                   };
                                   fieldContainer.dragOptions = {
                                       onDragStart: function() {
                                           dragStart();
                                       },
                                       onDragEnd: function() {
                                           dragEnd();
                                       }
                                   };
                                   fieldContainer.getRenderControlInstance = function() {
                                       return getControl(this.customInfo);
                                   };

                                   return fieldContainer;
                               }
                           });
                       }

                       var cachedLinkFunc = spCachingCompile.compile('formBuilder/directives/spFormBuilderToolbox/directives/spFormBuilderToolboxCustomControls/spFormBuilderToolboxCustomControls.tpl.html');
                       cachedLinkFunc(scope, function (clone) {
                           element.append(clone);
                       });
                   }
               };
           });
}());