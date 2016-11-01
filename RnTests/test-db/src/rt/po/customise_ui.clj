(ns rt.po.customise-ui
  (:require [rt.lib.util :refer [throw-not-implemented]]
            [rt.lib.wd :refer [right-click double-click wait-for-jq set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression]]
            [rt.po.app :as app :refer [make-app-url enable-config-mode choose-context-menu choose-modal-ok]]
            [rt.po.access-rules :as security]
            [clj-webdriver.taxi :refer [to element elements find-elements-under find-element-under attribute clear input-text value
                                        text click select-option selected? displayed? exists? select-by-text selected-options]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]))

(defn set-application [role-name]
  (select-by-text ".securityUI-content select[ng-model*=selectedTopMenuNode]" role-name) )

(defn get-application []
  (text (first (selected-options ".securityUI-content select[ng-model*=selectedTopMenuNode]"))) )

(defn set-role [role-name]
  (select-by-text ".securityUI-content select[ng-model*=selectedSubject]" role-name) )

(defn get-role []
  (text (first (selected-options ".securityUI-content select[ng-model*=selectedSubject]"))) )

(defn get-include-user []
  (selected? ".securityUI-content input[ng-model*=includeUsers]"))

(defn set-include-user [value]
  (when (not= (get-include-user) value)
    (click ".securityUI-content input[ng-model*=includeUsers]")))

(defn get-row-index [node-name]
  (let [element (element (str ".securityUI-content .ngCanvas .ngRow:contains(" node-name ")"))
        rowindex -1]
    (if (:webelement element)
      (security/get-row-index-from-row-element element) rowindex )
    )
   )

(defn can-modify? []
  (not (evaluate-angular-expression ".securityUI-content .ngCanvas .ngRow" "!canModifySelections()")))

(defn node-selected? [node-name]
  (let [rowindex (get-row-index node-name)]
    (if (> rowindex -1)
      (let [input (str ".securityUI-content .ngCanvas .ngRow[rowindex=" rowindex "] .ngCellText.col0 input")]
        (selected? input)))) )

(defn select-item [node-name]
 (if (security/save-button-visible?)
   (let [rowindex (get-row-index node-name)]
     (if (and (> rowindex -1) (can-modify?))
       (let [input (str ".securityUI-content .ngCanvas .ngRow[rowindex=" rowindex "] .ngCellText.col0 input")]
         (when-not (selected? input)
           (click input)))))) )

(defn deselect-item [node-name]
  (if (security/save-button-visible?)
    (let [rowindex (get-row-index node-name)]
      (if (and (> rowindex -1) (can-modify?))
        (let [input (str ".securityUI-content .ngCanvas .ngRow[rowindex=" rowindex "] .ngCellText.col0 input")]
          (when (selected? input)
            (click input)))))))

(defn select-all-from-node [node-name]
  (if (security/save-button-visible?)
    (let [rowindex (get-row-index node-name)]
      (if (and (> rowindex -1) (can-modify?))
        (let [row (element (str ".securityUI-content .ngCanvas .ngRow[rowindex=" rowindex "] .ngCellText.col0 input"))]
          (right-click row)
          (click (first (elements ".menuItem"))))))))

(defn deselect-all-from-node [node-name]
  (if (security/save-button-visible?)
    (let [rowindex (get-row-index node-name)]
      (if (and (> rowindex -1) (can-modify?))
        (let [row (element (str ".securityUI-content .ngCanvas .ngRow[rowindex=" rowindex "] .ngCellText.col0 input"))]
          (right-click row)
          (click (last (elements ".menuItem"))))))))