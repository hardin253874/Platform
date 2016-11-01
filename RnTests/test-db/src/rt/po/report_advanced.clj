;; The 'report relationship node advanced' dialog

(ns rt.po.report-advanced
  (:require [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd :refer [right-click set-input-value wait-for-jq]]
            [rt.lib.wd-rn :refer [test-id-css]]
            [rt.po.common :as common :refer [exists-present?]]
            [rt.po.report-view :refer [select-row-by-text]]
            [clj-webdriver.core :as core]
            [clj-webdriver.taxi :refer [text click value selected? select-by-text selected-options select-option element elements element attribute visible? *driver* wait-until]]))


(defn find-exact-type []
  (str ".relationshipadvanceddialog-view [ng-model*=selectedNodeExactType]")
  )

(defn get-exact-type []
  (selected? (find-exact-type)))

(defn set-exact-type [ value]
  (when (not= (get-exact-type) value)
    (click (find-exact-type) )))


(defn get-join-type []
  (text (first (selected-options (element ".relationshipadvanceddialog-view [ng-model*=selectedNodeExistMode]" ))))
  )

(defn set-join-type [value]
  (select-by-text (element ".relationshipadvanceddialog-view [ng-model*=selectedNodeExistMode]" ) value)
 )

(defn find-follow-recursion []
  (str ".relationshipadvanceddialog-view [ng-model*=selectedNodeCheckExistenceOnly]")
  )

(defn get-follow-recursion []
  (selected? (find-follow-recursion)))

(defn set-follow-recursion [ value]
  (when (not= (get-follow-recursion) value)
    (click (find-follow-recursion) )))


(defn get-recursion []
  (text (first (selected-options (element ".relationshipadvanceddialog-view [ng-model*=selectedNodeRecursionMode]" ))))
  )

(defn set-recursion [value]
  (select-by-text (element ".relationshipadvanceddialog-view [ng-model*=selectedNodeRecursionMode]" ) value)
  )

;; BUTTONS

(defn click-ok []
  (click "button:contains(OK)"))

(defn click-cancel []
  (click "button:contains(Cancel)"))