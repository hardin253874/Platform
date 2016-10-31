// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing color picker constants.    
    * It contains the following constants:
    * <ul>
    *   <li>namedColors - list of named colors</li>        
    * </ul>
    *
    * @module spColorPickerConstants    
    */
    angular.module('mod.common.ui.spColorPickerConstants', [])
        .constant('namedColors', [
            {
                name: 'Transparent',
                value: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                name: 'Black',
                value: { a: 255, r: 0, g: 0, b: 0 }
            },
            {
                name: 'White',
                value: { a: 255, r: 255, g: 255, b: 255 }
            },
            {
                name: 'Dim Gray',
                value: { a: 255, r: 105, g: 105, b: 105 }
            },
            {
                name: 'Gray',
                value: { a: 255, r: 128, g: 128, b: 128 }
            },
            {
                name: 'Dark Gray',
                value: { a: 255, r: 169, g: 169, b: 169 }
            },
            {
                name: 'Silver',
                value: { a: 255, r: 192, g: 192, b: 192 }
            },
            {
                name: 'Light Gray',
                value: { a: 255, r: 211, g: 211, b: 211 }
            },
            {
                name: 'Gainsboro',
                value: { a: 255, r: 220, g: 220, b: 220 }
            },
            {
                name: 'White Smoke',
                value: { a: 255, r: 245, g: 245, b: 245 }
            },
            {
                name: 'Maroon',
                value: { a: 255, r: 128, g: 0, b: 0 }
            },
            {
                name: 'Dark Red',
                value: { a: 255, r: 139, g: 0, b: 0 }
            },
            {
                name: 'Brown',
                value: { a: 255, r: 165, g: 42, b: 42 }
            },
            {
                name: 'Firebrick',
                value: { a: 255, r: 178, g: 34, b: 34 }
            },
            {
                name: 'Rosy Brown',
                value: { a: 255, r: 188, g: 143, b: 143 }
            },
            {
                name: 'Light Red',
                value: { a: 255, r: 255, g: 153, b: 153 }
            },
            {
                name: 'Vivid Red',
                value: { a: 255, r: 255, g: 90, b: 90 }
            },
            {
                name: 'Indian Red',
                value: { a: 255, r: 205, g: 92, b: 92 }
            },
            {
                name: 'Light Coral',
                value: { a: 255, r: 240, g: 128, b: 128 }
            },
            {
                name: 'Red',
                value: { a: 255, r: 255, g: 0, b: 0 }
            },
            {
                name: 'Snow',
                value: { a: 255, r: 255, g: 250, b: 250 }
            },
            {
                name: 'Misty Rose',
                value: { a: 255, r: 255, g: 228, b: 225 }
            },
            {
                name: 'Salmon',
                value: { a: 255, r: 250, g: 128, b: 114 }
            },
            {
                name: 'Tomato',
                value: { a: 255, r: 255, g: 99, b: 71 }
            },
            {
                name: 'Dark Salmon',
                value: { a: 255, r: 233, g: 150, b: 122 }
            },
            {
                name: 'Coral',
                value: { a: 255, r: 255, g: 127, b: 80 }
            },
            {
                name: 'Orange Red',
                value: { a: 255, r: 255, g: 69, b: 0 }
            },
            {
                name: 'Light Salmon',
                value: { a: 255, r: 255, g: 160, b: 122 }
            },
            {
                name: 'Sienna',
                value: { a: 255, r: 160, g: 82, b: 45 }
            },
            {
                name: 'Sea Shell',
                value: { a: 255, r: 255, g: 245, b: 238 }
            },
            {
                name: 'Saddle Brown',
                value: { a: 255, r: 139, g: 69, b: 19 }
            },
            {
                name: 'Chocolate',
                value: { a: 255, r: 210, g: 105, b: 30 }
            },
            {
                name: 'Sandy Brown',
                value: { a: 255, r: 244, g: 164, b: 96 }
            },
            {
                name: 'Peach Puff',
                value: { a: 255, r: 255, g: 218, b: 185 }
            },
            {
                name: 'Peru',
                value: { a: 255, r: 205, g: 133, b: 63 }
            },
            {
                name: 'Linen',
                value: { a: 255, r: 250, g: 240, b: 230 }
            },
            {
                name: 'Bisque',
                value: { a: 255, r: 255, g: 228, b: 196 }
            },
            {
                name: 'Dark Orange',
                value: { a: 255, r: 255, g: 140, b: 0 }
            },
            {
                name: 'Vivid Orange',
                value: { a: 255, r: 255, g: 132, b: 53 }
            },
            {
                name: 'Burly Wood',
                value: { a: 255, r: 222, g: 184, b: 135 }
            },
            {
                name: 'Antique White',
                value: { a: 255, r: 250, g: 235, b: 215 }
            },
            {
                name: 'Tan',
                value: { a: 255, r: 210, g: 180, b: 140 }
            },
            {
                name: 'Navajo White',
                value: { a: 255, r: 255, g: 222, b: 173 }
            },
            {
                name: 'Blanched Almond',
                value: { a: 255, r: 255, g: 235, b: 205 }
            },
            {
                name: 'Papaya Whip',
                value: { a: 255, r: 255, g: 239, b: 213 }
            },
            {
                name: 'Moccasin',
                value: { a: 255, r: 255, g: 228, b: 181 }
            },
            {
                name: 'Orange',
                value: { a: 255, r: 255, g: 165, b: 0 }
            },
            {
                name: 'Wheat',
                value: { a: 255, r: 245, g: 222, b: 179 }
            },
            {
                name: 'Old Lace',
                value: { a: 255, r: 253, g: 245, b: 230 }
            },
            {
                name: 'Floral White',
                value: { a: 255, r: 255, g: 250, b: 240 }
            },
            {
                name: 'Dark Goldenrod',
                value: { a: 255, r: 184, g: 134, b: 11 }
            },
            {
                name: 'Goldenrod',
                value: { a: 255, r: 218, g: 165, b: 32 }
            },
            {
                name: 'Cornsilk',
                value: { a: 255, r: 255, g: 248, b: 220 }
            },
            {
                name: 'Gold',
                value: { a: 255, r: 255, g: 215, b: 0 }
            },
            {
                name: 'Lemon Chiffon',
                value: { a: 255, r: 255, g: 250, b: 205 }
            },
            {
                name: 'Khaki',
                value: { a: 255, r: 240, g: 230, b: 140 }
            },
            {
                name: 'Pale Goldenrod',
                value: { a: 255, r: 238, g: 232, b: 170 }
            },
            {
                name: 'Dark Khaki',
                value: { a: 255, r: 189, g: 183, b: 107 }
            },
            {
                name: 'Beige',
                value: { a: 255, r: 245, g: 245, b: 220 }
            },
            {
                name: 'Olive',
                value: { a: 255, r: 128, g: 128, b: 0 }
            },
            {
                name: 'Light Goldenrod Yellow',
                value: { a: 255, r: 250, g: 250, b: 210 }
            },
            {
                name: 'Ivory',
                value: { a: 255, r: 255, g: 255, b: 240 }
            },
            {
                name: 'Light Yellow',
                value: { a: 255, r: 255, g: 255, b: 153 }
            },
            {
                name: 'Vivid Yellow',
                value: { a: 255, r: 255, g: 197, b: 40 }
            },
            {
                name: 'Yellow',
                value: { a: 255, r: 255, g: 255, b: 0 }
            },
            {
                name: 'Olive Drab',
                value: { a: 255, r: 107, g: 142, b: 35 }
            },
            {
                name: 'Yellow Green',
                value: { a: 255, r: 154, g: 205, b: 50 }
            },
            {
                name: 'Dark Olive Green',
                value: { a: 255, r: 85, g: 107, b: 47 }
            },
            {
                name: 'Green Yellow',
                value: { a: 255, r: 173, g: 255, b: 47 }
            },
            {
                name: 'Chartreuse',
                value: { a: 255, r: 127, g: 255, b: 0 }
            },
            {
                name: 'Lawn Green',
                value: { a: 255, r: 124, g: 252, b: 0 }
            },
            {
                name: 'Dark Green',
                value: { a: 255, r: 0, g: 100, b: 0 }
            },
            {
                name: 'Green',
                value: { a: 255, r: 0, g: 128, b: 0 }
            },
            {
                name: 'Forest Green',
                value: { a: 255, r: 34, g: 139, b: 34 }
            },
            {
                name: 'Dark Sea Green',
                value: { a: 255, r: 143, g: 188, b: 143 }
            },
            {
                name: 'Lime Green',
                value: { a: 255, r: 50, g: 205, b: 50 }
            },
            {
                name: 'Light Green',
                value: { a: 255, r: 153, g: 255, b: 153 }
            },
            {
                name: 'Vivid Green',
                value: { a: 255, r: 129, g: 255, b: 103 }
            },
            {
                name: 'Pale Green',
                value: { a: 255, r: 152, g: 251, b: 152 }
            },
            {
                name: 'Honeydew',
                value: { a: 255, r: 240, g: 255, b: 240 }
            },
            {
                name: 'Lime',
                value: { a: 255, r: 0, g: 255, b: 0 }
            },
            {
                name: 'Sea Green',
                value: { a: 255, r: 46, g: 139, b: 87 }
            },
            {
                name: 'Medium Sea Green',
                value: { a: 255, r: 60, g: 179, b: 113 }
            },
            {
                name: 'Spring Green',
                value: { a: 255, r: 0, g: 255, b: 127 }
            },
            {
                name: 'Mint Cream',
                value: { a: 255, r: 245, g: 255, b: 250 }
            },
            {
                name: 'Medium Spring Green',
                value: { a: 255, r: 0, g: 250, b: 154 }
            },
            {
                name: 'Medium Aquamarine',
                value: { a: 255, r: 102, g: 205, b: 170 }
            },
            {
                name: 'Aquamarine',
                value: { a: 255, r: 127, g: 255, b: 212 }
            },
            {
                name: 'Turquoise',
                value: { a: 255, r: 64, g: 224, b: 208 }
            },
            {
                name: 'Light Sea Green',
                value: { a: 255, r: 32, g: 178, b: 170 }
            },
            {
                name: 'Medium Turquoise',
                value: { a: 255, r: 72, g: 209, b: 204 }
            },
            {
                name: 'Dark Slate Gray',
                value: { a: 255, r: 47, g: 79, b: 79 }
            },
            {
                name: 'Teal',
                value: { a: 255, r: 0, g: 128, b: 128 }
            },
            {
                name: 'Dark Cyan',
                value: { a: 255, r: 0, g: 139, b: 139 }
            },
            {
                name: 'Pale Turquoise',
                value: { a: 255, r: 175, g: 238, b: 238 }
            },
            {
                name: 'Aqua',
                value: { a: 255, r: 0, g: 255, b: 255 }
            },
            {
                name: 'Azure',
                value: { a: 255, r: 240, g: 255, b: 255 }
            },
            {
                name: 'Cyan',
                value: { a: 255, r: 0, g: 255, b: 255 }
            },
            {
                name: 'Light Cyan',
                value: { a: 255, r: 224, g: 255, b: 255 }
            },
            {
                name: 'Dark Turquoise',
                value: { a: 255, r: 0, g: 206, b: 209 }
            },
            {
                name: 'Cadet Blue',
                value: { a: 255, r: 95, g: 158, b: 160 }
            },
            {
                name: 'Powder Blue',
                value: { a: 255, r: 176, g: 224, b: 230 }
            },
            {
                name: 'Light Blue',
                value: { a: 255, r: 153, g: 204, b: 255 }
            },
            {
                name: 'Vivid Blue',
                value: { a: 255, r: 90, g: 168, b: 255 }
            },
            {
                name: 'Deep Sky Blue',
                value: { a: 255, r: 0, g: 191, b: 255 }
            },
            {
                name: 'Sky Blue',
                value: { a: 255, r: 135, g: 206, b: 235 }
            },
            {
                name: 'Light Sky Blue',
                value: { a: 255, r: 135, g: 206, b: 250 }
            },
            {
                name: 'Steel Blue',
                value: { a: 255, r: 70, g: 130, b: 180 }
            },
            {
                name: 'Alice Blue',
                value: { a: 255, r: 240, g: 248, b: 255 }
            },
            {
                name: 'Dodger Blue',
                value: { a: 255, r: 30, g: 144, b: 255 }
            },
            {
                name: 'Slate Gray',
                value: { a: 255, r: 112, g: 128, b: 144 }
            },
            {
                name: 'Light Slate Gray',
                value: { a: 255, r: 119, g: 136, b: 153 }
            },
            {
                name: 'Light Steel Blue',
                value: { a: 255, r: 176, g: 196, b: 222 }
            },
            {
                name: 'Cornflower Blue',
                value: { a: 255, r: 100, g: 149, b: 237 }
            },
            {
                name: 'Royal Blue',
                value: { a: 255, r: 65, g: 105, b: 225 }
            },
            {
                name: 'Midnight Blue',
                value: { a: 255, r: 25, g: 25, b: 112 }
            },
            {
                name: 'Navy',
                value: { a: 255, r: 0, g: 0, b: 128 }
            },
            {
                name: 'Dark Blue',
                value: { a: 255, r: 0, g: 0, b: 139 }
            },
            {
                name: 'Medium Blue',
                value: { a: 255, r: 0, g: 0, b: 205 }
            },
            {
                name: 'Lavender',
                value: { a: 255, r: 230, g: 230, b: 250 }
            },
            {
                name: 'Blue',
                value: { a: 255, r: 0, g: 0, b: 255 }
            },
            {
                name: 'Ghost White',
                value: { a: 255, r: 248, g: 248, b: 255 }
            },
            {
                name: 'Slate Blue',
                value: { a: 255, r: 106, g: 90, b: 205 }
            },
            {
                name: 'Dark Slate Blue',
                value: { a: 255, r: 72, g: 61, b: 139 }
            },
            {
                name: 'Medium Slate Blue',
                value: { a: 255, r: 123, g: 104, b: 238 }
            },
            {
                name: 'Medium Purple',
                value: { a: 255, r: 147, g: 112, b: 219 }
            },
            {
                name: 'Blue Violet',
                value: { a: 255, r: 138, g: 43, b: 226 }
            },
            {
                name: 'Indigo',
                value: { a: 255, r: 75, g: 0, b: 130 }
            },
            {
                name: 'Dark Orchid',
                value: { a: 255, r: 153, g: 50, b: 204 }
            },
            {
                name: 'Dark Violet',
                value: { a: 255, r: 148, g: 0, b: 211 }
            },
            {
                name: 'Medium Orchid',
                value: { a: 255, r: 186, g: 85, b: 211 }
            },
            {
                name: 'Purple',
                value: { a: 255, r: 128, g: 0, b: 128 }
            },
            {
                name: 'Dark Magenta',
                value: { a: 255, r: 139, g: 0, b: 139 }
            },
            {
                name: 'Thistle',
                value: { a: 255, r: 216, g: 191, b: 216 }
            },
            {
                name: 'Plum',
                value: { a: 255, r: 221, g: 160, b: 221 }
            },
            {
                name: 'Violet',
                value: { a: 255, r: 238, g: 130, b: 238 }
            },
            {
                name: 'Fuchsia',
                value: { a: 255, r: 255, g: 0, b: 255 }
            },
            {
                name: 'Magenta',
                value: { a: 255, r: 255, g: 0, b: 255 }
            },
            {
                name: 'Orchid',
                value: { a: 255, r: 218, g: 112, b: 214 }
            },
            {
                name: 'Medium Violet Red',
                value: { a: 255, r: 199, g: 21, b: 133 }
            },
            {
                name: 'Deep Pink',
                value: { a: 255, r: 255, g: 20, b: 147 }
            },
            {
                name: 'Hot Pink',
                value: { a: 255, r: 255, g: 105, b: 180 }
            },
            {
                name: 'Lavender Blush',
                value: { a: 255, r: 255, g: 240, b: 245 }
            },
            {
                name: 'Pale Violet Red',
                value: { a: 255, r: 219, g: 112, b: 147 }
            },
            {
                name: 'Crimson',
                value: { a: 255, r: 220, g: 20, b: 60 }
            },
            {
                name: 'Pink',
                value: { a: 255, r: 255, g: 192, b: 203 }
            },
            {
                name: 'Light Pink',
                value: { a: 255, r: 255, g: 182, b: 193 }
            }
        ])
        .constant('namedColorsWithCustom', [
            {
                name: 'None',
                value: { a: 0, r: 0, g: 0, b: 0 }
            },
            {
                name: 'Custom',
                value: { a: 0, r: 128, g: 128, b: 128 }
            },
            {
                name: 'Transparent',
                value: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                name: 'Black',
                value: { a: 255, r: 0, g: 0, b: 0 }
            },
            {
                name: 'White',
                value: { a: 255, r: 255, g: 255, b: 255 }
            },
            {
                name: 'Dim Gray',
                value: { a: 255, r: 105, g: 105, b: 105 }
            },
            {
                name: 'Gray',
                value: { a: 255, r: 128, g: 128, b: 128 }
            },
            {
                name: 'Dark Gray',
                value: { a: 255, r: 169, g: 169, b: 169 }
            },
            {
                name: 'Silver',
                value: { a: 255, r: 192, g: 192, b: 192 }
            },
            {
                name: 'Light Gray',
                value: { a: 255, r: 211, g: 211, b: 211 }
            },
            {
                name: 'Gainsboro',
                value: { a: 255, r: 220, g: 220, b: 220 }
            },
            {
                name: 'White Smoke',
                value: { a: 255, r: 245, g: 245, b: 245 }
            },
            {
                name: 'Maroon',
                value: { a: 255, r: 128, g: 0, b: 0 }
            },
            {
                name: 'Dark Red',
                value: { a: 255, r: 139, g: 0, b: 0 }
            },
            {
                name: 'Brown',
                value: { a: 255, r: 165, g: 42, b: 42 }
            },
            {
                name: 'Firebrick',
                value: { a: 255, r: 178, g: 34, b: 34 }
            },
            {
                name: 'Rosy Brown',
                value: { a: 255, r: 188, g: 143, b: 143 }
            },
            {
                name: 'Light Red',
                value: { a: 255, r: 255, g: 153, b: 153 }
            },
            {
                name: 'Indian Red',
                value: { a: 255, r: 205, g: 92, b: 92 }
            },
            {
                name: 'Light Coral',
                value: { a: 255, r: 240, g: 128, b: 128 }
            },
            {
                name: 'Red',
                value: { a: 255, r: 255, g: 0, b: 0 }
            },
            {
                name: 'Snow',
                value: { a: 255, r: 255, g: 250, b: 250 }
            },
            {
                name: 'Misty Rose',
                value: { a: 255, r: 255, g: 228, b: 225 }
            },
            {
                name: 'Salmon',
                value: { a: 255, r: 250, g: 128, b: 114 }
            },
            {
                name: 'Tomato',
                value: { a: 255, r: 255, g: 99, b: 71 }
            },
            {
                name: 'Dark Salmon',
                value: { a: 255, r: 233, g: 150, b: 122 }
            },
            {
                name: 'Coral',
                value: { a: 255, r: 255, g: 127, b: 80 }
            },
            {
                name: 'Orange Red',
                value: { a: 255, r: 255, g: 69, b: 0 }
            },
            {
                name: 'Light Salmon',
                value: { a: 255, r: 255, g: 160, b: 122 }
            },
            {
                name: 'Sienna',
                value: { a: 255, r: 160, g: 82, b: 45 }
            },
            {
                name: 'Sea Shell',
                value: { a: 255, r: 255, g: 245, b: 238 }
            },
            {
                name: 'Saddle Brown',
                value: { a: 255, r: 139, g: 69, b: 19 }
            },
            {
                name: 'Chocolate',
                value: { a: 255, r: 210, g: 105, b: 30 }
            },
            {
                name: 'Sandy Brown',
                value: { a: 255, r: 244, g: 164, b: 96 }
            },
            {
                name: 'Peach Puff',
                value: { a: 255, r: 255, g: 218, b: 185 }
            },
            {
                name: 'Peru',
                value: { a: 255, r: 205, g: 133, b: 63 }
            },
            {
                name: 'Linen',
                value: { a: 255, r: 250, g: 240, b: 230 }
            },
            {
                name: 'Bisque',
                value: { a: 255, r: 255, g: 228, b: 196 }
            },
            {
                name: 'Dark Orange',
                value: { a: 255, r: 255, g: 140, b: 0 }
            },
            {
                name: 'Burly Wood',
                value: { a: 255, r: 222, g: 184, b: 135 }
            },
            {
                name: 'Antique White',
                value: { a: 255, r: 250, g: 235, b: 215 }
            },
            {
                name: 'Tan',
                value: { a: 255, r: 210, g: 180, b: 140 }
            },
            {
                name: 'Navajo White',
                value: { a: 255, r: 255, g: 222, b: 173 }
            },
            {
                name: 'Blanched Almond',
                value: { a: 255, r: 255, g: 235, b: 205 }
            },
            {
                name: 'Papaya Whip',
                value: { a: 255, r: 255, g: 239, b: 213 }
            },
            {
                name: 'Moccasin',
                value: { a: 255, r: 255, g: 228, b: 181 }
            },
            {
                name: 'Orange',
                value: { a: 255, r: 255, g: 106, b: 0 }
            },
            {
                name: 'Wheat',
                value: { a: 255, r: 245, g: 222, b: 179 }
            },
            {
                name: 'Old Lace',
                value: { a: 255, r: 253, g: 245, b: 230 }
            },
            {
                name: 'Floral White',
                value: { a: 255, r: 255, g: 250, b: 240 }
            },
            {
                name: 'Dark Goldenrod',
                value: { a: 255, r: 184, g: 134, b: 11 }
            },
            {
                name: 'Goldenrod',
                value: { a: 255, r: 218, g: 165, b: 32 }
            },
            {
                name: 'Cornsilk',
                value: { a: 255, r: 255, g: 248, b: 220 }
            },
            {
                name: 'Gold',
                value: { a: 255, r: 255, g: 162, b: 0 }
            },
            {
                name: 'Lemon Chiffon',
                value: { a: 255, r: 255, g: 250, b: 205 }
            },
            {
                name: 'Khaki',
                value: { a: 255, r: 240, g: 230, b: 140 }
            },
            {
                name: 'Pale Goldenrod',
                value: { a: 255, r: 238, g: 232, b: 170 }
            },
            {
                name: 'Dark Khaki',
                value: { a: 255, r: 189, g: 183, b: 107 }
            },
            {
                name: 'Beige',
                value: { a: 255, r: 245, g: 245, b: 220 }
            },
            {
                name: 'Olive',
                value: { a: 255, r: 128, g: 128, b: 0 }
            },
            {
                name: 'Light Goldenrod Yellow',
                value: { a: 255, r: 250, g: 250, b: 210 }
            },
            {
                name: 'Ivory',
                value: { a: 255, r: 255, g: 255, b: 240 }
            },
            {
                name: 'Light Yellow',
                value: { a: 255, r: 255, g: 255, b: 153 }
            },
            {
                name: 'Yellow',
                value: { a: 255, r: 255, g: 255, b: 0 }
            },
            {
                name: 'Olive Drab',
                value: { a: 255, r: 107, g: 142, b: 35 }
            },
            {
                name: 'Yellow Green',
                value: { a: 255, r: 154, g: 205, b: 50 }
            },
            {
                name: 'Dark Olive Green',
                value: { a: 255, r: 85, g: 107, b: 47 }
            },
            {
                name: 'Green Yellow',
                value: { a: 255, r: 173, g: 255, b: 47 }
            },
            {
                name: 'Chartreuse',
                value: { a: 255, r: 127, g: 255, b: 0 }
            },
            {
                name: 'Lawn Green',
                value: { a: 255, r: 124, g: 252, b: 0 }
            },
            {
                name: 'Dark Green',
                value: { a: 255, r: 0, g: 100, b: 0 }
            },
            {
                name: 'Green',
                value: { a: 255, r: 0, g: 128, b: 0 }
            },
            {
                name: 'Forest Green',
                value: { a: 255, r: 34, g: 139, b: 34 }
            },
            {
                name: 'Dark Sea Green',
                value: { a: 255, r: 143, g: 188, b: 143 }
            },
            {
                name: 'Lime Green',
                value: { a: 255, r: 50, g: 205, b: 50 }
            },
            {
                name: 'Light Green',
                value: { a: 255, r: 153, g: 255, b: 153 }
            },
            {
                name: 'Pale Green',
                value: { a: 255, r: 152, g: 251, b: 152 }
            },
            {
                name: 'Honeydew',
                value: { a: 255, r: 240, g: 255, b: 240 }
            },
            {
                name: 'Lime',
                value: { a: 255, r: 0, g: 255, b: 0 }
            },
            {
                name: 'Sea Green',
                value: { a: 255, r: 46, g: 139, b: 87 }
            },
            {
                name: 'Medium Sea Green',
                value: { a: 255, r: 60, g: 179, b: 113 }
            },
            {
                name: 'Spring Green',
                value: { a: 255, r: 0, g: 255, b: 127 }
            },
            {
                name: 'Mint Cream',
                value: { a: 255, r: 245, g: 255, b: 250 }
            },
            {
                name: 'Medium Spring Green',
                value: { a: 255, r: 0, g: 250, b: 154 }
            },
            {
                name: 'Medium Aquamarine',
                value: { a: 255, r: 102, g: 205, b: 170 }
            },
            {
                name: 'Aquamarine',
                value: { a: 255, r: 127, g: 255, b: 212 }
            },
            {
                name: 'Turquoise',
                value: { a: 255, r: 64, g: 224, b: 208 }
            },
            {
                name: 'Light Sea Green',
                value: { a: 255, r: 32, g: 178, b: 170 }
            },
            {
                name: 'Medium Turquoise',
                value: { a: 255, r: 72, g: 209, b: 204 }
            },
            {
                name: 'Dark Slate Gray',
                value: { a: 255, r: 47, g: 79, b: 79 }
            },
            {
                name: 'Teal',
                value: { a: 255, r: 0, g: 128, b: 128 }
            },
            {
                name: 'Dark Cyan',
                value: { a: 255, r: 0, g: 139, b: 139 }
            },
            {
                name: 'Pale Turquoise',
                value: { a: 255, r: 175, g: 238, b: 238 }
            },
            {
                name: 'Aqua',
                value: { a: 255, r: 0, g: 255, b: 255 }
            },
            {
                name: 'Azure',
                value: { a: 255, r: 240, g: 255, b: 255 }
            },
            {
                name: 'Cyan',
                value: { a: 255, r: 0, g: 255, b: 255 }
            },
            {
                name: 'Light Cyan',
                value: { a: 255, r: 224, g: 255, b: 255 }
            },
            {
                name: 'Dark Turquoise',
                value: { a: 255, r: 0, g: 206, b: 209 }
            },
            {
                name: 'Cadet Blue',
                value: { a: 255, r: 95, g: 158, b: 160 }
            },
            {
                name: 'Powder Blue',
                value: { a: 255, r: 176, g: 224, b: 230 }
            },
            {
                name: 'Light Blue',
                value: { a: 255, r: 153, g: 204, b: 255 }
            },
            {
                name: 'Deep Sky Blue',
                value: { a: 255, r: 0, g: 191, b: 255 }
            },
            {
                name: 'Sky Blue',
                value: { a: 255, r: 135, g: 206, b: 235 }
            },
            {
                name: 'Light Sky Blue',
                value: { a: 255, r: 135, g: 206, b: 250 }
            },
            {
                name: 'Steel Blue',
                value: { a: 255, r: 70, g: 130, b: 180 }
            },
            {
                name: 'Alice Blue',
                value: { a: 255, r: 240, g: 248, b: 255 }
            },
            {
                name: 'Dodger Blue',
                value: { a: 255, r: 30, g: 144, b: 255 }
            },
            {
                name: 'Slate Gray',
                value: { a: 255, r: 112, g: 128, b: 144 }
            },
            {
                name: 'Light Slate Gray',
                value: { a: 255, r: 119, g: 136, b: 153 }
            },
            {
                name: 'Light Steel Blue',
                value: { a: 255, r: 176, g: 196, b: 222 }
            },
            {
                name: 'Cornflower Blue',
                value: { a: 255, r: 100, g: 149, b: 237 }
            },
            {
                name: 'Royal Blue',
                value: { a: 255, r: 65, g: 105, b: 225 }
            },
            {
                name: 'Midnight Blue',
                value: { a: 255, r: 25, g: 25, b: 112 }
            },
            {
                name: 'Navy',
                value: { a: 255, r: 0, g: 0, b: 128 }
            },
            {
                name: 'Dark Blue',
                value: { a: 255, r: 0, g: 0, b: 139 }
            },
            {
                name: 'Medium Blue',
                value: { a: 255, r: 0, g: 0, b: 205 }
            },
            {
                name: 'Lavender',
                value: { a: 255, r: 230, g: 230, b: 250 }
            },
            {
                name: 'Blue',
                value: { a: 255, r: 0, g: 0, b: 255 }
            },
            {
                name: 'Ghost White',
                value: { a: 255, r: 248, g: 248, b: 255 }
            },
            {
                name: 'Slate Blue',
                value: { a: 255, r: 106, g: 90, b: 205 }
            },
            {
                name: 'Dark Slate Blue',
                value: { a: 255, r: 72, g: 61, b: 139 }
            },
            {
                name: 'Medium Slate Blue',
                value: { a: 255, r: 123, g: 104, b: 238 }
            },
            {
                name: 'Medium Purple',
                value: { a: 255, r: 147, g: 112, b: 219 }
            },
            {
                name: 'Blue Violet',
                value: { a: 255, r: 138, g: 43, b: 226 }
            },
            {
                name: 'Indigo',
                value: { a: 255, r: 75, g: 0, b: 130 }
            },
            {
                name: 'Dark Orchid',
                value: { a: 255, r: 153, g: 50, b: 204 }
            },
            {
                name: 'Dark Violet',
                value: { a: 255, r: 148, g: 0, b: 211 }
            },
            {
                name: 'Medium Orchid',
                value: { a: 255, r: 186, g: 85, b: 211 }
            },
            {
                name: 'Purple',
                value: { a: 255, r: 128, g: 0, b: 128 }
            },
            {
                name: 'Dark Magenta',
                value: { a: 255, r: 139, g: 0, b: 139 }
            },
            {
                name: 'Thistle',
                value: { a: 255, r: 216, g: 191, b: 216 }
            },
            {
                name: 'Plum',
                value: { a: 255, r: 221, g: 160, b: 221 }
            },
            {
                name: 'Violet',
                value: { a: 255, r: 238, g: 130, b: 238 }
            },
            {
                name: 'Fuchsia',
                value: { a: 255, r: 255, g: 0, b: 255 }
            },
            {
                name: 'Magenta',
                value: { a: 255, r: 255, g: 0, b: 255 }
            },
            {
                name: 'Orchid',
                value: { a: 255, r: 218, g: 112, b: 214 }
            },
            {
                name: 'Medium Violet Red',
                value: { a: 255, r: 199, g: 21, b: 133 }
            },
            {
                name: 'Deep Pink',
                value: { a: 255, r: 255, g: 20, b: 147 }
            },
            {
                name: 'Hot Pink',
                value: { a: 255, r: 255, g: 105, b: 180 }
            },
            {
                name: 'Lavender Blush',
                value: { a: 255, r: 255, g: 240, b: 245 }
            },
            {
                name: 'Pale Violet Red',
                value: { a: 255, r: 219, g: 112, b: 147 }
            },
            {
                name: 'Crimson',
                value: { a: 255, r: 220, g: 20, b: 60 }
            },
            {
                name: 'Pink',
                value: { a: 255, r: 255, g: 192, b: 203 }
            },
            {
                name: 'Light Pink',
                value: { a: 255, r: 255, g: 182, b: 193 }
            }
        ])
        .constant('namedFgBgColors', [
            {
                id: 'none',
                name: 'None',
                foregroundColor: { a: 255, r: 69, g: 80, b: 94 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'custom',
                name: 'Custom',
                foregroundColor: { a: 255, r: 69, g: 80, b: 94 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'blackOnRed',
                name: 'Black on Red',
                foregroundColor: { a: 255, r: 0, g: 0, b: 0 },
                backgroundColor: { a: 255, r: 255, g: 90, b: 90 }
            },
            {
                id: 'blackOnOrange',
                name: 'Black on Orange',
                foregroundColor: { a: 255, r: 0, g: 0, b: 0 },
                backgroundColor: { a: 255, r: 255, g: 132, b: 53 }
            },
            {
                id: 'blackOnYellow',
                name: 'Black on Yellow',
                foregroundColor: { a: 255, r: 0, g: 0, b: 0 },
                backgroundColor: { a: 255, r: 255, g: 197, b: 40 }
            },
            {
                id: 'blackOnGreen',
                name: 'Black on Green',
                foregroundColor: { a: 255, r: 0, g: 0, b: 0 },
                backgroundColor: { a: 255, r: 129, g: 255, b: 103 }
            },
            {
                id: 'blackOnBlue',
                name: 'Black on Blue',
                foregroundColor: { a: 255, r: 0, g: 0, b: 0 },
                backgroundColor: { a: 255, r: 90, g: 168, b: 255 }
            },
            {
                id: 'redText',
                name: 'Red text',
                foregroundColor: { a: 255, r: 255, g: 0, b: 0 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'orangeText',
                name: 'Orange text',
                foregroundColor: { a: 255, r: 255, g: 165, b: 0 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'goldText',
                name: 'Gold text',
                foregroundColor: { a: 255, r: 255, g: 162, b: 0 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'greenText',
                name: 'Green text',
                foregroundColor: { a: 255, r: 0, g: 128, b: 0 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'blueText',
                name: 'Blue text',
                foregroundColor: { a: 255, r: 0, g: 0, b: 255 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            },
            {
                id: 'brownText',
                name: 'Brown text',
                foregroundColor: { a: 255, r: 165, g: 42, b: 42 },
                backgroundColor: { a: 0, r: 255, g: 255, b: 255 }
            }
            
        ]);
}());