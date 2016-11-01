;; The 'report column format' dialog

(ns rt.po.report-format
  (:require [rt.lib.wd :refer [right-click set-input-value double-click find-element-with-text cancel-editable-edit-mode wait-for-jq]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression execute-script-on-element]]
            [rt.lib.wd-rn :refer [drag-n-drop test-id-css]]
            [rt.po.edit-form :as ef]
            [rt.test.core :refer [*test-context*]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.po.report-view :as rv]
            [rt.po.edit-form :as ef]
            [clojure.string :as string]
            [clojure.inspector :as inspector]
            [clj-webdriver.taxi :refer [text exists? click value selected? find-elements-under find-element-under find-element select-by-text options selected-options select-option element elements element attribute visible?]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]
            [clojure.string :as string]
            [clj-time.core :as t]
            [clj-time.coerce :as tc]
            [rt.po.common :refer [click-modal-dialog-button-and-wait exists-present? wait-until]]
            [rt.po.app :as app]))

(defn click-value-formatting []
  (let [selector ".spConditionalFormattingDialog a[ng-click*=select]:contains('Value Formatting')"]
    (wait-until #(exists-present? selector) 200)
    (click selector))
  (wait-until #(exists? ".labelBlockMedium:Contains('Alignment')")))

(defn click-conditional-formatting []
  (click (str ".spConditionalFormattingDialog a[ng-click*=select]:contains('Conditional Formatting')")))

(defn click-add-rule []
  (click "button:[ng-click='addRule()']"))

(defn get-highlight-rule-lists []
  (let [conditional-grid "[ng-grid='model.condFormatting.highlightRuleGridOptions'] .ngViewport"]
    (find-elements-under conditional-grid (by-css "[ng-repeat='row in renderedRows']"))))

(defn get-icon-rule-lists []
  (let [conditional-grid "[ng-grid='model.condFormatting.iconRuleGridOptions'] .ngViewport"]
    (find-elements-under conditional-grid (by-css "[ng-repeat='row in renderedRows']"))))

(defn remove-highlight-rule [row-index]
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "]"))]
    (click (find-element-under highlight-rule-row (by-css ".removeRuleButton")))))

(defn remove-icon-rule [row-index]
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.iconRuleGridOptions'] [rowindex=" row-index "]"))]
    (click (find-element-under highlight-rule-row (by-css ".removeRuleButton")))))

(defn hightlight-label-text-content-exist? [row-index]
  (try
    (exists? (element (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "] .leftMargin")))
    (catch Exception e
      false)))

(defn icon-label-text-content-exist? [row-index]
  (try
    (exists? (element (str "[ng-grid='model.condFormatting.iconRuleGridOptions'] [rowindex=" row-index "] .leftMargin")))
    (catch Exception e
      false)))

(defn highlight-rule-value-editor-content-exist? [row-index css-value]
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "]"))]
    (try
      (exists? (find-element-under highlight-rule-row (by-css (str "[id='valueEditorContent'] " css-value))))
      (catch Exception e
        false))))

(defn icon-rule-value-editor-content-exist? [row-index css-value]
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.iconRuleGridOptions'] [rowindex=" row-index "]"))]
    (try
      (exists? (find-element-under highlight-rule-row (by-css (str "[id='valueEditorContent'] " css-value))))
      (catch Exception e
        false))))

(defn progress-bar-value-editor-content-exist? [row css-value]
  (try
    (exists? (find-element-under row (by-css (str "[id='valueEditorContent'] " css-value))))
    (catch Exception e
      false)))



(defn get-child-count-of-highlight-value [row-index css-value]
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "]"))]
    (inspector/get-child-count (find-element-under highlight-rule-row (by-css (str "[id='valueEditorContent'] " css-value))))))

(defn skip-value []
  )

(defn insert-text-value [condition-row value]
  (set-input-value (find-element-under condition-row (by-css "[ng-model='valueEditorModel.value']")) value))

(defn insert-date-value [condition-row value]
  (set-input-value (find-element-under condition-row (by-css "[ng-model='internalModel.value']")) value))

(defn insert-time-value [condition-row value]
  ;; convert time string to hour and minute two parts e.g. "8:25"
  (let [time-array (string/split value #":")]
    (set-input-value (find-element-under condition-row (by-css "[ng-model='hours']")) (get time-array 0))
    (set-input-value (find-element-under condition-row (by-css "[ng-model='minutes']")) (get time-array 1))
    ))

(defn insert-date-and-time-value [condition-row value]
  ;; convert datetime string to date and time two parts. e.g. "2/18/2015 11:30"
  (let [date-time-array (string/split value #" ")]
    (insert-date-value condition-row (get date-time-array 0))
    (insert-time-value condition-row (get date-time-array 1))
    ))

(defn insert-number-value [condition-row value]
  (set-input-value (find-element-under condition-row (by-css "[ng-model='model.value']")) value))


(defn insert-choice-values [condition-row values]
  ;; click the dropdown icon button to popup the choice options
  (click (find-element-under condition-row (by-css ".dropdownButton")))

  ;; convert value string to array to loop through
  (let [values-array (string/split values #";")]
    (doall (map-indexed (fn [index value]
                          ;; click the checkbox
                          (click (last (elements (str "[ng-repeat*=entityCheckBoxItem]:contains('" value "') [type=checkbox]"))))
                          )
                        values-array)))

  ;; click the dropdown icon again to close choice options
  (click (find-element-under condition-row (by-css ".dropdownButton"))))

(defn insert-lookup-value [condition-row value]
  (let [lookup-control (find-element-under condition-row (by-css ".valueEditor-view [options*=pickerOptions] button[ng-click*=openDetail]"))]
    (click (element lookup-control))
    ;; filter to the desired type ... not necessary but doing it anyway
    (set-input-value ".entityReportPickerDialog .sp-search-control input" value)
    ;; seem to need to sleep a little... waiting on angular isn't enough
    ;; to be investigated
    (Thread/sleep 1000)
     
    ;; check current lookup picker is normal report or structure view report
    
    (cond
      (= (exists? (str ".structureViewPickerNodes")) true) (rv/select-structure-item-row-by-text value)
      :else (rv/select-row-by-text value ".entityReportPickerDialog .dataGrid-view"))
   
    ;; ok the typepicker
    (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]")
    ))


(defn insert-highlight-value [row-index condition-row value]
  (cond
    (= (highlight-rule-value-editor-content-exist? row-index ".dropdownButton") true)
    (insert-choice-values condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".inlineRelPicker") true)
    (insert-lookup-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".sp-date-and-time-control") true)
    (insert-date-and-time-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".sp-date-control") true)
    (insert-date-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".sp-time-control") true)
    (insert-time-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".sp-currency-control") true)
    (insert-number-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".sp-decimal-control") true)
    (insert-number-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index ".sp-number-control") true)
    (insert-number-value condition-row value)

    (= (highlight-rule-value-editor-content-exist? row-index "[ng-model='valueEditorModel.value']") true)
    (insert-text-value condition-row value)

    :else (skip-value)))

(defn insert-icon-value [row-index condition-row value]
  (cond
    (= (icon-rule-value-editor-content-exist? row-index ".dropdownButton") true)
    (insert-choice-values condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".inlineRelPicker") true)
    (insert-lookup-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".sp-date-and-time-control") true)
    (insert-date-and-time-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".sp-date-control") true)
    (insert-date-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".sp-time-control") true)
    (insert-time-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".sp-currency-control") true)
    (insert-number-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".sp-decimal-control") true)
    (insert-number-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index ".sp-number-control") true)
    (insert-number-value condition-row value)

    (= (icon-rule-value-editor-content-exist? row-index "[ng-model='valueEditorModel.value']") true)
    (insert-text-value condition-row value)

    :else (skip-value)))

(defn select-by-colour [highlight-rule-row colour]
  (let [q (str "a.fgBgDropdownMenuItem:visible:contains('" colour "')")]
    (click (find-element-under highlight-rule-row (by-css ".fgBgDropdownIcon")))
    (wait-until #(exists-present? q) 2000)
    (click q)))

(defn select-by-icon [icon-rule-row icon]
  (let [q (str "a:has([title=\"" icon "\"]:visible)")]
    (click (find-element-under icon-rule-row (by-css ".iconDropdownIcon")))
    (wait-until #(exists-present? q) 2000)
    (click q)))

(defn set-highlight-condition-rule-value [row-index condition-row oper value]
  (let [oper-control (find-element-under condition-row (by-css ".ruleOperator"))]
    ;; set operator
    (select-by-text (element oper-control) oper)
    (wait-for-angular)
    (case oper
      "Is defined" (skip-value)
      "Is not defined" (skip-value)
      "Yes" (skip-value)
      "No" (skip-value)
      "Today" (skip-value)
      "This month" (skip-value)
      "This quarter" (skip-value)
      "This year" (skip-value)
      "Current FY" (skip-value)
      (insert-highlight-value row-index condition-row value))
    (wait-for-angular)))

(defn set-icon-condition-rule-value [row-index condition-row oper value]
  (let [oper-control (find-element-under condition-row (by-css ".ruleOperator"))]
    (select-by-text (element oper-control) oper)
    (wait-for-angular)
    (case oper
      "Is defined" (skip-value)
      "Is not defined" (skip-value)
      "Yes" (skip-value)
      "No" (skip-value)
      "Today" (skip-value)
      "This month" (skip-value)
      "This quarter" (skip-value)
      "This year" (skip-value)
      "Current FY" (skip-value)
      (insert-icon-value row-index condition-row value))
    (wait-for-angular)))


(defn click-highlight-condition-picker-report [row-index]
     "click highlight condition field picker report

Syntax:
    (rt.po.report-format/click-highlight-condition-picker-report row-index)


Examples:
    (rt.po.report-format/click-highlight-condition-picker-report 0)

Prerequisites:
    Report builder"  
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "]"))]
    (cond
      (= (hightlight-label-text-content-exist? row-index) true) ;; find the label content exists
      (let [label-text (text (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "] .leftMargin"))]
        (case label-text
          "Else" (skip-value)                               ;; skip set value when the label is else
             (let [lookup-control (find-element-under highlight-rule-row (by-css ".valueEditor-view [options*=pickerOptions] button[ng-click*=openDetail]"))]
    			(click (element lookup-control)))
          )
        )

      :else
      (let [lookup-control (find-element-under highlight-rule-row (by-css ".valueEditor-view [options*=pickerOptions] button[ng-click*=openDetail]"))]
    			(click (element lookup-control)))
      )
    )
  )


(defn set-highlight-condition-rule [row-index oper value colour]
  ;;set oper first
  (let [highlight-rule-row (element (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "]"))]
    (cond
      (= (hightlight-label-text-content-exist? row-index) true) ;; find the label content exists
      (let [label-text (text (str "[ng-grid='model.condFormatting.highlightRuleGridOptions'] [rowindex=" row-index "] .leftMargin"))]
        (case label-text
          "Else" (skip-value)                               ;; skip set value when the label is else
          (set-highlight-condition-rule-value row-index highlight-rule-row oper value)
          )
        )

      :else
      (set-highlight-condition-rule-value row-index highlight-rule-row oper value)
      )

    (select-by-colour highlight-rule-row colour)))

(defn set-icon-condition-rule [row-index oper value icon]
  ;;set oper first
  (let [icon-rule-row (element (str "[ng-grid='model.condFormatting.iconRuleGridOptions'] [rowindex=" row-index "]"))]
    (cond
      (= (icon-label-text-content-exist? row-index) true)   ;; find the label content exists
      (let [label-text (text (str "[ng-grid='model.condFormatting.iconRuleGridOptions'] [rowindex=" row-index "] .leftMargin"))]
        (case label-text
          "Else" (skip-value)                               ;; skip set value when the label is else
          (set-icon-condition-rule-value row-index icon-rule-row oper value)
          )
        )

      :else
      (set-icon-condition-rule-value row-index icon-rule-row oper value)
      )

    (select-by-icon icon-rule-row icon)))


(defn set-highlight-conditions
  "
set highlight conditions


Syntax:
  ?(rt.po.report-format/set-highlight-conditions conditions-array)

Examples:
  ?(rt.po.report-format/set-highlight-conditions [{:oper \"=\"  :value \"24\" :colour \"Dark red on light red\"} {:oper \"<>\"  :value \"14\" :colour \"Dark blue on light blue\"} {:oper \"\" :value \"\" :colour \"Green background\"}])
  (rt.po.report-format/set-highlight-conditions [{:oper \">\"  :value \"2/16/2015 10:14\" :colour \"Dark red on light red\"} {:oper \"<\"  :value \"2/17/2015 11:24\" :colour \"Dark blue on light blue\"} {:oper \"\" :value \"\" :colour \"Green background\"}])
  (rt.po.report-format/set-highlight-conditions [{:oper \"Any of\"  :value \"Monday;Tuesday\" :colour \"Dark red on light red\"} {:oper \"Any of\"  :value \"Friday;Sunday\" :colour \"Dark blue on light blue\"} {:oper \"\" :value \"\" :colour \"Green background\"}])

Prerequisites:
  Report builder mode, open the Format Column dialog window, select 'Highlight' format option, the rules number should match condition controls"
  [rules]
  (try
    (wait-until #(expect-equals (count (get-highlight-rule-lists)) (count rules)) 3000)

    (catch Exception e
      (throw (Exception. (str "insert highlight conditions rules are not match condition controls ")))))



  (doall (map-indexed (fn [index rule]
                        (set-highlight-condition-rule index (str (get rule :oper)) (str (get rule :value)) (str (get rule :colour)))
                        )
                      rules)))

(defn set-icon-conditions
  "
set icon conditions


Syntax:
  ?(rt.po.report-format/set-icon-conditions conditions-array)

Examples:
  ?(rt.po.report-format/set-icon-conditions [{:oper \"=\"  :value \"24\" :icon \"Black Diamond Format Icon\"} {:oper \"<>\"  :value \"14\" :icon \"Green Circle Format Icon\"} {:oper \"\" :value \"\" :icon \"Red Star Format Icon\"}])
  (rt.po.report-format/set-icon-conditions [{:oper \">\"  :value \"2/16/2015 10:14\" :icon \"Black Diamond Format Icon\"} {:oper \"<\"  :value \"2/17/2015 11:24\" :icon \"Green Circle Format Icon\"} {:oper \"\" :value \"\" :icon \"Red Star Format Icon\"}])
  (rt.po.report-format/set-icon-conditions [{:oper \"Any of\"  :value \"Monday;Tuesday\" :icon \"Black Diamond Format Icon\"} {:oper \"Any of\"  :value \"Friday;Sunday\" :icon \"Green Circle Format Icon\"} {:oper \"\" :value \"\" :icon \"Red Star Format Icon\"}])

Prerequisites:
  Report builder mode, open the Format Column dialog window, select 'Highlight' format option, the rules number should match condition controls"
  [rules]
  (try
    (wait-until #(expect-equals (count (get-icon-rule-lists)) (count rules)) 3000)

    (catch Exception e
      (throw (Exception. (str "insert icon conditions rules are not match condition controls ")))))



  (doall (map-indexed (fn [index rule]
                        (set-icon-condition-rule index (str (get rule :oper)) (str (get rule :value)) (str (get rule :icon)))
                        )
                      rules)))

(defn get-progress-bar-minimum []
  (let [min-content ".displayTable:contains('Minimum')"]
    (attribute (find-element-under min-content (by-css "[ng-model='model.value']")) "value")))

(defn set-progress-bar-minimum [value]
  (let [min-content ".displayTable:contains('Minimum')"]
    (cond
      (= (progress-bar-value-editor-content-exist? min-content ".sp-date-and-time-control") true)
      (insert-date-and-time-value min-content value)

      (= (progress-bar-value-editor-content-exist? min-content ".sp-date-control") true)
      (insert-date-value min-content value)

      (= (progress-bar-value-editor-content-exist? min-content ".sp-time-control") true)
      (insert-time-value min-content value)

      :else (set-input-value (find-element-under min-content (by-css "[ng-model='model.value']")) value))))

(defn get-progress-bar-maximum []
  (let [max-content ".displayTable:contains('Maximum')"]
    (attribute (find-element-under max-content (by-css "[ng-model='model.value']")) "value")
    ))

(defn set-progress-bar-maximum [value]
  (let [max-content ".displayTable:contains('Maximum')"]
    (cond
      (= (progress-bar-value-editor-content-exist? max-content ".sp-date-and-time-control") true)
      (insert-date-and-time-value max-content value)

      (= (progress-bar-value-editor-content-exist? max-content ".sp-date-control") true)
      (insert-date-value max-content value)

      (= (progress-bar-value-editor-content-exist? max-content ".sp-time-control") true)
      (insert-time-value max-content value)

      :else (set-input-value (find-element-under max-content (by-css "[ng-model='model.value']")) value))))

(defn get-progress-bar-colour []
  (text ".spConditionalFormattingDialog .dropdownSelectedItemName"))

(defn set-progress-bar-colour
  "
set progress bar colour


Syntax:
  ?(rt.po.report-format/set-progress-bar-colour colour-name)

Examples:
  ??(rt.po.report-format/set-progress-bar-colour \"Dark Red\")

Prerequisites:
  Report builder mode, open the Format Column dialog window, select 'Progress Bar' format option"
  [value]
  (let [colour-content ".displayTable:contains('Colour')"]
    (click (find-element-under colour-content (by-css ".dropdownIcon")))
    (click (element (str "[ng-repeat='color in model.availableColors']:contains('" value "')")))))

(defn get-alignment []
  (text (first (selected-options (element ".spConditionalFormattingDialog [ng-model*=selectedAlignmentOption]")))))

(defn set-alignment [value]
  (select-by-text (element ".spConditionalFormattingDialog [ng-model*=selectedAlignmentOption]") value))

(defn get-value []
  (attribute (element ".spConditionalFormattingDialog [ng-model=\"model.value\"]") "value"))

(defn set-value [value]
  (set-input-value ".spConditionalFormattingDialog [ng-model=\"model.value\"]" value))

(defn find-hierarchy-report []
  (str ".spConditionalFormattingDialog div[ng-show*='valueFormatting.showStructureView']  select"))  
  
(defn get-hierarchy-report []
  (text (first (selected-options (element (find-hierarchy-report))))))

(defn set-hierarchy-report [value]
  (select-by-text (element (find-hierarchy-report)) value))
  
(defn get-decimal-places []
  (value ".spConditionalFormattingDialog .tab-pane div:contains('Decimal places:') input"))

(defn set-decimal-places [value]
  (set-input-value ".spConditionalFormattingDialog .tab-pane div:contains('Decimal places:') input" value))

(defn get-prefix []
  (attribute (element ".spConditionalFormattingDialog [ng-model=\"model.valueFormatting.prefix\"]") "value"))

(defn set-prefix [value]
  (set-input-value ".spConditionalFormattingDialog [ng-model=\"model.valueFormatting.prefix\"]" value))

(defn get-suffix []
  (attribute (element ".spConditionalFormattingDialog [ng-model=\"model.valueFormatting.suffix\"]") "value"))

(defn set-suffix [value]
  (set-input-value ".spConditionalFormattingDialog [ng-model=\"model.valueFormatting.suffix\"]" value))


(defn get-thumbnail-size []
  (text (first (selected-options (element ".spConditionalFormattingDialog [ng-model*=selectedEntity]")))))

(defn set-thumbnail-size [value]
  (select-by-text (element ".spConditionalFormattingDialog [ng-model*=selectedEntity]") value))

(defn get-thumbnail-scaling []
  (let [scaling-control (str "[ng-repeat='entity in entities']:has(.ng-touched)")]
    (text (find-element-under scaling-control (by-css ".ng-binding")))))

(defn set-thumbnail-scaling [value]
  (let [scaling-control (str "[ng-repeat='entity in entities']:has(.ng-binding:contains('" value "'))")]
    (click (find-element-under scaling-control (by-css "[type=radio]")))))

(defn get-datetime-format []
  (text (first (selected-options (element ".spConditionalFormattingDialog [ng-model*=selectedDateTimeFormat]")))))

(defn get-datetime-format-options []
  (wait-until #(exists? (element ".spConditionalFormattingDialog [ng-model*=selectedDateTimeFormat]")))
  (mapv #(text %) (options (element ".spConditionalFormattingDialog [ng-model*=selectedDateTimeFormat]"))))


(defn set-datetime-format [value]
  (wait-until #(exists? (element ".spConditionalFormattingDialog [ng-model*=selectedDateTimeFormat]")))
  (select-by-text (element ".spConditionalFormattingDialog [ng-model*=selectedDateTimeFormat]") value))

(defn get-sample []
  (text (element ".spConditionalFormattingDialog .valueFormattingSampleText")))


(defn get-format-type []
  (text (first (selected-options (element ".spConditionalFormattingDialog [ng-change*=onFormatTypeChanged]")))))

(defn set-format-type [value]
  (select-by-text (element ".spConditionalFormattingDialog [ng-change*=onFormatTypeChanged]") value))

(defn get-format-scheme []
  (text (first (selected-options (element ".spConditionalFormattingDialog [ng-change*=onSchemeChanged]")))))

(defn set-format-scheme [value]
  (select-by-text (element ".spConditionalFormattingDialog [ng-change*=onSchemeChanged]") value))

(defn click-add-rule []
  (click ".spConditionalFormattingDialog [ng-click*=addRule]"))
;; BUTTONS

(defn click-ok []
  (click "button:contains(OK)")
  (wait-for-angular)
  (Thread/sleep 1000))

(defn click-cancel []
  (click "button:contains(Cancel)"))
