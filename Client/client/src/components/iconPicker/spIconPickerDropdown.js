// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
      * Module implementing an icon picker dropdown.        
      *
      * @module spIconPickerDropdown    
      * @example            
         
      Using the spIconPickerDropdown:
     
      &lt;sp-icon-picker-dropdown options="options"/&gt;
     
      where options is an object with the following properties:
         - iconWidth {number}. Width in pixels of the icon thumbnails.
         - iconHeight {number}. Height in pixels of the icon thumbnails.
         - iconSizeId {EntityRef}. Entity ref of icon size entity.
         - iconIds {array of EntityRef}. Array of available icon ids.
         - selectedIconId {EntityRef}. The id of the selected icon.
      */
    angular.module('mod.common.ui.spIconPickerDropdown', ['mod.common.ui.spPopupProvider', 'mod.common.spXsrf'])
        .directive('spIconPickerDropdown', function (spPopupProvider, spXsrf) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'iconPicker/spIconPickerDropdown.tpl.html',
                scope: {
                    options: '='                    
                },
                link: function (scope, iElement, iAttrs) {
                    var IMAGE_BASE_URL = '/spapi/data/v1/image/thumbnail/',
                        popupProvider;


                    // Setup the model
                    scope.model = {                                                
                        iconWidth: 16,
                        iconHeight: 16,
                        iconSizeId: new spEntity.EntityRef('console:iconThumbnailSize'),
                        iconSizeIdEscaped: 'console-iconThumbnailSize',
                        iconIds: [],
                        selectedIconId: null
                    };


                    popupProvider = spPopupProvider(scope, iElement, {                        
                        templatePopupUrl: 'iconPicker/spIconPickerDropdownPopup.tpl.html'
                    });


                    // Setup watchers to watch for option changes

                    // Watch for icon width changes
                    scope.$watch('options.iconWidth', function (iconWidth) {
                        scope.model.iconWidth = iconWidth;
                        if (!scope.model.iconWidth) {
                            scope.model.iconWidth = 16;
                        }
                    });


                    // Watch for icon height changes
                    scope.$watch('options.iconHeight', function (iconHeight) {
                        scope.model.iconHeight = iconHeight;
                        if (!scope.model.iconHeight) {
                            scope.model.iconHeight = 16;
                        }
                    });


                    // Watch for icon size id changes
                    scope.$watch('options.iconSizeId', function (iconSizeId) {
                        if (!iconSizeId) {
                            // Default to icon size
                            scope.model.iconSizeId = new spEntity.EntityRef('console:iconThumbnailSize');
                        } else {
                            scope.model.iconSizeId = iconSizeId;
                        }

                        if (scope.model.iconSizeId) {
                            var escapedSizeId = String(scope.model.iconSizeId.getNsAliasOrId());
                            escapedSizeId = escapedSizeId.replace(':', '-');
                            
                            scope.model.iconSizeIdEscaped = escapedSizeId;
                        } else {
                            scope.model.iconSizeIdEscaped = '';                            
                        }
                    });


                    // Watch for icon ids changes
                    scope.$watch('options.iconIds', function (iconIds) {                        
                        if (iconIds && iconIds.length > 0) {
                            scope.model.iconIds = iconIds;
                        }                        
                    });


                    // Watch for selected icon id changes
                    scope.$watch('options.selectedIconId', function (selectedIconId) {
                        scope.model.selectedIconId = selectedIconId;
                    });

                    
                    // Push the selected icon id to the output
                    scope.$watch('model.selectedIconId', function (selectedIconId) {
                        scope.options.selectedIconId = selectedIconId;
                    });                    


                    // Get the selected icon style
                    scope.getSelectedIconStyle = function (id) {
                        var style = {};

                        if (id &&
                            id.getNsAliasOrId()) {
                            style['background-image'] = 'url(\'' + getIconUrl(id, 'console-iconThumbnailSize') + '\')';                            
                        }

                        style['background-repeat'] = 'no-repeat';

                        return style;
                    };


                    // Get the icon style
                    scope.getIconStyle = function (id) {
                        var style = {};

                        if (id &&
                            id.getNsAliasOrId()) {
                            style['background-image'] = 'url(\'' + getIconUrl(id, scope.model.iconSizeIdEscaped) + '\')';
                            
                        }

                        style['background-repeat'] = 'no-repeat';
                        style.width = scope.model.iconWidth + 'px';
                        style.height = scope.model.iconHeight + 'px';

                        return style;
                    };

                    scope.getIconName = function(id) {
                        return scope.options.iconNames && id ? scope.options.iconNames[id.id()] : '';
                    };


                    // Select the specified icon
                    scope.selectIcon = function (icon) {
                        scope.model.selectedIconId = icon;
                    };


                    // Handle drop down button click events
                    scope.dropDownButtonClicked = function (event) {
                        popupProvider.togglePopup(event);
                    };


                    // Private methods
                    function getIconUrl(id, sizeId) {
                        if (id &&
                            id.getNsAliasOrId() &&
                            sizeId) {
                            var imageId = String(id.getNsAliasOrId());
                            imageId = imageId.replace(':', '-');

                            var uri = IMAGE_BASE_URL + imageId + '/' + sizeId + '/core-scaleImageProportionally';

                            return spXsrf.addXsrfTokenAsQueryString(uri);
                        } else {
                            return '';
                        }
                    }
                }
            };
        });
}());