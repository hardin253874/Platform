(comment
  
  @rt.server/context
  @rt.server/posted-step-summary
  (rt.setup/get-settings)
  (reset! rt.server/posted-step-summary {})

  (rt.po.view-form/get-task-names)

  (let [results (->> {:root    {:id      "workflowRun"
                                :related [{:rel       {:id "workflowRunStatus" :as "status"}
                                           :forward   true
                                           :mustExist false}
                                          #_{:rel       {:id "errorLogEntry" :as "log"}
                                             :forward   true
                                             :mustExist false}]}
                      :selects [{:field "name" :displayAs "name"}
                                {:field "runCompletedAt" :displayAs "date"}
                                {:field "createdDate" :displayAs "createdDate"}
                                {:field "name" :on "status" :displayAs "status"}
                                {:field "runStepCounter" :displayAs "step"}
                                #_{:field "description" :on "log" :displayAs "log"}]
                      :conds   [{:expr {:field "name"}
                                 :oper "Equal"
                                 :val  "Convert Lead"}]}
                     (rt.lib.wd-rn/run-query)
                     (rt.lib.wd-rn/query-results-as-objects)
                     ;; date's seem to be in the following format... not sure where
                     ;; they are being converted to this local string format, but I suspect
                     ;; its in the structured query web API
                     ;(map #(assoc % :date (safe-date-parse "dd/MM/yyyy HH:mm:ss" (:date %))))
                     )]
    (println "get-workflow-run-status-records=>" results)
    results)

  )


(comment

  (do
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all])
    (require '[clojure.set :refer [subset?]])
    (require '[clj-webdriver.taxi :as taxi]))

  (rt.po.report-builder/view-report-direct "Foster University" (str "IconFormattingCurrency"))
  (rt.po.app/navigate-to-item "Foster University" "Reports/Totals-Number")

  (rt.po.app/node-in-nav-tree-exists? "Student report")

  (rn.app/open-security "Navigation Access")
  (rn.app/open-report "ReportProperties")

  (rt.po.report-builder/view-report "Foster University" (str (:section *tc*) "/ReportProperties"))

  (->> (elements "[rowindex]") (map #(taxi/attribute % "rowindex")))

  (rt.po.report-view/get-report-cell-text-content "19" "Balance")
  (rt.lib.wd-rn/report-grid-scroll-to-top)
  (rt.po.report-view/get-report-cell-text-content "20" "Balance")

  (let [name "Staff Report"
        {:keys [app-name folder-name]} {:app-name "Foster University", :folder-name "Reports"}]
    (rn.services.entity/get-entity-id-for-name name "report" {:app-name app-name :folder-name folder-name}))

  (rn.app/open-report "APIs" {:app-name "Readinow Core Data", :folder-name "Integration"})

  (rn.app/open-report "Staff Report" "Foster University" "Reports")


  (do
    ;; set the default test context 
    (rt.test.core/merge-tc {:target   :chrome
                            :tenant   "EDC"
                            :username "Administrator" :password "tacoT0wn"})

    (rt.test.core/merge-tc {:creds [{:username "ssmith" :password "Read1ssm"}
                                    {:username "jford" :password "Read1jfo"}
                                      {:username "fmiller" :password "Read1fmi"}
                                    {:username "tduffy" :password "Read1tdu"}
                                    {:username "oturner" :password "Read1otu"}
                                    {:username "smohan" :password "Read1smo"}
                                    {:username "svaldez" :password "Read1sva"}]})

    (rt.test.core/merge-tc {:leads [{:first "Joe" :last "Black"}
                                    {:first "Mike" :last "Hammer"}
                                    {:first "Billy" :last "Dunne"}]})

    (rt.test.core/merge-tc {:lead-title   (make-test-name "Lead")
                            :company      (make-test-name "Company")
                            :lead-details (get-random-tc-item :leads)})
    )

  (do
    (rn.common/start-app-and-login)
    (rn.common/start-app-and-login (get-random-tc-item :creds))
    (rt.po.app/navigate-to "CRM")
    (rt.po.app/select-app-tab "Leads")
    (rt.po.report-view/open-action-menu)
    (rt.po.app/choose-context-menu "New")
    (rt.po.edit-form/set-text-field-value "Lead title" (:lead-title *tc*))
    (rt.po.edit-form/set-text-field-value "First name" (-> *tc* :lead-details :first))
    (rt.po.edit-form/set-text-field-value "Last name" (-> *tc* :lead-details :last))
    (rt.po.edit-form/set-text-field-value "Company" (-> *tc* :company))
    (rt.po.edit-form/save)
    (rt.po.report-view/set-search-text (-> *tc* :lead-title))
    (rt.po.report-view/right-click-row-by-text (-> *tc* :lead-title))
    (rt.po.app/choose-context-menu "Convert Lead")
    (expect (taxi/wait-until #(subset? #{"Convert"} (set (rt.po.view-form/get-task-actions))) 60000))
    (rt.po.view-form/choose-task-action "Convert")
    (expect (taxi/wait-until #(subset? #{"OK"} (set (rt.po.view-form/get-task-actions))) 60000))
    (rt.po.view-form/choose-task-action "OK")

    (rt.po.app/select-app-tab "Accounts")
    (rt.po.report-view/set-search-text (-> *tc* :company))
    (rt.po.report-view/right-click-row-by-text (-> *tc* :company))
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.edit-form/set-multiline "Description" (str "Discovered due to lead: " (:lead-title *tc*)))
    (rt.po.edit-form/set-text-field-value "Address line 1" "23 Apple Lane")
    (rt.po.edit-form/set-text-field-value "City(txt)" "Orange Grove")
    (rt.po.edit-form/set-text-field-value "State(txt)" "QLD.")

    (rt.po.view-form/select-form-tab "Contacts")
    (rt.po.report-view/click-action-menu-button "Contact")
    (rt.po.edit-form/set-text-field-value "First name" (-> (get-random-tc-item :leads) :first))
    (rt.po.edit-form/set-text-field-value "Last name" (-> (get-random-tc-item :leads) :last))
    (rt.po.edit-form/save)

    (rt.po.edit-form/save) ; the account

    (rt.po.app/select-app-tab "Opportunities")
    (rt.po.report-view/set-search-text (-> *tc* :lead-title))
    (rt.po.report-view/double-click-row-by-text (-> *tc* :lead-title))

    (rt.po.view-form/select-form-tab "Items")
    (rt.po.report-view/click-action-menu-button "One-off Item")
    (rt.po.edit-form/set-text-field-value "Name" (make-test-name "one-off"))
    (rt.po.edit-form/set-choice-value-v2 "Product Category" "Hardware")
    (rt.po.edit-form/set-number-value-v2 "Quantity" "100")
    (rt.po.edit-form/set-number-value-v2 "Unit price" "19.99")
    (rt.po.edit-form/save)

    (rt.po.view-form/select-form-tab "Items")
    (rt.po.report-view/click-action-menu-button "Recurring Item")
    (rt.po.edit-form/set-text-field-value "Item" (make-test-name "recurring"))
    (rt.po.edit-form/set-choice-value-v2 "Product Category" "Professional Services")
    (rt.po.edit-form/set-number-value-v2 "Quantity" "13")
    (rt.po.edit-form/set-number-value-v2 "Unit price" "399")
    (rt.po.edit-form/save)

    (rt.po.view-form/open-tab-action-menu "Quote" "New")
    (rt.po.edit-form/set-text-field-value "Quote name" (make-test-name "quote"))
    (rt.po.edit-form/set-date-field-value "Valid until" (make-local-date 2199 12 31))
    (rt.po.edit-form/save)

    (rt.po.view-form/select-form-tab "Quote")
    (rt.po.report-view/set-search-text "quote-")
    (rt.po.report-view/select-row-by-text "quote-")
    (rt.po.report-view/click-action-menu-button "Create Quote")

    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-choice-value-v2 "Sales stage" "Qualification")
    (rt.po.edit-form/save)

    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-choice-value-v2 "Sales stage" "Closed Won")
    (rt.po.edit-form/save)

    (rt.po.view-form/form-nav-back)
    )

  (let [id :basic]
    (println ";; setup")
    (println (rt.test.core/get-test-steps-as-source id :setup))
    (println ";; steps")
    (println (rt.test.core/get-test-steps-as-source id)))

  (rt.test.core/merge-tc (rt.test.core/get-default-tc))
  (rt.test.core/merge-tc {:target :phantomjs :app-url "https://sg-mbp-2013.local"})

  rt.test.core/*tc*

  (rn.common/start-app-and-login)
  (rt.po.app/navigate-to "Foster University")
  (rt.po.app/select-navigator-item "Screens")
  (rt.po.app/select-navigator-item "Staff screen - all three")
  (rt.po.app/select-navigator-item "Reports")
  (rt.po.app/select-navigator-item "Student report")

  )

