(ns rt.po.screen-builder
  (:require [rt.lib.wd :refer [right-click]]
            [rt.lib.wd-rn :refer [drag-n-drop]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [clj-webdriver.taxi :refer [text click exists?]]))


(defn add-screen-toolbox-item [name]
  (drag-n-drop
    (str ".fb-toolbox-objectsviewer .itemname:contains('" name "')")
    ".sp-form-builder-container-content:first"))

(defn add-screen-toolbox-items [names]
  (doseq [name names]
    (add-screen-toolbox-item name)))

;;_.map(_.filter($($('.sp-form-builder-field-control')[1]).scope().containedControl._relationships, function (r) { return r.instances.length; }), function (r) { return r.id._alias})

(defn save-screen []
  (click ".sp-Form-Builder-ToolBar button[uib-tooltip=Save]")
  (wait-for-angular))

(defn close []
  (click ".sp-Form-Builder-ToolBar button[uib-tooltip=Cancel]"))

(defn select-application
  [app-name]
  (clj-webdriver.taxi/select-by-text "div.left-panel select.sp-combo-picker" app-name)
  (wait-for-angular))

(defn toolbox-item-exists
  [type-name item-name selector]
  (exists? (str "div.fb-toolbox-objectsviewer div.fb-toolbox-group:has(div.groupname:contains('" type-name "')) div[ng-repeat='" selector "']:has(div.itemname:contains('" item-name "'))")))

(defn toolbox-form-exists
  [type-name item-name]
  (toolbox-item-exists type-name item-name "form in getFormsForType(type.entity)"))

(defn toggle-toolbox-type
  [type-name]
  (when (exists? (str "div.fb-toolbox-objectsviewer div.fb-toolbox-group:has(div.groupname:contains('" type-name "'))"))
    (click (str "div.fb-toolbox-objectsviewer div.fb-toolbox-group:has(div.groupname:contains('" type-name "')) div>button"))
    (wait-for-angular)))


(defn add-item-from-toolbox
  "Adds an object to the screen. Example:

  (add-item-from-toolbox \"Student\" \"1. Students by faculty (p)\")
  "
  [type-name item-name selector]
  (drag-n-drop
    (str "div.fb-toolbox-objectsviewer div.fb-toolbox-group:has(div.groupname:contains('" type-name "')) div[ng-repeat='" selector "']:has(div.itemname:contains('" item-name "'))")
    ".sp-form-builder-container-content:first"))

(defn add-chart-from-toolbox
  "Adds a chart to the screen. Example:

  (add-chart-from-toolbox \"Student\" \"1. Students by faculty (p)\")
  "
  [type-name chart-name]
  (add-item-from-toolbox type-name chart-name "chart in getChartsForType(type.entity)"))

(defn add-report-from-toolbox
  "Adds a report to the screen. Example:

  (add-report-from-toolbox \"Student\" \"Student\")
  "
  [type-name report-name]
  (add-item-from-toolbox type-name report-name "report in getReportsForType(type.entity)"))

(defn add-form-from-toolbox
  "Adds a form to the screen. Example:

  (add-form-from-toolbox \"Student\" \"Students Form\")
  "
  [type-name form-name]
  (add-item-from-toolbox type-name form-name "form in getFormsForType(type.entity)"))

(defn save []
  (click ".sp-Form-Builder-ToolBar button[uib-tooltip*=Save]")
  (wait-for-angular))

(defn close []
  (click "button[ng-click*='Cancel']"))

(defn is-chart-on-screen
  [chart-name]
  (exists? (str "div.sp-form-builder-chart:contains('" chart-name "')")))

(defn is-form-on-screen
  [form-name]
  (exists? (str "span.sp-form-builder-form-control:contains('" form-name "')")))

(defn is-report-on-screen
  [report-name]
  (exists? (str "div.sp-form-builder-report-control:contains('" report-name "')")))

(defn delete-chart-from-screen
  [chart-name]
  (click (str "div.sp-form-builder-chart:contains('" chart-name "') span.sp-form-builder-adornment[ng-click='onCloseClick()']")))

(defn show-form-config-menu
  [form-name]
  (click (str "span.sp-form-builder-form-control:contains('" form-name "') span.transcludeConfigure span.sp-form-builder-adornment")))

(defn show-report-config-menu
  [report-name]
  (click (str "div.sp-form-builder-report-render-control:contains('" report-name "') span.transcludeConfigure span.sp-form-builder-adornment")))

(defn click-new-form
  [type-name]
  (click (str "div.fb-toolbox-objectsviewer div.fb-toolbox-group:has(div.groupname:contains('" type-name "')) div.subgroup:contains('Forms') :button.add[ng-click='newForm(type)']")))

(defn get-name []
  (text ".sp-Edit-Form-Heading"))

(defn public-private-visible?
  "True if the self-serve public-private filter link is visible."
  []
  (exists? ".component-filter"))

(defn public-private-text
  "Current text for the self-serve public-private filter link."
  []
  (text ".component-filter"))

(defn click-public-private
  "Click the self-serve public-private filter link."
  []
  (click ".component-filter"))