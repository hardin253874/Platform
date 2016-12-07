// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing relationship check box picker.
    * Displays one checkbox for each relationship allowing multiple relationships to be selected.
    * 
    * @module spRelationshipPicker
    * @example
        
    Using the spRelationshipPicker:
    
   spRelationshipDialog.showModalDialog(relationshipPickerOptions).then(function (result) {});
       
    where relationshipOptions is available on the controller with the following properties:
        - selectedNode - {object} - the selected entitytype node to load all relationships for current entity type, this is used for report Builder Only
        - entityTypeId - long - the entitytypeid - this is used for Form builder
        - selectedRelationshipIds = Array{long} - this is used for Form builder
    
    
    where the result output is available on the controller with the following properties
        - retAddedRelationships - Array{object}, this is used for report builder only
        - retRemovedRelationships - Array{object}, this is uesed for report builder only

        - retRelationshipIds - Array{long} the return Relationship Ids -  this is used for Form Builder
    Note: you only need to specify entityTypeId or entities.
    * 
    */
    angular.module('mod.common.ui.spRelationshipPicker', [
        'ui.bootstrap',
        'mod.common.spEntityService',
        'sp.navService',
        'mod.common.alerts',
        'mod.common.ui.spEntityComboPicker',
        'mod.ui.spTreeviewManager',
        'sp.app.settings',
        'mod.common.ui.spReportBuilder.spCustomJoinExpression',
        'mod.featureSwitch'])
        .controller("spRelationshipPickerController", function ($scope, $uibModalInstance, options, spAlertsService, spEntityService,
                                                             spNavService, spTreeviewManager, spAppSettings, rnFeatureSwitch) {
         $scope.entityTypeId = null;
         $scope.entityTypeName = '';
         $scope.selectedNode = null;
         $scope.currentSolutionId = 0;
         $scope.searchText = '';
         $scope.nameFieldId = 0;
         $scope.relationships = [];
         $scope.duplicateRelationships = [];
         $scope.derivedResources = [];
         $scope.filteredRelationships = [];
         $scope.filteredDerivedResources = [];                          
         $scope.retRelationship = null;
         $scope.relRelationships = [];
         $scope.retRemoveRelationships = [];

         $scope.showButton = false;
         $scope.hideAdvTab = true;
         $scope.isAdvCollapsed = true;
         $scope.postBack = false;
         $scope.getFields = false;
         $scope.showCustomJoin = rnFeatureSwitch.isFeatureOn('customJoins');

         $scope.selectedRelationshipIds = [];
         $scope.retRelationshipIds = [];

         // Selected definition
        

         $scope.model = {
             relationshipType: 'Relationships',
             relationshipTypes: ['Relationships', 'Lookups', 'Choicefields', 'All'],
             mode: 'View',
             modes: ['View', 'Edit'],
             searchText: '',
             addName : true,
             showHidden : false,
             solutionPickerOptions: { selectedEntityId: 0, selectedEntity: null, entities: null },
             gridBusyIndicator: {
                 type: 'spinner',
                 placement: 'element'
             },
             customTypePickerOptions: {
                 selectedEntity: null,
                 selectedEntities: null,
                 multiSelect: false,
                 pickerReportId: 'console:userTypesReport',
                 entityTypeId: 'core:type',
                 isDisabled: $scope.isExistReport
             },
             customJoinScript: '@parent.Name = Name',
             customTypeScriptOptions: {
                 childTypeId: 0,
                 parentTypeId: 0,
                 disabled: true
             }
         };

         // Watch reltionship Solution option to filte relationship by Appliction
         $scope.$watch('model.solutionPickerOptions.selectedEntityId', function () {
             if ($scope.model.solutionPickerOptions.selectedEntityId) {
                 //load filtered relationships
                 $scope.filteredRelationships = _.filter($scope.relationships, function (rel) {
                     return (
                                (rel.relationshipType.toLowerCase() === $scope.model.relationshipType.toLowerCase() || $scope.model.relationshipType === "All") &&
                                $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, rel.solutions) &&
                                $scope.searchRelationshipText(rel.relationshipName, rel.resourceName)
                            );
                 });
                 $scope.filteredDerivedResources = _.filter($scope.derivedResources, function (dev) {
                     return (
                                 $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, dev.solutions) &&
                                 $scope.searchRelationshipText(dev.relationshipName, dev.resourceName)
                     );
                 });
             }
         });
         
         // Watch custom type picker option
         $scope.$watch('model.customTypePickerOptions.selectedEntities', function () {
             var model = $scope.model;
             var typeId = sp.result(model, 'customTypePickerOptions.selectedEntities.0.idP');
             if (typeId) {
                 model.customTypeScriptOptions.childTypeId = typeId;
                 model.customTypeScriptOptions.disabled = false;
             }
         });
         $scope.$watch('entityTypeId', function () {
             $scope.model.customTypeScriptOptions.parentTypeId = $scope.entityTypeId;
         });

         $scope.clean = function (event) {
             //only IE contains the clean button which not support angular js ng-model update.
            if (event && event.currentTarget) {
                if ($scope.model.searchText !== event.currentTarget.innerText) {
                    $scope.model.searchText = event.currentTarget.innerText;
                }
            }
        };
              
         // Watch relationship type option to filter by relationhip, lookup, choice field
         $scope.$watch('model.relationshipType', function () {
             //load filtered relationships
             $scope.filteredRelationships = _.filter($scope.relationships, function (rel) {
                 return (
                            (rel.relationshipType.toLowerCase() === $scope.model.relationshipType.toLowerCase() || $scope.model.relationshipType === "All") &&
                            $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, rel.solutions) &&
                            $scope.searchRelationshipText(rel.relationshipName, rel.resourceName)
                        );
             });
         });

         // Watch the search text to filter relationships by replationship name or resource name
         $scope.$watch('model.searchText', function () {
             filterSearchText();
         });


         function filterSearchText() {
             //load filtered relationships
             $scope.filteredRelationships = _.filter($scope.relationships, function (rel) {
                 return (
                     (rel.relationshipType.toLowerCase() === $scope.model.relationshipType.toLowerCase() || $scope.model.relationshipType === "All") &&
                         $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, rel.solutions) &&
                         $scope.searchRelationshipText(rel.relationshipName, rel.resourceName)
                 );
             });
             $scope.filteredDerivedResources = _.filter($scope.derivedResources, function (dev) {
                 return (
                     $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, dev.solutions) &&
                         $scope.searchRelationshipText(dev.relationshipName, dev.resourceName)
                 );
             });
         }           
        

         $scope.$watch('model.showHidden', function ()
         {
             $scope.setEntity();
         });

         // Watch the selected entity id for changes and set the selected entity
         $scope.$watch('options', function () {
            
             $scope.relRelationships = [];
             //get currrent application id at init
             //TODO: remove solution filter now, set back following code until restore the function back
             // $scope.setSolutionOptions();
             //$scope.currentSolutionId = spNavService.getCurrentApplicationId();
             //if (!$scope.currentSolutionId) {
             //    $scope.currentSolutionId = 0;
             //}
             if (options.selectedNode)
             {
                 $scope.selectedNode = options.selectedNode;
                 $scope.entityTypeId = $scope.selectedNode.etid;
             } else if
             (options.entityTypeId) {
                 $scope.selectedNode = null;
                 $scope.entityTypeId = options.entityTypeId;
             }
             
             if (options.selectedRelationshipIds) {
                 $scope.selectedRelationshipIds = options.selectedRelationshipIds;
             } else {
                 $scope.selectedRelationshipIds = null;
             }
             

             if ($scope.nameFieldId === 0) {
                 spEntityService.getEntity('core:name', 'id', { hint: 'nameField', batch: true }).then(function (field) {
                     $scope.nameFieldId = field.id();
                 });
             }

             $scope.setEntity();
         });         

         // click ok button to return selected relationships
         $scope.ok = function () {
             if (($scope.relRelationships && $scope.relRelationships.length > 0) || ($scope.retRemoveRelationships && $scope.retRemoveRelationships.length > 0)) {
                 var retResult;

                 if ($scope.selectedNode) {

                     var nameField = null;
                     if ($scope.model.addName) {                         
                         for (var i = 0; i < $scope.relRelationships.length; i++) {                             
                             nameField = { "id": spUtils.newGuid(), "fid": $scope.nameFieldId, "fname": 'Name', "ftype": 'String', "isreq": true, "group": 'Default', "svid": 0, "relid": 0, "etid": $scope.relRelationships[i].eid, "ftid": 0, "ttid": 0, "aggm": "", "inrep": false, "inanal": false, "dname": $scope.relRelationships[i].relationshipName, "fieldOrder": 1 };
                         }
                     }


                     retResult = { relRelationships: $scope.relRelationships, addName: $scope.model.addName, nameField: nameField, removeRelationships: $scope.retRemoveRelationships, nameFieldId: $scope.nameFieldId };
                     $uibModalInstance.close(retResult);
                 }
                 else if ($scope.selectedRelationshipIds) {
                    
                     //update retRelationshipIds list
                     $scope.retRelationshipIds = $scope.selectedRelationshipIds;
                     if ($scope.relRelationships && $scope.relRelationships.length > 0) {
                         _.each($scope.relRelationships, function (relationship) {
                             $scope.retRelationshipIds.push(relationship.rid);
                            }
                        );
                     }
                     if ($scope.retRemoveRelationships && $scope.retRemoveRelationships.length > 0) {
                         _.each($scope.retRemoveRelationships, function (relationship) {
                             var removeIndex = _.lastIndexOf($scope.retRelationshipIds, relationship.rid);
                             if (removeIndex > -1) {
                                 $scope.retRelationshipIds.splice(removeIndex, 1);
                             }
                         }
                       );
                     }

                     retResult = { retRelationshipIds: $scope.retRelationshipIds };
                     $uibModalInstance.close(retResult);
                 }
             }
         };

         // click cancel to return report builder
         $scope.cancel = function () {
             $uibModalInstance.close(null);
         };

         //create solution options list
         $scope.setSolutionOptions = function () {

             spEntityService.getInstancesOfType('core:solution', 'alias, name, description, enumOrder', true)
                 .then(function(items) {
                    
                     items.sort(function (item1, item2) {
                         var eo1 = item1.entity.field('core:enumOrder'),
                             eo2 = item2.entity.field('core:enumOrder');

                         if (_.isNaN(eo1)) {
                             eo1 = 0;
                         }

                         if (_.isNaN(eo2)) {
                             eo2 = 0;
                         }

                         // Sort by enum then by name
                         if (eo1 === eo2) {
                             return item1.entity.field('core:name') < item2.entity.field('core:name') ? -1 : +1;
                         }

                         return eo1 < eo2 ? -1 : +1;
                     });

                     var entities = _.map(items, function (fi) {return fi.entity;});

                     if (entities) {

                         var idFunction = function () { return -1; };
                         var aliasFunction = function () { return "allapplications"; };
                         var nameFunction = function () { return "All Applications"; };
                         var descriptionFunction = function () { return "All Applications."; };
                         var entityFunction = function () { return null; };
                         var allSolution = { id: idFunction, alias: aliasFunction, getName: nameFunction, description: descriptionFunction, entity: entityFunction };
                         entities.splice(0, 0, allSolution);
                         $scope.model.solutionPickerOptions = { selectedEntityId: $scope.currentSolutionId, selectedEntity: null, entities: entities, showSelectOption: false };
                     }
                 });
         };

         //set entitytypeid and get entity properties by entityService
         $scope.setEntity = function () {
             if ($scope.entityTypeId) {
                 var requestOption = {
                     fields: true,
                     relationships: true,
                     fieldGroups: true,
                     ignoreInheritance: false,
                     ignoreOverrides: false,
                     derivedTypes: true
                 };
                 $scope.model.gridBusyIndicator.isBusy = true;
                 var rq = spResource.makeTypeRequest(requestOption);
                 spEntityService.getEntity($scope.entityTypeId, rq).then(
                     function (typeEntity) {
                         if (spAppSettings.selfServeNonAdmin) {
                             // make another call first to filter down to only relationships the self-serve user can see
                             $scope.filterRelationships(typeEntity);
                         } else {
                             var type = new spResource.Type(typeEntity);
                             var showAllPred = function() { return true; };
                             $scope.getResourcePromise(type, showAllPred);
                         }
                     }
                 );
             }
         };

         $scope.filterRelationships = function filterRelationships(typeEntity) {

             // Need to filter list of relationships to only show those that go from/to objects that the user can see.
             // This is a seriously messed up hack written under duress
             // Object visibility is only restricted in the query engine, because the platform requires everyone to be able to see all types (at the moment).
             // So need, to use query engine to determine if we can see relationships.
             // This can be achieved by using a 'getEntitiesOfType' query with a filter.  (but not a getEntities query)

             var type = new spResource.Type(typeEntity);
             var allRels = type.getAllRelationships({ showHidden: true });
             var allIds = _.map(allRels, function (r) { return r.getEntity().idP; });
             // Ensure user has permission to see objects at both ends (done using filter because objects are only filtered in report engine)
             var filter = '[From Type] is not null and [To Type] is not null and (';

             _.forEach(allIds, function(id) {
                 filter += 'id(context())=' + id + ' or ';
             });
             filter = _.trimEnd(filter, ' or ') + ')';

             spEntityService.getEntitiesOfType('core:relationship', 'id', { filter: filter, hint: 'Rptbuilder relationships' }).then( function(visibleRels) {
                 $scope.applyRelationshipFilter(type, visibleRels);
             });
         };

         $scope.applyRelationshipFilter = function applyRelationshipFilter(type, visibleRels) {
             // Visible rels contains an array of relationships that the user is allowed to see
             // Update the 
             var visRelIds = _.keyBy(visibleRels, function (visRel) { return visRel.idP; });
             var canSeeRel = function (relationship) {
                 var relId = relationship.getEntity().idP;
                 return visRelIds[relId];
             };
             $scope.getResourcePromise(type, canSeeRel);
         };
        
         //the getentity promise, build the relationship array to load in dialog page
         $scope.getResourcePromise = function(resource, canSeeRelPred)
         {
             var deriveds, derivedTypes;
             if (!resource) {
                 return;
             }

             $scope.entityTypeName = resource.getName();
             $scope.relationships = [];
             $scope.derivedResources = [];
             var resourceoptions = { showHidden: $scope.model.showHidden };
             resource.getRelationships(resourceoptions).forEach(
                 function (relationship) {
                     if (!canSeeRelPred(relationship))
                         return;
                     if (!relationship.isStructureLevel()) {
                         var relationshipJson = $scope.getRelationshipJson(relationship, "relationships");
                         if (relationshipJson) {
                             $scope.relationships.push(relationshipJson);
                         }
                     }
                 }                
             );
             
             resource.getLookups(resourceoptions).forEach(
                function (relationship) {                    
                    if (!canSeeRelPred(relationship))
                        return;
                    var lookupJson = $scope.getRelationshipJson(relationship, "lookups");
                    if (lookupJson) {
                        $scope.relationships.push(lookupJson);
                    }                    
                }
            );
             
             resource.getChoiceFields(resourceoptions).forEach(
                function (relationship) {
                    if (!relationship.isReverse()) {
                        var choicefieldsJson = $scope.getRelationshipJson(relationship, "choicefields");
                        if (choicefieldsJson) {
                            $scope.relationships.push(choicefieldsJson);
                        }
                    }
                }
            );
    
             deriveds = spResource.getDerivedTypesAndSelf(resource.getEntity());
             derivedTypes = _.without(deriveds, resource.getEntity());
             if (derivedTypes) {
                 derivedTypes.forEach(function (derived) {
                     $scope.derivedResources.push($scope.getDerivedTypeJson(derived, $scope.entityTypeName));
                     }
                 );
             }
             
             if ($scope.derivedResources.length > 0) {
                 $scope.hideAdvTab = false;
             }

             reloadRelationships();
             //build UML diagram
             $scope.buildDiagram();
             $scope.model.gridBusyIndicator.isBusy = false;
         };

         function reloadRelationships() {
             //load filtered relationships
             $scope.filteredRelationships = _.filter($scope.relationships, function (rel) {
                 return (
                            (rel.relationshipType.toLowerCase() === $scope.model.relationshipType.toLowerCase() || $scope.model.relationshipType === "All") &&
                            $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, rel.solutions) &&
                            $scope.searchRelationshipText(rel.relationshipName, rel.resourceName)
                        );
             });

             $scope.filteredDerivedResources = _.filter($scope.derivedResources, function (dev) {
                 return (
                             $scope.existsInSolution($scope.model.solutionPickerOptions.selectedEntityId, dev.solutions) &&
                            $scope.searchRelationshipText(dev.relationshipName, dev.resourceName)
                 );
             });

         }

         //check current replationship's solution exist in solution range
         $scope.existsInSolution = function(solutionId, solutions) {

             return true;

             //todo remove solution filter function now
             //restore following code until required

             //if (!solutionId) {
             //    return false;
             //}
            
             ////todo
             //if (solutionId === -1) {
             //    return true;
             //}
             //var exists = false;

             //if (solutionId == solutions) {
             //    exists = true;
             //} else {
             //    for (var i = 0; i < solutions.length; i++) {
             //        if (solutions[i].id() === solutionId) {
             //            exists = true;
             //            break;
             //        }
             //    }
             //}

             //return exists;

         };
         
         //search reltionship name and resource name by text
         $scope.searchRelationshipText = function(relationshipName, resourceName)
         {
             if ($scope.model.searchText && $scope.model.searchText.length > 0) {
                 return relationshipName.toLowerCase().indexOf($scope.model.searchText.toLowerCase()) >= 0 || resourceName.toLowerCase().indexOf($scope.model.searchText.toLowerCase()) >= 0;
             }
             else
             {
                 return true;
             }
         };

         $scope.getRelationshipJson = function (relationship,  relationshipType) {
             var relationshipName, resourceName, typeName, isSelected, toTypeId, dir;
             if (relationship) {
                 dir = relationship.isReverse() ? 'Reverse' : 'Forward';
                 try {
                     relationshipName = relationship.getName();
                 } catch(e) {
                     relationshipName = '';
                 }
                 try {
                     if (!relationship.getEntity()) {
                         console.log('Error in relationship ' + relationshipName + ': can\'t get relationship entity object ');
                     } else if (!relationship.getEntity().getFromType || !relationship.getEntity().getFromType()) {
                         console.log('Error in relationship ' + relationshipName + ': can\'t get relationship fromtype object ');
                     } else if (!relationship.getEntity().getToType || !relationship.getEntity().getToType()) {
                         console.log('Error in relationship ' + relationshipName + ': can\'t get relationship totype object ');
                     }

                     if (relationship.isReverse()) {
                         if (relationship.getEntity().getFromName) {
                             resourceName = relationship.getEntity().getFromName();
                         }

                         typeName = relationship.getEntity().getFromType().getName();
                     } else {
                         if (relationship.getEntity().getToName) {
                             resourceName = relationship.getEntity().getToName();
                         }
                         typeName = relationship.getEntity().getToType().getName();
                     }

                     //resourceName = relationship.isReverse() ? relationship.getEntity().getFromType().getName() : relationship.getEntity().getToType().getName();
                 } catch(e) {
                     resourceName = '';
                 }
                 if (!resourceName) {
                     resourceName = relationshipName;
                 }


                 try {
                     toTypeId = relationship.isReverse() ? relationship.getEntity().getFromType().eid().id() : relationship.getEntity().getToType().eid().id();
                 } catch(e) {
                     toTypeId = 0;
                 }
                 isSelected = $scope.isSelectedRelationship(relationship);

                 //set relationshiptype
                 
                 var rel = { "id": spUtils.newGuid(), "rid": relationship.getEntity().eid().id(), "relationshipName": relationshipName, "resourceName": resourceName, "typeName": typeName,  "relationshipType": relationshipType, "isSelected": isSelected, "eid": toTypeId, "solutions": relationship.getSolutions(), "dir": dir, "cols": [] };
                 if (isSelected === true && $scope.selectedNode) {
                     var selectedRelationships = _.filter($scope.selectedNode.children, function(childNode) {
                         return (
                             rel.rid === childNode.relid && rel.eid === childNode.etid
                         );
                     });

                     if (selectedRelationships && selectedRelationships.length > 1) {
                         var count = 0;
                         _.forEach(selectedRelationships, function() {

                             count++;
                             if (count > 1) {
                                 var cloneRel = { "id": spUtils.newGuid(), "rid": rel.rid, "relationshipName": rel.relationshipName, "resourceName": rel.resourceName, "relationshipType": rel.relationshipType, "isSelected": rel.isSelected, "eid": rel.eid, "solutions": rel.solutions, "dir": rel.dir, "cols": [] };
                                 $scope.duplicateRelationships.push(cloneRel);
                             }
                         });
                     }
                 }
                 return rel;
             }

             return null;
         };

         $scope.getDerivedTypeJson = function (derived, entityTypeName) {
             var relationshipName, resourceName, relationshipType,  isSelected, toTypeId;
             relationshipName = entityTypeName + ' as ' + derived.name;
             resourceName = derived.name;
             toTypeId = derived.eid().id();
             relationshipType = "derivedResources";
             
             isSelected = $scope.isSelectedDerivedType(derived);
             return { "id": spUtils.newGuid(), "rid": 0, "relationshipName": relationshipName, "resourceName": resourceName, "relationshipType": relationshipType, "isSelected": isSelected, "eid": toTypeId, "solutions": derived.getInSolution(), "dir": 'Forward', "cols": [] };
         };

         //double click row to select replationship to post back
         $scope.onRowDoubleClicked = function (relationship) {
             //disable row double click function now
             //if (relationship.isSelected === true) {
             //    //alert('This relationship is selected');
             //    spAlertsService.addAlert('This relationship is selected', spAlertsService.sev.Error);
             //} else {
             //    $scope.retRelationship = relationship;

             //    spEntityService.getEntity(relationship.eid, spResource.makeTypeRequest()).then
             //    (
             //        function (typeEntity) {
             //            $scope.getToTypeResourcePromise(new spResource.Type(typeEntity));
             //        }
             //    );

             //}
         };

         $scope.addCustomRelationship = function () {
             var typeEntity = sp.result($scope.model, 'customTypePickerOptions.selectedEntities.0');
             if (!typeEntity)
                 return;      

             var customJson = {
                 "id": spUtils.newGuid(),
                 "rid": 0,
                 "relationshipName": typeEntity.name,
                 "resourceName": typeEntity.name,
                 "relationshipType": "customJoin",
                 "isSelected": true,
                 "predicateScript": $scope.model.customJoinScript,
                 "eid": typeEntity.idP,
                 "cols": []
             };
             $scope.addRelationship(customJson);
         }

         $scope.addRelationship = function (relationship) {
             $scope.showButton = false;
             var wasSelected = relationship.isSelected;
             relationship.isSelected = true;
             
             $scope.mapRelationships(relationship.id, true);
             if (!$scope.relRelationships) {
                 $scope.relRelationships = [];
             }

            
            var relationshipUid = relationship.id;

            if (wasSelected) {
                relationshipUid = spUtils.newGuid();
                $scope.relRelationships.push({ "id": relationshipUid, "rid": relationship.rid, "relationshipName": relationship.relationshipName, "resourceName": relationship.resourceName, "typeName": relationship.typeName, "relationshipType": relationship.relationshipType, "dir": relationship.dir, "isSelected": true, "eid": relationship.eid, "solutions": relationship.solutions, "cols": null, "predicateScript": relationship.predicateScript });
            } else {
                $scope.relRelationships.push(relationship);
            }


             if (relationship.eid > 0) {
                 spTreeviewManager.getNodeColumns(relationship.eid, $scope.nameFieldId, function(columns) {
                     if (relationship) {
                         relationship.cols = columns;
                     }
                     $scope.showButton = true;

                 });
             }

             $scope.buildDiagram();

         };

         //Remove relationship from current selected list, if it is preselected relationship,add to remove list
         $scope.removeRelationship = function(relationship) {
             $scope.showButton = false;

             if (!$scope.retRemoveRelationships) {
                 $scope.retRemoveRelationships = [];
             }
            
             var relationshipCount = _.filter($scope.relRelationships, function (rel) {
                 return (rel.rid === relationship.rid);
             });

             var preSelectedRelationships = null;            
             if ($scope.selectedNode) {
                 preSelectedRelationships = _.filter($scope.selectedNode.children, function(rel) {
                     return (rel.relid === relationship.rid);
                 });                                 
             }
             else if ($scope.selectedRelationshipIds) {
                 preSelectedRelationships = _.filter($scope.selectedRelationshipIds, function (relId) {
                     return (relId === relationship.rid);
                 });
             }
             
             var isPreSelectedRelationship = preSelectedRelationships && preSelectedRelationships.length > 0;


             if (relationshipCount.length > 1 && isPreSelectedRelationship) {
                 //do nothing
             }             
             else if (relationshipCount.length === 1 && isPreSelectedRelationship)
             {
                 $scope.mapRelationships(relationship.id, false);
             }
             else if (relationshipCount.length === 0 && isPreSelectedRelationship) {
                 $scope.mapRelationships(relationship.id, false);
                 $scope.retRemoveRelationships.push(relationship);
                 
                 if (preSelectedRelationships && preSelectedRelationships.length > 1 && $scope.duplicateRelationships.length > 0) {
                     //remove from duplicaterelationships                 
                     var index = -1;
                     for (var i = 0; i < $scope.duplicateRelationships.length; i++) {
                         if ($scope.duplicateRelationships[i].rid === relationship.rid) {
                             index = i;
                             break;
                         }
                     }
                    
                     if (index > -1) {
                         $scope.duplicateRelationships.splice(index, 1);
                     }
                 }
             }
             else if (relationshipCount.length === 1 && !isPreSelectedRelationship)
             {
                 $scope.mapRelationships(relationship.id, false);
             }
             else if (relationshipCount.length > 1 && !isPreSelectedRelationship) {
                 //do nothing
                 
             }
              
             $scope.relRelationships = _.without($scope.relRelationships, relationship);

             if ($scope.relRelationships.length > 0 || $scope.retRemoveRelationships.length > 0) {
                 $scope.showButton = true;
             }

             reloadRelationships();

             $scope.buildDiagram();
         };

         //check current dericed typed relationship is selected in report or not
         $scope.isSelectedDerivedType = function (derived) {

             try {
                 var selected;
                 if ($scope.selectedNode) {
                     selected = _.filter($scope.selectedNode.children, function (childNode) {
                         // the following code is totally broken ... at least two unresolved vars
                         // so nulling the whole thing out as now we are jshint checking this
                         // BUT not sure when this is therefore ever used as it would crash it
                         // it was.
                         //return  relationship.getEntity().getToType().eid().id() === childNode.etid;
                         return false;
                     });
                 }
                 else if ($scope.selectedRelationshipIds) {
                     selected = _.filter($scope.selectedRelationshipIds, function (relationshipId) {
                         return (
                             derived.getEntity().eid().id() === relationshipId
                         );
                     });
                 } else {
                     selected = null;
                 }

                 if (selected && selected.length > 0) {
                     return true;
                 }

                 selected = _.filter($scope.relRelationships.length, function (rel) {
                     return (
                         derived.getEntity().eid().id() === rel.rid
                     );
                 });

                 if (selected && selected.length > 0) {
                     return true;
                 }

                 return false;
             }
             catch (e) {
                 return false;
             }
         };

         //check current reltionship is selected in report or not
         $scope.isSelectedRelationship = function (relationship)
         {
             try {                
                 var selected;
                 var toTypeId = relationship.isReverse() ? relationship.getEntity().getFromType().eid().id() : relationship.getEntity().getToType().eid().id();
                 var relationshipName;
                 // for relationship which fromType and toType are same, not just check relationship id , also need check the relationship name
                 try {
                     relationshipName = relationship.isReverse() ? relationship.getEntity().fromName : relationship.getEntity().toName;
                 } catch (ex) {
                     
                 }
                 if ($scope.selectedNode) {
                     selected = _.filter($scope.selectedNode.children, function(childNode) {
                         return (
                             relationship.getEntity().eid().id() === childNode.relid && toTypeId === childNode.etid && (relationshipName && relationshipName === childNode.name)
                         );
                     });
                 }
                 else if ($scope.selectedRelationshipIds) {
                     selected = _.filter($scope.selectedRelationshipIds, function (relationshipId) {
                         return (
                             relationship.getEntity().eid().id() === relationshipId
                         );
                     });
                 } else {
                     selected = null;
                 }

                 if (selected && selected.length > 0) {
                     return true;
                 }
                 
                 selected = _.filter($scope.relRelationships.length, function (rel) {
                     return (
                         relationship.getEntity().eid().id() === rel.rid
                     );
                 });
                 
                 if (selected && selected.length > 0) {
                     return true;
                 }

                 return false;
             }
             catch (e) {
                 return false;
             }
         };

         //get field type's database displayname
         $scope.getDatabaseTypeDisplayName = function (fieldType) {
           
             switch (fieldType.alias()) {
                 case 'core:stringField':
                     return "String";
                 case 'core:intField':
                     return "Int32";
                 case 'core:dateField':
                     return "Date";
                 case 'core:timeField':
                     return "Time";
                 case 'core:dateTimeField':
                     return "DateTime";
                 case 'core:decimalField':
                     return "Decimal";
                 case 'core:currencyField':
                     return "Currency";
                 case 'core:boolField':
                     return "Bool";
                 case 'core:autoNumberField':
                     return "AutoNumber";
                 default:
                     // return empty string
                     return "";
             }
         };

         function cloneField(field) {
             if (field) {
                 return { "id": spUtils.newGuid(), "fid": field.fid, "fname": field.fname, "ftype": field.ftype, "isreq": field.isreq, "group": field.group, "svid": field.svid, "relid": field.relid, "ftid": field.ftid, "ttid": field.ttid, "aggm": field.aggm, "inrep": field.inrep, "inanal": field.inanal };
             } else {
                 return null;
             }
         }

         $scope.mapRelationships = function(id, isSelected) {
             _.map($scope.filteredRelationships, function (relationship)
                 {
                 if (id === relationship.id)
                     {
                     relationship.isSelected = isSelected;
                     }
                     return relationship;
                 }
            );
             
             _.map($scope.filteredDerivedResources, function (relationship) {
                 if (id === relationship.id) {
                     relationship.isSelected = isSelected;
                 }
                 return relationship;
             }
            );
             
             _.map($scope.relationships, function (relationship) {
                 if (id === relationship.id) {
                     relationship.isSelected = isSelected;
                 }
                 return relationship;
             }
           );

             _.map($scope.derivedResources, function (relationship) {
                 if (id === relationship.id) {
                     relationship.isSelected = isSelected;
                 }
                 return relationship;
             }
            );
         };
         
         function handler() {
             console.log.apply(console, ['event %o'].concat(arguments));
         }

         var initialized = false;
         $scope.buildDiagram = function() {
             var height = 380;
             var allRelationships = getAvailableRelationships();
             if (allRelationships.length > 12) {
                 height = 380 + (allRelationships.length - 12) * 30;
             }
             if (!initialized || height > 380) {
                 $scope.setupDiagram(height);
                 initialized = true;
             }
             $scope.updateDiagram(allRelationships);
         };
         
         $scope.setupDiagram = function (height) {

             

             var div = d3.select("#canvasDiagram");
             
             var existSVG = d3.select("#canvasDiagram").select("svg");
             if (existSVG) {
                 existSVG.remove();
             }


             var svg =
                 div.append('svg')
                     .attr("x", 0).attr("y", 0)
                     .attr("width", 200).attr("height", height);

             var g = svg.append('g');
             
             // Append root node
             g.append('rect').attr("x", 0).attr("y", 0).attr("width", 190).attr("height", 22).attr("fill", "#414141");
             g.append('text').text($scope.selectedNode.name).attr("x", 22).attr("y", 16).attr('fill', '#efefef').style("font-size", 12);


         };


         function getAvailableRelationships() {
             var allRelationships = [];


             _.forEach($scope.relationships, function (rel) {
                 if (rel.isSelected === true) {
                     allRelationships.push(rel);
                 }
             });

             _.forEach($scope.duplicateRelationships, function (rel) {
                 allRelationships = _.without(allRelationships, rel);
                 allRelationships.push(rel);
             });

             _.forEach($scope.relRelationships, function (rel) {
                 allRelationships = _.without(allRelationships, rel);
                 allRelationships.push(rel);
             });

             return allRelationships;
         }

         $scope.updateDiagram = function (allRelationships) {

             var g = d3.select("#canvasDiagram").select("svg");

             $scope.buildRelationshipsRect(allRelationships, g);
         };

         $scope.buildRelationshipsRect = function(relationships, container) {

             var nodes = container.selectAll(".node")
                 .data(relationships, function (d) {
                     return d.id;
                 });
            
             nodes.exit().remove();
             
             var g = nodes.enter()
                 .append("g")
                 .attr("class", "node");

             nodes.attr("transform", function (d, i) {
                 return "translate(20, " + (i * 30) + ")";
             });
            
             var pickImage = function (d) {
                 var imgPath = '';
                 switch (d.relationshipType) {
                     case "relationships":
                         imgPath = 'assets/images/relationshipstype.png';
                         break;
                     case "lookups":
                         imgPath = 'assets/images/lookupstype.png';
                         break;
                     case "choicefields":
                         imgPath = 'assets/images/relationshipInstance.png';
                         break;
                     case "derivedResources":
                         imgPath = 'assets/images/derivedResourcestype.png';
                         break;
                     case "customJoin":
                         imgPath = 'assets/images/customJointype.png';
                         break;
                     default:
                         imgPath = 'assets/images/relationshipstype.png';
                         break;
                 }
                 return imgPath;
             };

             var removeRel = function(d) {
                 if (d) {
                     $scope.removeRelationship(d);
                     $scope.$apply();
                 }
             };

             g.append('polyline').attr('stroke', '#414141').attr("fill", "none").attr("stroke-width", "2").attr("points", "-10,8 -10,38 0,38");
             g.append('rect').attr("x", 0).attr("y", 25).attr("width", 170).attr("height", 22).attr("fill", "#414141");
             g.append('image').attr("xlink:href", pickImage).attr("x", 5).attr("y", 28).attr("width", "16").attr("height", "18");
             g.append('text').text(function (d) { return d.relationshipName; }).attr("x", 32).attr("y", 40).attr('fill', '#eee').style("font-size", 12);
             g.append('image').attr("xlink:href", "assets/images/remove_w.png").attr("x", 150).attr("y", 28).attr("width", "16").attr("height", "16").attr('cursor', 'pointer').attr("title", function (d) { return d.relationshipName; }).on("click", removeRel);
         };

     })
    .factory('spRelationshipDialog', function (spDialogService) {
        // setup the dialog
        var exports = {
            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Select Relationship',
                    keyboard: true,
                    windowClass: 'modal relationshipdialog-view',
                    templateUrl: 'relationshipPicker/spRelationshipPicker.tpl.html',
                    controller: 'spRelationshipPickerController',
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