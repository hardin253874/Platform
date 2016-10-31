// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
   * svgRelationshipInfo displays relationship cardinality, ownership info using svg.
   *
   * @module svgRelationshipInfo
   * @example

   Using the svgRelationshipInfo:

   &lt;sp-date-control model="myModel"&gt;&lt;/sp-date-control&gt

   where options is available on the scope with the following members:

   Properties:
       - options {object} - options.
   
   */

    angular.module('mod.app.configureDialog.relationshipProperties.svgRelationshipInfo', ['ui.bootstrap', 'mod.app.editForm.designerDirectives', 'mod.common.ui.spDialogService'])
        .directive('svgRelationshipInfo', function () {
            return {
                restrict: 'E',
                transclude: false,
                replace: false,
                scope: {
                    options: '=?'
                },
                templateUrl: 'configDialogs/relationshipsProperties/svgRelationshipInfo/svgRelationshipInfo.tpl.html',
                link: function ($scope) {


                    $scope.model = {
                        height: 80,
                        width: 227,
                        rectHeight: 20,
                        rectWidth: 90,
                        startX: 0,
                        startY: 0,
                        horizontalSpacing: 41,      // horizontal spacing between left side and right side
                        rectVerticalSpacing: 3,     // vertical spacing between rectangles      
                        overlapOffsetX: 4,         // incase of 2 svgs overlapping
                        overlapOffsetY: 5,         // incase of 2 svgs overlapping
                        rightOffsetY: 5,            // right side rectangles get pushed down
                        showLineMarkers: false,
                        svg1: {
                            left:{},
                            right:{}
                        },
                        svg2: {
                            left:{},
                            right:{}
                        }
                    };


                    $scope.$watch("options.svgs", function () {
                        if (!$scope.options || !$scope.options.svgs)
                            return;
                        if (_.isArray($scope.options.svgs) && $scope.options.svgs.length > 0) {
                            calculateSvgs($scope.options.svgs);
                        }
                    });
                                        
                    function calculateSvgs(svgsInternal) {
                        setShowLineMarkers(svgsInternal);

                        // set the points required as starting point
                        calculatePoints(svgsInternal);

                        // calculate the width and height of main svg after calculating left, right and multiple svgs
                        //setSvgDimensions(svgsInternal);

                        // calculate rectangle values
                        calculateRectangleValues(svgsInternal);
                        
                        // calculate connectors
                        calculateConnectorValues(svgsInternal);
                    }

                    function setShowLineMarkers(svgsInternal) {
                        $scope.model.showLineMarkers = _.some(svgsInternal, function (svg) {
                            return spUtils.result(svg, 'leftRects.showLineMarkers') || spUtils.result(svg, 'rightRects.showLineMarkers');
                        });
                    }

                    function calculatePoints(svgsInternal) {
                        // set the points for svgs
                        if(svgsInternal.length > 0) {        // only handles creating 2 seperate svgs
                            
                            setPointValues(svgsInternal[0], true);

                            if(svgsInternal.length > 1) {
                                setPointValues(svgsInternal[1], false);
                            }
                        }
                    }
                    
                    function setPointValues(svg, isFirstSvg) {
                        var leftRectCount = svg.leftRects.numOfRects;
                        var rightRectCount = svg.rightRects.numOfRects;

                        var leftX = isFirstSvg ? $scope.model.startX : $scope.model.startX + $scope.model.overlapOffsetX;
                        var rightX = leftX + $scope.model.rectWidth + $scope.model.horizontalSpacing;       // no need to apply offsetX as this point is calculated relative to leftX
                        var leftY = isFirstSvg ? $scope.model.startY : $scope.model.startY + $scope.model.overlapOffsetY;
                        var rightY = svg.rightRects.hasOffsetDown ? leftY + $scope.model.rightOffsetY : leftY; // isFirstSvg ? $scope.model.startY : $scope.model.startY + $scope.model.rightOffsetY;
                        
                        var tempLeftHeight = (leftRectCount * $scope.model.rectHeight) + ((leftRectCount - 1) * $scope.model.rectVerticalSpacing);
                        var leftTotalHeight = isFirstSvg ? tempLeftHeight : tempLeftHeight + $scope.model.overlapOffsetY;                               // add overlap offset if second svg
                        leftTotalHeight = svg.rightRects.hasOffsetDown ? leftTotalHeight + $scope.model.rightOffsetY : leftTotalHeight;                 // add right side offset if need to push down the right side rectangles (adding it to left side height so that the lefty should be when there is no downoffset on right.)
                        
                        var tempRightHeight = (rightRectCount * $scope.model.rectHeight) + ((rightRectCount - 1) * $scope.model.rectVerticalSpacing);
                        var rightTotalHeight = isFirstSvg ? tempRightHeight : tempRightHeight + $scope.model.overlapOffsetY;                            // add overlap offset if second svg
                        rightTotalHeight = svg.rightRects.hasOffsetDown ? rightTotalHeight + $scope.model.rightOffsetY : rightTotalHeight;              // add right side offset if need to push down the right side rectangles


                        var tempY1 = isFirstSvg ? $scope.model.startY : $scope.model.startY + $scope.model.overlapOffsetY;
                        tempY1 = svg.rightRects.hasOffsetDown ? (tempY1 + $scope.model.rightOffsetY) : tempY1;
                        
                        var tempY2Start, tempY2;
                        if (!isFirstSvg && leftRectCount === 1 && rightRectCount === 0) {   // handling an edge case in current requirements
                            leftY = $scope.model.svg1.left.y + $scope.model.overlapOffsetY;
                        }
                        else if (leftRectCount > rightRectCount) {
                            tempY2Start = Math.round((leftTotalHeight - rightTotalHeight) / 2);
                            tempY2Start = svg.rightRects.hasOffsetDown ? tempY2Start - $scope.model.rightOffsetY : tempY2Start;
                            
                            tempY2 = isFirstSvg ? tempY2Start : tempY2Start + $scope.model.overlapOffsetY;
                            tempY2 = svg.rightRects.hasOffsetDown ? (tempY2 + $scope.model.rightOffsetY) : tempY2;

                            leftY = tempY1; 
                            rightY = tempY2;

                        }
                        else if (rightRectCount > leftRectCount) {
                            tempY2Start = Math.round((rightTotalHeight - leftTotalHeight) / 2);
                            tempY2Start = svg.rightRects.hasOffsetDown ? tempY2Start - $scope.model.rightOffsetY : tempY2Start;
                            
                            tempY2 = isFirstSvg ? tempY2Start : tempY2Start + $scope.model.overlapOffsetY;
                            tempY2 = svg.rightRects.hasOffsetDown ? (tempY2 + $scope.model.rightOffsetY) : tempY2;
                            
                            rightY = tempY1;
                            leftY = tempY2;
                        }

                        // assign values to model
                        if(isFirstSvg) {
                            $scope.model.svg1.left.x = leftX;
                            $scope.model.svg1.right.x = rightX;
                            $scope.model.svg1.left.y = leftY;
                            $scope.model.svg1.right.y = rightY;
                            $scope.model.svg1.left.totalHeight = leftTotalHeight;
                            $scope.model.svg1.right.totalHeight = rightTotalHeight;
                        }
                        else {
                            $scope.model.svg2.left.x = leftX;
                            $scope.model.svg2.right.x = rightX;
                            $scope.model.svg2.left.y = leftY;
                            $scope.model.svg2.right.y = rightY;
                            $scope.model.svg2.left.totalHeight = leftTotalHeight;
                            $scope.model.svg2.right.totalHeight = rightTotalHeight;
                        }
                    }
                    
                    function setSvgDimensions(svgsInternal) {
                        // todo: set the width of svg

                        // set the height of svg
                        if (svgsInternal.length > 0) {        // only handles creating 2 seperate svgs
                            // check the totalheight of both left ot right sides and set the bigger one as the height
                            if (svgsInternal.length > 1) {
                                $scope.model.height = $scope.model.svg2.left.totalHeight > $scope.model.svg2.right.totalHeight ? $scope.model.svg2.left.totalHeight : $scope.model.svg2.right.totalHeight;
                            }
                            else {
                                $scope.model.height = $scope.model.svg1.left.totalHeight > $scope.model.svg1.right.totalHeight ? $scope.model.svg1.left.totalHeight : $scope.model.svg1.right.totalHeight;
                            }
                        }
                    }
                    
                    function calculateRectangleValues(svgsInternal) {
                        // set the points for svgs
                        if (svgsInternal.length > 0) {        // only handles creating 2 seperate svgs
                            // svg1
                            var svg1 = svgsInternal[0];
                            $scope.model.svg1.leftRects = getRectangleValues(svg1.leftRects, $scope.model.svg1.left, $scope.model.rectWidth, $scope.model.rectHeight, $scope.model.rectVerticalSpacing);       // calculate svg1: left side
                            $scope.model.svg1.rightRects = getRectangleValues(svg1.rightRects, $scope.model.svg1.right, $scope.model.rectWidth, $scope.model.rectHeight, $scope.model.rectVerticalSpacing);    // calculate svg1: right side

                            if (svgsInternal.length > 1) {
                                // svg2
                                var svg2 = svgsInternal[1];
                                $scope.model.svg2.leftRects = getRectangleValues(svg2.leftRects, $scope.model.svg2.left, $scope.model.rectWidth, $scope.model.rectHeight, $scope.model.rectVerticalSpacing);       // calculate svg2: left side
                                $scope.model.svg2.rightRects = getRectangleValues(svg2.rightRects, $scope.model.svg2.right, $scope.model.rectWidth, $scope.model.rectHeight, $scope.model.rectVerticalSpacing);    // calculate svg2: right side
                            }
                        }
                    }
                    
                    function calculateConnectorValues(svgsInternal) {
                        if (svgsInternal.length > 0) {        // only handles creating 2 seperate svgs
                            // svg1
                            var svg1 = svgsInternal[0];
                            $scope.model.svg1.connectors = getConnectorValues(svg1, $scope.model.svg1, $scope.model.rectWidth, $scope.model.rectHeight, $scope.model.rectVerticalSpacing);       // calculate svg1 connectors
                            
                            if (svgsInternal.length > 1) {
                                // svg2
                                var svg2 = svgsInternal[1];
                                $scope.model.svg2.connectors = getConnectorValues(svg2, $scope.model.svg2, $scope.model.rectWidth, $scope.model.rectHeight, $scope.model.rectVerticalSpacing);       // calculate svg2 connectors
                            }
                        }
                    }
                    
                    function getConnectorValues(rectSettings, svgPointValues, rectWidth, rectHeight, verticalSpacing) {
                        var connectors = [];
                        var numLeft = rectSettings.leftRects.numOfRects;
                        var numRight = rectSettings.rightRects.numOfRects;
                        var style = !spUtils.isNullOrUndefined(rectSettings.leftRects.connectorStyle) ? 'line-' + rectSettings.leftRects.connectorStyle : 'line-normal';
                        var markerStart = rectSettings.leftRects.showLineMarkers ? 'url(#triangleFrom)' : null;
                        var markerEnd = rectSettings.rightRects.showLineMarkers ? 'url(#triangleTo)' : null;

                        var x1, y1, x2, y2;
                        var pointsLeft = [], pointsRight = [];
                        x1 = svgPointValues.left.x + rectWidth;
                        y1 = svgPointValues.left.y + Math.round((rectHeight / 2));
                        x2 = svgPointValues.right.x;
                        y2 = svgPointValues.right.y + Math.round((rectHeight / 2));                                                

                        pointsLeft = getPoints(numLeft, x1, y1, rectHeight, verticalSpacing);
                        
                        pointsRight = getPoints(numRight, x2, y2, rectHeight, verticalSpacing);                                               

                        _.forEach(pointsLeft, function (pointL, indexL) {
                            _.forEach(pointsRight, function (pointR, indexR) {
                                var point = {
                                    x1: pointL.x,
                                    y1: pointL.y,
                                    x2: pointR.x,
                                    y2: pointR.y,
                                    style: style,
                                    markerStart: markerStart,
                                    markerEnd: markerEnd
                                };
                                
                                // If we have configured line markers
                                // then vertically space out the lines
                                // so that the line markers do not overlap.
                                // Otherwise all the line markers
                                // point to the same spot.
                                if (markerStart &&
                                    numLeft !== numRight &&
                                    numRight > 1 &&
                                    pointL.y !== pointR.y) {
                                    point.y1 = Math.round((pointL.y - (rectHeight / 2)) + ((rectHeight / (numRight + 1)) * (indexR + 1)));
                                }

                                // If we have configured line markers
                                // then vertically space out the lines
                                // so that the line markers do not overlap.
                                // Otherwise all the line markers
                                // point to the same spot.
                                if (markerEnd &&
                                    numLeft !== numRight &&
                                    numLeft > 1 &&
                                    pointL.y !== pointR.y) {
                                    point.y2 = Math.round((pointR.y - (rectHeight / 2)) + ((rectHeight / (numLeft + 1)) * (indexL + 1)));
                                }

                                connectors.push(point);
                            });
                        });

                        return connectors;
                    }
                    
                    function getPoints(numPoints, x, y, rectHeight, verticalSpacing){
                        var points = [];
                        for (var i = 1; i <= numPoints; i++) {
                            points.push(
                                {
                                    x: x,
                                    y: y
                                }
                            );

                            y = y + rectHeight + verticalSpacing;
                        }
                        return points;
                    }
                    
                    function getRectangleValues(rectSettings, pointValues, rectWidth, rectHeight, verticalSpacing) {
                        var returnRects = [];
                        var rectX = pointValues.x;
                        var rectY = pointValues.y;
                        var rectText = rectSettings.text;
                        var rectStyle = !spUtils.isNullOrUndefined(rectSettings.rectStyle) ? 'rect-' + rectSettings.rectStyle : 'rect-normal';
                        var textStyle = !spUtils.isNullOrUndefined(rectSettings.textStyle) ? 'text-' + rectSettings.textStyle : 'text-normal';
                        
                        for (var i = 0; i < rectSettings.numOfRects; i++) {

                            returnRects.push(
                                {
                                    x: rectX,
                                    y: rectY,
                                    rectWidth: rectWidth,
                                    rectHeight: rectHeight,
                                    rectStyle: rectStyle,
                                    text: rectText,
                                    textStyle: textStyle,
                                    textX: rectX + Math.round((rectWidth / 2)),
                                    textY: rectY + Math.round((rectHeight / 2))
                                }
                            );
                            
                            rectY = rectY + rectHeight + verticalSpacing;
                        }
                        return returnRects;
                    }
                }
            };
        });
}());