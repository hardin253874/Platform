// Copyright 2011-2016 Global Software Innovation Pty Ltd

/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport,
 sp, spEntity, jsonString, jsonBool, jsonLookup, spEditForm,
 spFormControlVisibilityServiceTestData */

describe("editForm|spec:|spFormControlVisibilityService", function () {
    "use strict";            

    const calcData = spFormControlVisibilityServiceTestData;

    beforeEach(module("mod.app.editFormServices"));

    beforeEach(function () {
        // Stub out any services
        var rnFeatureSwitchStub = {
            isFeatureOn: function() {
                return true;
            }
        };
        
        module("mod.app.spFormControlVisibilityService", function ($provide) {
            $provide.value("rnFeatureSwitch", rnFeatureSwitchStub);            
        });
     });

    beforeEach(inject(function ($injector, $rootScope) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    describe("getControlsWithVisibilityCalculations", function() {
        it("getControlsWithVisibilityCalculations form with calculations", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithCalculations;
            var controls = spFormControlVisibilityService.getControlsWithVisibilityCalculations(formControl);

            expect(controls.length).toEqual(2);

            var descriptionControl = _.find(controls, function(c) {
                return c.id() === calcData.descriptionControlId;
            });

            expect(descriptionControl.visibilityCalculation).toEqual("[Name] = 'name test'");

            var nameControl = _.find(controls, function(c) {
                return c.id() === calcData.nameControlId;
            });

            expect(nameControl.visibilityCalculation).toEqual("[Description] = 'desc test'");
        }));

        it("getControlsWithVisibilityCalculations form with no calculations", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithNoCalculations;
            var controls = spFormControlVisibilityService.getControlsWithVisibilityCalculations(formControl);

            expect(controls.length).toEqual(0);
        }));
    });

    describe("doesFormHaveControlsWithVisibilityCalculations", function() {
        it("doesFormHaveControlsWithVisibilityCalculations form with calculations", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithCalculations;
            var haveCalcs = spFormControlVisibilityService.doesFormHaveControlsWithVisibilityCalculations(formControl);

            expect(haveCalcs).toBeTruthy();
        }));

        it("doesFormHaveControlsWithVisibilityCalculations form with no calculations", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithNoCalculations;
            var haveCalcs = spFormControlVisibilityService.doesFormHaveControlsWithVisibilityCalculations(formControl);

            expect(haveCalcs).toBeFalsy();
        }));
    });


    describe("registerFormDataChangeListener", function() {
        it("registerFormDataChangeListener form with calculations update dependent field", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithCalculations;
            var formData = spFormControlVisibilityServiceTestData.formData;
            var visibilityCalcDependencies = {
        
            };

            // Description control depends on name field
            visibilityCalcDependencies[calcData.descriptionControlId] = {
                fields: [calcData.nameFieldId],
                relationships: []
            };

            // Name control depends on description field
            visibilityCalcDependencies[calcData.nameControlId] = {
                fields: [calcData.descriptionFieldId],
                relationships: []
            };

            var done = false;

            function onUpdateControlVisibility(calculationsToUpdate) {
                expect(calculationsToUpdate[calcData.descriptionControlId]).toEqual("[Name] = 'name test'");
                expect(_.keys(calculationsToUpdate).length).toEqual(1);
                done = true;
            }

            spFormControlVisibilityService.registerFormDataChangeListener(formControl, formData, visibilityCalcDependencies, onUpdateControlVisibility);

            runs(function() {
                done = false;

                // change the form data. This should trigger the description control calc to update
                formData.setField(calcData.nameFieldId, "new value");
            });

            waitsFor(function() {
                return done;
            });
        }));

        it("registerFormDataChangeListener form with calculations update dependent relationship", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithCalculations;
            var formData = spFormControlVisibilityServiceTestData.formData;
            var visibilityCalcDependencies = {
        
            };

            // Description control depends on lookup
            visibilityCalcDependencies[calcData.descriptionControlId] = {
                fields: [],
                relationships: [calcData.lookupFieldId]
            };

            var done = false;

            function onUpdateControlVisibility(calculationsToUpdate) {
                expect(calculationsToUpdate[calcData.descriptionControlId]).toEqual("[Name] = 'name test'");
                expect(_.keys(calculationsToUpdate).length).toEqual(1);
                done = true;
            }

            spFormControlVisibilityService.registerFormDataChangeListener(formControl, formData, visibilityCalcDependencies, onUpdateControlVisibility);

            runs(function() {
                done = false;

                // change the form data. This should trigger the description control calc to update
                formData.setLookup(calcData.lookupFieldId, 50000);
            });

            waitsFor(function() {
                return done;
            });
        }));

        it("registerFormDataChangeListener form with calculations update non dependent field", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithCalculations;
            var formData = spFormControlVisibilityServiceTestData.formData;
            var visibilityCalcDependencies = {
        
            };

            // Description control depends on name field
            visibilityCalcDependencies[calcData.descriptionControlId] = {
                fields: [calcData.nameFieldId],
                relationships: []
            };

            // Name control depends on description field
            visibilityCalcDependencies[calcData.nameControlId] = {
                fields: [calcData.descriptionFieldId],
                relationships: []
            };

            var done = false;
            var updateCallbackFired = false;

            function onUpdateControlVisibility(calculationsToUpdate) {
                updateCallbackFired = true;
            }

            spFormControlVisibilityService.registerFormDataChangeListener(formControl, formData, visibilityCalcDependencies, onUpdateControlVisibility);

            runs(function() {
                done = false;

                // change the form data. This should trigger the description control calc to update
                formData.setField(10000, "new value", spEntity.DataType.String);

                // Set a timer which will trigger after a second.
                // If the updateCallback hasn;t fired
                setTimeout(function() {
                    if (!updateCallbackFired) {
                        done = true;
                    }
                }, 1000);
            });

            waitsFor(function() {
                return done;
            });
        }));

        it("registerFormDataChangeListener form with calculations update non dependent relationship", inject(function(spFormControlVisibilityService) {
            var formControl = calcData.formWithCalculations;
            var formData = spFormControlVisibilityServiceTestData.formData;
            var visibilityCalcDependencies = {
        
            };

            // Description control depends on name field
            visibilityCalcDependencies[calcData.descriptionControlId] = {
                fields: [],
                relationships: [calcData.lookupFieldId]
            };

            var done = false;
            var updateCallbackFired = false;

            function onUpdateControlVisibility(calculationsToUpdate) {
                updateCallbackFired = true;
            }

            spFormControlVisibilityService.registerFormDataChangeListener(formControl, formData, visibilityCalcDependencies, onUpdateControlVisibility);

            runs(function() {
                done = false;

                // change the form data. This should trigger the description control calc to update
                formData.setLookup(90000, 90000);

                // Set a timer which will trigger after a second.
                // If the updateCallback hasn;t fired
                setTimeout(function() {
                    if (!updateCallbackFired) {
                        done = true;
                    }
                }, 1000);
            });

            waitsFor(function() {
                return done;
            });
        }));
    });

    describe("registerControlVisibilityHandler and updateControlsVisibility", function() {
        it("updateControlsVisibility triggers registered callback", inject(function(spFormControlVisibilityService, $rootScope) {
            var scope = $rootScope.$new(true);
            var callbackFired = false;

            function controlVisibilityHandler(formControlId, isVisible) {
                if (formControlId === 10000 && isVisible) {
                    callbackFired = true;
                }
            }

            // Register the handler
            spFormControlVisibilityService.registerControlVisibilityHandler(scope, 10000, controlVisibilityHandler);

            var visibilityData = {
                "10000": true,
                "20000": false
            };            

            spFormControlVisibilityService.updateControlsVisibility(scope, visibilityData);

            $rootScope.$apply();

            expect(callbackFired).toBeTruthy();
        }));        
    });

    describe("getVisibleControls", function() {
        it("getVisibleControls direct visibility", inject(function (spFormControlVisibilityService, spEditForm) {
            var formControls = spEditForm.getFormControls(calcData.formWithCalculations);

            var visibilityData = {
                "5000": true,
                "6000": false
            }; 
            
            var visibleControls = spFormControlVisibilityService.getVisibleControls(formControls, visibilityData);                       

            expect(_.find(visibleControls, c => c.id() === 5000)).toBeTruthy();
            expect(_.find(visibleControls, c => c.id() === 6000)).toBeFalsy();
        }));

        it("getVisibleControls indirect visibility", inject(function(spFormControlVisibilityService, spEditForm) {
            var formControls = spEditForm.getFormControls(calcData.formWithNameDescInContainer);

            var visibilityData = {
                "5000": true,
                "6000": false
            }; 
            
            var visibleControls = spFormControlVisibilityService.getVisibleControls(formControls, visibilityData);                       

            expect(_.find(visibleControls, c => c.id() === 5000)).toBeTruthy();
            expect(_.find(visibleControls, c => c.id() === 6000)).toBeFalsy();
        }));        
    });
});