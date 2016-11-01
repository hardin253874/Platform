;; The 'report column total' dialog

(ns rt.po.report-total
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value find-element-with-text wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.po.common :as common :refer [exists-present?]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? exists? select-option element elements element attribute visible?]]))

(defn get-show-grand-total []
  (selected? (element ".spAggregateOptionsDialog-view [ng-model*=showGrandTotals]"))
  )

(defn set-show-grand-total [value]
  (when (not= (get-show-grand-total) value)
    (click (element ".spAggregateOptionsDialog-view [ng-model*=showGrandTotals]") ))
  )

(defn get-show-option-label []
  (selected? (element ".spAggregateOptionsDialog-view [ng-model*=showOptionLabels]"))
  )


(defn set-show-option-label [value]
  (when (not= (get-show-option-label) value)
    (click (element ".spAggregateOptionsDialog-view [ng-model*=showOptionLabels]") ))
  )

(defn get-show-sub-total []
  (selected? (element ".spAggregateOptionsDialog-view [ng-model*=showSubTotals]" ))
  )


(defn set-show-sub-total [value]
  (when (not= (get-show-sub-total) value)
    (click (element ".spAggregateOptionsDialog-view [ng-model*=showSubTotals]" ) ))
  )


(defn show-sub-total-exists? []
  (exists? (element ".spAggregateOptionsDialog-view [ng-model*=showSubTotals]"))
  )



(defn find-summarise-option [name]
  (str "div:has(:contains('" name "')) > input"))

(defn get-summarise-option-value [name]
  (selected? (find-summarise-option name)))

(defn set-summarise-option-value [name value]
  (when (not= (get-summarise-option-value name) value)
    (click (find-summarise-option name) )))


(defn get-count []
  (get-summarise-option-value "Count")
  )

(defn set-count [value]
  (set-summarise-option-value "Count" value)
  )

(defn get-count-unique []
  (get-summarise-option-value "Count unique")
  )

(defn set-count-unique [value]
  (set-summarise-option-value "Count unique" value)
  )

(defn get-count-all []
  (get-summarise-option-value "Count all")
  )

(defn set-count-all [value]
  (set-summarise-option-value "Count all" value)
  )


(defn get-sum []
  (get-summarise-option-value "Sum")
  )

(defn set-sum [value]
  (set-summarise-option-value "Sum" value)
  )

(defn get-avg []
  (get-summarise-option-value "Average")
  )

(defn set-avg [value]
  (set-summarise-option-value "Average" value)
  )

(defn get-max []
  (get-summarise-option-value "Max")
  )

(defn set-max [value]
  (set-summarise-option-value "Max" value)
  )

(defn get-min []
  (get-summarise-option-value "Min")
  )

(defn set-min [value]
  (set-summarise-option-value "Min" value)
  )

;; BUTTONS

(defn click-ok []
  (click "button:contains(OK)"))

(defn click-cancel []
  (click "button:contains(Cancel)"))