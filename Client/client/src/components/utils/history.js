// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Module with various miscellaneous functions.
 * We may add wrappers here to various 3rd party library routines...
 *  @namespace spUtils
 */

var spUtils = spUtils || {}; // jshint ignore:line

(function() {

    var exports;
    var doNothing = function() {
    };

    spUtils.History = function History() {
        this._undoList = [];
        this._redoList = [];
        this._lastCommandNotSupportingUndo = null;
        this._changeCounter = 0;
        this._notifyList = [];

    };
    exports = spUtils.History.prototype;


    /**
     * Runs a command.
     *
     * @param {object} command An object having a run function, and an optional undo function.
     *
     * @function
     * @name spUtils.History#run
     */
    exports.run = function(command) {
        if (!command || !command.run) {
            console.error('Invalid command argument');
            return;
        }
        command.run();
        this._redoList.length = 0;
        if (command.undo) {
            this._undoList.push(command);
        } else {
            this._lastCommandNotSupportingUndo = command;
            this._undoList.length = 0; //clear
        }

        // notify listeners
        _.map(this._notifyList, function (f) {
            f();
        });

        this._changeCounter++;
    };


    /**
     * Undoes the last command.
     *
     * @function
     * @name spUtils.History#undo
     */
    exports.undo = function () {
        if (this._undoList.length) {
            var command = this._undoList.pop();
            if (!command.isBookmark) {
                command.undo();
            }
            this._redoList.push(command);
            this._changeCounter++;
        }
    };
    

    /**
     * Ruruns the last undone command.
     *
     * @function
     * @name spUtils.History#redo
     */
    exports.redo = function () {
        if (this._redoList.length) {
            var command = this._redoList.pop();
            if (!command.isBookmark) {
                (command.redo || command.run)();
            }
            this._undoList.push(command);
            this._changeCounter++;
        }
    };


    /**
     * Clears the past and future.
     *
     * @function
     * @name spUtils.History#clear
     */
    exports.clear = function () {
        this.clearUndoList();
        this.clearRedoList();
    };


    /**
     * Is there anything in the undo buffer.
     *
     * @function
     * @name spUtils.History#canUndo
     */
    exports.canUndo = function () {
        return this._undoList.length > 0;
    };


    /**
     * Is there anything in the redo buffer.
     *
     * @function
     * @name spUtils.History#canRedo
     */
    exports.canRedo = function () {
        return this._redoList.length > 0;
    };


    /**
     * Clears the past.
     *
     * @function
     * @name spUtils.History#clearUndoList
     */
    exports.clearUndoList = function () {
        this._undoList.length = 0;
    };


    /**
     * Clears the redo list.
     *
     * @function
     * @name spUtils.History#clearRedoList
     */
    exports.clearRedoList = function () {
        this._redoList.length = 0;
    };


    /**
     * Adds a bookmark to the command sequence and returns it.
     *
     * @param {string} name Optional. The name of the bookmark
     * @returns {object} The bookmark object.
     *
     * @function
     * @name spUtils.History#addBookmark
     */
    exports.addBookmark = function(name) {
        var that = this;
        var command = {
            isBookmark: true,
            name: name,
            run: doNothing,
            undo: doNothing,
            history: this
        };
        command.undo = function() {
            this.history.undoBookmark(this);
        };
        command.redo = function () {
            this.history.redoBookmark(this);
        };
        command.changedSinceBookmark = function () {
            return this.history.changedSinceBookmark(this);
        };
        command.endBookmark = function () {
            var endMark = {
                isBookmarkEnd: true,
                run: doNothing,
                undo: doNothing
            };
            that.run(endMark);
            command._end = endMark;
        };
        this.run(command);
        return command;
    };


    /**
     * Rolls back to just before the specified bookmark.
     *
     * @param {object} bookmark Optional. The bookmark object to roll back to, or if unspecified rolls back to the last bookmark.
     *
     * @function
     * @name spUtils.History#undoBookmark
     */
    exports.undoBookmark = function (bookmark) {
        var match = function (cmd) {
            return cmd.isBookmark && (cmd === bookmark || !bookmark);
        };
        if (!_.some(this._undoList, match)) {
            console.warn('Undo history does not contain any matching bookmarks to undo.');
            return;
        }
        while (this._undoList.length) {
            var peek = _.last(this._undoList);
            this.undo();
            if (match(peek))
                break;
        }
    };



    /**
     * Reruns to the completion of the specified bookmark.
     *
     * @param {object} bookmark Optional. The bookmark object to roll back to, or if unspecified rolls back to the last bookmark.
     *
     * @function
     * @name spUtils.History#undoBookmark
     */
    exports.redoBookmark = function (bookmark) {
        if (bookmark && !bookmark._end) {
            console.error('Cannot redo a bookmark if endBookmark was not called on it.');
        }

        var match = function (cmd) {
            return cmd.isBookmarkEnd && (!bookmark || cmd === bookmark._end);
        };
        if (!_.some(this._redoList, match)) {
            console.warn('Redo history does not contain the bookmark endpoint.');
            return;
        }
        while (this._redoList.length) {
            var peek = _.last(this._redoList);
            this.redo();
            if (match(peek))
                break;
        }
    };


    /**
     * Returns true if there may have been changes since the bookmark.
     *
     * @param {object} bookmark The bookmark object to check.
     *
     * @function
     * @name spUtils.History#changedSinceBookmark
     */
    exports.changedSinceBookmark = function (bookmark) {
        if (this._undoList.length > 0) {
            var peekUndo = _.last(this._undoList);
            if (peekUndo === bookmark)
                return false;
        }
        if (this._redoList.length > 0) {
            var peekRedo = _.last(this._redoList);
            if (peekRedo === bookmark)
                return false;
        }
        return true;
    };



    /**
     * Add a change listener to the history
     *
     * @param {function} callback function called when a change occurs
     * @returns a function which when called removed the listener;
     */
    exports.addChangeListener = function (fn) {
        var that = this;
        this._notifyList.push(fn);

        return function () {
            that._notifyList = _.without(that._notifyList, fn);
        };
    };

    
    /**
     *
     * Get the change counter. This counter is incremented every time there is a change to the graph. 
     *
     */
    exports.getChangeCounter = function () {
        return this._changeCounter;
    };




})();
