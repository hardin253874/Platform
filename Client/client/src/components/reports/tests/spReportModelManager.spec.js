// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals reportTestData */

describe('Reports|View|spReportModelManager|spec:', function () {
    'use strict';

    // Setup      

    beforeEach(function () {
        // Stub out the nav service        
        var navServiceStub = {
            getChildHref: function () {
                return '';
            }
        };

        var tenantSettingsStub = {
            getCurrencySymbol: function () {
                return {
                    then: function (caller) {
                        caller('$');
                    }
                };
            },
            getTemplateReportIds: function () {
                return {
                    then: function (caller1) {
                        caller1({});
                        return {
                            then: function (caller2) {
                                caller2({});
                            }
                        };
                    }
                };
            }
        };

        module('mod.ui.spReportModelManager', function ($provide) {
            $provide.value('spNavService', navServiceStub);
            $provide.value('spTenantSettings', tenantSettingsStub);
        });

        module('mod.ui.spReportAggregateDataManager', function ($provide) {
            $provide.value('spTenantSettings', tenantSettingsStub);
        });
    });

    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    // Set the mocked data
    beforeEach(inject(function (spReportService, spEntityService) {
        // Mock the report data
        spReportService.mockGetReportData(12345, reportTestData.allFields);
    }));


    // Tests


    it('create model manager', inject(function ($rootScope, spReportModelManager, spReportAggregateDataManager) {
        var modelManager = spReportModelManager(null),
            model = modelManager.createModel();

        var report = {
            "meta": {
                "title": "RPT Single Grp Multiple Agg",
                "dfid": 9314,
                "typefmtstyle": {
                    "String": ["Highlight",
                        "Icon"],
                    "Int32": ["Highlight",
                        "Icon",
                        "ProgressBar"],
                    "ChoiceRelationship": ["Highlight",
                        "Icon"]
                },
                "rcols": {
                    "9354": {
                        "ord": 0,
                        "title": "AA_Employee",
                        "type": "String",
                        "fid": 2239,
                        "maxlen": 200,
                        "regex": "^[^<>]+$",
                        "regexerr": "The field cannot contain angled brackets.",
                        "entityname": true
                    },
                    "9357": {
                        "ord": 1,
                        "title": "Manager",
                        "type": "InlineRelationship",
                        "hide": true,
                        "fid": 2239,
                        "tid": 6542,
                        "maxlen": 200,
                        "regex": "^[^<>]+$",
                        "regexerr": "The field cannot contain angled brackets."
                    },
                    "9360": {
                        "ord": 2,
                        "title": "Age",
                        "type": "String",
                        "fid": 5497,
                        "defval": "",
                        "ro": true
                    },
                    "9363": {
                        "ord": 3,
                        "title": "Title",
                        "type": "ChoiceRelationship",
                        "fid": 2239,
                        "tid": 8275,
                        "maxlen": 200,
                        "regex": "^[^<>]+$",
                        "regexerr": "The field cannot contain angled brackets."
                    },
                    "9366": {
                        "ord": 4,
                        "title": "Status",
                        "type": "ChoiceRelationship",
                        "fid": 2239,
                        "tid": 5463,
                        "maxlen": 200,
                        "regex": "^[^<>]+$",
                        "regexerr": "The field cannot contain angled brackets."
                    },
                    "9369": {
                        "ord": 5,
                        "title": "Employee Number",
                        "type": "String",
                        "fid": 6210,
                        "defval": "",
                        "ro": true
                    },
                    "9372": {
                        "ord": 6,
                        "title": "Job Title",
                        "type": "String",
                        "fid": 5518,
                        "defval": ""
                    }
                },
                "choice": {
                    "8275": [{
                        "id": 5204,
                        "name": "Mr."
                    },
                        {
                            "id": 8767,
                            "name": "Ms."
                        },
                        {
                            "id": 7231,
                            "name": "Mrs."
                        },
                        {
                            "id": 7495,
                            "name": "Benevolent Dictator"
                        },
                        {
                            "id": 8846,
                            "name": "President"
                        },
                        {
                            "id": 6271,
                            "name": "Jedi"
                        }],
                    "5463": [{
                        "id": 8621,
                        "name": "Available"
                    },
                        {
                            "id": 5256,
                            "name": "On Leave"
                        },
                        {
                            "id": 7814,
                            "name": "Ill or Injured"
                        },
                        {
                            "id": 5130,
                            "name": "Unknown"
                        },
                        {
                            "id": 7355,
                            "name": "Lost the will to live"
                        },
                        {
                            "id": 7608,
                            "name": "Not Available"
                        }]
                },
                "inline": {
                    "6542": 3943
                },
                "anlcols": {
                    "9400": {
                        "ord": 0,
                        "title": "AA_Employee",
                        "type": "String",
                        "oper": "Unspecified",
                        "doper": "Contains"
                    },
                    "9401": {
                        "ord": 1,
                        "title": "Manager",
                        "type": "InlineRelationship",
                        "tid": 6542,
                        "oper": "Unspecified",
                        "doper": "AnyOf"
                    },
                    "9402": {
                        "ord": 2,
                        "title": "Age",
                        "type": "Int32",
                        "oper": "Unspecified",
                        "doper": "GreaterThan",
                        "value": "9999"
                    },
                    "9403": {
                        "ord": 3,
                        "title": "Title",
                        "type": "ChoiceRelationship",
                        "tid": 8275,
                        "oper": "Unspecified",
                        "doper": "AnyOf"
                    },
                    "9404": {
                        "ord": 4,
                        "title": "Status",
                        "type": "ChoiceRelationship",
                        "tid": 5463,
                        "oper": "Unspecified",
                        "doper": "AnyOf"
                    },
                    "9405": {
                        "ord": 5,
                        "title": "Employee Number",
                        "type": "Int32",
                        "oper": "Unspecified",
                        "doper": "GreaterThan"
                    },
                    "9406": {
                        "ord": 6,
                        "title": "Job Title",
                        "type": "String",
                        "oper": "Unspecified",
                        "doper": "Contains"
                    }
                },
                "rdata": [{
                    "map": 0,
                    "hdrs": [{
                        "9357": {

                        }
                    }],
                    "aggs": {
                        "9360": [{
                            "style": "aggAverage",
                            "value": "45"
                        }],
                        "9369": [{
                            "style": "aggCount",
                            "value": "1"
                        }]
                    }
                },
                    {
                        "map": 0,
                        "hdrs": [{
                            "9357": {
                                "vals": {
                                    "4634": "Glenn Uidam"
                                }
                            }
                        }],
                        "aggs": {
                            "9360": [{
                                "style": "aggAverage",
                                "value": "25"
                            }],
                            "9369": [{
                                "style": "aggCount",
                                "value": "2"
                            }]
                        }
                    },
                    {
                        "map": 0,
                        "hdrs": [{
                            "9357": {
                                "vals": {
                                    "6060": "Peter Aylett"
                                }
                            }
                        }],
                        "aggs": {
                            "9360": [{
                                "style": "aggAverage",
                                "value": "32"
                            }],
                            "9369": [{
                                "style": "aggCount",
                                "value": "6"
                            }]
                        }
                    },
                    {
                        "map": 0,
                        "hdrs": [{
                            "9357": {
                                "vals": {
                                    "6133": "Jude Jacobs"
                                }
                            }
                        }],
                        "aggs": {
                            "9360": [{
                                "style": "aggAverage",
                                "value": "29"
                            }],
                            "9369": [{
                                "style": "aggCount",
                                "value": "11"
                            }]
                        }
                    },
                    {
                        "map": 0,
                        "hdrs": [{
                            "9357": {
                                "vals": {
                                    "8464": "Scott Hopwood"
                                }
                            }
                        }],
                        "aggs": {
                            "9360": [{
                                "style": "aggAverage",
                                "value": "30"
                            }],
                            "9369": [{
                                "style": "aggCount",
                                "value": "2"
                            }]
                        }
                    },
                    {
                        "map": 0,
                        "hdrs": [{
                            "9357": {
                                "vals": {
                                    "8787": "Mitchell Murray"
                                }
                            }
                        }],
                        "aggs": {
                            "9360": [{
                                "style": "aggAverage",
                                "value": "34"
                            }],
                            "9369": [{
                                "style": "aggCount",
                                "value": "5"
                            }]
                        }
                    },
                    {
                        "map": 1,
                        "hdrs": [{
                            "9357": {

                            }
                        }],
                        "aggs": {
                            "9360": [{
                                "style": "aggAverage",
                                "value": "31"
                            }],
                            "9369": [{
                                "style": "aggCount",
                                "value": "27"
                            }]
                        }
                    }]
            },
            "gdata": [{
                "eid": 4618,
                "values": [{
                    "val": "Peter Shomar"
                },
                    {
                        "val": "35"
                    },
                    {
                        "vals": {
                            "5204": "Mr."
                        }
                    },
                    {
                        "vals": {
                            "7608": "Not Available"
                        }
                    },
                    {
                        "val": "9084"
                    },
                    {
                        "val": "Finance Manager"
                    }]
            },
                {
                    "eid": 4634,
                    "values": [{
                        "val": "Glenn Uidam"
                    },
                        {
                            "val": "38"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "5130": "Unknown"
                            }
                        },
                        {
                            "val": "2140"
                        },
                        {
                            "val": "National Operations Manager"
                        }]
                },
                {
                    "eid": 4668,
                    "values": [{
                        "val": "Ganesh Kumar"
                    },
                        {
                            "val": "25"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "5942"
                        },
                        {
                            "val": "Motion Graphics Designer"
                        }]
                },
                {
                    "eid": 4671,
                    "values": [{
                        "val": "Michelle Smith"
                    },
                        {
                            "val": "37"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "7060"
                        },
                        {
                            "val": "Contracts Manager"
                        }]
                },
                {
                    "eid": 4897,
                    "values": [{
                        "val": "Diana Walker"
                    },
                        {
                            "val": "21"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "7323"
                        },
                        {
                            "val": "Product Manager"
                        }]
                },
                {
                    "eid": 5010,
                    "values": [{
                        "val": "Brad Stevens"
                    },
                        {
                            "val": "40"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "5571"
                        },
                        {
                            "val": "Dev Ops Manager"
                        }]
                },
                {
                    "eid": 5047,
                    "values": [{
                        "val": "Karen Jones"
                    },
                        {
                            "val": "30"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "2119"
                        },
                        {
                            "val": "Software QA"
                        }]
                },
                {
                    "eid": 5134,
                    "values": [{
                        "val": "Kun Dai"
                    },
                        {
                            "val": "70"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "4514"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 5138,
                    "values": [{
                        "val": "Jennifer Blaylock"
                    },
                        {
                            "val": "25"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "3381"
                        },
                        {
                            "val": "Contracts & Commercial Manager"
                        }]
                },
                {
                    "eid": 5210,
                    "values": [{
                        "val": "Aimee Kutukoff"
                    },
                        {
                            "val": "22"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "3856"
                        },
                        {
                            "val": "Operations & Sales Support"
                        }]
                },
                {
                    "eid": 5452,
                    "values": [{
                        "val": "Steve Gibbon"
                    },
                        {
                            "val": "21"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "8363"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 5487,
                    "values": [{
                        "val": "David Quint"
                    },
                        {
                            "val": "17"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "5130": "Unknown"
                            }
                        },
                        {
                            "val": "9720"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 5668,
                    "values": [{
                        "val": "Chantelle Fitzgerald"
                    },
                        {
                            "val": "28"
                        },
                        {
                            "vals": {
                                "7231": "Mrs."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "3520"
                        },
                        {
                            "val": "Operations & Sales Support"
                        }]
                },
                {
                    "eid": 5835,
                    "values": [{
                        "val": "Nino Carabella"
                    },
                        {
                            "val": "21"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "2064"
                        },
                        {
                            "val": "Product Manager"
                        }]
                },
                {
                    "eid": 5978,
                    "values": [{
                        "val": "Anurag Sharma"
                    },
                        {
                            "val": "28"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "1882"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 6034,
                    "values": [{
                        "val": "Sri Korada"
                    },
                        {
                            "val": "32"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "3595"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 6133,
                    "values": [{
                        "val": "Jude Jacobs"
                    },
                        {
                            "val": "45"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "1001"
                        },
                        {
                            "val": "Director"
                        }]
                },
                {
                    "eid": 7069,
                    "values": [{
                        "val": "Tina Adlakha"
                    },
                        {
                            "val": "5"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "3659"
                        },
                        {
                            "val": "Software QA Lead"
                        }]
                },
                {
                    "eid": 7581,
                    "values": [{
                        "val": "Voula Zubcic"
                    },
                        {
                            "val": "33"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "9593"
                        },
                        {
                            "val": "Accountant"
                        }]
                },
                {
                    "eid": 7947,
                    "values": [{
                        "val": "Adrian Williamson"
                    },
                        {
                            "val": "21"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "4621"
                        },
                        {
                            "val": "Software QA"
                        }]
                },
                {
                    "eid": 8017,
                    "values": [{
                        "val": "Martin Kalitis"
                    },
                        {
                            "val": "40"
                        },
                        {
                            "vals": {
                                "6271": "Jedi"
                            }
                        },
                        {
                            "vals": {
                                "7355": "Lost the will to live"
                            }
                        },
                        {
                            "val": "5208"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 8063,
                    "values": [{
                        "val": "Joanne Beale"
                    },
                        {
                            "val": "42"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "7814": "Ill or Injured"
                            }
                        },
                        {
                            "val": "9292"
                        },
                        {
                            "val": "Assistant Accountant"
                        }]
                },
                {
                    "eid": 8464,
                    "values": [{
                        "val": "Scott Hopwood"
                    },
                        {
                            "val": "40"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "6141"
                        },
                        {
                            "val": "Software Architect"
                        }]
                },
                {
                    "eid": 8544,
                    "values": [{
                        "val": "Con Christou"
                    },
                        {
                            "val": "29"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "5283"
                        },
                        {
                            "val": "Software Engineer"
                        }]
                },
                {
                    "eid": 8668,
                    "values": [{
                        "val": "Peter Choi"
                    },
                        {
                            "val": "21"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "8621": "Available"
                            }
                        },
                        {
                            "val": "4199"
                        },
                        {
                            "val": "Graphics Designer"
                        }]
                },
                {
                    "eid": 8787,
                    "values": [{
                        "val": "Mitchell Murray"
                    },
                        {
                            "val": "58"
                        },
                        {
                            "vals": {
                                "5204": "Mr."
                            }
                        },
                        {
                            "vals": {
                                "5130": "Unknown"
                            }
                        },
                        {
                            "val": "5842"
                        },
                        {
                            "val": "Finance and Commercial Manager"
                        }]
                },
                {
                    "eid": 9185,
                    "values": [{
                        "val": "Abida Begum"
                    },
                        {
                            "val": "25"
                        },
                        {
                            "vals": {
                                "8767": "Ms."
                            }
                        },
                        {
                            "vals": {
                                "5256": "On Leave"
                            }
                        },
                        {
                            "val": "4937"
                        },
                        {
                            "val": "Software QA"
                        }]
                }]
        };

        var adm = spReportAggregateDataManager(report.meta);

        expect(modelManager.getModel()).toBe(model);
    }));


    it('should load paged data', inject(function ($rootScope, spReportModelManager, spReportService, $timeout) {
        var scope = $rootScope,
            modelManager = spReportModelManager(null),
            allFieldsNoData = angular.copy(reportTestData.allFields),
            model = modelManager.createModel();

        allFieldsNoData.gdata = [];

        model.reportId = 12345;

        // Get the report data
        modelManager.getReportData();
        scope.$digest();
        $timeout.flush();

        expect(model.gridOptions.rowData.length).toBe(5);

        // Simulate a page load
        model.startIndex = 5;
        modelManager.getReportData();
        scope.$digest();
        $timeout.flush();

        expect(model.gridOptions.rowData.length).toBe(10);

        // Simulate no more data by clearing the mock data
        spReportService.mockGetReportData(12345, allFieldsNoData);

        model.startIndex = 10;
        modelManager.getReportData();
        scope.$digest();
        $timeout.flush();

        expect(model.gridOptions.rowData.length).toBe(10);
        expect(model.moreDataAvailable).toBe(false);

        // Reset the mock data
        spReportService.mockGetReportData(12345, reportTestData.allFields);

        // Load grid from the first index
        model.startIndex = 0;
        model.moreDataAvailable = true;
        modelManager.getReportData();
        scope.$digest();
        $timeout.flush();

        expect(model.gridOptions.rowData.length).toBe(5);
    }));


    it('validate nav service object', inject(function ($rootScope, spReportModelManager, $timeout) {
        var scope = $rootScope,
            modelManager = spReportModelManager(null),
            navServiceObject,
            model = modelManager.createModel();

        model.reportId = 12345;

        // Get the report data
        modelManager.getReportData();
        scope.$digest();
        $timeout.flush();

        expect(model.gridOptions.rowData.length).toBe(5);

        // Verify a nav service object is returned
        navServiceObject = modelManager.getNavServiceData();
        expect(navServiceObject).toBeTruthy();

        // Validate nav service object methods
        expect(navServiceObject.selectedIndex).toBe(0);

        expect(navServiceObject.selected().id).toBe(4279);
        expect(navServiceObject.next().id).toBe(4266);
        expect(navServiceObject.prev().id).toBe(4279);

        // Set the selected item to be in the middle
        navServiceObject.setSelected({
            id: 3879
        });

        expect(navServiceObject.selectedIndex).toBe(3);

        expect(navServiceObject.selected().id).toBe(3879);
        expect(navServiceObject.next().id).toBe(4726);
        expect(navServiceObject.prev().id).toBe(3998);

        // Select the last object        
        navServiceObject.setSelected({
            id: 4726
        });

        expect(navServiceObject.selectedIndex).toBe(4);

        expect(navServiceObject.selected().id).toBe(4726);
        expect(navServiceObject.next().id).toBe(4726);
        expect(navServiceObject.prev().id).toBe(3879);

        // Select a missing object        
        navServiceObject.setSelected({
            id: 111111111111
        });

        expect(navServiceObject.selectedIndex).toBe(4);

        expect(navServiceObject.selected().id).toBe(4726);
        expect(navServiceObject.next().id).toBe(4726);
        expect(navServiceObject.prev().id).toBe(3879);
    }));
});