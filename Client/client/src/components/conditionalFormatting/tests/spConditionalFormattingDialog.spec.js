// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */

describe('Console|Controls|spConditionalFormattingDialog|spec:|spConditionalFormattingDialog directive', function () {
    'use strict';

    var iconThumbnailSize,        
        condFormatIcons = [];        

    // Load the modules
    beforeEach(module('mod.common.ui.spConditionalFormattingDialog'));
    beforeEach(module('mod.ui.spConditionalFormattingConstants'));
    beforeEach(module('mod.common.ui.spTypeOperatorService'));
    
    //beforeEach(module('app-templates'));
    //beforeEach(module('component-templates'));
    
    beforeEach(module('conditionalFormatting/spConditionalFormattingDialog.tpl.html'));
    beforeEach(module('conditionalFormatting/spConditionalFormattingTab.tpl.html'));
    beforeEach(module('conditionalFormatting/spHighlightColorCell.tpl.html'));
    beforeEach(module('conditionalFormatting/spHighlightFormattingRules.tpl.html'));
    beforeEach(module('conditionalFormatting/spIconCell.tpl.html'));
    beforeEach(module('conditionalFormatting/spIconFormattingRules.tpl.html'));
    beforeEach(module('conditionalFormatting/spOperationCell.tpl.html'));
    beforeEach(module('conditionalFormatting/spProgressBarFormatting.tpl.html'));
    beforeEach(module('conditionalFormatting/spValueCell.tpl.html'));
    beforeEach(module('conditionalFormatting/spValueFormattingTab.tpl.html'));
    beforeEach(module('conditionalFormatting/spImageFormattingTab.tpl.html'));
    beforeEach(module('valueEditor/singleLineTextControl.tpl.html'));
    beforeEach(module('valueEditor/spValueEditor.tpl.html'));
    beforeEach(module('editForm/directives/spFieldTitle/spFieldTitle.tpl.html'));
    beforeEach(module('editForm/directives/spSingleLineTextControl/spSingleLineTextControl.tpl.html'));
    beforeEach(module('editForm/directives/spTitlePlusMarkers/spTitlePlusMarkers.tpl.html'));
    beforeEach(module('editForm/directives/spCustomValidationMessage/spCustomValidationMessage.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerFgBgDropdown.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));
    beforeEach(module('iconPicker/spIconPickerDropdown.tpl.html'));
    beforeEach(module('editForm/partials/fieldControlOnForm.tpl.html'));
    beforeEach(module('entityPickers/entityComboPicker/spEntityComboPicker.tpl.html'));
    
    beforeEach(module('app.editFormModules'));

    beforeEach(module('mockedEntityService'));

    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        spEntityService.mockGetEntityJSON([{
                    id: 'core:tenantGeneralSettingsInstance',
                    tenantCurrencySymbol: '$',
                    finYearStartMonth:
                        { id: 111 },
                    tenantConsoleThemeSettings:
                        { id: 222 }
                }
            ]
        );

        // Set the data we wish the mock to return
        iconThumbnailSize = spEntity.fromJSON({
            id: { id: 22222, ns: 'console', alias: 'iconThumbnailSize' },
            'console:thumbnailWidth': '16',
            'console:thumbnailHeight': '16'
        });
        spEntityService.mockGetEntity(iconThumbnailSize);

        condFormatIcons.push(spEntity.fromJSON({
            id: { id: 3000, ns: 'core', alias: 'blackCircleCondFormat' },
            'core:formatIconOrder': '0',
            'core:condFormatImage': {
                id: { id: 3001, ns: 'core', alias: 'blackCircleCondFormatIcon' }
            }
        }));

        condFormatIcons.push(spEntity.fromJSON({
            id: { id: 4000, ns: 'core', alias: 'greenCircleCondFormat' },
            'core:formatIconOrder': '1',
            'core:condFormatImage': {
                id: { id: 4001, ns: 'core', alias: 'greenCircleCondFormatIcon' }
            }
        }));

        spEntityService.mockGetEntitiesOfType('conditionalFormatIcon', condFormatIcons);        

        spEntityService.mockGetInstancesOfTypeRawData('console:thumbnailSizeEnum', entityTestData.thumbnailSizesTestData);
    }));


    afterEach(inject(function ($document) {
        var body = $document.find('body');
        body.find('div.modal').remove();
        body.find('div.modal-backdrop').remove();
        body.removeClass('modal-open');
    }));


    it('create dialog, show and cancel', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,        
            dialogOptions = {                
            };

        // Show the dialog
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result).toBe(false);
        });
        
        scope.$digest();

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, show and ok, no changes', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Name',
                type: 'String',                
                condFormatting: {
                    displayText: true,
                    format: 'Highlight'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('Highlight');
            expect(result.condFormatting.highlightRules.length).toBe(1);
            expect(result.condFormatting.highlightRules[0].operator).toBe('Unspecified');
            expect(result.condFormatting.highlightRules[0].value).toBeUndefined();
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, clear formatting', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true,
                    format: 'ProgressBar',
                    progressBarRule: {
                        color: {
                            a: 255,
                            r: 20,
                            g: 20,
                            b: 20
                        },
                        minimumValue: 20,
                        maximumValue: 100
                    }
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('None');
            expect(result.condFormatting.progressBarRule).toBeUndefined();
            expect(result.condFormatting.highlightRules).toBeUndefined();
            expect(result.condFormatting.iconRules).toBeUndefined();
        });

        scope.$digest();

        // Change values
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'None';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        expect(scope.$$childHead.$$childHead.canShowAddRulesButton()).toBe(false);

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, load with existing progress bar formatting', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true,
                    format: 'ProgressBar',
                    progressBarRule: {
                        minimumValue: 50,
                        maximumValue: 300,
                        color: {
                            a: 255,
                            r: 110,
                            g: 120,
                            b: 130
                        }
                    }
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('ProgressBar');
            expect(result.condFormatting.progressBarRule.minimumValue).toBe(50);
            expect(result.condFormatting.progressBarRule.maximumValue).toBe(300);
            expect(result.condFormatting.progressBarRule.color.a).toBe(255);
            expect(result.condFormatting.progressBarRule.color.r).toBe(110);
            expect(result.condFormatting.progressBarRule.color.g).toBe(120);
            expect(result.condFormatting.progressBarRule.color.b).toBe(130);
        });
       
        scope.$digest();        

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, add progress bar Int32 - valid values', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true,
                    format: 'ProgressBar'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('ProgressBar');
            expect(result.condFormatting.progressBarRule.minimumValue).toBe(50);
            expect(result.condFormatting.progressBarRule.maximumValue).toBe(300);
            expect(result.condFormatting.progressBarRule.color.a).toBe(255);
            expect(result.condFormatting.progressBarRule.color.r).toBe(110);
            expect(result.condFormatting.progressBarRule.color.g).toBe(120);
            expect(result.condFormatting.progressBarRule.color.b).toBe(130);
        });
       
        scope.$digest();

        // Change values
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.minimumValue = 50;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.maximumValue = 300;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.a = 255;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.r = 110;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.g = 120;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.b = 130;

        expect(scope.$$childHead.$$childHead.canShowAddRulesButton()).toBe(false);

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, add progress bar Int32 - invalid values', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true,
                    format: 'ProgressBar'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions, null);

        scope.$digest();

        // Change values
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.minimumValue = 500;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.maximumValue = 300;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.a = 255;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.r = 110;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.g = 120;
        scope.$$childHead.$$childHead.model.condFormatting.progressBarRule.color.b = 130;

        scope.$$childHead.$$childHead.ok();

        expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, load existing highlight rules', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',
                condFormatting: {
                    displayText: true,
                    format: 'Highlight',                    
                    highlightRules: [
                        {
                            operator: 'Equal',
                            value: 10,
                            color: {
                                foregroundColor: { a: 255, r: 11, g: 12, b: 13 },
                                backgroundColor: { a: 255, r: 21, g: 22, b: 23 }
                            }
                        },
                        {
                            operator: 'Unspecified',                                
                            color: {
                                foregroundColor: { a: 255, r: 31, g: 32, b: 33 },
                                backgroundColor: { a: 255, r: 41, g: 42, b: 43 }
                            }
                        }
                    ]                    
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('Highlight');
            expect(result.condFormatting.highlightRules.length).toBe(2);
            expect(result.condFormatting.highlightRules[0].operator).toBe('Equal');
            expect(result.condFormatting.highlightRules[0].value).toBe(10);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.a).toBe(255);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.r).toBe(11);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.g).toBe(12);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.b).toBe(13);

            expect(result.condFormatting.highlightRules[0].color.backgroundColor.a).toBe(255);
            expect(result.condFormatting.highlightRules[0].color.backgroundColor.r).toBe(21);
            expect(result.condFormatting.highlightRules[0].color.backgroundColor.g).toBe(22);
            expect(result.condFormatting.highlightRules[0].color.backgroundColor.b).toBe(23);

            expect(result.condFormatting.highlightRules[1].operator).toBe('Unspecified');
            expect(result.condFormatting.highlightRules[1].color.foregroundColor.a).toBe(255);
            expect(result.condFormatting.highlightRules[1].color.foregroundColor.r).toBe(31);
            expect(result.condFormatting.highlightRules[1].color.foregroundColor.g).toBe(32);
            expect(result.condFormatting.highlightRules[1].color.foregroundColor.b).toBe(33);

            expect(result.condFormatting.highlightRules[1].color.backgroundColor.a).toBe(255);
            expect(result.condFormatting.highlightRules[1].color.backgroundColor.r).toBe(41);
            expect(result.condFormatting.highlightRules[1].color.backgroundColor.g).toBe(42);
            expect(result.condFormatting.highlightRules[1].color.backgroundColor.b).toBe(43);
        });

        scope.$digest();        

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, add highlight rule - valid rules', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true                    
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('Highlight');
            expect(result.condFormatting.highlightRules.length).toBe(2);
            expect(result.condFormatting.highlightRules[0].operator).toBe('Equal');
            expect(result.condFormatting.highlightRules[0].value).toBe(10);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.a).toBe(255);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.r).toBe(11);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.g).toBe(12);
            expect(result.condFormatting.highlightRules[0].color.foregroundColor.b).toBe(13);

            expect(result.condFormatting.highlightRules[0].color.backgroundColor.a).toBe(255);
            expect(result.condFormatting.highlightRules[0].color.backgroundColor.r).toBe(21);
            expect(result.condFormatting.highlightRules[0].color.backgroundColor.g).toBe(22);
            expect(result.condFormatting.highlightRules[0].color.backgroundColor.b).toBe(23);

            expect(result.condFormatting.highlightRules[1].operator).toBe('Unspecified');
        });

        scope.$digest();
        
        // Change values        
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'Highlight';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        scope.$digest();
        
        expect(scope.$$childHead.$$childHead.canShowAddRulesButton()).toBe(true);

        scope.$$childHead.$$childHead.addRule();
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].operator = _.find(scope.$$childHead.$$childHead.model.condFormatting.operators, function (o) {
            return o.id === 'Equal';
        });
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].value = 10;
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].color = {
            foregroundColor: { a: 255, r: 11, g: 12, b: 13 },
            backgroundColor: { a: 255, r: 21, g: 22, b: 23 }
        };                

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, add highlight rule - invalid rules no operator', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions, null);

        scope.$digest();

        // Change values        
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'Highlight';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        scope.$digest();

        scope.$$childHead.$$childHead.addRule();
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].operator = null;
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].value = 10;
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].color = {
            foregroundColor: { a: 255, r: 11, g: 12, b: 13 },
            backgroundColor: { a: 255, r: 21, g: 22, b: 23 }
        };

        scope.$$childHead.$$childHead.ok();

        expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, add highlight rule - invalid rules no value', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',                
                condFormatting: {
                    displayText: true
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions, null);

        scope.$digest();

        // Change values        
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'Highlight';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        scope.$digest();

        scope.$$childHead.$$childHead.addRule();
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].operator = _.find(scope.$$childHead.$$childHead.model.condFormatting.operators, function (o) {
            return o.id === 'Equal';
        });
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].value = null;
        scope.$$childHead.$$childHead.model.condFormatting.highlightRules[0].color = {
            foregroundColor: { a: 255, r: 11, g: 12, b: 13 },
            backgroundColor: { a: 255, r: 21, g: 22, b: 23 }
        };

        scope.$$childHead.$$childHead.ok();

        expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, load existing icon rules', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',
                condFormatting: {
                    displayText: true,
                    format: 'Icon',
                    iconRules: [
                        {
                            operator: 'Equal',
                            value: 10,
                            imgId: 4001
                        },
                        {
                            operator: 'Unspecified',
                            imgId: 5001
                        }
                    ]
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('Icon');
            expect(result.condFormatting.iconRules.length).toBe(2);
            expect(result.condFormatting.iconRules[0].operator).toBe('Equal');
            expect(result.condFormatting.iconRules[0].value).toBe(10);
            expect(result.condFormatting.iconRules[0].imgId).toBe(4001);

            expect(result.condFormatting.iconRules[1].operator).toBe('Unspecified');
        });
       
        scope.$digest();        

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, add icon rule - valid rules', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Age',
                type: 'Int32',
                condFormatting: {
                    displayText: true
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.condFormatting.format).toBe('Icon');
            expect(result.condFormatting.iconRules.length).toBe(2);
            expect(result.condFormatting.iconRules[0].operator).toBe('Equal');
            expect(result.condFormatting.iconRules[0].value).toBe(10);
            expect(result.condFormatting.iconRules[0].imgId).toBe(4001);

            expect(result.condFormatting.iconRules[1].operator).toBe('Unspecified');
        });

        scope.$digest();

        // Change values        
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'Icon';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        scope.$digest();

        expect(scope.$$childHead.$$childHead.canShowAddRulesButton()).toBe(true);

        scope.$$childHead.$$childHead.addRule();
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].operator = _.find(scope.$$childHead.$$childHead.model.condFormatting.operators, function (o) {
            return o.id === 'Equal';
        });
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].value = 10;
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].icon.selectedIconId = _.find(scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].icon.iconIds, function (iid) {
            return iid.getId() === 4001;
        });        

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, add icon rule - invalid rules no operator', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',
                condFormatting: {
                    displayText: true
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions, null);

        scope.$digest();

        // Change values        
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'Icon';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        scope.$digest();

        scope.$$childHead.$$childHead.addRule();
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].operator = null;
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].value = 10;
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].imgId = 4001;

        scope.$$childHead.$$childHead.ok();

        expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, add icon rule - invalid rules no value', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,            
            dialogOptions = {
                name: 'Age',
                type: 'Int32',
                condFormatting: {
                    displayText: true
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions, null);

        scope.$digest();

        // Change values        
        scope.$$childHead.$$childHead.model.condFormatting.selectedFormatType = _.find(scope.$$childHead.$$childHead.model.condFormatting.formatTypes, function (ft) {
            return ft.id === 'Icon';
        });

        scope.$$childHead.$$childHead.onFormatTypeChanged();

        scope.$digest();

        scope.$$childHead.$$childHead.addRule();
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].operator = _.find(scope.$$childHead.$$childHead.model.condFormatting.operators, function (o) {
            return o.id === 'Equal';
        });
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].value = null;
        scope.$$childHead.$$childHead.model.condFormatting.iconRules[0].imgId = 4001;

        scope.$$childHead.$$childHead.ok();

        expect(scope.$$childHead.$$childHead.model.errors.length).toBe(1);

        scope.$$childHead.$$childHead.cancel();

        scope.$digest();
    }));


    it('create dialog, value formatting Currency', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Currency',
                valueFormatting: {                
                    prefix: '',
                    suffix: '',
                    decimalPlaces: 2,
                    alignment: ''
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {            
            expect(result.valueFormatting.prefix).toBe('P');
            expect(result.valueFormatting.suffix).toBe('S');
            expect(result.valueFormatting.decimalPlaces).toBe(4);
            //expect(result.valueFormatting.alignment).toBe('Left');
        });

        scope.$digest();

        // Change values               
        
        scope.$$childHead.$$childHead.model.valueFormatting.prefix = 'P';
        scope.$$childHead.$$childHead.model.valueFormatting.suffix = 'S';
        scope.$$childHead.$$childHead.model.valueFormatting.decimalPlacesModel.value = 4;
        scope.$$childHead.$$childHead.model.valueFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.valueFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Left';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, value formatting Int32', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Int32',
                valueFormatting: {
                    prefix: '',
                    suffix: '',                    
                    alignment: ''
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.prefix).toBe('P');
            expect(result.valueFormatting.suffix).toBe('S');
            expect(result.valueFormatting.decimalPlaces).toBeUndefined();
            //expect(result.valueFormatting.alignment).toBe('Right');
        });

        scope.$digest();

        // Change values               

        scope.$$childHead.$$childHead.model.valueFormatting.prefix = 'P';
        scope.$$childHead.$$childHead.model.valueFormatting.suffix = 'S';
        scope.$$childHead.$$childHead.model.valueFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.valueFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Right';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, value formatting no trimming prefix suffix', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Int32',
                valueFormatting: {
                    prefix: '',
                    suffix: '',
                    alignment: ''
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.prefix).toBe('P ');
            expect(result.valueFormatting.suffix).toBe(' S');
            expect(result.valueFormatting.decimalPlaces).toBeUndefined();
            //expect(result.valueFormatting.alignment).toBe('Right');
        });

        scope.$digest();

        // Change values               

        scope.$$childHead.$$childHead.model.valueFormatting.prefix = 'P ';
        scope.$$childHead.$$childHead.model.valueFormatting.suffix = ' S';
        scope.$$childHead.$$childHead.model.valueFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.valueFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Right';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    xit('create dialog, value formatting Bool', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Bool',
                valueFormatting: {
                    boolDisplayAs: '',                    
                    alignment: ''
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.boolDisplayAs).toBe('TrueFalse');            
            expect(result.valueFormatting.alignment).toBe('Right');
        });

        scope.$digest();

        // Change values                       
        scope.$$childHead.$$childHead.model.valueFormatting.selectedBooleanFormat = _.find(scope.$$childHead.$$childHead.model.valueFormatting.booleanFormats, function (bf) {
            return bf.id === 'TrueFalse';
        });
        scope.$$childHead.$$childHead.model.valueFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.valueFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Right';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, value formatting Date', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Date',
                valueFormatting: {
                    dateTimeFormatName: '',
                    alignment: ''
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.dateTimeFormatName).toBe('DayMonth');
            //expect(result.valueFormatting.alignment).toBe('Right');
        });

        scope.$digest();

        // Change values                       
        scope.$$childHead.$$childHead.model.valueFormatting.selectedDateTimeFormat = _.find(scope.$$childHead.$$childHead.model.valueFormatting.dateTimeFormats, function (df) {
            return df.id === 'DayMonth';
        });
        scope.$$childHead.$$childHead.model.valueFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.valueFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Right';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, value formatting String', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'String',
                valueFormatting: {
                    lines: '',
                    alignment: ''
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.lines).toBe(4);
            //expect(result.valueFormatting.alignment).toBe('Right');
        });

        scope.$digest();

        // Change values                       
        scope.$$childHead.$$childHead.model.valueFormatting.linesModel.value = 4;
        scope.$$childHead.$$childHead.model.valueFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.valueFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Right';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, image formatting', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Image',
                imageFormatting: {
                    imageScaleId: 0,
                    imageSizeId: 0,
                    alignment: ''
                }
            };        

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.imageFormatting.imageSizeId).toBe(12522);
            expect(result.imageFormatting.imageScaleId).toBe(34567);
            //expect(result.imageFormatting.alignment).toBe('Right');
        });

        scope.$digest();

        // Change values                       
        scope.$$childHead.$$childHead.model.imageFormatting.thumbnailSizePicker.selectedEntityId = 12522;
        scope.$$childHead.$$childHead.model.imageFormatting.thumbnailScalingPicker.selectedEntityId = 34567;
        scope.$$childHead.$$childHead.model.imageFormatting.selectedAlignmentOption = _.find(scope.$$childHead.$$childHead.model.imageFormatting.alignmentOptions, function (oa) {
            return oa.id === 'Right';
        });

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, load value formatting Currency', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Currency',
                valueFormatting: {
                    prefix: 'PCurr',
                    suffix: 'SCurr',
                    decimalPlaces: 5,
                    alignment: 'Right'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.prefix).toBe('PCurr');
            expect(result.valueFormatting.suffix).toBe('SCurr');
            expect(result.valueFormatting.decimalPlaces).toBe(5);
            //expect(result.valueFormatting.alignment).toBe('Right');
        });

        scope.$digest();        

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));

    it('create dialog, load value formatting Int32', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Int32',
                valueFormatting: {
                    prefix: 'PInt',
                    suffix: 'SInt',
                    alignment: 'Centre'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.prefix).toBe('PInt');
            expect(result.valueFormatting.suffix).toBe('SInt');
            expect(result.valueFormatting.decimalPlaces).toBeUndefined();
            //expect(result.valueFormatting.alignment).toBe('Centre');
        });        

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    xit('create dialog, load value formatting Bool', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Bool',
                valueFormatting: {
                    boolDisplayAs: 'YesNo',
                    alignment: 'Right'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.boolDisplayAs).toBe('YesNo');
            expect(result.valueFormatting.alignment).toBe('Right');
        });       

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, load value formatting Date', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Date',
                valueFormatting: {
                    dateTimeFormatName: 'dateShort',
                    alignment: 'Right'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.dateTimeFormatName).toBe('dateShort');
            //expect(result.valueFormatting.alignment).toBe('Right');
        });
       
        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, load value formatting String', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'String',
                valueFormatting: {
                    lines: 5,
                    alignment: 'Left'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.valueFormatting.lines).toBe(5);
            //expect(result.valueFormatting.alignment).toBe('Left');
        });        

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));


    it('create dialog, load image formatting', inject(function ($rootScope, spConditionalFormattingDialog) {
        var scope = $rootScope,
            dialogOptions = {
                name: 'Cost',
                type: 'Image',
                imageFormatting: {
                    imageSizeId: 12522,
                    imageScaleId: 34567,
                    alignment: 'Right'
                }
            };

        // Setup dialog options           
        spConditionalFormattingDialog.showModalDialog(dialogOptions).then(function (result) {
            expect(result.imageFormatting.imageSizeId).toBe(12522);
            expect(result.imageFormatting.imageScaleId).toBe(34567);
            //expect(result.imageFormatting.alignment).toBe('Right');
        });                

        scope.$digest();

        scope.$$childHead.$$childHead.ok();

        scope.$digest();
    }));
});