// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* jshint bitwise:false */
/* global _, console, Globalize, unescape, Hammer */

/**
 * Module with various miscellaneous functions.
 * We may add wrappers here to various 3rd party library routines...
 *  @namespace spUtils
 */

var spUtils = spUtils || {}; // jshint ignore:line
var sp = spUtils; // jshint ignore:line

(function (spUtils) {
    'use strict';

    // Until we decide where to put these static strings 
    spUtils.imageFileTypeFilter = 'image/*';
    spUtils.spreadsheetFileTypeFilter = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,.csv,.txt';
    spUtils.strToday = 'TODAY';
    spUtils.strNow = 'NOW';
    spUtils.invalidTimeMsgText = 'Invalid time value';
    spUtils.invalidDateMsgText = function () {
        return 'Invalid format, the date must be in the format ' + Globalize.culture().calendars.standard.patterns.d;
    };

    /**
     * push array of values onto an array
     */
    spUtils.pushArray = function (arr, values) {
        if (values) {
            arr.push.apply(arr, values);
        }
    };

    /**
     * Put a cookie from the browser.
     *
     * @param {string} cname Cookie name.
     * @param {string} cvalue Cookie value.
     */
    spUtils.putCookie = function (cname, cvalue, exdays) {
        exdays = exdays || 1000;
        var d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + cvalue + "; " + expires + "; path=/";
    };

    /**
     * Gets a cookie from the browser.
     *
     * @param {string} name Cookie name.
     * @returns {string} Cookie value, or undefined.
     */
    spUtils.getCookie = function (name) {
        var i, x, y, cookies = document.cookie.split(";");
        for (i = 0; i < cookies.length; i++) {
            x = cookies[i].substr(0, cookies[i].indexOf("="));
            y = cookies[i].substr(cookies[i].indexOf("=") + 1);
            x = x.replace(/^\s+|\s+$/g, "");
            if (x === name) {
                return unescape(y);
            }
        }
    };


    /**
     * Gets a value from the specified cookie. Note: you are probably better off using something else that supports dependency injection.
     *
     * @param {string} cookie
     * @param {string} name
     * @returns {string}  Cookie value name, or undefined.
     */
    spUtils.getCookieValue = function (cookie, name) {
        if (!cookie) {
            return;
        }

        var key, value, keyValuePairs = cookie.split("&");
        var i;
        for (i = 0; i < keyValuePairs.length; i++) {
            key = keyValuePairs[i].substr(0, keyValuePairs[i].indexOf("="));
            value = keyValuePairs[i].substr(keyValuePairs[i].indexOf("=") + 1);
            if (key === name) {
                return value;
            }
        }
    };


    /**
     * Deletes the specified cookie. Note: you are probably better off using something else that supports dependency injection.
     *
     * @param {string} name
     */
    spUtils.deleteCookie = function (name) {
        var date = new Date();
        var expires;
        date.setTime(date.getTime() + (-1 * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toGMTString();
        document.cookie = name + "=" + expires + "; path=/";
    };


    /**
     * TODO.
     * Searches an array for an object that has a specified property matching a specified value.
     *
     * @param {Array} arr Array of object data.
     * @param {string} key Name of key to look up.
     * @param {*} value Value to find.
     * @returns {Object} The first instance, or undefined if not found.
     */
    spUtils.findByKey = function (arr, key, value) {
        /// findByKey returns the value of the given property, whether that property is a field, getter or function
        return _.find(arr, function (item) {
            return _.result(item, key) == value;
        });
    };


    /**
     * TODO.
     */
    spUtils.syncArrays = function (srcArr, destArr, compareFunc, createFunc, updateFunc, deleteFunc, context) {
        /// sync the destArr with the srcArr using the given functions as needed

        // mark each for deletion
        _.each(destArr, function (destItem) {
            destItem.__canDelete = true;
        });

        // go over each src item and create or update the dest as needed
        _.each(srcArr, function (srcItem) {
            var dest = _.find(destArr, function (destItem) {
                return compareFunc.call(context, srcItem, destItem) === 0;
            });
            if (!dest) {
                dest = createFunc.call(context, srcItem);
                destArr.push(dest);
            } else {
                updateFunc.call(context, dest, srcItem);
                dest.__canDelete = false;
            }
        });

        // remove any that weren't touched
        var i;
        for (i = destArr.length - 1; i >= 0; i = i - 1) {
            if (destArr[i].__canDelete) {
                if (deleteFunc) {
                    deleteFunc.call(context, destArr[i]);
                }
                destArr.splice(i, 1);
            }
        }
    };


    /**
     * TODO.
     */
    spUtils.filterList = function (items, filterText) {
        /// return a filtered list of items ordered with better matches first

        if (items && filterText) {
            var exactMatches, startsWithMatches, anyMatches;

            filterText = filterText.toLowerCase();

            exactMatches = items.filter(function (item) {
                return _.find(item, function (p) {
                    return p && (_.isString(p) && p.toLowerCase() === filterText || p == filterText);
                });
            });
            startsWithMatches = items.filter(function (item) {
                return _.find(item, function (p) {
                    return p && (_.isString(p) && p.toLowerCase().indexOf(filterText) === 0);
                });
            });
            anyMatches = items.filter(function (item) {
                return _.find(item, function (p) {
                    return p && (_.isString(p) && p.toLowerCase().match(filterText));
                });
            });
            items = _.union(exactMatches, startsWithMatches, anyMatches);
        }
        return items;
    };


    /**
     * Accepts a string, and converts it to a bool.
     * 'true', 'yes', '1' are treated as true.
     * 'false', 'no', '0' are treated as false.
     * Other calls fall back to the default of Boolean(string).
     *
     * @param {string} string Input text.
     * @returns {boolean} True or false.
     */
    spUtils.stringToBoolean = function (string) {
        if (!string) {
            return false;
        }
        switch ((''+string).toLowerCase()) {
            case "true":
            case "yes":
            case "1":
                return true;
            case "false":
            case "no":
            case "0":
            case null:
                return false;
            default:
                return Boolean(string);
        }
    };


    /**
     * Accepts a string, and converts the first letter to upper case.
     * Nulls and undefined get passed through unchanged.
     *
     * @param {string} string Input text.
     * @returns {string} Text with the first letter capitalized.
     */
    spUtils.capitaliseFirstLetter = function (string) {
        if (!string)
            return string;
        return string.charAt(0).toUpperCase() + string.slice(1);
    };

    /**
     * Accepts a string, and returns true if the string is a number.
     * Nulls and undefined return false.
     *
     * @param {string} value Input text.
     * @returns {boolean} True if the string is a number.
     */
    spUtils.stringIsNumber = function (value) {
        return !isNaN(value) && value !== null && value.length;
    };

    /**
     * Calls a function over and over while the result is truthy.
     * The motivation for this is to put while-loops in code without having to pull out inner function definitions.
     *
     * @param {Function} fn The function to call.
     */
    spUtils.doWhile = function (fn) {
        while (true) {
            var res = fn();
            if (!res)
                return;
        }
    };


    /**
     * Returns the given argument, converting to a number type if it happens to
     * be a string representation of a number. Feel free to rename this!
     *
     * @param {number|string} value The value.
     * @returns {number|string} The result.
     */
    spUtils.coerseToNumberOrLeaveAlone = function (value) {
        var temp;
        if (_.isString(value) && !isNaN((temp = parseInt(value, 10)))) {
            return temp;
        }
        return value;
    };

    /**
     * Walks a graph of nodes, visiting each one once. Pass in a node or array of nodes to start with.
     * Then pass a function that will return an array of related nodes.
     *
     * @param {Function} fnGetRelated Accepts a node, and returns array of related nodes.
     * @param {Array} startNodes Single node or array of nodes to start with.
     * @returns {Array} Array of all nodes that got visited. No guarantees about the order.
     *
     * @example
     <pre>spUtils.walkGraph(function(n) { return n.getRelated(); }, node);</pre>
     */
    spUtils.walkGraph = function (fnGetRelated, startNodes) {
        if (fnGetRelated === null || fnGetRelated === undefined) {
            throw new Error('fnGetRelated is null');
        }
        if (startNodes === null || startNodes === undefined) {
            return [];
        }
        var result = [];
        var queue = _.isArray(startNodes) ?
            startNodes.slice()    // copy
            : [startNodes];     // array of single

        var node;

        while (queue.length) {
            // Deque a node from front
            node = queue.shift();

            // Have we visited it?
            if (_.includes(result, node))
                continue;
            result.push(node);

            // Visit friends
            var friends = fnGetRelated(node);
            if (friends && friends.length) {
                queue = queue.concat(friends);
            }
        }
        return result;
    };

    /**
     * Walks a graph of nodes, visiting each one once. Pass in a node or array of nodes to start with.
     * Then pass a function that will return an array of related nodes.
     *
     * @param {Function} fnGetRelated Accepts a node, and returns array of related nodes.
     * @param {Array} startNodes Single node or array of nodes to start with.
     * @returns {Array} Array of all nodes that got visited. No guarantees about the order.
     *
     * @example
     <pre>spUtils.walkGraphSorted(node, function(n) { return n.getRelated(); });</pre>
     */
    spUtils.walkGraphSorted = function (fnGetRelated, startNodes) {
        if (fnGetRelated === null || fnGetRelated === undefined) {
            throw new Error('fnGetRelated is null');
        }
        if (startNodes === null || startNodes === undefined) {
            return [];
        }

        var all = spUtils.walkGraph(fnGetRelated, startNodes);

        _.forEach(all, function (node) {
            node._spUtils_incoming = 0;
            node._spUtils_outgoing = fnGetRelated(node);
        });
        _.forEach(all, function (node) {
            _.forEach(node._spUtils_outgoing, function (related) {
                related._spUtils_incoming++;
            });
        });

        var allCopy = all.slice();
        var result = [];
        spUtils.doWhile(function () {
            // find something with no incoming
            var candidate = _.find(all, function (node) {
                return node._spUtils_incoming === 0;
            });
            if (!candidate) {
                // no work (or a cycle) .. just grab any un-done work if there's a cycle, otherwise break out
                candidate = _.find(all, function (node) {
                    return node._spUtils_incoming > 0;
                });
                if (!candidate)
                    return false; // break
            }
            candidate._spUtils_incoming = -1; // flag as done
            _.forEach(candidate._spUtils_outgoing, function (related) {
                related._spUtils_incoming--;
            });
            result.push(candidate);
            return true; // continue
        });

        // clean up
        _.forEach(allCopy, function (node) {
            node._spUtils_incoming = undefined;
            node._spUtils_outgoing = undefined;
        });
        return result;
    };


    /**
     * The EPOC for the date component for storing of times
     */
    spUtils.timeEpoc = new Date(1973, 0, 1);

    /**
     * Convert a string from the database into a native js object. Used for default values.
     *
     * @param {String} dataType The dbType of the string.
     * @param {String} value the value.
     * @returns {Object} The converted value.
     */
    spUtils.convertDbStringToNative = function (dataType, value) {
        var result;
        if (value === null) {
            result = null;
        } else {
            switch (dataType) {
                case 'Decimal':
                case 'Currency':
                    result = value === '' ? null : parseFloat(value);
                    break;
                case 'Number':

                case 'Int32':
                    result = value === '' ? null : parseInt(value, 10);
                    break;
                case 'String':
                case 'Xml':
                case 'Guid':
                    result = value === '' ? null : value;   // empty strings should be represented as nulls
                    break;
                case 'Time':
                    result = value === '' ? null : spUtils.getUtcDateByUtcTimeString(value);    //string value represents the utc time.
                    break;
                case 'Date':
                case 'DateTime':
                    result = value === '' ? null : spUtils.getUtcDateByUtcDateString(value, dataType);    //string value represents the utc Date.
                    break;
                case 'Bool':
                    result = value === '' ? false : spUtils.stringToBoolean(value);
                    break;
                default:
                    console.log('Unrecognized field type', dataType, value || '<null>');
                    throw new Error('Unrecognized field type: ' + dataType + ' ' + (value || '<null>'));
            }
        }
        return result;
    };

    /**
     * Convert a string (representing a valid utc date or (TODAY) to a native js date object(utc).
     *
     * @param {String} string (representing an utc date).
     * @returns {Object|null} The converted js date object.
     */
    spUtils.getUtcDateByUtcDateString = function (utcDateString, dataType) {

        if (utcDateString) {
            var tempDate = new Date();

            if (dataType === 'Date' && utcDateString === "TODAY") {
                tempDate.setHours(0, 0, 0, 0);
                return spUtils.translateToUtc(tempDate);
            }
            if (dataType === 'DateTime' && utcDateString === "NOW") {
                return tempDate;
            }
            var localDate = new Date(utcDateString);
            if (isNaN(localDate)) {
                return null;
            }

            if (localDate && dataType === 'Date') {
                return spUtils.translateToUtc(localDate);
            }
            return localDate;
        }
        return null;
    };

    /**
     * Returns the date format pattern (e.g. d/MM/yyyy).
     *
     * @returns {string} The format value.
     */
    spUtils.getDateDisplayFormat = function () {
        return Globalize.culture().calendar.patterns['d'];
    };

    /**
     * Returns the time format pattern (e.g. h:mm tt).
     *
     * @returns {string} The format value.
     */
    spUtils.getTimeDisplayFormat = function () {
        return Globalize.culture().calendar.patterns['t'];
    };

    /**
     * Hack needs to be removed later. Try to parse a string as a valid date using Globalize using current cultures date formats.
     * and if the parsed value is not a valid date then try to parse using custom format .
     *
     * @returns {date|null} The result.
     */
    spUtils.parseDateString = function (dateString) {
        return spUtils.parseDate(dateString);
    };


    /////
    // Determine whether the date is valid.
    /////
    spUtils.isValidDate = function (value) {
        if (Object.prototype.toString.call(value) === "[object Date]") {
            if (isNaN(value.getTime())) {
                return false;
            } else {
                return true;
            }
        } else {
            return false;
        }
    };

    /////
    // Parse the specified value into a Date object (where possible).
    /////
    spUtils.parseDate = function (value) {

        var result = null;

        if (_.isDate(value)) {
            result = value;
        } else if (_.isString(value) || _.isFinite(value)) {
            result = Globalize.parseDate(value);
        }

        if (!spUtils.isValidDate(result)) {
            result = undefined;
        }

        return result;
    };

    ////
    // Compare two times, ignoring date parts
    ///
    spUtils.compareTimeOnly = function (date1, date2) {

        if (!date1 && !date2)
            return true;

        if (!date1 || !date2)
            return false;

        return date1.getUTCHours() === date2.getUTCHours() &&
            date1.getUTCMinutes() === date2.getUTCMinutes() &&
            date1.getUTCSeconds() === date2.getUTCSeconds();
    };

    /**
     * Convert a string (representing an utc time) from the database into a native js date object(utc). Used for default values.
     *
     * @param {String} string (representing an utc time).
     * @returns {Object|null} The converted value.
     */
    spUtils.getUtcDateByUtcTimeString = function (utcTimeString) {

        if (utcTimeString) {
            var localDate = new Date(spUtils.timeEpoc.toDateString() + ' ' + utcTimeString);

            if (isNaN(localDate)) {
                return null;
            }

            if (localDate) {
                var tempDate = new Date();
                tempDate.setUTCFullYear(1753, 0, 1);
                tempDate.setUTCHours(localDate.getHours(), localDate.getMinutes(), 0, 0);
                return tempDate;
            }
        }
        return null;
    };

    /**
     * Returns a new local date from the utc parts of the provided date
     *
     * @param {date|string} utcDate The utcDate.
     * @returns {date|null} The result.
     */
    spUtils.translateToLocal = function (utcDate) {
        if (utcDate) {
            var localDate = new Date(utcDate.getUTCFullYear(), utcDate.getUTCMonth(), utcDate.getUTCDate());
            if (!isNaN(localDate)) {
                return localDate;
            }
        }
        return null;
    };

    /**
     * Returns a new local date and set the utc parts to the local parts of the provided date
     *
     * @param {date|string} localDate The localDate.
     * @returns {date|null} The result.
     */
    spUtils.translateToUtc = function (localDate) {
        if (localDate) {

            var tempDate = new Date();
            tempDate.setUTCFullYear(localDate.getFullYear(), localDate.getMonth(), localDate.getDate());
            tempDate.setUTCHours(0, 0, 0, 0);

            if (!isNaN(tempDate)) {
                return tempDate;
            }
        }
        return null;
    };

    /**
     * Returns a js date that has date component set to 1-Jan-1973 and time component set to the utc time component of provided date object.
     *
     * @param {date} The server storage date.
     * @returns {date|null} The result.
     */
    spUtils.translateFromServerStorageDateTime = function (utcDate) {
        if (utcDate && _.isDate(utcDate)) {
            // note: the time control won't bind to any date prior to 1-1-1970( Unix time start). but we're not interested in the date component here anyway, so just hard-code to 1973.
            var localDate = new Date(1973, 0, 1, utcDate.getUTCHours(), utcDate.getUTCMinutes(), 0, 0);
            if (!isNaN(localDate)) {
                return localDate;
            }
        }
        return null;
    };

    /**
     * Returns a js date that has utc date component set to 1-1-1753 and utc time component set to the time component of provided date object.
     *
     * @param {date} localDate The localDate.
     * @returns {date|null} The result.
     */
    spUtils.translateToServerStorageDateTime = function (localDate) {
        if (localDate && _.isDate(localDate)) {
            // note: 1-1-1753 is the date that must be used for all time values in the SoftwarePlatform database
            var tempDate = new Date();
            tempDate.setUTCFullYear(1753, 0, 1);
            tempDate.setUTCHours(localDate.getHours(), localDate.getMinutes(), 0, 0);

            if (!isNaN(tempDate)) {
                return tempDate;
            }
        }
        return null;
    };

    /**
     * Convert a string from the UI into a into a native js object.
     *
     * @param {String} dataType The dbType of the string.
     * @param {String} value the value.
     * @returns {Object} The converted value.
     */
    spUtils.convertUiStringToNative = function (dataType, value) {

        // don't do anything special at the moment.
        return spUtils.convertDbStringToNative(dataType, value);

    };


    /**
     * Returns all instances of arr1 that are in arr2
     * using a custom predicate to perform the comparisons.
     *
     * @param {Array} arr1 Array to be filtered, and contents possibly returned.
     * @param {Array} arr2 Array, possibly of something different, to be excluded.
     * @param {Function} predicate Predicate that accepts (value1,value2) from arr1, arr2 respectively, and returns true if they match.
     * @returns {Array} A subset of arr1.
     */
    spUtils.intersect = function (arr1, arr2, predicate, _reject) {
        if (!arr1) {
            return arr1;
        }
        if (!arr2) {
            arr2 = [];
        }
        var res = _.filter(arr1, function (val1) {
            var tmp = _.some(arr2, function (val2) {
                return predicate(val1, val2);
            });
            if (_reject)
                return !tmp;
            return tmp;
        });
        return res;
    };


    /**
     * Returns all instances of arr1 that are not in arr2
     * using a custom predicate to perform the comparisons.
     *
     * @param {Array} arr1 Array to be filtered, and contents possibly returned.
     * @param {Array} arr2 Array, possibly of something different, to be excluded.
     * @param {Function} predicate Predicate that accepts (value1,value2) from arr1, arr2 respectively, and returns true if they match.
     * @returns {Array} A subset of arr1.
     */
    spUtils.except = function (arr1, arr2, predicate) {
        return spUtils.intersect(arr1, arr2, predicate, true);
    };


    /**
     * Returns a function that will call _.result over a given property.
     *
     * @param {string} propertyName The property name to be accessed.
     * @returns {Function} A function that gets the property, which may be a direct property or an accessor function.
     */
    spUtils.getter = function (propertyName) {
        return function (arg) {
            return _.result(arg, propertyName);
        };
    };

    /**
     * Generate a new rfc4122 version 4 compliant guid
     */
    spUtils.newGuid = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };

    /**
     * Return the value (or function result) at the end of a given path, or undefined if any member
     * is undefined. Based on the _.result function - read about that (underscore.js or lodash.js)
     *
     * Examples are sp.result(obj, 'model.data.obj.name') and sp.result(obj, 'model.data.entity.getName')
     * see the test cases - you can even index into arrays.
     *
     * @todo add an optional default for when we come across undefined or null
     * @todo add another function for deep setting, say resultSet
     *
     * @param obj the object we start with
     * @param path the path of properties and functions
     */
    spUtils.result = function (obj, path) {
        if (!obj || !path) return undefined;
        return _.reduce(_.isArray(path) ? path : path.split('.'), function (a, b) {
            return _.result(a, b);
        }, obj);
    };

    /**
     * Like underscore/lodash pluck but deep based on result function
     */
    spUtils.pluckResult = function (seq, path) {
        return _.map(seq, function (x) {
            return spUtils.result(x, path);
        });
    };

    /**
     * Return true if the given argument is a non-empty array
     */
    spUtils.isNonEmptyArray = function (a) {
        return a && _.isArray(a) && a.length;
    };


    /**
     * Return a function that calls the given function and returns the not (!) of its return value.
     */
    spUtils.negate = function (fn) {
        return function () {
            return !fn.apply(null, arguments);
        };
    };


    /**
     * Gets the specified contrast fore color base on background color
     *
     * @param {object} background color.
     * @returns {string} rgba css string representing the color.
     */
    spUtils.getContrastColor = function (color) {

        var r = 0, g = 0, b = 0, a = 1;

        if (!color) {
            return null;
        }

        // Counting the perceptive luminance - human eye favors green color... 
        var index = 1 - (0.299 * color.r + 0.587 * color.g + 0.114 * color.b) / 255;

        if (index < 0.5) {
            // bright colors - black font
            r = 0;
            g = 0;
            b = 0;
        } else {
            // dark colors - light green font
            r = 255;
            g = 255;
            b = 255;
        }

        if (color.a >= 0) {
            a = color.a / 255;
        }

        return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';

    };

    /**
     * Checks if the provided color is a lighter color
     *
     * @param {color} background color.
     * @returns {boolean} rgba css string representing the color.
     */
    spUtils.isColorLighterThanMiddleGray = function (color) {
        if (!color) {
            return null;
        }
        
        // Counting the perceptive luminance - human eye favors green color... 
        var index = 1 - (0.299 * color.r + 0.587 * color.g + 0.114 * color.b) / 255;

        return index < 0.5;
    };

    /**
     * Gets the specified color as a css rgba string.
     *
     * @param {object} color The color.
     * @returns {string} rgba css string representing the color.
     */
    spUtils.getCssColorFromRgb = function (color) {
        var r = 0, g = 0, b = 0, a = 1;

        if (!color) {
            return null;
        }

        if (color.r) {
            r = color.r;
        }

        if (color.g) {
            g = color.g;
        }

        if (color.b) {
            b = color.b;
        }

        if (color.a >= 0) {
            a = color.a / 255;
        }

        return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
    };
    /**
     * Gets the specified color as a ARGB string.
     *
     * @param {object} color The color.
     * @returns {string} rgba css string representing the color.
     */
    spUtils.getARGBStringFromRgb = function (color) {
        var r = '00', g = '00', b = '00', a = '01';
        if (!color) {
            return null;
        }

        if (color.r) {
            r = color.r.toString(16);
            if (r.length === 1)
                r = '0' + r;
        }

        if (color.g) {
            g = color.g.toString(16);
            if (g.length === 1)
                g = '0' + g;
        }

        if (color.b) {
            b = color.b.toString(16);
            if (b.length === 1)
                b = '0' + b;
        }

        if (!_.isUndefined(color.a)) {
            a = color.a.toString(16);
            if (a.length === 1)
                a = '0' + a;
        }

        return a + r + g + b;
    };

    /**
     * Gets the specified ARGB color string as a css rgba string.
     *
     * @param {string} argbColorString The color.
     * @returns {string} rgba css string representing the color.
     */
    spUtils.getCssColorFromARGBString = function (argbColorString) {
        var color = spUtils.getColorFromARGBString(argbColorString);
        if (!color) {
            return null;
        }

        return 'rgba(' + color.r + ',' + color.g + ',' + color.b + ',' + color.a + ')';
    };

    /**
     * Gets the specified ARGB color string as color object.
     *
     * @param {string} argbColorString The color.
     * @returns {object} rgba color object.
     */
    spUtils.getColorFromARGBString = function (argbColorString) {
        var color = {r: 0, g: 0, b: 0, a: 1};
        var colorRegEx;
        if (!argbColorString) {
            return null;
        }
        argbColorString = argbColorString.trim().replace(/^#/, '');
        if (argbColorString.length === 4) {
            colorRegEx = /^#?([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})$/i.exec('#' + argbColorString);
            if (colorRegEx) {
                color.a = parseInt(colorRegEx[1], 16);
                color.r = parseInt(colorRegEx[2], 16);
                color.g = parseInt(colorRegEx[3], 16);
                color.b = parseInt(colorRegEx[4], 16);
            }
        }
        else if (argbColorString.length === 8) {
            colorRegEx = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec('#' + argbColorString);
            if (colorRegEx) {
                color.a = parseInt(colorRegEx[1], 16);
                color.r = parseInt(colorRegEx[2], 16);
                color.g = parseInt(colorRegEx[3], 16);
                color.b = parseInt(colorRegEx[4], 16);
            }
        }

        return color;
    };

    /**
     * Gets the darker specified ARGB color string as a css rgba string.
     * @param {string} argbColorString The color.
     * @returns {string} rgba css string representing the color.
     */
    spUtils.getDarkerCssColorFromARGBString = function (argbColorString) {
        var r = 0, g = 0, b = 0, a = 1;
        var colorRegEx;
        if (!argbColorString) {
            return null;
        }
        argbColorString = argbColorString.trim().replace(/^#/, '');
        if (argbColorString.length === 4) {
            colorRegEx = /^#?([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})$/i.exec('#' + argbColorString);
            if (colorRegEx) {
                a = parseInt(colorRegEx[1], 16);
                r = parseInt(colorRegEx[2], 16) > 19 ? parseInt(colorRegEx[2], 16) - 8 : parseInt(colorRegEx[2], 16);
                g = parseInt(colorRegEx[3], 16) > 19 ? parseInt(colorRegEx[3], 16) - 8 : parseInt(colorRegEx[3], 16);
                b = parseInt(colorRegEx[4], 16) > 19 ? parseInt(colorRegEx[4], 16) - 8 : parseInt(colorRegEx[4], 16);
            }
        }
        else if (argbColorString.length === 8) {
            colorRegEx = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec('#' + argbColorString);
            if (colorRegEx) {
                a = parseInt(colorRegEx[1], 16);
                r = parseInt(colorRegEx[2], 16) > 19 ? parseInt(colorRegEx[2], 16) - 8 : parseInt(colorRegEx[2], 16);
                g = parseInt(colorRegEx[3], 16) > 19 ? parseInt(colorRegEx[3], 16) - 8 : parseInt(colorRegEx[3], 16);
                b = parseInt(colorRegEx[4], 16) > 19 ? parseInt(colorRegEx[4], 16) - 8 : parseInt(colorRegEx[4], 16);
            }
        }


        return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
    };

    /**
     * Gets the lighter specified ARGB color string as a css rgba string.
     * @param {string} argbColorString The color.
     * @returns {string} rgba css string representing the color.
     */
    spUtils.getLighterCssColorFromARGBString = function (argbColorString) {
        var r = 0, g = 0, b = 0, a = 1;
        var colorRegEx;
        if (!argbColorString) {
            return null;
        }
        argbColorString = argbColorString.trim().replace(/^#/, '');
        if (argbColorString.length === 4) {
            colorRegEx = /^#?([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})([a-f\d]{1})$/i.exec('#' + argbColorString);
            if (colorRegEx) {
                a = parseInt(colorRegEx[1], 16);
                r = parseInt(colorRegEx[2], 16) < 245 ? parseInt(colorRegEx[2], 16) + 12 : parseInt(colorRegEx[2], 16);
                g = parseInt(colorRegEx[3], 16) < 245 ? parseInt(colorRegEx[3], 16) + 12 : parseInt(colorRegEx[3], 16);
                b = parseInt(colorRegEx[4], 16) < 245 ? parseInt(colorRegEx[4], 16) + 12 : parseInt(colorRegEx[4], 16);
            }
        }
        else if (argbColorString.length === 8) {
            colorRegEx = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec('#' + argbColorString);
            if (colorRegEx) {
                a = parseInt(colorRegEx[1], 16);
                r = parseInt(colorRegEx[2], 16) < 245 ? parseInt(colorRegEx[2], 16) + 12 : parseInt(colorRegEx[2], 16);
                g = parseInt(colorRegEx[3], 16) < 245 ? parseInt(colorRegEx[3], 16) + 12 : parseInt(colorRegEx[3], 16);
                b = parseInt(colorRegEx[4], 16) < 245 ? parseInt(colorRegEx[4], 16) + 12 : parseInt(colorRegEx[4], 16);
            }
        }


        return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
    };


    /**
     * Takes a string, and a dictionary (object) of string substitutions, and performs the replacement.
     * e.g sp.subst('Hello %name% %val%', {'%name%':'Peter', '%val%','123'}) -> 'Hello Peter 123'
     *
     * @param {string} name Text.
     * @param {object} values Dictionary of substitutions.
     * @returns {string} Text with replacements.
     */
    spUtils.subst = function (text, values) {
        if (!text || !values)
            return text;
        var cur = text;
        _.forEach(_.keys(values), function (key) {
            cur = cur.replace(key, values[key]);
        });
        return cur;
    };

    /**
     * Is it null or undefined
     */
    spUtils.isNullOrUndefined = function (val) {
        return val === null || val === undefined;
    };

    /**
     * Converts a type name to an icon path
     */
    spUtils.convertTypeToImageUrl = function (type, cardinality) {
        if (!type)
            return undefined;

        var icon = type;
        switch (type) {
            case 'Bool':
                icon = 'Boolean';
                break;
            case 'Int32':
                icon = 'Int';
                break;
            case 'InlineRelationship':
            case 'UserInlineRelationship':
                switch (cardinality) {
                    case 'OneToOne':
                    case 'ManyToOne':
                        icon = 'Lookup';
                        break;
                    case 'OneToMany':
                    case 'ManyToMany':
                        icon = 'Relationship';
                        break;
                    default:
                        icon = 'Lookup';
                        break;
                }
                break;
            case 'ChoiceRelationship':
                icon = 'Choice';
                break;
            case 'StructureLevels':
                icon = 'StructureView';
                break;
        }
        if (icon !== 'Image' && icon !== 'Relationship') {
            icon = icon + 'Field';
        }
        var url = 'assets/images/itemicon/' + icon + '.png';
        return url;
    };

    /**
     * For use when you really want to intercept two-way Angular bindings.
     *
     * Example
     * <input type="checkbox" ng-model="asProp(getSharedAxis, setSharedAxis, series, 'primaryAxis').value">Share primary</input>
     * ...
     * $scope.asProp = spUtils.asProp;
     * $scope.getSharedAxis = function(series, alias) { return ... };
     * $scope.setSharedAxis = function(newValue, series, alias) {  };
     *
     * Takes a getter and a setter function.
     * And optionally additional arguments.
     * Returns an object with a single property called 'value'.
     * 'value' uses the provided getter and setter functions.
     * Additional arguments are passed to the getter and setter:
     * getter(additionalArgs) -> currentValue
     * setter(newValue, additionalArgs)
     *
     */
    spUtils.asProp = function (getter, setter /* .. args */) {
        var res = {};
        var args = _.toArray(arguments).slice(2);
        Object.defineProperty(res, 'value', {
            get: function () {
                return getter.apply(null, args);
            },
            set: function (val) {
                var args2 = [val].concat(args);
                setter.apply(null, args2);
            },
            enumerable: true,
            configurable: true
        });
        return res;
    };


    /**
     * Takes a string and escapes it so that it can be used as a regular expression.
     *
     * @param {string} str Regular expression text.
     * @returns {string} Escaped text.
     */
    spUtils.escapeRegExp = function (str) {
        if (str) {
            return str.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, '\\$&');
        } else {
            return str;
        }
    };

    /**
     * Converts an ID number or alias string to a format for embedding into Urls.
     */
    spUtils.aliasOrIdUri = function (aliasOrId) {
        var split;

        if (_.isFinite(aliasOrId) || spUtils.stringIsNumber(aliasOrId)) {
            return aliasOrId + '/';
        } else {
            split = aliasOrId.split(':');

            if (split.length === 1) {
                return 'core/' + aliasOrId;
            } else if (split.length === 2) {
                return split[0] + '/' + split[1];
            } else {
                throw new Error('unexpected number of colons in the alias.');
            }
        }
    };

    /**
     *  Touch support. Add a hammer handler to an element (Remember to pass the element, not the array)
     * @param {string} verb One of the hammer actions - touch press etc.
     */
    spUtils.addHammerHandler = function (verb, iElement, fn) {
        var hammertime = new Hammer(iElement);
        hammertime.on(verb, fn);
    };

    /**
     *  Touch support. remove a hammer handler from an element
     */
    spUtils.removeHammerHandler = function (verb, iElement, fn) {
        try {
            var hammertime = new Hammer(iElement);
            hammertime.off(verb, fn);
        } catch (e) {
            // ignore -- hammertime does not well with the handler already being added. 
        }
    };

    /**
     *  Compares two strings.
     *  Comparison is case-insensitive.
     *  Also takes numerics into consideration. E.g. 'test31' should sort before 'test200'
     *  If string1 comes before string2, -1 is returned.
     *  If string1 comes after string2, 1 is returned.
     *  If they are considered identical, zero is returned.
     */
    spUtils.naturalCompare = function (string1, string2) {
        if (!string1) return string2 ? -1 : 0;
        if (!string2) return 1;

        var type = function (ch) {
            if (ch === null) return 0;
            var isLetter = ch.toUpperCase() !== ch.toLowerCase();
            if (isLetter) return 3;
            if (ch >= '0' && ch <= '9') return 2;
            return 1;
        };
        var compCh = function (ch1, ch2) {
            if (ch1 < ch2) return -1;
            if (ch1 > ch2) return 1;
            return 0;
        };

        var wasNum = false;
        var numRes = 0;
        for (var i = 0; i <= string1.length; i++) {
            var ch1 = i === string1.length ? null : string1[i];
            var ch2 = i === string2.length ? null : string2[i];
            var type1 = type(ch1);
            var type2 = type(ch2);
            if (type1 !== type2) {
                if (wasNum) {
                    if (type1 === 2 || type2 === 2) return type1 === 2 ? 1 : -1; // if anyone is still a number, put the shorter number first
                    if (numRes !== 0) return numRes; // numbers were same length, but not the same
                }
                return type1 < type2 ? -1 : 1; // if types are different, sort first by type
            } // after here type1===type2
            var isNum = type1 === 2;
            if (isNum) {
                if (numRes === 0) {
                    numRes = compCh(ch1, ch2);
                }
            } else if (wasNum && numRes !== 0) {
                return numRes;
            }
            wasNum = isNum;
            if (ch1 === ch2)
                continue;
            if (type1 === 1) {
                var res1 = compCh(ch1, ch2);
                if (res1 !== 0)
                    return res1;
            }
            if (type1 === 3) {
                var res2 = compCh(ch1.toLowerCase(), ch2.toLowerCase());
                if (res2 !== 0)
                    return res2;
            }
        }
        return 0;
    };

    /**
     *  Sorts an array, yielding a separate sorted array,
     *  using a custom comparer function that accepts two values and returns -ve, 0, or +ve.
     */
    spUtils.sortBy = function (array, comparer) {
        var wrapped = _.map(array, function (s) {
            return {val: s};
        });
        wrapped.sort(function (o1, o2) {
            return comparer(o1.val, o2.val);
        });
        var sorted = _.map(wrapped, 'val');
        return sorted;
    };

    /**
     *  Sorts an array, yielding a separate sorted array.
     *  See notes in naturalCompare for the sorting applied.
     *  Accepts simplified argument pattern as lodash. (callback is optional, or can be a function, or a pluck)
     */
    spUtils.naturalSort = function (array, callback, thisArg) {
        if (!array) return array;
        var cb = _.bind(_.iteratee(callback), thisArg);
        var res = spUtils.sortBy(array, function (o1, o2) {
            var s1 = cb(o1);
            var s2 = cb(o2);
            return spUtils.naturalCompare(s1, s2);
        });
        return res;
    };

    /**
     *  Call to reset our performance timer.
     */
    spUtils.resetTime = function () {
        if (window.performance) {
            spUtils.lastNav = window.performance.now();
            spUtils.lastLog = null;
        }
    };
    spUtils.resetTime();

    /**
     *  Call to log the time since the performance timer was reset.
     */
    spUtils.logTime = function (message, showAlert) {
        if (window.performance) {
            var now = window.performance.now();
            var time = now - spUtils.lastNav;
            var msg = message + ' ' + Math.round(time) + 'ms';
            if (spUtils.lastLog)
                msg += ' \u0394' + Math.round(now - spUtils.lastLog) + 'ms';

            console.log(msg);
            if (showAlert) window.alert(msg);
            spUtils.lastLog = now;
        }
    };

    spUtils.parseFloat = function (val) {
        var result = Globalize.parseFloat(val);
        if (_.isNaN(result)) {
            result = undefined;
        }

        return result;
    };
    

    /**
     * Convert a sentense case string into title case
     * Adapted From GitHub rvagg/titlecase MIT license
     **/
    var articles =  [ 'the', 'a', 'an', 'some' ];

    var conjunctions = ['as', 'because', 'for', 'and', 'nor', 'but', 'or', 'yet', 'so'];

    var prepositions = [
        'a', 'abaft', 'aboard', 'about', 'above', 'absent', 'across', 'afore', 'after', 'against', 'along', 'alongside', 'amid', 'amidst', 'among', 'amongst', 'an', 'apropos',
        'apud', 'around', 'as', 'aside', 'astride', 'at', 'athwart', 'atop', 'barring', 'before', 'behind', 'below', 'beneath', 'beside', 'besides', 'between', 'beyond', 'but', 'by',
        'circa', 'concerning', 'despite', 'down', 'during', 'except', 'excluding', 'failing', 'following', 'for', 'forenenst', 'from', 'given', 'in', 'including', 'inside', 'into',
        'like', 'mid', 'midst', 'minus', 'modulo', 'near', 'next', 'notwithstanding', 'o\'', 'of', 'off', 'on', 'onto', 'opposite', 'out', 'outside', 'over', 'pace', 'past',
        'per', 'plus', 'pro', 'qua', 'regarding', 'round', 'sans', 'save', 'since', 'than', 'through', 'throughout', 'thru', 'thruout', 'till', 'times', 'to', 'toward', 'towards',
        'under', 'underneath', 'unlike', 'until', 'unto', 'up', 'upon', 'versus', 'via', 'vice',
        'vis-a-vis' /* note the accent has been dropped due to problem with jshint */,
        'with', 'within', 'without', 'worth'
    ];

    var smallWords = /^(a|an|and|as|at|but|by|en|for|if|in|nor|of|on|or|per|the|to|vs?\.?|via)$/i;

    var laxWords = articles.concat(prepositions).concat(conjunctions)
          .concat(smallWords.source.replace(/(^\^\(|\)\$$)/g, '').split('|'))
          .concat(['is']); // a personal preference
    var laxWordsRe = new RegExp('^(' + laxWords.join('|') + ')$', 'i');


    function titleCase(str, smallWords) {
        if (!str)
            return str;
        return str.replace(/[A-Za-z0-9\u00C0-\u00FF]+[^\s-]*/g, function (match, index, title) {
            if (index > 0 && index + match.length !== title.length &&
              match.search(smallWords) > -1 && title.charAt(index - 2) !== ':' &&
              (title.charAt(index + match.length) !== '-' || title.charAt(index - 1) === '-') &&
              title.charAt(index - 1).search(/[^\s-]/) < 0) {
                return match.toLowerCase();
            }

            if (match.substr(1).search(/[A-Z]|\../) > -1) {
                return match;
            }

            return match.charAt(0).toUpperCase() + match.substr(1);
        });
    }

    spUtils.toTitleCase = function toLaxTitleCase(str) {
        return titleCase(str, laxWordsRe);
    };

    /**
    * Compare two versions strings of the format n.n.n 
    * 0 if they are the same.
    * < 0 if v2 is greater than v1
    * > 0 if v1 is greater than v2
    */
    spUtils.compareVersionStrings = function (ver1, ver2) {
        if (!ver1 || !ver2) { 
            throw Error('compareVersionString cannot take null values');
        }

        var safeParse = function (n) {
            return parseInt(n, 10);
        };

        var sv1 = _(ver1).split('.', 3).map(safeParse).value();
        var sv2 = _(ver2).split('.', 3).map(safeParse).value();

        return _.reduce(_.zip(sv1, sv2), function (acc, a) {
            if (acc !== 0)
                return acc;

            var left = a[0] || 0;
            var right = a[1] || 0;

            return left - right;
        }, 0);
    };

})(spUtils);

