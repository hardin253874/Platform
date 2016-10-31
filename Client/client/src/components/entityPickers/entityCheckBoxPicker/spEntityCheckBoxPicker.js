// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    
    /**
    * Module implementing an entity check box picker.
    * Displays one checkbox for each entity allowing multiple entities to be selected.
    * 
    * @module spEntityCheckBoxPicker
    * @example
        
    Using the spEntityCheckBoxPicker:
    
    &lt;sp-entity-check-box-picker options="checkBoxOptions"&gt;&lt;/sp-entity-check-box-picker&gt
       
    where checkBoxOptions is available on the controller with the following properties:
        - selectedEntityIds {array of number | string} - the selected entity ids.
        - selectedEntities {array of Entity} - output only (optional). If this member is defined it will be assigned the selected entities.
        - entities {array of Entity} - the array of Entity objects to pick from.
        - entityTypeId - {number|string} - the type id of the entities to pick from. A service call will be made to get all instances of this type. If the entityTypeId that is specified is an enumType the entities are sorted by the enumOrder, otherwise they are sorted by name.
        - hiddenAliases - {array of string} - list of hidden aliases. Only valid when entityTypeId is specified
        - disabled - {bool} - true to disable the control, false otherwise
    
    Note: you only need to specify entityTypeId or entities.
    * 
    */
    angular.module('mod.common.ui.spEntityCheckBoxPicker', ['mod.common.ui.entityPickerControllers'])
        .directive('spEntityCheckBoxPicker', function () {
            return {
                restrict: 'E',
                templateUrl: 'entityPickers/entityCheckBoxPicker/spEntityCheckBoxPicker.tpl.html',
                controller: 'multiEntityPickerController',
                scope: {
                    options: '=',
                    formControl: '=?',
                    parentControl: '=?'
                },
                link: function (scope, element) {

                    scope.$on('gather', function (event, callback) {
                        callback(scope.formControl, scope.parentControl, element);
                    });
                }
            };
        });
}());