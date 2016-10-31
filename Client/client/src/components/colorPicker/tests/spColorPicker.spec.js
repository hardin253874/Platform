// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|ColorPickers|spColorPicker|spec:|spColorPicker directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spColorPickers'));
    beforeEach(module('mod.common.ui.spColorPickerConstants'));    
    beforeEach(module('colorPicker/spColorPicker.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));    

    function changeInputValueTo(el, value, $sniffer, scope) {
        el.val(value);
        el.trigger($sniffer.hasEvent('input') ? 'input' : 'change');
        scope.$digest();
    }

    it('should replace HTML element with appropriate content', inject(function ($rootScope, $compile, $templateCache) {
        var scope = $rootScope,
            element,            
            pickerDiv,
            colorInputContainers,
            aInput, rInput, gInput, bInput;
        
        scope.selectedColor = { a: 20, r: 30, g: 40, b: 50 };
        
        element = angular.element('<sp-color-picker color="selectedColor"></sp-color-picker>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        pickerDiv = element.find('div.spColorPicker-view');       
        expect(pickerDiv.length).toBe(1);

        colorInputContainers = element.find('.colorNumberInputContainer');

        aInput = $(colorInputContainers[0]).find('input');
        rInput = $(colorInputContainers[1]).find('input');
        gInput = $(colorInputContainers[2]).find('input');
        bInput = $(colorInputContainers[3]).find('input');
        
        expect(element.find('#aSlider').slider('value')).toBe(20);
        expect(element.find('#rSlider').slider('value')).toBe(30);
        expect(element.find('#gSlider').slider('value')).toBe(40);
        expect(element.find('#bSlider').slider('value')).toBe(50);

        // Verify output color
        expect(aInput.val()).toBe('20');
        expect(rInput.val()).toBe('30');
        expect(gInput.val()).toBe('40');
        expect(bInput.val()).toBe('50');
    }));


    it('updates color when rgb number inputs change', inject(function ($rootScope, $compile, $templateCache, $sniffer) {
        var scope = $rootScope,
            element,
            colorInputContainers,            
            aInput, rInput, gInput, bInput;
         
        scope.selectedColor = { a: 20, r: 30, g: 40, b: 50 };
        
        element = angular.element('<sp-color-picker color="selectedColor"></sp-color-picker>');
        $compile(element)(scope);
        scope.$digest();       

        expect(scope.$$childHead.color.a).toBe(scope.selectedColor.a);
        expect(scope.$$childHead.color.r).toBe(scope.selectedColor.r);
        expect(scope.$$childHead.color.g).toBe(scope.selectedColor.g);
        expect(scope.$$childHead.color.b).toBe(scope.selectedColor.b);        

        colorInputContainers = element.find('.colorNumberInputContainer');

        aInput = $(colorInputContainers[0]).find('input');
        rInput = $(colorInputContainers[1]).find('input');
        gInput = $(colorInputContainers[2]).find('input');
        bInput = $(colorInputContainers[3]).find('input');

        // Verify output color
        expect(aInput.val()).toBe('20');
        expect(rInput.val()).toBe('30');
        expect(gInput.val()).toBe('40');
        expect(bInput.val()).toBe('50');

        // Change number inputs
        changeInputValueTo(aInput, 25, $sniffer, scope);
        changeInputValueTo(rInput, 35, $sniffer, scope);
        changeInputValueTo(gInput, 45, $sniffer, scope);
        changeInputValueTo(bInput, 55, $sniffer, scope);

        expect(element.find('#aSlider').slider('value')).toBe(25);
        expect(element.find('#rSlider').slider('value')).toBe(35);
        expect(element.find('#gSlider').slider('value')).toBe(45);
        expect(element.find('#bSlider').slider('value')).toBe(55);

        // Verify output color
        expect(scope.selectedColor.a).toBe(25);
        expect(scope.selectedColor.r).toBe(35);
        expect(scope.selectedColor.g).toBe(45);
        expect(scope.selectedColor.b).toBe(55);
    }));


    it('updates color when rgb slider inputs change', inject(function ($rootScope, $compile, $templateCache, $sniffer) {
        var scope = $rootScope,
            element,
            colorInputContainers,
            aSlider, rSlider, gSlider, bSlider,
            aInput, rInput, gInput, bInput;

        scope.selectedColor = { a: 20, r: 30, g: 40, b: 50 };
        
        element = angular.element('<sp-color-picker color="selectedColor"></sp-color-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childHead.color.a).toBe(scope.selectedColor.a);
        expect(scope.$$childHead.color.r).toBe(scope.selectedColor.r);
        expect(scope.$$childHead.color.g).toBe(scope.selectedColor.g);
        expect(scope.$$childHead.color.b).toBe(scope.selectedColor.b);

        colorInputContainers = element.find('.colorNumberInputContainer');

        aInput = $(colorInputContainers[0]).find('input');
        rInput = $(colorInputContainers[1]).find('input');
        gInput = $(colorInputContainers[2]).find('input');
        bInput = $(colorInputContainers[3]).find('input');

        // Verify output color
        expect(aInput.val()).toBe('20');
        expect(rInput.val()).toBe('30');
        expect(gInput.val()).toBe('40');
        expect(bInput.val()).toBe('50');

        // Change slider inputs
        aSlider = element.find('#aSlider').slider();
        aSlider.slider('option', 'slide').call(aSlider, { originalEvent: {type: 'mouse'} }, { value: 25 });
        scope.$digest();

        rSlider = element.find('#rSlider').slider();
        rSlider.slider('option', 'slide').call(rSlider, { originalEvent: { type: 'mouse' } }, { value: 35 });
        scope.$digest();

        gSlider = element.find('#gSlider').slider();
        gSlider.slider('option', 'slide').call(gSlider, { originalEvent: { type: 'mouse' } }, { value: 45 });
        scope.$digest();

        bSlider = element.find('#bSlider').slider();
        bSlider.slider('option', 'slide').call(bSlider, { originalEvent: { type: 'mouse' } }, { value: 55 });
        scope.$digest();

        // Verify output color
        expect(aInput.val()).toBe('25');
        expect(rInput.val()).toBe('35');
        expect(gInput.val()).toBe('45');
        expect(bInput.val()).toBe('55');

        expect(scope.selectedColor.a).toBe(25);
        expect(scope.selectedColor.r).toBe(35);
        expect(scope.selectedColor.g).toBe(45);
        expect(scope.selectedColor.b).toBe(55);
    }));


    it('updates color when hue slider inputs change', inject(function ($rootScope, $compile, $templateCache, $sniffer) {
        var scope = $rootScope,
            element,
            colorInputContainers,
            hueSlider,
            aInput, rInput, gInput, bInput;

        scope.selectedColor = { a: 255, r: 0, g: 255, b: 255 };
        
        element = angular.element('<sp-color-picker color="selectedColor"></sp-color-picker>');
        $compile(element)(scope);
        scope.$digest();

        expect(scope.$$childHead.color.a).toBe(scope.selectedColor.a);
        expect(scope.$$childHead.color.r).toBe(scope.selectedColor.r);
        expect(scope.$$childHead.color.g).toBe(scope.selectedColor.g);
        expect(scope.$$childHead.color.b).toBe(scope.selectedColor.b);

        colorInputContainers = element.find('.colorNumberInputContainer');

        aInput = $(colorInputContainers[0]).find('input');
        rInput = $(colorInputContainers[1]).find('input');
        gInput = $(colorInputContainers[2]).find('input');
        bInput = $(colorInputContainers[3]).find('input');

        // Verify output color
        expect(aInput.val()).toBe('255');
        expect(rInput.val()).toBe('0');
        expect(gInput.val()).toBe('255');
        expect(bInput.val()).toBe('255');

        // Change slider inputs
        hueSlider = element.find('#hueSlider').slider();
        hueSlider.slider('option', 'slide').call(hueSlider, { originalEvent: { type: 'mouse' } }, { value: 1000 });
        scope.$digest();        

        expect(aInput.val()).toBe('255');
        expect(rInput.val()).toBe('255');
        expect(gInput.val()).toBe('0');
        expect(bInput.val()).toBe('0');

        expect(element.find('#aSlider').slider('value')).toBe(255);
        expect(element.find('#rSlider').slider('value')).toBe(255);
        expect(element.find('#gSlider').slider('value')).toBe(0);
        expect(element.find('#bSlider').slider('value')).toBe(0);

        // Verify output color
        expect(scope.selectedColor.a).toBe(255);
        expect(scope.selectedColor.r).toBe(255);
        expect(scope.selectedColor.g).toBe(0);
        expect(scope.selectedColor.b).toBe(0);
    }));


    it('updates color when named dropdown changes', inject(function ($rootScope, $compile, $templateCache, $sniffer, namedColors) {
        var scope = $rootScope,
            element,
            pickerDiv,
            dropDownButton,
            colorInputContainers,
            menuItems,
            colorName,
            namedColor,
            dropDownContainer,
            aInput, rInput, gInput, bInput;

        scope.selectedColor = { a: 20, r: 30, g: 40, b: 50 };
        
        element = angular.element('<sp-color-picker color="selectedColor"></sp-color-picker>');
        $compile(element)(scope);
        scope.$digest();

        // Verify that the html element has been replaced
        pickerDiv = element.find('div.spColorPicker-view');
        expect(pickerDiv.length).toBe(1);

        dropDownContainer = element.find('.colorDropDownContainer');
        dropDownButton = $(dropDownContainer).find('.dropdownButton');
        dropDownButton.click();
        scope.$digest();

        menuItems = $(dropDownContainer).find('a.dropdownMenuItem');
        colorName = $(menuItems[2]).find('span').text();
        namedColor = _.find(namedColors, function (nc) {
            return nc.name == colorName;
        });
        if (!namedColor) {
            namedColor = {
                name: 'Custom',
                value: { a: 20, r: 30, g: 40, b: 50 }
            };
        }
        $(menuItems[2]).click();
        scope.$digest();
        
        colorInputContainers = element.find('.colorNumberInputContainer');

        aInput = $(colorInputContainers[0]).find('input');
        rInput = $(colorInputContainers[1]).find('input');
        gInput = $(colorInputContainers[2]).find('input');
        bInput = $(colorInputContainers[3]).find('input');

        expect(element.find('#aSlider').slider('value')).toBe(namedColor.value.a);
        expect(element.find('#rSlider').slider('value')).toBe(namedColor.value.r);
        expect(element.find('#gSlider').slider('value')).toBe(namedColor.value.g);
        expect(element.find('#bSlider').slider('value')).toBe(namedColor.value.b);
        
        expect(aInput.val()).toBe(namedColor.value.a.toString());
        expect(rInput.val()).toBe(namedColor.value.r.toString());
        expect(gInput.val()).toBe(namedColor.value.g.toString());
        expect(bInput.val()).toBe(namedColor.value.b.toString());

        expect(scope.selectedColor.a).toBe(namedColor.value.a);
        expect(scope.selectedColor.r).toBe(namedColor.value.r);
        expect(scope.selectedColor.g).toBe(namedColor.value.g);
        expect(scope.selectedColor.b).toBe(namedColor.value.b);
    }));
});