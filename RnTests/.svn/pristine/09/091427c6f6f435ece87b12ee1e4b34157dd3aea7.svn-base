;; The 'report add relationship' dialog

(ns rt.po.report-add-relationship
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.po.common :as common :refer [exists-present? wait-until]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [exists? text click value selected? select-by-text selected-options select-option element elements element attribute visible?]]))


(defn find-checkbox-field [name]
  (str "div:has(:contains('" name "')) > input"))

(defn get-checkbox-field-value [name]
  (selected? (find-checkbox-field name)))

(defn set-checkbox-field-value [name value]
  (when (not= (get-checkbox-field-value name) value)
    (click (find-checkbox-field name) )))



;; Advanced OPTIONS PANEL
(defn options-expanded? []
  (exists-present? ".relationshipdialog-view [uib-collapse].in"))

(defn options-expand []
  (when (not (options-expanded?))
    (click ".btnAdvancedOptions")
    (wait-for-angular)))




(defn get-type []
  (text (first (selected-options (element ".relationshipdialog-view [ng-model*=relationshipType]" ))))
  )

(defn set-type [value]
  (select-by-text (element ".relationshipdialog-view [ng-model*=relationshipType]" ) value)
  )

;; add/remove relationship by add-relationship dialog window
(defn find-add-relationship-button [name]
  (str "tr:has(td:contains('" name "')) [ng-click*=addRelationship]")
  )

(defn add-relationship [name]
  (click (find-add-relationship-button name) )
  )

(defn find-remove-relationship-button [name]
  (first (elements (str ".rightContainer image[title*='" name "']")))
  )

(defn remove-relationship [name]
  (click (find-remove-relationship-button name))
  )

(defn get-filter-relationship-name []
  (attribute (element ".relationshippicker-view [ng-model*=searchText]") "value"))

(defn set-filter-relationship-name [value]
  (set-input-value ".relationshippicker-view [ng-model*=searchText]" value))


(defn get-add-name-field-to-the-report []
  (selected? (element ".relationshippicker-view [ng-model='model.addName']")))

(defn set-add-name-field-to-the-report [value]
  (options-expand)
  (wait-until #(exists? (element ".relationshippicker-view [ng-model='model.addName']"))  2000)
  (when (not= (get-add-name-field-to-the-report) value)
    (click (element ".relationshippicker-view [ng-model='model.addName']"))))

(defn get-show-hidden-relationship []
  (selected? (element ".relationshippicker-view [ng-model='model.showHidden']")))

(defn set-show-hidden-relationship [value]
  (options-expand)
  (wait-until #(exists? (element ".relationshippicker-view [ng-model='model.showHidden']"))  2000)
  (when (not= (get-show-hidden-relationship) value)
    (click (element ".relationshippicker-view [ng-model='model.showHidden']"))))

(defn click-relationships-tab []
      (click "[ng-click*=select]:contains('Relationships')"))

(defn click-derived-type-tab []
      (click "[ng-click*=select]:contains('Derived')"))


;; BUTTONS

(defn click-ok []
  (click "button:contains(OK)"))

(defn click-cancel []
  (click "button:contains(Cancel)"))