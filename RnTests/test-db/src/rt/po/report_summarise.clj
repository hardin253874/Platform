;; The 'report summarise' dialog


(ns rt.po.report-summarise
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.po.common :as common :refer [exists-present? click-modal-dialog-button-and-wait]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? select-option element elements element attribute visible?]]))



(defn get-summarise-option [column-name summarise-option]
  (selected? (element (str ".summariseoptiondialog-view .ngSelectionCell:contains('" summarise-option "') [title='" column-name "']")))
  )

(defn set-summarise-option [column-name summarise-option value]
  (when (not= (get-summarise-option column-name summarise-option) value)
    (click (element (str ".summariseoptiondialog-view .ngSelectionCell:contains('" summarise-option "') [title='" column-name "']")) ))
  )

(defn get-show-values [column-name]
  (get-summarise-option column-name "Show values")
  )

(defn set-show-values [column-name value]
  (set-summarise-option column-name "Show values" value)
  )

(defn get-count [column-name]
  (get-summarise-option column-name "Count")
  )

(defn set-count [column-name value]
  (set-summarise-option column-name "Count" value)
  )

(defn get-count-unique [column-name]
  (get-summarise-option column-name "Count unique")
  )

(defn set-count-unique [column-name value]
  (set-summarise-option column-name "Count unique" value)
  )

(defn get-count-all [column-name]
  (get-summarise-option column-name "Count all")
  )

(defn set-count-all [column-name value]
  (set-summarise-option column-name "Count all" value)
  )


(defn get-sum [column-name]
  (get-summarise-option column-name "Sum")
  )

(defn set-sum [column-name value]
  (set-summarise-option column-name "Sum" value)
  )

(defn get-avg [column-name]
  (get-summarise-option column-name "Average")
  )

(defn set-avg [column-name value]
  (set-summarise-option column-name "Average" value)
  )

(defn get-max [column-name]
  (get-summarise-option column-name "Max")
  )

(defn set-max [column-name value]
  (set-summarise-option column-name "Max" value)
  )

(defn get-min [column-name]
  (get-summarise-option column-name "Min")
  )

(defn set-min [column-name value]
  (set-summarise-option column-name "Min" value)
  )

(defn get-list [column-name]
  (get-summarise-option column-name "List")
  )

(defn set-list [column-name value]
  (set-summarise-option column-name "List" value)
  )


(defn click-show-tree []
  (click "button:contains(Show Tree)"))

(defn click-hide-tree []
  (click "button:contains(Hide Tree)"))

(defn click-tree-node [node-name]
  (click (element (str ".summariseoptiondialog-view .summariseNode:contains('" node-name "')")))
  )
;; BUTTONS

(defn click-ok-dialog []
  (click-modal-dialog-button-and-wait ".modal-footer [ng-repeat='btn in buttons']:contains(OK)"))

(defn click-cancel-dialog []
  (click-modal-dialog-button-and-wait ".modal-footer [ng-repeat='btn in buttons']:contains(Cancel)"))

(defn click-remove-summarise []
  (click "button:contains(Remove Summarise)")
  (click-ok-dialog))

(defn click-ok []
  (comment click "button:contains(OK)")
  (click-modal-dialog-button-and-wait ".modal-footer button:contains(OK)"))

(defn click-cancel []
  (comment click "button:contains(Cancel)")
  (click-modal-dialog-button-and-wait ".modal-footer button:contains(Cancel)"))
