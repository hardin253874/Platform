(ns rt.scripts.rn
  (:require [rt.test.core :refer [*test-context*]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser]]
            [rt.lib.wd-rn :refer [run-query query-results-as-objects]]
            [rt.lib.util :refer :all]
            rt.po.app
            rt.po.app-toolbox
            rt.po.chart-builder
            rt.po.chart-new
            rt.po.chart-view
            rt.po.common
            rt.po.edit-form
            rt.po.form-builder
            rt.po.form-builder-config
            rt.po.form-builder-assign-parent
            rt.po.form-properties
            rt.po.import-spreadsheet
            rt.po.report-add-relationship
            rt.po.report-advanced
            rt.po.report-builder
            rt.po.report-calculated
            rt.po.report-format
            rt.po.report-new
            rt.po.report-sort-option
            rt.po.report-summarise
            rt.po.report-total
            rt.po.report-view
            rt.po.report-actions
            rt.po.screen
            rt.po.screen-builder
            rt.po.view-form
            rt.po.workflow
            rt.po.access-rules
            rt.po.access-rules-new
            rt.po.customise-ui
            rt.po.document-library))

;; more tinkering with some ideas here

;; thinking about how we can use this namespace to provide
;; convenient access to the typical scripts and app driver functions
;; we want test script writers to use.

;; a script writer just needs (rn/close-builder :auto)

(defmulti close-builder identity)
(defmethod close-builder :chart-builder [b] (rt.po.chart-builder/close))
(defmethod close-builder :report-builder [_] (rt.po.report-builder/close))
(defmethod close-builder :screen-builder [_] (rt.po.screen-builder/close))
(defmethod close-builder :default [_]
  ;; todo work out which builder we are in and close it
  (throw (Exception. "not implemented")))

(comment
  ;; fall back to the default method
  (close-builder :auto)

  ;; call the one for chart builder... duh
  (close-builder :chart-builder))

(defn init []
  )
