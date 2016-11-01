(ns rt.scripts.perf
  (:require [rt.test.db :refer [add-tests]]
            [rt.test.core :refer [*test-context* run-tests]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser set-input-value]]
            [rt.lib.wd-ng :refer [wait-for-angular apply-angular-expression execute-script-on-element]]
            [rt.lib.wd-rn :refer [run-query query-results-as-objects]]
            [rt.lib.util :refer :all]
            [rt.po.app :as app]
            [rt.po.app-toolbox :as tb]
            [rt.po.form-builder :as fb]
            [rt.po.screen-builder :as sb]
            [rt.po.chart-builder :as cb]
            [rt.po.report-builder :as rb]
            [rt.po.edit-form :as ef]
            [rt.po.view-form :as vf]
            [rt.po.report-view :as rv]
            [rt.po.import-spreadsheet :as import]
            [rt.scripts.common :refer [start-app-and-login]]
            [clojure.set :refer [subset?]]
            [clojure.string :as string]
            [rt.scripts.common :as common]
            [clj-webdriver.taxi :as taxi]))

;; The scripts here are used to assess performance and so we wait for angular
;; before returning from the script as this means work following the last action
;; in the script is counted as part of this script, not the next
;; TODO - consider making this kind of thing a function of the test runner
;; and indicating in the test case, and not on the script itself.

(defn open-employee-form []
  (let [data (merge {:app-name "Test Solution"
                     :nav-path "Test Solution/AA_Reports/AA_All Fields"
                     :row-text "Test 01"}
                    *test-context*)
        {:keys [app-name nav-path row-text]} data]
    (app/navigate-to-item app-name nav-path)
    (rv/right-click-row-by-text row-text)
    (app/choose-context-menu "View")
    (wait-for-angular)))

(defn open-form-builder []
  (app/enable-config-mode)
  (vf/open-form-builder)
  (wait-for-angular))

(defn open-field-properties-modal []
  (let [control-name (:control-name *test-context*)
        control-name (or control-name "Single Line")]
    (fb/run-field-properties-dialog control-name {})
    (wait-for-angular)))

(defn navigate-to-report [& [data]]
  (let [data (merge {:app-name "Test Solution"
                     :nav-path "Test Solution/AA_Reports/AA_All Fields"}
                    *test-context* data)
        {:keys [app-name nav-path]} data]
    (app/navigate-to-item app-name nav-path)
    (wait-for-angular)))

(defn choose-report-action-menu [& [data]]
  (let [data (merge {:menu-item "New"} *test-context* data)
        {:keys [menu-item]} data]
    (rv/open-action-menu)
    (app/choose-context-menu menu-item)
    (wait-for-angular)))


(defn create-suite-metrics-objects
  "Script to create a new object for capturing test metrics"
  []
  (app/enable-app-toolbox)
  (tb/select-application-filter "")

  (doseq [s (rt.test.core/get-run-suite-summaries (rt.test.db/get-last-test-run-id))]
    (let [obj-name (str "RT-Metrics-" (-> s :suite name))]

      (when (empty? (tb/get-object-names obj-name))

        (tb/create-object {:name obj-name :description "Captures test metrics"})
        (fb/save)

        (fb/add-container-to-form)

        (let [fields [{:name "run-id" :type "Text"}
                      {:name "run-date" :type "Date and Time"}
                      {:name "suite" :type "Text"}
                      {:name "server-version" :type "Text"}
                      {:name "client-version" :type "Text"}
                      {:name "host-name" :type "Text"}]
              fields (concat fields
                             (map #(hash-map :name % :type "Text") (remove keyword? (keys s))))]

          (doseq [{:keys [name type]} fields]
            (fb/add-field-to-form type)
            (fb/set-field-attributes type {:name name})))

        (fb/save)
        (fb/close)))))

(defn create-perf-suite-metrics-object
  "Script to create a new object for capturing test metrics"
  []
  (app/enable-app-toolbox)
  (tb/select-application-filter "")

  (when (empty? (tb/get-object-names "RT-PerfSuite-Metrics"))

    (tb/create-object {:name "RT-PerfSuite-Metrics" :description "Captures test metrics"})
    (fb/save)

    (fb/add-container-to-form)

    (let [fields [{:name "run-id" :type "Text"}
                  {:name "run-date" :type "Date and Time"}
                  {:name "suite" :type "Text"}
                  {:name "server-version" :type "Text"}
                  {:name "client-version" :type "Text"}
                  {:name "host-name" :type "Text"}
                  {:name "metric-id" :type "Text"}
                  {:name "form/save" :type "Number"}
                  {:name "screen/small" :type "Number"}
                  {:name "screen/medium" :type "Number"}
                  {:name "screen/large" :type "Number"}
                  {:name "sb/open/large" :type "Number"}]
          fields (concat fields )]

      (doseq [{:keys [name type]} fields]
        (fb/add-field-to-form type)
        (fb/set-field-attributes type {:name name})))

    (fb/save)
    (fb/close)))

(defn create-metrics-object
  "Script to create a new object for capturing test metrics"
  []
  (app/enable-app-toolbox)
  (tb/select-application-filter "")

  (when (empty? (tb/get-object-names "RT-TestRun-Metrics"))

    (tb/create-object {:name "RT-TestRun-Metrics" :description "Captures test metrics"})
    (fb/save)

    (fb/add-container-to-form)

    (fb/add-field-to-form "Text")
    (fb/set-field-attributes "Text" {:name "Test Run Id"})

    (fb/add-field-to-form "Text")
    (fb/set-field-attributes "Text" {:name "Test Id"})

    (fb/add-field-to-form "Text")
    (fb/set-field-attributes "Text" {:name "Script"})

    (fb/add-field-to-form "Number")
    (fb/set-field-attributes "Number" {:name "Duration"})

    (fb/add-field-to-form "Date and Time")
    (fb/set-field-attributes "Date and Time" {:name "Run Date"})

    (fb/save)
    (fb/close)))

(defn create-metrics-report []
  (app/enable-app-toolbox)
  (tb/select-application-filter "")
  (tb/set-object-filter "rt-")

  (let [object-name "RT-TestRun-Metrics"
        report-name "RT-TestRun-Metrics-Report"
        fields #{"Test Run Id" "Test Id" "Script" "Duration" "Run Date"}]

    (when-not (some #(= report-name %) (tb/get-report-names "RT-TestRun-Metrics"))

      (tb/create-report {:name report-name :object-type object-name})

      ;; alt way of creating a report
      ;;(tb/create-report-via-toolbox-object {:name report-name :object-type object-name})

      ;; should now be in the report builder

      (doseq [f fields]
        (rb/select-field-in-report f)
        (rb/select-field-in-analyser f))

      ;; all our fields should now appear in analyser and the report

      (do
        (expect (subset? fields (set (rb/get-analyser-fields-in-popup))))
        (expect (subset? fields (set (rb/get-analyser-fields-in-toolbox))))
        (expect (subset? fields (set (rb/get-fields-in-report-view))))
        (expect (subset? fields (set (rb/get-selected-fields-in-toolbox)))))

      ;; uncheck the Name as we don't use it

      (rb/select-field-in-report "Name" false)
      (rb/select-field-in-analyser "Name" false)

      ;; add some calculated fields

      (rb/add-calculated-field "Secs" "[Duration]/1000")
      (rb/add-calculated-field "Run" "convert(string, [Run Date])")

      ;; save the report

      (rb/save)
      (rb/close))))

(defn create-metrics-screen []
  (app/navigate-to "Home")
  (app/enable-config-mode)
  (app/add-screen "RT-TestRun-Metrics-Screen" "")
  (tb/select-application-filter "")
  (tb/set-object-filter "rt-")

  (tb/create-chart-via-toolbox-object {:name "RT-TestRun-Metrics-Chart" :object-type "RT-TestRun-Metrics" :chart-type "Line"})
  (println (datetime-str) "chart created, now configuring Primary")
  (cb/drag-source-to-target "Run" "Primary")
  (println (datetime-str) "chart created, now configuring Values")
  (cb/drag-source-to-target "Secs" "Values")
  (println (datetime-str) "chart created, now configuring Colour")
  (cb/drag-source-to-target "Script" "Colour")
  (println (datetime-str) "chart configured, now saving")
  (cb/save-chart)
  (println (datetime-str) "chart saved, now closing builder")
  (cb/close)

  (tb/select-application-filter "")
  (sb/add-screen-toolbox-items ["RT-TestRun-Metrics-Chart" "RT-TestRun-Metrics-Report"])
  (sb/save-screen)
  (sb/close))

(defn generate-metrics-csv [file-name]
  (rt.test.core/write-run-metrics-csv file-name 3))

(defn import-metrics [file-name]
  (app/clear-all-alerts)
  (app/enable-app-toolbox)
  (tb/open-quick-link "Import Spreadsheet")
  (import/choose-file-to-import (str (System/getProperty "user.dir") "\\" file-name))
  (import/choose-next)
  (import/choose-target-object "RT-TestRun-Metrics")
  (import/choose-next)
  (import/set-field-mapping {"run-id"   "Test Run Id"
                             "datetime" "Run Date"
                             "test-id"  "Test Id"
                             "script"   "Script"
                             "time"     "Duration"})
  (import/choose-next)
  (import/wait-for-complete (* 6 60 1000)))

(defn remove-previous-metrics []

  (try
    (app/navigate-to-item "Help" "Help Topics/All Reports")
    (rv/set-search-text "RT-")
    (rv/delete-record "RT-TestRun-Metrics-Report")

    (catch Exception ex))

  (try
    (app/enable-app-toolbox)
    (tb/select-application-filter "")
    (tb/set-object-filter "rt-")
    (tb/delete-object "RT-TestRun-Metrics")

    (catch Exception ex))

  (try
    (app/navigate-to "Home")
    (app/delete-nav-items-matching #"RT-.*Metrics-.*")

    (catch Exception ex)))

(defn init []
  )

(comment

  (rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})
  (rt.app/setup-environment {:app-url "https://spstdev24d.sp.local"})
  (rt.app/setup-environment {:app-url "https://spdarren02.sp.local"})
  (rt.app/setup-environment)

  (alter-var-root (var *test-context*)
                  (constantly {:target   :chrome
                               :tenant   "EDC"
                               :username "Administrator"
                               :password "tacoT0wn"}))

  (start-app-and-login)
  (create-perf-suite-metrics-object)
  (create-suite-metrics-objects)

  (rt.repl/run-tests :perf-app)
  (rt.repl/run-tests :perf-app/chrome/t5)
  (rt.repl/run-tests :qa-daily/chrome/import-metrics)
  (rt.repl/run-tests :post-build-fast)
  (rt.repl/run-tests :qa-daily/chrome/test-cb)

  (rt.app/setup-environment {:app-url "https://syd1steve01.softwareplatform.com"})
  (alter-var-root (var *test-context*)
                  (constantly {:target   :chrome
                               :tenant   "ABC"
                               :username "Administrator"
                               :password "tacoT0wn"}))

  (remove-previous-metrics)
  (create-metrics-object)
  (create-metrics-report)
  (create-metrics-screen)

  (generate-metrics-csv "test-times.csv")

  (do
    (import-metrics "test-times.csv")
    ;(Thread/sleep 5000)

    #_(doseq [n (range 10)]
      (println (count (taxi/elements ".showDetails:contains('action completed')")) (taxi/displayed? "a.btn:contains(Restart)"))
      (Thread/sleep 300))

    (taxi/wait-until #(taxi/present? ".showDetails:contains('action completed')") 10000 500)

    (println "done"  (count (taxi/elements ".showDetails:contains('action completed')")))

    #_(timeit "open metrics"
            (do
              (app/navigate-to-item "Home" "RT-TestRun-Metrics-Screen")
              (wait-for-angular))))


  ;; todo test if re-import adds to or replaces for same
  ;; todo write workflow to purge duplicates ??
  ;; todo OR add a key to the import

  )


