;; The 'New Chart' dialog

(ns rt.po.chart-new
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.po.common :as common :refer [exists-present? click-tab-heading wait-until]]
            [rt.po.report-new :as repnew]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? element elements element attribute visible?]]))


;; GENERIC ACCESSORS
;; E.g.:
;; (get-checkbox-field-value "Enable on desktop")
;; (get-checkbox-field-value "Enable on tablet")
;; (get-checkbox-field-value "Enable on mobile")

(defn ^:private find-checkbox-field [name]
  (str "div:has(:contains('" name "')) > input"))

(defn get-checkbox-field-value [name]
  (selected? (find-checkbox-field name)))

(defn set-checkbox-field-value [name value]
  (when (not= (get-checkbox-field-value name) value)
    (click (find-checkbox-field name) )))


;; OPTIONS PANEL
(defn options-expanded? []
  (exists-present? ".sp-new-type-dialog [uib-collapse].in"))

(defn options-expand []
  (when-not (options-expanded?)
    (click ".option")
    (wait-until options-expanded?)))

(defn set-icon-only [value]
  (click ".sp-new-type-dialog [options*=iconPickerOptions] button[ng-click*=spEntityCompositePickerModal]")
  (repnew/set-lookup-object value))

(defn set-icon [value]
  (options-expand)
  (click-tab-heading "Format")
  (set-icon-only value))

;; SCALAR FIELDS

(defn get-name []
  (ef/string-field-value "Name"))

(defn set-name [value]
  (ef/set-string-field-value "Name" value))

(defn get-description []
  (ef/string-field-value "Description"))

(defn set-description [description]
  (ef/set-string-field-value "Description" description))

(defn get-enable-on-desktop []
  (get-checkbox-field-value "Enable on desktop"))

(defn set-enable-on-desktop [value]
  (options-expand)
  (set-checkbox-field-value "Enable on desktop" value))

(defn get-enable-on-tablet []
  (get-checkbox-field-value "Enable on tablet"))

(defn set-enable-on-tablet [value]
  (options-expand)
  (set-checkbox-field-value "Enable on tablet" value))

(defn get-enable-on-mobile []
  (get-checkbox-field-value "Enable on mobile"))

(defn set-enable-on-mobile [value]
  (options-expand)
  (set-checkbox-field-value "Enable on mobile" value))


;; CHART TYPES

(defn get-chart-type []
  (attribute (element ".chart-types .chart-type.selected img") "uib-tooltip"))

(defn set-chart-type [chart-type]
  (click (str ".chart-type img[uib-tooltip*=\"" chart-type "\"]")))

(defn get-chart-types []
  "Get a list of chart types."
  (->> (elements ".chart-types .chart-type img")
       (map #(attribute % "uib-tooltip"))))

(defn chart-type-selected? [chart-type]
  "Returns true if the chart type is selected."
  (if (element (str ".chart-types .selected img[uib-tooltip*='" chart-type "']")) true false))


;; BUTTONS

(defn click-ok []
  (click "button:contains(OK)"))

(defn click-cancel []
  (click "button:contains(Cancel)"))


;; MISC

(defn set-chart-properties [{:keys [name description chart-type]}]
  (when name
    (set-name name))
  (when description
    (set-description description))
  (when chart-type
    (set-chart-type chart-type)))
