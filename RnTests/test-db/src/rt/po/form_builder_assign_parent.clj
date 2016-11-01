;; The 'Form builder assign parent' dialog

(ns rt.po.form-builder-assign-parent
  (:require [rt.po.edit-form]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.po.common :refer [exists-present? click-modal-dialog-button-and-wait]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core]
            [clj-webdriver.taxi :refer [text click value selected? exists? select-by-text selected-options wait-until select-option element elements element find-element-under attribute visible?]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]))

(defn click-ok []
  (click-modal-dialog-button-and-wait ".sp-form-builder-assign-parent button:contains(OK)"))

(defn click-cancel []
  (click-modal-dialog-button-and-wait ".sp-form-builder-assign-parent button:contains(Cancel)"))

(defn set-parent-element
  [parent-name]
  (clj-webdriver.taxi/select-by-text "select[ng-model='model.selectedParent']" parent-name))
