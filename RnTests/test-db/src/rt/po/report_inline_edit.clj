(ns rt.po.report-inline-edit
  (:require [rt.lib.util :refer [throw-not-implemented] :as util]
            [rt.lib.wd :refer [right-click double-click wait-for-jq set-input-value prepare-script-arg wait-until-displayed find-element-with-text]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [get-entity get-color-name-from-rgb-css]]
            [rt.test.expects :refer [expect expect-equals]]
            [rn.services.entity :refer [get-entity-id-for-name]]
            [clojure.string :as string]
            [clj-time.format :as tf]
            [rt.po.common :as common :refer [set-time-control-value exists-present? wait-until wait-until-busy-indicator-done]]
            [rt.po.app :as app :refer [make-app-url enable-config-mode context-menu-exists? choose-context-menu choose-modal-ok]]
            [clj-webdriver.taxi :refer [flash to element elements *driver* find-elements-under find-element-under attribute clear input-text value select-by-text
                                        text click select-option selected? displayed? exists? execute-script present?]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]
            [clojure.data.json :as json]
            [clj-webdriver.taxi :as taxi]))

;; INLINE EDITING DRIVERS

(defn- get-row-column-selector
  "Gets a css selector to select the specified row and column by index.

  Example:
  =========

  (get-row-column-selector 5 5)"
  ([rowIndex columnIndex grid-locator]
    (str grid-locator " div.ngRow[rowindex='" rowIndex "'] div.col" columnIndex))
  ([rowIndex columnIndex]
    (get-row-column-selector rowIndex columnIndex ".spreport-view")))

(defn- get-row-selector
  "Gets a css selector to select the specified row by index.

  Example:
  =========

  (get-row-selector 5)"
  ([rowIndex grid-locator]
    (str grid-locator " div.ngRow[rowindex='" rowIndex "']"))
  ([rowIndex]
    (get-row-selector rowIndex ".spreport-view")))

(defn get-inline-edit-row-state
  "Gets the state of the inline editing row by index. Returns either edit, changed, error, saved or view.

  Example:
  =========

  (get-inline-edit-row-state 5)"
  ([rowIndex grid-locator]
    (wait-until #(exists-present? (get-row-selector rowIndex grid-locator)) 5000)
    (cond
      (exists-present? (str (get-row-selector rowIndex grid-locator) " div.rn-row-wrapper.rn-inline-edit")) "edit"
      (exists-present? (str (get-row-selector rowIndex grid-locator) " div.rn-row-wrapper.rn-inline-changed")) "changed"
      (exists-present? (str (get-row-selector rowIndex grid-locator) " div.rn-row-wrapper.rn-inline-error")) "error"
      (exists-present? (str (get-row-selector rowIndex grid-locator) " div.rn-row-wrapper.rn-inline-saved")) "saved"
      (exists-present? (str (get-row-selector rowIndex grid-locator) " div.rn-row-wrapper.rn-inline-")) "view"
      :else "unknown"))
  ([rowIndex]
    (get-inline-edit-row-state rowIndex ".spreport-view")))

(defn is-selected-row-in-inline-edit-mode
  "Returns true if the selected row is in inline edit mode, false otherwise.

  Example:
  =========

  (is-selected-row-in-inline-edit-mode)"
  ([grid-locator]
    (= "edit" (get-inline-edit-row-state (rt.po.report-view/get-first-selected-row-index grid-locator) grid-locator)))
  ([]
    (is-selected-row-in-inline-edit-mode ".spreport-view")))

(defn- get-inline-edit-toolbar-button-selector
  "Gets the css selector for the inline edit toolbar button.

  Example:
  =========

  (get-inline-edit-toolbar-button-selector \"enterInlineEditMode\")"
  [method grid-locator]
    (str grid-locator " div.rn-inline-edit-toolbar button[ng-click*='" method "']"))

(defn- click-inline-edit-toolbar-button
  "Clicks the specified inline edit toolbar button.

  Example:
  =========

  (click-inline-edit-toolbar-button \"enterInlineEditMode\")"
  [method grid-locator]
  (let [button-selector (get-inline-edit-toolbar-button-selector method grid-locator)]
    (wait-until #(exists-present? button-selector))
    (click button-selector)))

(defn is-report-in-inline-edit-mode
  "Returns true if the report is in inline edit mode, false otherwise.

  Example:
  =========

  (is-report-in-inline-edit-mode)"
  ([grid-locator]
    (not (exists-present? (get-inline-edit-toolbar-button-selector "enterInlineEditMode" grid-locator))))
  ([]
    (is-report-in-inline-edit-mode ".spreport-view")))

(defn enter-inline-edit-mode
  "Puts the report into inline edit mode.

  Example:
  =========

  (enter-inline-edit-mode)"
  ([grid-locator]
    (click-inline-edit-toolbar-button "enterInlineEditMode" grid-locator)
    (wait-until #(is-selected-row-in-inline-edit-mode grid-locator) 10000))
  ([]
    (enter-inline-edit-mode ".spreport-view")))

(defn save-inline-edits
  "Saves any inline edits.

  Example:
  =========

  (save-inline-edits)"
  ([grid-locator]
    (click-inline-edit-toolbar-button "saveInlineEdits" grid-locator)
    (wait-until #(not (is-selected-row-in-inline-edit-mode grid-locator)) 10000))
  ([]
    (save-inline-edits ".spreport-view")))

(defn cancel-inline-edits
  "Cancels any inline edits.

  Example:
  =========

  (cancel-inline-edits)"
  ([grid-locator]
    (click-inline-edit-toolbar-button "cancelInlineEdits" grid-locator)
    (wait-until #(not (is-selected-row-in-inline-edit-mode grid-locator)) 10000))
  ([]
    (cancel-inline-edits ".spreport-view")))

(defn get-row-column-with-focus []
  "Returns the row and column index with the current focus.

  Example:
  =========

  (get-row-column-with-focus)"
  (-> (clj-webdriver.taxi/execute-script
    "var currentElement = $(document.activeElement);
     var foundColumn = false;
     var foundRow = false;
     var result = {
        rowIndex: -1,
        columnIndex: -1
     };

      // Walk the parents until we find the ngCell element
      while (currentElement) {
          var parent = currentElement.parent();

          if (!parent || !parent.length) {
              break;
          }

          if (!foundColumn &&
              parent.attr('sp-data-grid-row-col-scope') &&
              parent.hasClass('ngCell')) {
              result.columnIndex = _.toInteger(parent.attr('sp-data-grid-row-col-scope'));
              foundColumn = true;
          }

          if (!foundRow &&
              parent.attr('rowindex') &&
              parent.hasClass('ngRow')) {
              result.rowIndex = _.toInteger(parent.attr('rowindex'));
              foundRow = true;
          }

          if (foundRow && foundColumn) {
            break;
          }

          currentElement = parent;
      }

      return JSON.stringify(result);
    "
  [])
  (json/read-json)))

(defn- does-row-column-have-focus
  "Returns true if the element in the specified row and column has the focus, false otherwise.

  Example:
  =========

  (does-row-column-have-focus 10 20)"
  [rowIndex columnIndex]
  (wait-until #(>= (:rowIndex (get-row-column-with-focus)) 0) 10000)
  (let [rowColumnWithFocus (get-row-column-with-focus)]
    (and (= rowIndex (:rowIndex rowColumnWithFocus)) (= columnIndex (:columnIndex rowColumnWithFocus)))))

(defn select-report-row-by-row-column
  "Selects the row by row and column.

  Example:
  =========

  (select-report-row-by-row-column 10 20)"
  ([rowIndex columnIndex grid-locator]
    (cond
      (is-report-in-inline-edit-mode)
        (do
          (wait-until #(exists-present? (get-row-column-selector rowIndex columnIndex grid-locator)) 5000)
          (click (get-row-column-selector rowIndex columnIndex grid-locator))
          (wait-until #(does-row-column-have-focus rowIndex columnIndex) 10000))
      (not (is-report-in-inline-edit-mode))
        (click (get-row-selector rowIndex grid-locator))))
  ([rowIndex columnIndex]
    (select-report-row-by-row-column rowIndex columnIndex ".spreport-view")))

(defn select-next-row-by-down-key []
  "Selects the next row by sending a down key to the active row.

  Example:
  =========

  (select-next-row-by-down-key)"
  (wait-until #(>= (:rowIndex (get-row-column-with-focus)) 0) 10000)
  (let [startRowColumn (get-row-column-with-focus)]
    (rt.po.common/send-down-key-to-active-element)
    (wait-until #(= (:rowIndex (get-row-column-with-focus)) (inc (:rowIndex startRowColumn))) 10000)
    (let [nextRowColumn (get-row-column-with-focus)]
      (cond
        (not= (:rowIndex nextRowColumn) (inc (:rowIndex startRowColumn))) (throw (Exception.
          (str "Expected row with index " (inc (:rowIndex startRowColumn)) " to have focus. Actual row with focus " (:rowIndex nextRowColumn))))
        (not= (:columnIndex nextRowColumn) (:columnIndex startRowColumn)) (throw (Exception.
          (str "Expected column with index " (:columnIndex startRowColumn) " to have focus. Actual column with focus " (:columnIndex nextRowColumn))))))))

(defn select-previous-row-by-up-key []
  "Selects the previous row by sending an up key to the active row.

  Example:
  =========

  (select-previous-row-by-up-key)"
  (wait-until #(>= (:rowIndex (get-row-column-with-focus)) 0) 10000)
  (let [startRowColumn (get-row-column-with-focus)]
    (rt.po.common/send-up-key-to-active-element)
    (wait-until #(= (:rowIndex (get-row-column-with-focus)) (dec (:rowIndex startRowColumn))) 10000)
    (let [nextRowColumn (get-row-column-with-focus)]
      (cond
        (not= (:rowIndex nextRowColumn) (dec (:rowIndex startRowColumn))) (throw (Exception.
          (str "Expected row with index " (dec (:rowIndex startRowColumn)) " to have focus. Actual row with focus " (:rowIndex nextRowColumn))))
        (not= (:columnIndex nextRowColumn) (:columnIndex startRowColumn)) (throw (Exception.
          (str "Expected column with index " (:columnIndex startRowColumn) " to have focus. Actual column with focus " (:columnIndex nextRowColumn))))))))

(defn select-next-column-by-tab-key []
  "Selects the next column by sending a tab key to the active column.

  Example:
  =========

  (select-next-column-by-tab-key)"
  (wait-until #(>= (:rowIndex (get-row-column-with-focus)) 0) 10000)
  (let [startRowColumn (get-row-column-with-focus)]
    (rt.po.common/send-tab-key-to-active-element)
    (wait-until #(= (:columnIndex (get-row-column-with-focus)) (inc (:columnIndex startRowColumn))) 10000)
    (let [nextRowColumn (get-row-column-with-focus)]
      (cond
        (not= (:columnIndex nextRowColumn) (inc (:columnIndex startRowColumn))) (throw (Exception.
          (str "Expected column with index " (inc (:columnIndex startRowColumn)) " to have focus. Actual column with focus " (:columnIndex nextRowColumn))))
        (not= (:rowIndex nextRowColumn) (:rowIndex startRowColumn)) (throw (Exception.
          (str "Expected row with index " (:rowIndex startRowColumn) " to have focus. Actual row with focus " (:rowIndex nextRowColumn))))))))

(defn count-inline-rows-with-state
  "Returns a count of the number of inline rows with the specified state which is either saved, changed, error, edit.

  Example:
  =========

  (count-inline-rows-with-state \"saved\")"
  ([state grid-locator]
    (count (elements (str grid-locator " div.ngRow div.rn-row-wrapper.rn-inline-" state))))
  ([state]
    (count-inline-rows-with-state state ".spreport-view")))

(defn get-inline-row-indexes-with-state
  "Returns the indexes of the inline rows with the specified state which is either saved, changed, error, edit.

  Example:
  =========

  (get-inline-row-indexes-with-state \"saved\")"
  ([state grid-locator]
   (let [elements (elements (str grid-locator " div.ngRow:has(div.rn-row-wrapper.rn-inline-" state ")"))]
     (map #(Integer. (attribute % "rowindex")) elements)))
  ([state]
   (get-inline-row-indexes-with-state state ".spreport-view")))

(defn does-inline-edit-row-column-have-validation-error
  "Returns true if the specified row and column has an error, false otherwise.

  Example:
  =========

  does-inline-edit-row-column-have-validation-error 0 1)"
  ([rowIndex columnIndex grid-locator]
    (wait-until #(exists-present? (get-row-column-selector rowIndex columnIndex grid-locator)) 5000)
    (exists-present? (str (get-row-column-selector rowIndex columnIndex grid-locator) " sp-inline-edit-form > sp-custom-validation-message span.custom-validation-messages img")))
  ([rowIndex columnIndex]
    (does-inline-edit-row-column-have-validation-error rowIndex columnIndex ".spreport-view")))

(defn- find-control-container
  "Gets the control container for the inline edit control and the specified row and column index.

  Example:
  =========

  (find-control-container 0 1)"
  ([rowIndex columnIndex grid-locator]
    (wait-until #(exists-present? (get-row-column-selector rowIndex columnIndex grid-locator)) 5000)
    (str (get-row-column-selector rowIndex columnIndex grid-locator) " .edit-form-control-container .edit-form-value"))
  ([rowIndex columnIndex]
    (find-control-container rowIndex columnIndex ".spreport-view")))

;; STRING FIELD FUNCTIONS
(defn get-inline-string
  "Gets the value from the inline string control at the specified row and column index.

  Example:
  =========

  (get-inline-string 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-string-field (find-control-container rowIndex columnIndex grid-locator)))

(defn set-inline-string
  "Sets the value for the inline string control at the specified row and column index.

  Example:
  =========

  (set-inline-string 0 1 \"Test\")"
  [rowIndex columnIndex value & [grid-locator]]
  (rt.po.common/set-string-field (find-control-container rowIndex columnIndex grid-locator) value))

;; MULTILINE STRING FIELD FUNCTIONS
(defn get-inline-multiline
  "Gets the value from the inline multiline string control at the specified row and column index.

  Example:
  =========

  (get-inline-multiline 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-multiline-field (find-control-container rowIndex columnIndex grid-locator)))

(defn set-inline-multiline
  "Sets the value for the inline multiline string control at the specified row and column index.

  Example:
  =========

  (set-inline-multiline 0 1 \"Test\")"
  [rowIndex columnIndex value & [grid-locator]]
  (rt.po.common/set-multiline-field (find-control-container rowIndex columnIndex grid-locator) value))

;; BOOL FIELD FUNCTIONS
(defn get-inline-bool
  "Gets the value from the inline bool control at the specified row and column index.

  Example:
  =========

  (get-inline-bool 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-bool-field (find-control-container rowIndex columnIndex grid-locator)))

(defn set-inline-bool
  "Sets the value for the inline bool control at the specified row and column index.

  Example:
  =========

  (set-inline-bool 0 1 true)"
  [rowIndex columnIndex value & [grid-locator]]
  (rt.po.common/set-bool-field (find-control-container rowIndex columnIndex grid-locator) value))

;; COMBO FIELD FUNCTIONS
(defn get-inline-combo
  "Gets the value from the inline combo control at the specified row and column index.

  Example:
  =========

  (get-inline-combo 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-combo-field (find-control-container rowIndex columnIndex grid-locator)))

(defn set-inline-combo
  "Sets the value for the inline combo control at the specified row and column index.

  Example:
  =========

  (set-inline-combo 0 1 \"Combo Value\")"
  [rowIndex columnIndex value & [grid-locator]]
  (rt.po.common/set-combo-field (find-control-container rowIndex columnIndex grid-locator) value))

;; TIME FIELD FUNCTIONS
(defn get-inline-time
  "Gets the time value from the inline time control at the specified row and column index.

  Example:
  =========

  (get-inline-time 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-string-field (str (find-control-container rowIndex columnIndex grid-locator) " div.sp-time-control-input-cell")))

(defn set-inline-time
  "Sets the time value for the inline time control at the specified row and column index.

  Example:
  =========

  (set-inline-time 0 1 \"5:21 AM\")"
  [rowIndex columnIndex value & [grid-locator]]
  (let [dateTime (tf/parse (tf/formatter "h:m a") value)]
    (let [h (tf/unparse (tf/formatter "h") dateTime)
          m (tf/unparse (tf/formatter "m") dateTime)
          meridian (tf/unparse (tf/formatter "a") dateTime)]
      (rt.po.common/set-time-control-value-via-popup (find-control-container rowIndex columnIndex grid-locator) h m meridian))))

;; DATE FIELD FUNCTIONS
(defn get-inline-date
  "Gets the date value from the inline date control at the specified row and column index.

  Example:
  =========

  (get-inline-date 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-string-field (str (find-control-container rowIndex columnIndex grid-locator) " div.sp-date-control-value-wrapper")))

(defn set-inline-date
  "Sets the date value for the inline date control at the specified row and column index.

  Example:
  =========

  (set-inline-date 0 1 \"20/11/2016\")"
  [rowIndex columnIndex value & [grid-locator]]
  (rt.po.common/set-string-field (str (find-control-container rowIndex columnIndex grid-locator) " div.sp-date-control-value-wrapper") value))

;; LOOKUP FIELD FUNCTIONS
(defn get-inline-lookup
  "Gets the value from the inline lookup control at the specified row and column index.

  Example:
  =========

  (get-inline-lookup 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/get-lookup-field (find-control-container rowIndex columnIndex grid-locator)))

(defn set-inline-lookup
  "Sets the value for the inline lookup control at the specified row and column index.

  Example:
  =========

  (set-inline-lookup 0 1 \"Lookup Value\")"
  [rowIndex columnIndex value & [grid-locator]]
  (rt.po.common/set-lookup-field (find-control-container rowIndex columnIndex grid-locator) value))

(defn clear-inline-lookup
  "Clears the value for the inline lookup control at the specified row and column index.

  Example:
  =========

  (clear-inline-lookup 0 1)"
  [rowIndex columnIndex & [grid-locator]]
  (rt.po.common/clear-lookup-field (find-control-container rowIndex columnIndex grid-locator)))