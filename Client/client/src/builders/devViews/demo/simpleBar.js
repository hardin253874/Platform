// Copyright 2011-2016 Global Software Innovation Pty Ltd
// deprecated

angular.module('sp.common.charts.spD3SimpleBar', [])
    .directive('spD3SimpleBar', function () {
        "use strict";

        /*
         * CSS such as the following needs to be included to make
         * this work. todo - need to work out a better way...
         */
//        div.bar {
//            display: inline-block;
//            width: 10px;
//            height: 70px;     /* We'll override this later */
//            background-color: teal;
//            margin-right: 2px;
//        }
//
//        div.hbar {
//            display: block;
//            width: 50px;    /* We'll override this later */
//            height: 10px;
//            background-color: teal;
//            margin-bottom: 2px;
//        }

        function valueToPx(d) {
            var d0 = d;
            if (typeof d === 'string') {
                d = parseInt(d, 10);
                if (isNaN(d)) {
                    d = d0.length;
                }
            }
            //console.log('px from %s is %s', d0, d);
            d = Math.min(100, d);
            return (d * 5) + 'px';
        }

        function updateBar(sel) {
//                sel.attr('class', 'bar')
//                   .style('height', valueToPx)
//                   .attr('title', function(d) { return d; });
            sel.attr('class', 'hbar')
                .style('width', valueToPx)
                .attr('title', function (d) {
                    return d;
                });
        }

        return {
            scope: {
                d3Data: '='
            },
            link: function (scope, el, attrs) {
                scope.$watch('d3Data', function (d3Data) {
                    var p;

                    if (d3Data) {
                        p = d3.select(el[0]).selectAll('div')
                            .data(d3Data);

                        updateBar(p);
                        updateBar(p.enter().append("div"));

                        p.exit().remove();
                    }
                });
            }
        };
    });

