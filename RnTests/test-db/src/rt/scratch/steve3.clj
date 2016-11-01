(comment
  ;; typical scratch file ...

  (rt.app/setup-environment {:app-url "https://sptrunkfe.sp.local"})

  ;; get some symbols in place that we normally have when running
  (do
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all])
    (require '[clojure.set :refer [subset?]])
    (require '[clj-webdriver.taxi :as taxi]))


  ;; what namespace are we running in?
  (println *ns*)
  ;; what is the doc for a function?
  (clojure.repl/doc rt.po.app/navigate-to-item)

  ;; set the default test context 
  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :chrome})))

  ;; what it says
  (rn.common/start-app-and-login {:username "Nelle.Odom" :password "Nelle.Odom1"})
  (rn.common/start-app-and-login)

  (rn.app/open-report "Foster University" "RT-Section-20160209-085607-d676/SummariseCurrency")
  (rn.app/open-report "Foster University" "RT-Section-20160209-085607-d676/SummariseCurrency")
  (rn.app/open-report "SummariseCurrency" {:app-name    "Foster University"
                                           :folder-name "RT-Section-20160209-085607-d676"})
  (rn.app/open-report-builder "SummariseCurrency" {:app-name    "Foster University"
                                                   :folder-name "RT-Section-20160209-085607-d676"})


  ;; now we do test specifics

  ;; Note that once a set of steps is worked out
  ;; you can paste them into a test in a step edit box and it will
  ;; expand to create multiple steps

  (rt.po.app/navigate-to "CRM")
  (rt.po.app/select-app-tab "Leads")

  (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")

  (rt.po.edit-form/set-lookup "Lead Owner" "Ciccu")

  (rt.po.edit-form/set-text-field-value "Lead title" "Mr (or is this something else??)")
  (rt.po.edit-form/set-text-field-value "First name" "Joe")
  (rt.po.edit-form/set-text-field-value "Last name" "Black")

  (rt.po.edit-form/set-text-field-value "Company" "AAA A new company")

  (rt.po.edit-form/save)

  (comment -- should be back at the Leads screen --)
  (comment -- missing driver functions to check current screen name --)

  (rt.po.report-view/set-search-text "Joe Black")
  (rt.po.report-view/right-click-row-by-text "Joe Black")
  (rt.po.app/choose-context-menu "Convert Lead")

  (expect-equals 1 (count (rt.po.view-form/get-task-names)))
  (expect-match #"Convert" (rt.po.view-form/get-task-name))
  (expect-equals '("Convert" "Cancel") (rt.po.view-form/get-task-actions))
  (rt.po.view-form/choose-task-action "Convert")

  (nth (rt.po.report-view/get-active-tab-column-values "Subjects") 2)
  (rt.po.report-view/get-report-column-names)
  (rt.po.report-view/get-active-tab-cell-value 0 "Subjects")

  )