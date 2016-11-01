;; The 'report calculated column' dialog

(ns rt.po.report-calculated
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular apply-angular-expression]]
            [rt.lib.wd :refer [right-click double-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.po.common :as common :refer [exists-present?]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? select-by-text selected-options select-option element elements element attribute visible? *driver* wait-until]]))



(defn get-column-name []
  (attribute (element ".calculatedfielddialog-view [ng-model*=columnName]") "value"))

(defn set-column-name [value]
  (set-input-value ".calculatedfielddialog-view [ng-model*=columnName]" value))

(defn get-calculation-script []
  (attribute (element ".calculatedfielddialog-view .expression-editor-control") "value"))

(defn set-calculation-script [expr]
  (apply-angular-expression
    ".calculatedfielddialog-view .expression-editor-control"
    (str "model.script = \"" expr "\""))
  )

(defn select-hint [value]
  (click ".calculatedfielddialog-view [ng-click*=showHints]")
  (double-click (str ".CodeMirror-hints li:contains('" value "')"))
  )

(defn select-function [value]
  (click ".calculatedfielddialog-view [ng-click*=showFunctions]")
  (double-click (str ".functions tr:contains('" value "')"))
  )

(defn ok-button-disabled? []
  (exists-present? (str (common/get-button-selector "OK") "[disabled='disabled']")))

;; BUTTONS

(defn click-ok []
  (click "button:contains(OK)"))

(defn click-cancel []
  (click "button:contains(Cancel)"))