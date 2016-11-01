(ns rt.po.chart-view
  (:require [rt.lib.wd :refer [right-click]]
            [rt.lib.wd-rn :refer [drag-n-drop]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.po.common :refer [safe-text exists-present? wait-until]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click elements attribute visible?]]))

;; HELPERS
(defn ^:private text-list [q]
  "Returns selected elements as a vector of text contents."
  (into [] (map core/text (elements q))))

(defn ^:private attr-list [attr-name q]
  "Returns attributes from selected elements as a vector of text."
  (into [] (->> (elements q)
                (map #(attribute % attr-name)))))

;; CHART LOCATION

(defn ^:private find-chart [chart]
  (if chart
    (str ".chart-render-control-container:has(.form-title:contains('" chart "'))")
    (str ".sp-chart-svg")))

(defn chart-visible? [chart]
  (exists-present? (find-chart chart)))

;; returns true if the chart was rendered without errors .. todo: implement checks
(defn chart-loaded-ok? [chart]
  (chart-visible? chart))

;; READING CHART TEXT

(defn get-chart-title [chart]
  (safe-text (str (find-chart chart) " .chart-title")))

(defn get-x-axis-title [chart]
  (safe-text (str (find-chart chart) " .x.axis-label text")))

(defn get-y-axis-title [chart]
  (safe-text (str (find-chart chart) " .y.axis-label text")))

(defn get-x-axis-values [chart]
  (text-list (str (find-chart chart) " .x.axis .tick.major text")))

(defn get-y-axis-values [chart]
  (text-list (str (find-chart chart) " .y.axis .tick.major text")))

(defn has-legend? [chart]
  (not (empty? (elements (str (find-chart chart) " .legend")))))

(defn get-legend-values [chart]
  (text-list (str (find-chart chart) " .legend .item text")))

(defn get-series-on-chart [chart]
  (attr-list "name" (str (find-chart chart) " .series-root .series")))

(defn wait-for-chart [chart]
  (wait-until #(chart-loaded-ok? chart) 5000))

;; driver functions for pie chart.
(defn select-pie-slice [chart primary-name]
  (wait-for-chart chart)
  (when-not (chart-visible? chart)
    (throw (Exception. (str "Cannot find pie chart " chart))))
  (let [el-selector (str (find-chart chart) " .arc[primary='" primary-name "']")]
    (wait-until #(exists-present? el-selector) 5000)
    (Thread/sleep 1500)
    (click el-selector)
    (Thread/sleep 1000))
  (wait-for-angular))

(defn get-pie-slice-value [chart primary-name]
  (wait-for-chart chart)
  (when-not (chart-visible? chart)
    (throw (Exception. (str "Cannot find pie chart " chart))))
  (let [el-selector (str (find-chart chart) " .arc[primary='" primary-name "']")]
    (wait-until #(exists-present? el-selector) 5000)
    (Thread/sleep 1500)
    (attribute el-selector "value")))

(defn select-data-point [chart primary-name & [value-name series-name]]
  (wait-for-chart chart)
  (when-not (chart-visible? chart)
    (throw (Exception. (str "Cannot find chart " chart))))
  (let [series-el (if series-name (str " .series[name='" series-name "']") " .series")
        value-le (if value-name (str))]
    (let [el-selector (str (find-chart chart) series-el " g[primary='" primary-name "']")]
      (wait-until #(exists-present? el-selector) 5000)
      (Thread/sleep 1500)
      (click el-selector))))

(defn get-data-point-value [chart primary-name & [series-name]]
  (wait-for-chart chart)
  (when-not (chart-visible? chart)
    (throw (Exception. (str "Cannot find chart " chart))))
  (let [series-el (if series-name (str " .series[name='" series-name "']") " .series")]
    (let [el-selector (str (find-chart chart) series-el " g[primary='" primary-name "']")]
      (wait-until #(exists-present? el-selector) 5000)
      (Thread/sleep 1500)
      (attribute el-selector "value"))))

(defn get-data-point-name-value [dp-element]
  {:name (attribute dp-element "primary")
   :value (attribute dp-element "value")})

(defn get-data-point-values [chart & [series-name]]
  (wait-for-chart chart)
  (when-not (chart-visible? chart)
    (throw (Exception. (str "Cannot find chart " chart))))
  (let [series-el (if series-name (str " .series[name='" series-name "']") " .series")]
    (Thread/sleep 2000)
    (mapv #(get-data-point-name-value %) (elements (str (find-chart chart) series-el " g[primary]")))))

(defn get-first-data-point-value [dp-values name]
  (:value (first (filter #(= (:name %) name) dp-values))))

(defn select-area-chart [chart & [series-name] ]
  (wait-for-chart chart)
  (when-not (chart-visible? chart)
    (throw (Exception. (str "Cannot find chart " chart))))
  (let [series-el (if series-name (str " .series[name='" series-name "']") " .series")]
    (let [el-selector (str (find-chart chart) series-el)]
      (wait-until #(exists-present? el-selector) 5000)
      (Thread/sleep 1500)
      (click el-selector))))
