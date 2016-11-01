;; The 'New Report' dialog

(ns rt.po.report-new
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.po.common :as common :refer [exists-present? set-search-text-input-value
                                             click-modal-dialog-button-and-wait click-tab-heading wait-until]]
            [rt.po.report-view :as rv :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? exists? select-by-text options selected-options select-option element elements element find-element-under attribute visible?]]
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]]
            [clj-webdriver.taxi :as taxi]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))


;; GENERIC ACCESSORS
;; E.g.:
;; (get-checkbox-field-value "Default Display Report")
;; (get-checkbox-field-value "Default Picker Report")
;; (get-checkbox-field-value "Hide Action Bar")
;; (get-checkbox-field-value "Hide Report Header")
;; (get-checkbox-field-value "Enable on desktop")
;; (get-checkbox-field-value "Enable on tablet")
;; (get-checkbox-field-value "Enable on mobile")

(defn find-checkbox-field [name]
  (str "div:has(:contains('" name "')) > input"))

(defn get-checkbox-field-value [name]
  (selected? (find-checkbox-field name)))

(defn set-checkbox-field-value [name value]
  (when (not= (get-checkbox-field-value name) value)
    (click (find-checkbox-field name))))


(defn get-showall-field-value []
  (selected? (element "[ng-model*=showAll]")))

(defn set-showall-field-value [value]
  (when (not= (get-showall-field-value) value)
    (click (element "[ng-model*=showAll]"))))



;; OPTIONS PANEL
(defn options-expanded? []
  (exists-present? ".reportPropertyDialog-view [uib-collapse].in"))

(defn options-expand []
  (when-not (options-expanded?)
    (click ".option")
    (wait-until options-expanded?)))

(defn set-lookup-object [type-name & [column-index]]

  ;; filter to the desired type ... not necessary but doing it anyway
  (set-search-text-input-value ".entityReportPickerDialog .sp-search-control input" type-name)
  ;; seem to need to sleep a little... waiting on angular isn't enough
  ;; to be investigated
  ;;(Thread/sleep 1000)

  ;; choose the type
  (rv/select-row-by-text type-name ".entityReportPickerDialog .dataGrid-view" column-index)


  ;; ok the typepicker
  (click-modal-dialog-button-and-wait ".inlineRelationPickerDialog .modal-footer button[ng-click*=ok]"))

(defn find-field [label]
  (or (first
        (filter
          exists-present?
          (list
            (str ".row:has(.cell:contains('" label ":'))")
            (str ".edit-form-control-container:has(.edit-form-title span:contains('" label "')) .edit-form-value")
            (str "label[style*=table-row]:has(div[style*=table-cell]:contains('" label ":'))"))))
      (str ":contains('No row found for label " label "')"))) ;; encode warning message in a valid selector

(defn get-name []
  (attribute (element ".reportPropertyDialog-view [ng-model*=reportName]") "value"))

(defn set-name [value]
  (set-input-value ".reportPropertyDialog-view [ng-model*=reportName]" value))

(defn get-description []
  (attribute (element ".reportPropertyDialog-view [ng-model*=reportDesc]") "value"))

(defn set-description [value]
  (set-input-value ".reportPropertyDialog-view [ng-model*=reportDesc]" value))

(defn get-report-base-on []
  (attribute (element ".reportPropertyDialog-view [options*=pickerOptions] [ng-model*=displayString]") "value"))

(defn set-report-base-on [value]
  ;; open the entity type picker
  (click ".reportPropertyDialog-view [options*=pickerOptions] button[ng-click*=spEntityCompositePickerModal]")
  (set-lookup-object value 0))

(defn click-advanced-tab []
  (click-tab-heading "Advanced"))

(defn click-format-tab []
  (click-tab-heading "Format"))

(defn click-deploy-tab []
  (click-tab-heading "Deploy"))

(defn get-application-used-in-only [& [check-value]]
  (let [check-value (or check-value false)]
    (when (= check-value true)
      (wait-until #(not= (attribute (element ".reportPropertyDialog-view [options*=applicationPickerOptions] [ng-model*=displayString]") "value") "") 3000))
    (attribute (element ".reportPropertyDialog-view [options*=applicationPickerOptions] [ng-model*=displayString]") "value")))

(defn set-application-used-in-only [value]
  (click (element ".reportPropertyDialog-view [options*=applicationPickerOptions] button[ng-click*=spEntityCompositePickerModal]"))
  (set-lookup-object value))


(defn get-application-used-in []
  (options-expand)
  (click-advanced-tab)
  (get-application-used-in-only))

(defn set-application-used-in [value]
  (options-expand)
  (click-advanced-tab)
  (set-application-used-in-only value))

(defn get-report-form-only [& [check-value]]
  (let [check-value (or check-value false)]
    (println (map #(clj-webdriver.taxi/html %) (selected-options (element "[options*=formOptions] select"))))
    (println (text (first (selected-options (element "[options*=formOptions] select")))))
    (when (= check-value true)
      (wait-until #(> (count (options (element "[options*=formOptions] select"))) 1) 3000))
    (text (first (selected-options (element "[options*=formOptions] select"))))))

(defn set-report-form-only [value]
  ;; added the following as 1 in 5 times the options weren't there yet, even if waiting 3 secs
  (wait-until #(> (count (elements "[options*=formOptions] select option")) 1) 10000)

  ;; note - changed this from (element (element "...") "...")) as this was invalid use of the element function
  ;; and just may have worked by accident. Another bug was fixed that made this even possibly working not work any more
  ;; and so have rewritten to maintain the intent.
  (select-by-text (or (element "tr:has(label:contains('Report form'))") (element "[options*=formOptions] select")) value))

(defn get-report-form []
  (options-expand)
  (click-advanced-tab)
  (get-report-form-only))

(defn set-report-form [value]
  (options-expand)
  (click-advanced-tab)
  (set-report-form-only value))

(defn get-icon-only []
  (attribute (element ".reportPropertyDialog-view [options*=iconPickerOptions] [ng-model*=displayString]") "value"))

(defn set-icon-only [value]
  (click ".reportPropertyDialog-view [options*=iconPickerOptions] button[ng-click*=spEntityCompositePickerModal]")
  (set-lookup-object value 1))

(defn get-icon []
  (options-expand)
  (click-format-tab)
  (get-icon-only))

(defn set-icon [value]
  (options-expand)
  (click-format-tab)
  (set-icon-only value))

(defn get-style-only []
  ;; note - changed this from (element (element "...") "...")) as this was invalid use of the element function
  ;; and just may have worked by accident. Another bug was fixed that made this even possibly working not work any more
  ;; and so have rewritten to maintain the intent.
  (text (first (selected-options (or (element "tr:has(label:contains('Style'))") (element "[options*=styleOptions] select"))))))

(defn set-style-only [value]
  ;; note - changed this from (element (element "...") "...")) as this was invalid use of the element function
  ;; and just may have worked by accident. Another bug was fixed that made this even possibly working not work any more
  ;; and so have rewritten to maintain the intent.
  (select-by-text (or (element "tr:has(label:contains('Style'))") (element "[options*=styleOptions] select")) value))

(defn get-style []
  (options-expand)
  (click-format-tab)
  (get-style-only))

(defn set-style [value]
  (options-expand)
  (click-format-tab)
  (set-style-only value))

(defn get-default-display-report-only []
  (selected? (element ".reportPropertyDialog-view [ng-model*=defaultDisplayReport]")))

(defn set-default-display-report-only [value]
  (when (not= (get-default-display-report-only) value)
    (click (element ".reportPropertyDialog-view [ng-model*=defaultDisplayReport]"))))

(defn get-default-display-report []
  (options-expand)
  (click-advanced-tab)
  (get-default-display-report-only))

(defn set-default-display-report [value]
  (options-expand)
  (click-advanced-tab)
  (set-default-display-report-only value))

(defn get-default-picker-report-only []
  (selected? (element ".reportPropertyDialog-view [ng-model*=defaultPickerReport]")))

(defn set-default-picker-report-only [value]
  (when (not= (get-default-picker-report-only) value)
    (click (element ".reportPropertyDialog-view [ng-model*=defaultPickerReport]"))))

(defn get-default-picker-report []
  (options-expand)
  (wait-for-angular)
  (click-advanced-tab)
  (get-default-picker-report-only))

(defn set-default-picker-report [value]
  (options-expand)
  (wait-for-angular)
  (click-advanced-tab)
  (set-default-picker-report-only value))

(defn get-hide-action-bar-only []
  (selected? (element ".reportPropertyDialog-view [ng-model*=hideActionBar]")))

(defn set-hide-action-bar-only [value]
  (when (not= (get-hide-action-bar-only) value)
    (click (element ".reportPropertyDialog-view [ng-model*=hideActionBar]"))))

(defn get-hide-action-bar []
  (options-expand)
  (click-format-tab)
  (get-hide-action-bar-only))

(defn set-hide-action-bar [value]
  (options-expand)
  (click-format-tab)
  (set-hide-action-bar-only value))

(defn get-hide-report-header-only []
  (selected? (element ".reportPropertyDialog-view [ng-model*=hideReportHeader]")))

(defn set-hide-report-header-only [value]
  (when (not= (get-hide-report-header-only) value)
    (click (element ".reportPropertyDialog-view [ng-model*=hideReportHeader]"))))

(defn get-hide-report-header []
  (options-expand)
  (click-format-tab)
  (get-hide-report-header-only))

(defn set-hide-report-header [value]
  (options-expand)
  (click-format-tab)
  (set-hide-report-header-only value))

(defn get-enable-on-desktop-only []
  (expect-equals "desktop-checked" (attribute (element "button:contains(Desktop)") "class")))

(defn set-enable-on-desktop-only [value]
  (when (not= (get-enable-on-desktop-only) value)
    (click "button:contains(Desktop)")))

(defn get-enable-on-desktop []
  (options-expand)
  (click-deploy-tab)
  (get-enable-on-desktop-only))

(defn set-enable-on-desktop [value]
  (options-expand)
  (click-deploy-tab)
  (set-enable-on-desktop-only value))

(defn get-enable-on-tablet-only []
  (expect-equals "tablet-checked" (attribute (element "button:contains(Tablet)") "class")))

(defn set-enable-on-tablet-only [value]
  (when (not= (get-enable-on-tablet-only) value)
    (click "button:contains(Tablet)")))

(defn click-tablet []
  (click "button:contains(Tablet)"))

(defn click-desktop []
  (click "button:contains(Desktop)"))

(defn click-mobile []
  (click "button:contains(Mobile)"))

(defn get-enable-on-tablet []
  (options-expand)
  (click-deploy-tab)
  (get-enable-on-tablet-only))

(defn set-enable-on-tablet [value]
  (options-expand)
  (click-deploy-tab)
  (set-enable-on-tablet-only value))

(defn get-enable-on-mobile-only []
  (expect-equals "mobile-checked" (attribute (element "button:contains(Mobile)") "class")))

(defn set-enable-on-mobile-only [value]
  (when (not= (get-enable-on-mobile-only) value)
    (click "button:contains(Mobile)")))

(defn get-enable-on-mobile []
  (options-expand)
  (click-deploy-tab)
  (get-enable-on-mobile-only))

(defn set-enable-on-mobile [value]
  (options-expand)
  (click-deploy-tab)
  (set-enable-on-mobile-only value))

;; BUTTONS

(defn click-ok []
  (common/click-ok))

(defn click-cancel []
  (common/click-cancel))

;; MISC

(defn set-report-properties [{:keys [name description]}]
  (when name
    (set-name name))
  (when description
    (set-description description)))
