// Copyright 2011-2016 Global Software Innovation Pty Ltd
var chartBuilderTestData;
(function (chartBuilderTestData) {
    // GET:  (with correct ID for 'Internet Access' report)
    // https://syd1dev19.entdata.local/spapi/data/v1/report/7105?metadata=basic&page=0,0
    chartBuilderTestData.internetAccessReport =
    {
        "meta": {
            "title": "Internet Access",
            "typefmtstyle": {
                "String": [
                   "Highlight",
                   "Icon"
                ],
                "Int32": [
                   "Highlight",
                   "Icon",
                   "ProgressBar"
                ]
            },
            "rcols": {
                "9857": {
                    "ord": 0,
                    "title": "State",
                    "type": "String",
                    "fid": 7779
                },
                "5813": {
                    "ord": 1,
                    "title": "With Internet",
                    "type": "Int32",
                    "fid": 8852
                },
                "9696": {
                    "ord": 2,
                    "title": "Without Internet",
                    "type": "Int32",
                    "fid": 7934
                }
            },
            "anlcols": {
                "5388": {
                    "ord": 1,
                    "title": "Without Internet",
                    "type": "Int32",
                    "oper": "Unspecified",
                    "doper": "GreaterThan"
                },
                "5686": {
                    "ord": 2,
                    "title": "With Internet",
                    "type": "Int32",
                    "oper": "Unspecified",
                    "doper": "GreaterThan"
                },
                "7495": {
                    "ord": 0,
                    "title": "State",
                    "type": "String",
                    "oper": "Unspecified",
                    "doper": "Contains"
                }
            }
        }
    };


})(chartBuilderTestData || (chartBuilderTestData = {}));
