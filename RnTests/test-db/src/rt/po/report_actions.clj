;; The 'Action' dialog

(ns rt.po.report-actions
  (:require [rt.po.edit-form]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.po.common :refer [exists-present?]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core]
            [clj-webdriver.taxi :refer [text click value selected? exists? select-by-text selected-options wait-until select-option element elements element find-element-under attribute visible?]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]
            [clojure.string :as string]))

(defn click-record-actions-tab []
  (click "div.reportactionsdialog-view [heading='Record Actions']"))

(defn click-report-actions-tab []
  (click "div.reportactionsdialog-view [heading='Report Actions']"))

(defn click-ok []
  (click "div.reportactionsdialog-view button:contains(OK)")
  (rt.lib.wd-ng/wait-for-angular))

(defn click-cancel []
  (click "div.reportactionsdialog-view button:contains(Cancel)"))

(defn set-checkbox-value
  [action-label value column]
  (let [checkboxQuery (str "div.reportactionsdialog-view div.tab-pane:visible div.ngViewport div.ngCanvas div.ngRow:has(div.ngCell:contains(" action-label ")) div.ngCell." column " :checkbox")]
    (let [checked (exists-present? (str checkboxQuery ":checked"))]
      (when (and value (not checked)) (click checkboxQuery))
      (when (and (not value) checked) (click checkboxQuery)))))

(defn set-enabled-value
  [action-label enabled]
  (set-checkbox-value action-label enabled "col2"))

(defn set-show-button-value
  [action-label enabled]
  (set-checkbox-value action-label enabled "col3"))

(defn is-show-button-disabled?
  [action-label]
  (let [checkboxQuery (str "div.reportactionsdialog-view div.tab-pane:visible div.ngViewport div.ngCanvas div.ngRow:has(div.ngCell:contains(" action-label ")) div.ngCell.col3 :checkbox:disabled")]
    (exists-present? checkboxQuery)))