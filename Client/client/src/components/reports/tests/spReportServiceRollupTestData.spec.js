// Copyright 2011-2016 Global Software Innovation Pty Ltd
var reportTestData;
(function (reportTestData) {
    // GET:
    // https://syd1dev20.entdata.local/spapi/data/v1/report/xxx?metadata=full&page=0,1000
    reportTestData.rollupReport = {
        "meta": {
            "title": "Temperatures",
            "typefmtstyle": {
                "DateTime": ["Highlight",
                "Icon",
                "ProgressBar"],
                "Decimal": ["Highlight",
                "Icon",
                "ProgressBar"]
            },
            "rcols": {
                "6600": {
                    "ord": 0,
                    "title": "Month",
                    "type": "DateTime",
                    "fid": 10071
                },
                "8921": {
                    "ord": 1,
                    "title": "Season",
                    "type": "String",
                    "hide": true,
                    "fid": 4952
                },
                "6184": {
                    "ord": 2,
                    "title": "Min",
                    "type": "Decimal",
                    "fid": 8370,
                    "places": 3
                },
                "10667": {
                    "ord": 3,
                    "title": "Max",
                    "type": "Decimal",
                    "fid": 9017,
                    "places": 3
                }
            },
            "sort": [{
                "colid": "8921",
                "order": "Ascending"
            }],
            "anlcols": {
                "5392": {
                    "ord": 1,
                    "title": "Season",
                    "type": "String",
                    "oper": "Unspecified",
                    "doper": "Contains"
                },
                "6758": {
                    "ord": 0,
                    "title": "Month",
                    "type": "DateTime",
                    "oper": "Unspecified",
                    "doper": "GreaterThan"
                },
                "7874": {
                    "ord": 3,
                    "title": "Max",
                    "type": "Decimal",
                    "oper": "Unspecified",
                    "doper": "GreaterThan"
                },
                "9601": {
                    "ord": 2,
                    "title": "Min",
                    "type": "Decimal",
                    "oper": "Unspecified",
                    "doper": "GreaterThan"
                }
            },
            "valrules": {
                "6184": {
                    "places": 3
                },
                "10667": {
                    "places": 3
                }
            },
            "rmeta": {
                "showgt": true,
                "showst": true,
                "groups": [{
                    "8921": {
                        "style": "groupList",
                        "value": "Season"
                    }
                }],
                "aggs": {
                    "6184": [{
                        "style": "aggCount",
                        "type": "Int32"
                    },
                    {
                        "style": "aggSum",
                        "type": "Decimal"
                    },
                    {
                        "style": "aggMin",
                        "type": "Decimal"
                    },
                    {
                        "style": "aggMax",
                        "type": "Decimal"
                    }]
                }
            },
            "rdata": [{
                "map": 1,
                "total": 12,
                "hdrs": [{
                    "8921": {
                        "val": ""
                    }
                }],
                "aggs": {
                    "6184": [{
                        "value": "12"
                    },
                    {
                        "value": "164.600"
                    },
                    {
                        "value": "8.000"
                    },
                    {
                        "value": "18.700"
                    }]
                }
            },
            {
                "map": 0,
                "total": 3,
                "hdrs": [{
                    "8921": {
                        "val": "Autumn"
                    }
                }],
                "aggs": {
                    "6184": [{
                        "value": "3"
                    },
                    {
                        "value": "54.800"
                    },
                    {
                        "value": "17.500"
                    },
                    {
                        "value": "18.700"
                    }]
                }
            },
            {
                "map": 0,
                "total": 3,
                "hdrs": [{
                    "8921": {
                        "val": "Spring"
                    }
                }],
                "aggs": {
                    "6184": [{
                        "value": "3"
                    },
                    {
                        "value": "27.900"
                    },
                    {
                        "value": "8.000"
                    },
                    {
                        "value": "11.000"
                    }]
                }
            },
            {
                "map": 0,
                "total": 3,
                "hdrs": [{
                    "8921": {
                        "val": "Summer"
                    }
                }],
                "aggs": {
                    "6184": [{
                        "value": "3"
                    },
                    {
                        "value": "46.500"
                    },
                    {
                        "value": "13.500"
                    },
                    {
                        "value": "17.500"
                    }]
                }
            },
            {
                "map": 0,
                "total": 3,
                "hdrs": [{
                    "8921": {
                        "val": "Winter"
                    }
                }],
                "aggs": {
                    "6184": [{
                        "value": "3"
                    },
                    {
                        "value": "35.400"
                    },
                    {
                        "value": "9.200"
                    },
                    {
                        "value": "14.700"
                    }]
                }
            }]
        },
        "gdata": [{
            "eid": 6033,
            "values": [{
                "val": "2013-03-31T13:00:00Z"
            },
            {
                "val": "Autumn"
            },
            {
                "val": "17.500"
            },
            {
                "val": "24.700"
            }]
        },
        {
            "eid": 6694,
            "values": [{
                "val": "2013-02-28T13:00:00Z"
            },
            {
                "val": "Autumn"
            },
            {
                "val": "18.700"
            },
            {
                "val": "25.700"
            }]
        },
        {
            "eid": 8482,
            "values": [{
                "val": "2013-01-31T13:00:00Z"
            },
            {
                "val": "Autumn"
            },
            {
                "val": "18.600"
            },
            {
                "val": "25.800"
            }]
        },
        {
            "eid": 6304,
            "values": [{
                "val": "2013-07-31T14:00:00Z"
            },
            {
                "val": "Spring"
            },
            {
                "val": "8.000"
            },
            {
                "val": "16.200"
            }]
        },
        {
            "eid": 6663,
            "values": [{
                "val": "2013-09-30T14:00:00Z"
            },
            {
                "val": "Spring"
            },
            {
                "val": "11.000"
            },
            {
                "val": "19.900"
            }]
        },
        {
            "eid": 9611,
            "values": [{
                "val": "2013-08-31T14:00:00Z"
            },
            {
                "val": "Spring"
            },
            {
                "val": "8.900"
            },
            {
                "val": "17.700"
            }]
        },
        {
            "eid": 4766,
            "values": [{
                "val": "2013-10-31T13:00:00Z"
            },
            {
                "val": "Summer"
            },
            {
                "val": "13.500"
            },
            {
                "val": "22.000"
            }]
        },
        {
            "eid": 6396,
            "values": [{
                "val": "2013-11-30T13:00:00Z"
            },
            {
                "val": "Summer"
            },
            {
                "val": "15.500"
            },
            {
                "val": "23.600"
            }]
        },
        {
            "eid": 9880,
            "values": [{
                "val": "2012-12-31T13:00:00Z"
            },
            {
                "val": "Summer"
            },
            {
                "val": "17.500"
            },
            {
                "val": "25.100"
            }]
        },
        {
            "eid": 6904,
            "values": [{
                "val": "2013-05-31T14:00:00Z"
            },
            {
                "val": "Winter"
            },
            {
                "val": "11.500"
            },
            {
                "val": "19.300"
            }]
        },
        {
            "eid": 8203,
            "values": [{
                "val": "2013-04-30T14:00:00Z"
            },
            {
                "val": "Winter"
            },
            {
                "val": "14.700"
            },
            {
                "val": "22.400"
            }]
        },
        {
            "eid": 10241,
            "values": [{
                "val": "2013-06-30T14:00:00Z"
            },
            {
                "val": "Winter"
            },
            {
                "val": "9.200"
            },
            {
                "val": "16.900"
            }]
        }]
    };
})(reportTestData || (reportTestData = {}));