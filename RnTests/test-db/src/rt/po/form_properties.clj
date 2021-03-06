(ns rt.po.form-properties
  (:require [rt.po.edit-form :as ef]
            [rt.po.view-form :as vf]
            [rt.lib.wd :refer [set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.po.report-view :refer [select-row-by-text]]
            [rt.po.common :as common :refer [exists-present? set-search-text-input-value click-modal-dialog-button-and-wait]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text clear click value selected? element elements attribute visible? find-element-under find-elements-under input-text]]
            [clj-webdriver.taxi :as taxi]))

(def select-tab vf/select-form-tab)

;; Driver functions for form properties
;;helper function
(defn select-row-from-report [value]
  ;; filter to the desired type ... not necessary but doing it anyway
  (set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" value)
  ;; choose the type
  (select-row-by-text value ".entityReportPickerDialog .dataGrid-view")
  ;; ok the typepicker
  (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))


(defn get-form-name []
  (common/get-string "Name"))

(defn set-form-name [value]
  (common/set-string "Name" value))

(defn get-form-description []
  (common/get-multiline "Description"))

(defn set-form-description [value]
  (common/set-multiline "Description" value))

(defn get-form-applications []
  (common/get-lookup "Applications"))

(defn set-form-applications [value]
  (common/set-lookup "Applications" value))

(defn clear-form-applications []
  (common/clear-lookup "Applications"))

(defn get-form-icon []
  (common/get-lookup "Icon"))

(defn set-form-icon [value]
  (common/set-lookup "Icon" value))

(defn clear-form-icon []
  (common/clear-lookup "Icon"))

(defn get-default-form []
  (common/get-bool "Default form"))

(defn set-default-form [value]
  (common/set-bool "Default form" value))
  
(defn get-show-help []
  (common/get-bool "Show help"))

(defn set-show-help [value]
  (common/set-bool "Show help" value))  

(defn get-enable-convert []
  (common/get-bool "Enable convert"))

(defn set-enable-convert [value]
  (common/set-bool "Enable convert" value))

(defn get-display-workflow-tasks []
  (common/get-bool "Display workflow tasks"))

(defn set-display-workflow-tasks [value]
  (common/set-bool "Display workflow tasks" value))

;; Driver functions for object properties

(defn get-object-name []
  (common/get-string "Object name"))

(defn set-object-name [value]
  (common/set-string "Object name" value))

(defn get-object-script-name []
  (common/get-string "Script name"))

(defn set-object-script-name [value]
  (common/set-string "Script name" value))

(defn get-object-description []
  (value (last (elements ".edit-form-control-container:contains(Description) textarea"))))

(defn set-object-description [value]
  (clear (last (elements ".edit-form-control-container:contains(Description) textarea")))
  (input-text (last (elements ".edit-form-control-container:contains(Description) textarea")) value))

(defn get-object-applications []
  (let [elm (first (filter exists-present? (elements ".edit-form-control-container:has(.edit-form-title span:contains(Applications)) .edit-form-value")))]
    (value (find-element-under elm {:tag :input}))))

(defn set-object-applications [value]
  (let [elm (first (filter exists-present? (elements ".edit-form-control-container:has(.edit-form-title span:contains(Applications)) .edit-form-value")))]
    (click (find-element-under elm {:css "button[uib-popover=Edit]"}))
    (select-row-from-report value)))

(defn clear-object-applications []
  (let [elm (first (filter exists-present? (elements ".edit-form-control-container:has(.edit-form-title span:contains(Applications)) .edit-form-value")))]
    (click (find-element-under elm {:css "button[uib-popover=Clear]"}))))

(defn get-object-applications []
  (let [elm (first (filter exists-present? (elements ".edit-form-control-container:has(.edit-form-title span:contains(Applications)) .edit-form-value")))]
    (value (find-element-under elm {:tag :input}))))

(defn set-object-applications [value]
  (let [elm (first (filter exists-present? (elements ".edit-form-control-container:has(.edit-form-title span:contains(Applications)) .edit-form-value")))]
    (click (find-element-under elm {:css "button[uib-popover=Edit]"}))
    (select-row-from-report value)))

(defn clear-object-applications []
  (let [elm (first (filter exists-present? (elements ".edit-form-control-container:has(.edit-form-title span:contains(Applications)) .edit-form-value")))]
    (click (find-element-under elm {:css "button[uib-popover=Clear]"}))))

(defn get-object-icon []
  (common/get-lookup "Icon"))

(defn set-object-icon [value]
  (common/set-lookup "Icon" value))

(defn clear-object-icon []
  (common/clear-lookup "Icon"))

(defn get-extends-from []
  (common/get-lookup "Extends from"))

(defn set-extends-from [value]
  (common/set-lookup "Extends from" value))

(defn clear-extends-from []
  (common/clear-lookup "Extends from"))

(defn get-abstract-type []
  (common/get-bool "Abstract type"))

(defn set-abstract-type [value]
  (common/set-bool "Abstract type" value))

(defn get-multiple-type []
  (common/get-bool "Multiple type"))

(defn set-multiple-type [value]
  (common/set-bool "Multiple type" value))

(defn click-ok []
  (click-modal-dialog-button-and-wait ".modal-footer button:contains(OK)"))

(defn click-cancel []
  (click-modal-dialog-button-and-wait ".modal-footer button:contains(Cancel)"))


