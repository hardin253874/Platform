// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing an entity radio picker.
    * Displays a list of radio buttons one for each entity allowing a single entity to be selected.
    * 
    * @module spEntityRadioPicker
    * @example

    Using the spEntityRadioPicker:
    
    &lt;sp-entity-radio-picker options="radioOptions"&gt;&lt;/sp-entity-radio-picker&gt
       
    where radioOptions is available on the controller with the following properties:
        - selectedEntityId - {number|string} - the selected entity id.
        - selectedEntity {Entity} - output only (optional). If this member is defined it will be assigned the selected entity.
        - entities {array of Entity} - the array of Entity objects to pick from.
        - entityTypeId - {number|string} - the type id of the entities to pick from. A service call will be made to get all instances of this type. If the entityTypeId that is specified is an enumType the entities are sorted by the enumOrder, otherwise they are sorted by name.
        - orderField - {string} - name of a field on the entities to order them by.
        - hiddenAliases - {array of string} - list of hidden aliases. Only valid when entityTypeId is specified
        - disabled - {bool} - true to disable the control, false otherwise
    
    Note: you only need to specify entityTypeId or entities.
    * 
    */
    angular.module('mod.common.ui.spEntityRadioPicker', ['mod.common.ui.entityPickerControllers'])
        .directive('spEntityRadioPicker', function () {
            return {
                restrict: 'E',
                templateUrl: 'entityPickers/entityRadioPicker/spEntityRadioPicker.tpl.html',                
                controller: 'singleEntityPickerController',
                scope: {
                    options: '='
                },
                link: function(scope) {
                    scope.orderEntities = function(entity) {
                        if (scope.options.orderField) {
                            return sp.result(entity, scope.options.orderField);
                        }
                        return entity;
                    };
                }
            };
        });
}());