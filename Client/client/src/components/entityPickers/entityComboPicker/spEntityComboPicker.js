// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    
    /**
    * Module implementing an entity combo picker.
    * 
    * @module spEntityComboBoxPicker
    * @example
        
    Using the spEntityComboBoxPicker:
    
    &lt;sp-entity-combo-picker options="comboOptions"&gt;&lt;/sp-entity-combo-picker&gt
       
    where comboOptions is available on the controller with the following properties:
        - selectedEntityId - {number} - the selected entity id.
        - selectedEntity {Entity} - output only (optional). If this member is defined it will be assigned the selected entity.
        - entities {array of Entity} - the array of Entity objects to pick from.
        - entityTypeId - {number|string} - the type id of the entities to pick from. A service call will be made to get all instances of this type. If the entityTypeId that is specified is an enumType the entities are sorted by the enumOrder, otherwise they are sorted by name.
        - hiddenAliases - {array of string} - list of hidden aliases. Only valid when specified when entityTypeId is specified
        - disabled - {bool} - true to disable the control, false otherwise
    
    Note: you only need to specify entityTypeId or entities.
    * 
    */
    angular.module('mod.common.ui.spEntityComboPicker', ['mod.common.ui.entityPickerControllers'])
        .directive('spEntityComboPicker', function () {
            return {
                restrict: 'E',
                templateUrl: 'entityPickers/entityComboPicker/spEntityComboPicker.tpl.html',                
                controller: 'singleEntityPickerController',
                scope: {
                    options: '='
                }
            };
        });
}());