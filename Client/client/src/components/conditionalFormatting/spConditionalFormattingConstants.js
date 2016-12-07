// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing conditional formatting constants.
    * It contains the following constants:
    * <ul>
    * </ul>

    * @module spConditionalFormattingConstants
    */
    angular.module('mod.ui.spConditionalFormattingConstants', ['mod.common.ui.spColorPickerConstants'])
        .factory('condFormattingConstants', function (namedFgBgColors) {
            // This is implemented as a factory rather than using constant as the returned objects reference each other
            var exportedConstants = {};

            exportedConstants.progressBarSchemes = [
                {
                    name: 'None',
                    color: {
                        foregroundColor: { a: 255, r: 0, g: 0, b: 0 }
                    }
                },
                {
                    name: 'Light Red',
                    color: {
                        foregroundColor: { a: 255, r: 255, g: 153, b: 153 }
                    }
                },
                {
                    name: 'Light Yellow',
                    color: {
                        foregroundColor: { a: 255, r: 255, g: 255, b: 102 }
                    }
                },
                {
                    name: 'Light Green',
                    color: {
                        foregroundColor: { a: 255, r: 144, g: 238, b: 144 }
                    }
                },
                {
                    name: 'Light Blue',
                    color: {
                        foregroundColor: { a: 255, r: 173, g: 216, b: 230 }
                    }
                }
            ];


            function getNamedFgBgColor(colorId) {
                return _.find(namedFgBgColors, function (fgBgColor) {
                    return fgBgColor.id === colorId;
                });
            }


            exportedConstants.highlightSchemes = [
                {
                    name: 'None',
                    length: 1,
                    colors: [
                        getNamedFgBgColor('none')
                    ]
                },
                {
                    name: '4 step - Red, Yellow, Green, Blue',
                    length: 4,
                    colors: [
                        getNamedFgBgColor('blackOnRed'),
                        getNamedFgBgColor('blackOnYellow'),
                        getNamedFgBgColor('blackOnGreen'),
                        getNamedFgBgColor('blackOnBlue')
                    ]
                },
                {
                    name: '3 step - Red, Yellow, Green',
                    length: 3,
                    colors: [
                        getNamedFgBgColor('blackOnRed'),
                        getNamedFgBgColor('blackOnYellow'),
                        getNamedFgBgColor('blackOnGreen')
                    ]
                },
                {
                    name: '2 step - Red, Green',
                    length: 2,
                    colors: [
                        getNamedFgBgColor('blackOnRed'),
                        getNamedFgBgColor('blackOnGreen')
                    ]
                },
                {
                    name: '2 step - Red Highlight',
                    length: 2,
                    colors: [
                        getNamedFgBgColor('blackOnRed'),
                        getNamedFgBgColor('none')
                    ]
                },
                {
                    name: '2 step - Green Highlight',
                    length: 2,
                    colors: [
                        getNamedFgBgColor('blackOnGreen'),
                        getNamedFgBgColor('none')
                    ]
                }
            ];

            exportedConstants.iconSchemes = [
                {
                    name: 'None',
                    length: 1,
                    icons: [
                        new spEntity.EntityRef('blackCircleCondFormatIcon')
                    ]
                },
                {
                    name: '4 step - Circle',
                    length: 4,
                    icons: [
                        new spEntity.EntityRef('greenCircleCondFormatIcon'),
                        new spEntity.EntityRef('yellowCircleCondFormatIcon'),
                        new spEntity.EntityRef('redCircleCondFormatIcon'),
                        new spEntity.EntityRef('blackCircleCondFormatIcon')
                    ]
                },
                {
                    name: '3 step - Arrow',
                    length: 3,
                    icons: [
                        new spEntity.EntityRef('greenUpArrowCondFormatIcon'),
                        new spEntity.EntityRef('yellowRightArrowCondFormatIcon'),
                        new spEntity.EntityRef('redDownArrowCondFormatIcon')
                    ]
                },
                {
                    name: '3 step - Circle',
                    length: 3,
                    icons: [
                        new spEntity.EntityRef('greenCircleCondFormatIcon'),
                        new spEntity.EntityRef('yellowCircleCondFormatIcon'),
                        new spEntity.EntityRef('redCircleCondFormatIcon')
                    ]
                },
                {
                    name: '3 step - Other',
                    length: 3,
                    icons: [
                        new spEntity.EntityRef('greenTickCondFormatIcon'),
                        new spEntity.EntityRef('yellowExclamationMarkCondFormatIcon'),
                        new spEntity.EntityRef('redCrossCondFormatIcon')
                    ]
                },
                {
                    name: '2 step - Arrow',
                    length: 2,
                    icons: [
                        new spEntity.EntityRef('greenUpArrowCondFormatIcon'),
                        new spEntity.EntityRef('redDownArrowCondFormatIcon')
                    ]
                },
                {
                    name: '2 step - Circle',
                    length: 2,
                    icons: [
                        new spEntity.EntityRef('greenCircleCondFormatIcon'),
                        new spEntity.EntityRef('redCircleCondFormatIcon')
                    ]
                },
                {
                    name: '2 step - Other',
                    length: 2,
                    icons: [
                        new spEntity.EntityRef('greenTickCondFormatIcon'),
                        new spEntity.EntityRef('redCrossCondFormatIcon')
                    ]
                }
            ];

            exportedConstants.formatTypeEnum = {
                None: 'None',
                Highlight: 'Highlight',
                Icon: 'Icon',
                ProgressBar: 'ProgressBar'
            };

            exportedConstants.formatTypes = [
                {
                    id: exportedConstants.formatTypeEnum.None,
                    name: 'None',
                    schemes: [
                        {
                            length: 0,
                            name: 'None'
                        }
                    ]
                },
                {
                    id: exportedConstants.formatTypeEnum.Highlight,
                    name: 'Highlight',
                    schemes: exportedConstants.highlightSchemes
                },
                {
                    id: exportedConstants.formatTypeEnum.Icon,
                    name: 'Icon',
                    schemes: exportedConstants.iconSchemes
                },
                {
                    id: exportedConstants.formatTypeEnum.ProgressBar,
                    name: 'Progress Bar',
                    schemes: exportedConstants.progressBarSchemes
                }
            ];

            exportedConstants.defaultTypeFormats = {
                String: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon
                ],
                Int32: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon,
                    exportedConstants.formatTypeEnum.ProgressBar
                ],
                Decimal: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon,
                    exportedConstants.formatTypeEnum.ProgressBar
                ],
                Currency: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon,
                    exportedConstants.formatTypeEnum.ProgressBar
                ],
                DateTime: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon,
                    exportedConstants.formatTypeEnum.ProgressBar
                ],
                Date: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon,
                    exportedConstants.formatTypeEnum.ProgressBar
                ],
                Time: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon,
                    exportedConstants.formatTypeEnum.ProgressBar
                ],
                Bool: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon
                ],
                ChoiceRelationship: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon
                ],
                RelatedResource: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon
                ],
                InlineRelationship: [
                    exportedConstants.formatTypeEnum.Highlight,
                    exportedConstants.formatTypeEnum.Icon
                ]
            };


            exportedConstants.dateTimeFormats =
            {
                Time: [
                    {
                        name: 'Default',
                        formatName: 'time12Hour'
                    },
                    {
                        name: '24 Hour',
                        formatName: 'time24Hour'
                    },
                    {
                        name: 'Hour only',
                        formatName: 'timeHour'
                    }
                ],
                Date: [
                    {
                        name: 'Default',
                        formatName: 'dateShort'
                    },
                    {
                        name: 'Day Month',
                        formatName: 'dateDayMonth'
                    },
                    {
                        name: 'Long',
                        formatName: 'dateLong'
                    },
                    {
                        name: 'Month',
                        formatName: 'dateMonth'
                    },
                    {
                        name: 'Month Year',
                        formatName: 'dateMonthYear'
                    },
                    {
                        name: 'Quarter',
                        formatName: 'dateQuarter'
                    },
                    {
                        name: 'Quarter Year',
                        formatName: 'dateQuarterYear'
                    },
                    {
                        name: 'Year',
                        formatName: 'dateYear'
                    },
                    {
                        name: 'Weekday',
                        formatName: 'dateWeekday'
                    }
                ],
                DateTime: [
                    {
                        name: 'Default',
                        formatName: 'dateTimeShort'
                    },
                    {
                        name: '24 Hour',
                        formatName: 'dateTime24Hour'
                    },
                    {
                        name: 'Day Month',
                        formatName: 'dateTimeDayMonth'
                    },
                    {
                        name: 'Day Month Time',
                        formatName: 'dateTimeDayMonthTime'
                    },
                    {
                        name: 'Long',
                        formatName: 'dateTimeLong'
                    },
                    {
                        name: 'Sortable',
                        formatName: 'dateTimeSortable'
                    },
                    {
                        name: 'Date only',
                        formatName: 'dateTimeDate'
                    },
                    {
                        name: 'Time only',
                        formatName: 'dateTimeTime'
                    },
                    {
                        name: 'Month',
                        formatName: 'dateTimeMonth'
                    },
                    {
                        name: 'Month Year',
                        formatName: 'dateTimeMonthYear'
                    },
                    {
                        name: 'Quarter',
                        formatName: 'dateTimeQuarter'
                    },
                    {
                        name: 'Quarter Year',
                        formatName: 'dateTimeQuarterYear'
                    },
                    {
                        name: 'Year',
                        formatName: 'dateTimeYear'
                    },
                    {
                        name: 'Weekday',
                        formatName: 'dateTimeWeekday'
                    },
                    {
                        name: 'Hour only',
                        formatName: 'dateTimeHour'
                    }
                ]
            };


            exportedConstants.booleanFormats = [
                {
                    id: 'YesNo',
                    name: 'Yes / No'
                },
                {
                    id: 'TrueFalse',
                    name: 'True / False'
                }
            ];


            exportedConstants.alignmentOptions = [
                {
                    id: 'Default',
                    name: 'Default'
                },
                {
                    id: 'Left',
                    name: 'Left'
                },
                {
                    id: 'Centre',
                    name: 'Centre'
                },
                {
                    id: 'Right',
                    name: 'Right'
                }
            ];

            exportedConstants.entityListFormatOptions = [
                {
                    id: 'commaSeparatedList',
                    name: 'Comma separated list'
                },
                {
                    id: 'stackedList',
                    name: 'Vertical list'
                }
            ];

            return exportedConstants;
        });
}());