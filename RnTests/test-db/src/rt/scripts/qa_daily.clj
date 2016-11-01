(ns rt.scripts.qa-daily
  (:require [rt.test.db :refer [add-tests]]
            [rt.test.core :refer [*test-context* run-tests]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser wait-for-jq]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.wd-rn :refer [run-query query-results-as-objects]]
            [rt.lib.util :refer :all]
            [rt.po.app :as app]
            [rt.po.app-toolbox :as tb]
            [rt.po.form-builder :as fb]
            [rt.po.screen-builder :as sb]
            [rt.po.report-builder :as rb]
            [rt.po.edit-form :as ef]
            [rt.po.view-form :as vf]
            [rt.po.report-view :as rv]
            [rt.scripts.common :refer [start-app-and-login]]
            [clojure.set :refer [subset?]]
            [rt.po.chart-builder :as cb]
            [rt.po.import-spreadsheet :as import]))

(defn verify-screen-builder-smoke-test
  "Verify the results of the smoke test scenario for the screen builder."
  {:test-script true}
  [screen-name]
  ;; todo run a query to verify the screen was created and contains the added items.
  ;; todo wrap this in a handy driver function / low level script

  (let [results (-> {:root    {:id "console:screen" :related []}
                     :selects [{:field "name"}]
                     :conds   [{:expr {:field "name"}
                                :oper "Equal"
                                :val  screen-name}]}
                    (run-query)
                    (query-results-as-objects))]

    (expect-equals 1 (count results))))

(defn form-builder-new-object []
  (app/enable-app-toolbox)
  (let [object-name (make-test-name "RT-Object")]
    (tb/create-object {:name object-name :description "Test object - to be deleted"})))

(defn form-builder-add-text-playing-around []
  (fb/add-field-to-form "Text")
  ;; changes control label but not the field name
  (fb/set-field-control-attributes "Text" {:display-name "Yahoo"})
  ;; sets the field name (now if this was done before the label then the label would
  ;; have changed, i think
  (fb/set-field-attributes "Text" {:name "Text 1"})
  ;; set the label
  (fb/set-field-control-attributes "Text 1" {:display-name "Text 1"})
  ;; set both via the props dialog
  (fb/run-field-properties-dialog "Text 1" {:name "Hammer Time" :display-name "What!"})

  ;; todo - the following needs the display name... as left over from when the form was wrong
  (fb/run-field-properties-dialog "What!" {:name "Text 1" :display-name "Text 1"}))

(defn form-builder-add-text []
  (fb/add-field-to-form "Text")
  (fb/set-field-control-attributes "Text" {:display-name "Text 1"})
  (fb/set-field-attributes "Text" {:name "Text 1"}))

(defn form-builder-add-multiline []
  (fb/add-field-to-form "Multiline Text")
  (fb/set-field-control-attributes "Multiline Text" {:display-name "Multiline 1"})
  (fb/set-field-attributes "Multiline Text" {:name "Multiline 1"}))

(defn form-builder-add-number []
  (fb/add-field-to-form "Number")
  (fb/set-field-control-attributes "Number" {:display-name "Number 1"})
  (fb/set-field-attributes "Number" {:name "Number 1"}))

(defn form-builder-add-boolean []
  (fb/add-field-to-form "Yes/No")
  (fb/set-field-attributes "Yes/No" {:name "Boolean 1"})
  ;; the following fails for bools as there is no test-id
  #_(fb/set-field-control-attributes "New 'Yes/No' field" {:display-name "Boolean 1"}))

(defn form-builder-add-currency []
  (fb/add-field-to-form "Currency")
  ;; this should change both name and display name
  ;; TODO - verify
  (fb/set-field-attributes "Currency" {:name "Currency 1"}))

(defn form-builder-add-datetime []
  (fb/add-field-to-form "Date and Time")
  (fb/set-field-control-attributes "Date and Time" {:display-name "DateTime 1"})
  (fb/set-field-attributes "Date and Time" {:name "DateTime 1"}))

(defn form-builder-add-rel []
  (fb/add-field-to-form "Relationship")
  ;; by default we are using AA_Drink as the target type and that also becomes the default name
  (fb/run-rel-properties-dialog "AA_Drink" {:name         "Rel 1"
                                               :display-name "Rel 1"
                                               :lookup-type  "AA_Drink"}))

(defn form-builder-add-field-to-group []
  (comment
    ;; not quite working....

    (fb/add-field-to-field-group "Text")
    (fb/set-field-attributes "Text 2" {:display-name "Text 2"})

    (fb/add-container-to-form)
    (fb/add-field-group-to-form "New Field Group")))


(defn form-builder-add-choice []

  (fb/add-field-to-form "Choice")

  ;; have to save here to work around a choice field issue (captured in the known issues suite)
  ;; remove this once that is fixed
  (fb/save)

  (fb/run-field-properties-dialog "Choice" {:name          "Choice 1"
                                            :display-name  "Choice 1"
                                            :choice-values ["Monday" "Tuesday" "Wednesday" "Thursday" "Friday" "Saturday" "Sunday"]}))

(defn form-builder-add-lookup []

  (fb/add-field-to-form "Lookup")

  ;; by default we are using AA_Employee as the target type and that also becomes the default name
  (fb/run-lookup-properties-dialog "AA_Employee" {:name         "Lookup 1"
                                                  :display-name "Lookup 1"
                                                  :lookup-type  "AA_Actor"}))

(defn report-builder-smoke-test
  "Performing the qa daily report builder tests.

  Expect the form builder script has been run to create expected ReadiNow Objects."
  []

  ;; shouldn't have to do this, but working around a bug in the create report form
  (app/navigate-to "Home")

  (app/enable-app-toolbox)

  ;; filter to our test objects and then we'll choose the last created
  ;; the reasoning being to avoid broken objects from previous runs... not
  ;; that the last is the best... need to review this
  (tb/select-application-filter "[Select]")
  (tb/set-object-filter "rt-object")

  (let [object-name (last (tb/get-object-names))
        report-name (make-test-name "RT-Report")
        fields #{"Text 1" "Multiline 1" "Number 1" "Currency 1"
                 "DateTime 1" "Boolean 1"}
        ;;temp removing some fields we are having issues with in the formbuilder test
        #_#{"Text 1" "Multiline 1" "Number 1" "Currency 1"
          "DateTime 1" "Boolean 1" "Choice 1" "Lookup 1"}
        ]

    (when-not object-name
      (throw (Exception. "Failed to find RT Object to report on... earlier test failed??")))

    (tb/create-report {:name report-name :object-type object-name})

    ;; alt way of creating a report
    ;;(tb/create-report-via-toolbox-object {:name report-name :object-type object-name})

    ;; should now be in the report builder

    ;; if we weren't for some reason (say we have a bug ... it happened) then open it
    ;;(tb/open-report-builder object-name report-name)

    (doseq [f fields]
      (rb/select-field-in-report f)
      (rb/select-field-in-analyser f))

    ;; all our fields should now appear in analyser and the report

    (do
      (expect (subset? fields (set (rb/get-analyser-fields-in-popup))))
      (expect (subset? fields (set (rb/get-analyser-fields-in-toolbox))))
      (expect (subset? fields (set (rb/get-fields-in-report-view))))
      (expect (subset? fields (set (rb/get-selected-fields-in-toolbox)))))

    ;; add our rel and ensure is in the report

    (rb/add-relationship "Rel 1")
    (expect (subset? #{"Rel 1"} (set (rb/get-fields-in-report-view))))

    ;; save the report

    (rb/save)
    (rb/close)

    ;; back at admin toolbox
    ))

(defn import-spreadsheet [file-name]
  (do
    (app/enable-app-toolbox)

    ;; check our object exists that we want to import into
    (tb/select-application-filter "[Select]")
    (tb/set-object-filter "rt-object")
    (when (empty? (tb/get-object-names))
      (throw (Exception. "Failed to find RT Object to import into... earlier test failed??")))

    (tb/open-quick-link "Import Spreadsheet")
    (import/choose-file-to-import (str (System/getProperty "user.dir") "\\" file-name))
    (Thread/sleep 1000)
    (import/set-heading-row 2)
    (import/set-data-start-row 3)
    (import/choose-next)
    (import/choose-target-object "RT-Object")
    (import/choose-next)
    (import/set-field-mapping "String" "Text 1")
    (import/set-field-mapping "Currency" "Currency 1")
    (import/set-field-mapping "DateTime" "DateTime 1")
    (import/set-field-mapping "Boolean" "Boolean 1")
    (import/choose-next)))


(defn form-builder-smoke-test
  "daily smoke test of app toolkit and form builder"
  []
  (do
    (form-builder-new-object)
    (fb/add-container-to-form)
    #_(form-builder-add-text)
    (fb/add-field-to-form "Text")
    (Thread/sleep 500)
    (fb/set-field-control-attributes "Text" {:display-name "Text 1"})
    (fb/set-field-attributes "Text" {:name "Text 1"}))
  (form-builder-add-multiline)
  (form-builder-add-number)
  (form-builder-add-boolean)
  (form-builder-add-currency)
  (form-builder-add-datetime)
  (form-builder-add-choice)
  (form-builder-add-lookup)
  (fb/add-container-to-form)
  (form-builder-add-rel)
  (form-builder-add-field-to-group)
  (fb/save)

  ;(app/navigate-to "Home")
  ;(expect (not (app/are-changes-pending?)))
  )

(defn get-default-data []
  {:tenant   "EDC"
   :username "Administrator"
   :password "tacoT0wn"})

(defn init []
  )

(comment
  (do

    (rt.app/setup-environment)
    ;(rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})

    (alter-var-root (var *test-context*)
                    (constantly
                      (merge (get-default-data)
                             {:target :chrome}
                             ;{:target :firefox}
                             )))
    (start-app-and-login))

  (do
    (rt.po.app/navigate-to "Test Solution")
    (rt.po.app/navigate-to-item "Test Solution" "Test Solution/AA_Automation/AA_AutomationReport")
    (rt.po.app/enable-config-mode)
    (rt.po.app/load-property-of-nav-item  "AA_AutomationReport")
    (rt.po.report-new/options-expand)
    (rt.po.report-new/click-advanced-tab)
    (rt.po.report-new/set-report-form-only "AA_All Fields Form")
    (rt.test.expects/expect-equals  "AA_All Fields Form" (rt.po.report-new/get-report-form-only))
    )

  (rt.app/setup-environment {:app-url "https://spst02.sp.local"})
  (rt.app/setup-environment {:app-url "https://spst09.sp.local"})
  (rt.app/setup-environment {:app-url "https://spst07.sp.local"})
  (rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})
  (rt.app/setup-environment)
  (rt.app/setup-environment {:app-url "https://syd1dev24.entdata.local"})

  (run-tests [:post-build])

  (run-tests [:perf-app/chrome/t0])
  (run-tests [:post-build-fast])
  (run-tests [:qa-daily/chrome/test-report-and-form])
  (run-tests [:qa-daily/chrome/test-fb])
  (run-tests [:qa-daily/chrome/test-report-and-form])
  (run-tests [:qa-daily/chrome/charts/test-new-chart-drivers])

  (run-tests [:qa-daily/chrome/test-fb :qa-daily/chrome/import-spreadsheet])
  (run-tests [:qa-daily/chrome/import-spreadsheet])

  (rt.repl/run-tests :qa-daily/chrome/import-spreadsheet)
  (rt.repl/run-tests :qa-daily/chrome/import-spreadsheet)
  (rt.repl/run-tests :form-builder/test-choice-field)
  (rt.repl/run-tests :qa-daily/chrome/charts/test-chart-builder-drivers-properties)
  (rt.repl/run-tests :qa-daily/chrome/charts/test-chart-builder-drivers-targets)

  (import-spreadsheet "data/DailyRegressionTestData.xlsx")

  (rt.repl/run-tests :qa-daily/chrome/reports/test-report-property-drivers)
  (rt.repl/run-tests :edit-form/test-view-form)

  (stop-browser)

  ;; to debug a script do something like...

  (rt.app/setup-environment)

  (alter-var-root (var *tc*)
                  (constantly
                    (merge (get-default-data)
                           {:target :chrome}
                           ;{:target :firefox}
                           )))


  (alter-var-root (var *tc*)
                  (constantly
                    (merge (get-default-data)
                           {:target :chrome}
                           {:target :firefox}
                           #_(get-navigation-test-data)
                           #_(get-screen-builder-tests-data-2)
                           {:test {:id :my-test :name "my test"}})))

  (report-and-form-test-tabs)

  (clojure.pprint/pprint *test-context*)

  (stop-browser)

  (do
    (rt.repl/reset)
    ;(rt.app/setup-environment {:app-url "https://spstdev24e.sp.local"})
    ;(rt.repl/run-tests :steve)
    (rt.repl/run-tests :perf-app/chrome/t0 :perf-app/chrome/t1 :perf-app/chrome/t2))

  (navigation-builder-smoke-test)
  (screen-builder-smoke-test)
  (do
    (form-builder-smoke-test)
    (report-builder-smoke-test))
  (open-employee-form-control-props)

  (rt.setup/extract-data-resource "dailyregressiontestdata.xlsx")
  (import-spreadsheet "data/dailyregressiontestdata.xlsx")

  (rt.po.app/enable-app-toolbox)
  (rt.po.app-toolbox/set-object-filter "rt-")
  (rt.po.app-toolbox/select-application-filter "")

  (do
    (form-builder-new-object)
    (fb/add-container-to-form)
    (form-builder-add-text)
    (form-builder-add-multiline)
    (form-builder-add-number)
    (form-builder-add-boolean)
    (form-builder-add-currency)
    (form-builder-add-datetime)
    (form-builder-add-choice)
    (form-builder-add-lookup)
    (fb/add-container-to-form)
    (form-builder-add-rel))




  )
