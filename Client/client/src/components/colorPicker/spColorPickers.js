// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing color pickers.    
    */
    angular.module('mod.common.ui.spColorPickers', [
        'mod.common.ui.spColorPickerDropdown',        
        'mod.common.ui.spColorPicker',
        'mod.common.ui.spColorPickerPopup',
        'mod.common.ui.spColorPickerFgBgDropdown',
        'mod.common.ui.spColorPickerFgBgDialog',
        'mod.common.ui.spColorPickerDialog'
    ]);
}());