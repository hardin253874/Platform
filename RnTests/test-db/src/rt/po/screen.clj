(ns rt.po.screen
  (:require [rt.lib.wd :refer [right-click]]
            [rt.lib.wd-rn :refer [drag-n-drop]]
            [clj-webdriver.taxi :refer [text click exists? element find-element-under]]
            [clj-webdriver.core :refer [by-css]]
            [rt.po.common :as common :refer [exists-present?]]
            [rt.po.app :refer [choose-context-menu enable-config-mode]]))

(defn open-screen-builder
  "Open the screen builder on the current screen via the configure icon."
  []
  (enable-config-mode)
  (click ".screen-config-panel [src*=configure]")
  (choose-context-menu "Modify"))

(defn is-chart-on-screen
  [chart-name]
  (exists? (str "div.chart-render-control-container:has(div.form-title:contains('" chart-name "'))")))

(defn is-report-on-screen
  [report-name]
  (exists? (str "span.report-render-control:has(div.form-title:contains('" report-name "'))")))

(defn is-form-on-screen
  [form-name]
  (exists? (str "form.form-render-control:has(div.form-title:contains('" form-name "'))")))

 (defn is-hero-text-on-screen [title]
 (exists? (str ".hero-text-control:contains('" title "')"))) 
  
 (defn get-hero-text-on-screen
  [title]
  (element (str ".hero-text-control:contains('" title "')")))
 
 (defn click-hero-text-on-screen 
	[title]
	(let [hero-control (get-hero-text-on-screen title)]
		(let [hero-data (find-element-under hero-control (by-css ".hero-data"))]
			(click hero-data)
		)))
 
  (defn get-hero-text-data-on-screen 
	[title]
	(let [hero-control (get-hero-text-on-screen title)]
		(let [hero-data (find-element-under hero-control (by-css ".hero-data"))]
			(text hero-data)
		)))
 
(defn get-report-on-screen
  [report-name]
  (element (str ".report-render-control:contains('" report-name "')")))

(defn open-report-action-menu-on-screen
  [report-name]
  (click (element (str ".report-render-control:contains('" report-name "') button:contains(Actions)"))))

(defn open-report-menu-on-screen-by-name
  [report-name button-name]
  (click (element (str ".report-render-control:contains('" report-name "') button:contains(" button-name ")"))))  
  
(defn show-config-menu []
  (click ".screen-config-panel .screen-config-panel-button"))

(defn click-form-edit-button [form-name]
 (let [el (element (str ".form-render-control .form-title:contains('"form-name"') button[title='Edit']"))]
   (if (exists-present? el)
     (click el))) )

(defn click-form-save-button [form-name]
  (let [el (element (str ".form-render-control .form-title:contains('"form-name"') button[title='Save']"))]
    (if (exists-present? el)
      (click el))) )

(defn click-form-close-button [form-name]
  (let [el (element (str ".form-render-control .form-title:contains('"form-name"') button[title='Cancel']"))]
    (if (exists-present? el)
      (click el))) )