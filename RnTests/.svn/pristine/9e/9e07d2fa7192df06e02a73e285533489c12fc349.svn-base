(comment

  (rt.po.common/get-today-day )  
   (rt.po.common/get-today-month )  
   (rt.po.common/get-today-year )
  
  
  (rt.po.view-form/set-search-text "test" )
  	(expect-equals "corp-obj" (rt.po.report-view/get-report-cell-text-content 0 "Corporate Objectives") )
  
  (rt.po.edit-form/set-choice-value-v2 "Approval status" "Not Started")
  (rt.po.view-form/open-action-menu-for-relationship "KEY INDICATOR RESULTS")
  (rt.po.app/choose-context-menu "New")
  
  
  
  
      (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")
  	(rt.po.edit-form/set-text-field-value "Name" (:kpi-forecast-name *tc*))
  	(rt.po.edit-form/set-choice-value-v2 "Indicator Type" "KPI")
  	(rt.po.edit-form/set-choice-value-v2 "Monitoring Type" "Forecast")
  	(rt.po.edit-form/set-choice-value-v2 "Expected Trend" "Increasing")
  	(rt.po.edit-form/set-choice-value-v2 "Reporting Frequency" "Monthly")
  
  	(rt.po.edit-form/set-number-value-v2 "Forecast Value" 200)
  (rt.po.edit-form/set-number-value-v2 "% Maximum Delta" 10)
  	(rt.po.edit-form/set-date-field-value "Next Review Date" (make-local-date 2016 12 12))
  (rt.po.edit-form/set-choice-value-v2 "Approval status" "Not Started")
  	(rt.po.edit-form/save)
  	(rt.po.view-form/open-action-menu-for-relationship "KEY INDICATOR RESULTS")
  	(rt.po.app/choose-context-menu "New")
  	(rt.po.edit-form/set-text-field-value "Name" (:kpiforecast-result *tc*))
  	(rt.po.edit-form/set-number-value-v2 "Value" 50)
  (rt.po.edit-form/save)
    	(rt.po.edit-form/click-back-button)
  
  
  
  
  
    (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")
  	(rt.po.edit-form/set-text-field-value "Name" (:kpi-percent-name *tc*))
  	(rt.po.edit-form/set-choice-value-v2 "Indicator Type" "KRI")
  	(rt.po.edit-form/set-choice-value-v2 "Monitoring Type" "Percent Change")
  	(rt.po.edit-form/set-choice-value-v2 "Expected Trend" "Increasing")
  	(rt.po.edit-form/set-choice-value-v2 "Reporting Frequency" "Monthly")
    (rt.po.edit-form/set-date-field-value "Next Review Date" (make-local-date 2016 12 12))
    (rt.po.edit-form/set-choice-value-v2 "Approval status" "Not Started")  
  	(rt.po.edit-form/set-number-value-v2 "Forecast Value" 200)
    (rt.po.edit-form/set-number-value-v2 "% Change" 50)
  	(rt.po.edit-form/save)
  	(rt.po.view-form/open-action-menu-for-relationship "KEY INDICATOR RESULTS")
  	(rt.po.app/choose-context-menu "New")
  	(rt.po.edit-form/set-text-field-value "Name" (:kpipercent-result *tc*))
  	(rt.po.edit-form/set-number-value-v2 "Value" 50)
    (rt.po.edit-form/save)
    (rt.po.edit-form/click-back-button)
  
  
    (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")
  	(rt.po.edit-form/set-text-field-value "Name" (:kpi-threshold-name *tc*))
  	(rt.po.edit-form/set-choice-value-v2 "Indicator Type" "SLA")
  	(rt.po.edit-form/set-choice-value-v2 "Monitoring Type" "Threshold")
  	(rt.po.edit-form/set-choice-value-v2 "Expected Trend" "Increasing")
  	(rt.po.edit-form/set-choice-value-v2 "Reporting Frequency" "Monthly")
    (rt.po.edit-form/set-date-field-value "Next Review Date" (make-local-date 2016 12 12))
    (rt.po.edit-form/set-choice-value-v2 "Approval status" "Not Started")  
    (rt.po.edit-form/set-choice-value-v2 "Threshold Type" "Above Upper Threshold")
  	(rt.po.edit-form/set-number-value-v2 "Upper Threshold" 200)  	
  	(rt.po.edit-form/save)
  	(rt.po.view-form/open-action-menu-for-relationship "KEY INDICATOR RESULTS")
  	(rt.po.app/choose-context-menu "New")
  	(rt.po.edit-form/set-text-field-value "Name" (:kpithreshold-result *tc*))
  	(rt.po.edit-form/set-number-value-v2 "Value" 201)
    (rt.po.edit-form/save)
    (rt.po.edit-form/click-back-button)
  
  
  
  (rt.po.report-view/set-search-text (:kpi-avg-name *tc*))
  (expect-equals (:kpi-avg-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Key Performance Indicator"))
  
    (rt.po.report-view/set-search-text (:kpi-forecast-name *tc*))
  (expect-equals ( :kpi-forecast-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Key Performance Indicator"))
  
    (rt.po.report-view/set-search-text (:kpi-percent-name *tc*))
  (expect-equals (:kpi-percent-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Key Performance Indicator"))
  
    (rt.po.report-view/set-search-text (:kpi-threshold-name *tc*))
  (expect-equals (:kpi-threshold-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Key Performance Indicator"))
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
    (rt.po.view-form/open-action-menu-for-report "My Issues")
  (rt.po.app/choose-context-menu "New")
  
  
  (rt.po.view-form/set-search-text-for-report "My Issues" "123")
  
  	(rt.po.edit-form/set-lookup "Devices" "Brother Fax Machine")
  	(rt.po.edit-form/open-lookup "Devices")
  
  
  	(rt.po.edit-form/open-lookup "Applications")
  	(rt.po.edit-form/click-new-button)
  	(rt.po.edit-form/set-string-field-value "Name" (:app-name *tc*))
  (rt.po.edit-form/set-choice-value-v2 "Application Type" "PC Application")
    (rt.po.edit-form/save)
  
  
  (rt.po.edit-form/open-lookup "Organisation levels")
  	(rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Administration" "")
  (rt.po.common/click-ok)
  (rt.po.edit-form/save)
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  (rt.po.app/navigate-to "Operational Risk Management")
  (rt.po.app/select-app-tab "Risk and Control Assessment")
  
  (rt.po.report-view/set-search-text "By Organisation" "Op-Risk")
    (rt.po.report-view/set-search-text "Op-Risk")
  
  (rt.po.app/select-navigator-item "RCSA By Organisation")
  (rt.po.report-view/set-search-text (:orm-name *tc*))
  	(rt.po.report-view/select-row-by-text (:issue-name *tc*))
  
  
  (rt.po.screen/click-form-edit-button "")
  (rt.po.edit-form/set-choice-value-v2 "Inherent Impact" "Catastrophic")
  (rt.po.edit-form/set-choice-value-v2 "Inherent Likelihood" "Almost Certain")
  
  	(rt.po.view-form/open-action-menu-for-container "CONTROLS")
  	(rt.po.app/choose-context-menu "Link")
  	(rt.po.edit-form/choose-in-entity-picker-dialog "Buy a spare coffee machine")
  
    (rt.po.edit-form/set-number-value-v2 "Finanical Impact" "120000")
  (rt.po.edit-form/set-choice-value-v2 "Residual Impact" "Moderate")
  (rt.po.edit-form/set-choice-value-v2 "Residual Likelihood" "Possible")
  (rt.po.screen/click-form-save-button "")
  
  
  
  (rt.po.app/select-navigator-item "RCSA By Process")
  (rt.po.report-view/set-search-text (:orm-name *tc*))
  	(rt.po.report-view/select-row-by-text (:orm-name *tc*))   
  (expect-equals "Catastrophic" (rt.po.view-form/get-field-value "Inherent Impact"))
  (expect-equals "Almost Certain" (rt.po.view-form/get-field-value "Inherent Likelihood"))
  (rt.po.screen/click-form-edit-button "")
  (rt.po.edit-form/set-choice-value-v2 "Inherent Impact" "Major")  
  (rt.po.screen/click-form-save-button "")
  
    (rt.po.app/select-navigator-item "RCSA By Product and Service")
  (rt.po.report-view/set-search-text (:orm-name *tc*))
  	(rt.po.report-view/select-row-by-text (:orm-name *tc*))    
  (expect-equals "Major" (rt.po.view-form/get-field-value "Inherent Impact"))
  (expect-equals "Almost Certain" (rt.po.view-form/get-field-value "Inherent Likelihood"))
  (rt.po.screen/click-form-edit-button "")  
  (rt.po.edit-form/set-choice-value-v2 "Inherent Likelihood" "Likely")
  (rt.po.screen/click-form-save-button "")
  (expect-equals "Likely" (rt.po.view-form/get-field-value "Inherent Likelihood"))
  
  
  
  
  (+ 2 4)
  
  (expect (not (= "0" (rt.po.report-view/get-report-cell-text-content 0 "Number of risks")))   )
  (expect (not (= "" (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Time Sent")))))
  
  
  
 (identity {:issue-name (rt.lib.util/make-test-name "Issue"), :person-lastname (rt.lib.util/make-test-name "PerLast"), 
            :info-name (rt.lib.util/make-test-name "Info"), :app-name (rt.lib.util/make-test-name "Apps"), :person-firstname (rt.lib.util/make-test-name "PerFirst"), 
            :task-name (rt.lib.util/make-test-name "Task"), :org-name (rt.lib.util/make-test-name "OrgStructure"), 
            :site-name (rt.lib.util/make-test-name "Site"), :device-name (rt.lib.util/make-test-name "Device")})
  

  (rt.po.app/select-navigator-item "Organisation Structure")
  (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")
  (rt.po.edit-form/set-text-field-value "Name" (:org-name *tc*))
  
  (rt.po.edit-form/open-lookup "Parent organisation level")
  (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Administration" "")
  (rt.po.common/click-ok)
  
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (rt.po.report-view/set-search-text (:org-name *tc*))
  (expect-equals (:org-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Organisation Structure"))
  
  
(
  (rt.po.app/select-navigator-item "Sites")
  (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")
  (rt.po.edit-form/set-text-field-value "Name" (:site-name *tc*))
  
  (rt.po.edit-form/open-lookup "Parent Site")
  (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "China" "")
  (rt.po.common/click-ok)
  
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (rt.po.report-view/set-search-text (:org-name *tc*))
  (expect-equals (:site-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Sites"))

  )
  
  
  (rt.po.app/select-navigator-item "Applications")
  (rt.po.report-view/open-action-menu)
  (rt.po.app/choose-context-menu "New")
  (rt.po.edit-form/set-text-field-value "Name" (:app-name *tc*))
  (rt.po.edit-form/set-choice-value-v2 "Application Type" "PC Application")  
  
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (rt.po.report-view/set-search-text (:app-name *tc*))
  (expect-equals (:app-name *tc*) (rt.po.report-view/get-report-cell-text-content 0 "Sites"))
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  (rt.po.view-form/open-action-menu-for-relationship "RECOVERY TEST NOTIFICATIONS")
  (rt.po.app/choose-context-menu "New")
  (rt.po.edit-form/set-text-field-value "Subject" (:notif-name *tc*))
  (rt.po.edit-form/set-bool-field-value "Email" true)
  (rt.po.edit-form/set-bool-field-value "SMS" true)
  (rt.po.app/choose-context-menu "Link")
  (rt.po.edit-form/choose-in-entity-picker-dialog "Adam")
  (rt.po.edit-form/save)
  
  (rt.po.view-form/set-search-text-for-relationship "Recovery Test Notifications" (:notif-name *tc*))
  (expect-equals "" (first (rt.po.report-view/get-column-values-in-relationship "Recovery Test Notifications" "Time Last Sent")))
  (rt.po.report-view/click-send-notification-button-in-relationship "Recovery Test Notifications")
  (rn.common/wait-until-settled)
  (rt.po.report-view/click-refresh-now-in-relationship "Recovery Test Notifications")
  (expect (not (= "" (first (rt.po.report-view/get-column-values-in-relationship "Recovery Test Notifications" "Time Last Sent")))))
  
  
  
  (rt.po.view-form/click-task-action-v2 "Activate Plans")
  (rn.common/wait-until-settled)
  (rt.po.edit-form/set-workflow-lookup "Plans" "ITDR-Plan")
  (rt.po.edit-form/click-workflow-done)
  
  (def ActivatedPlanName (rt.po.report-builder/get-grouped-row-content "Business Continuity Plan" 0 "Recovery Plan"))
  (rt.po.report-view/double-click-row-by-text-in-relationship "Activated Plans" ActivatedPlanName)
  
  
   
  (rt.po.view-form/open-action-menu-for-relationship "TASKS")
  (rt.po.app/choose-context-menu "New")
  (rt.po.edit-form/set-text-field-value "Subject" (:task-name *tc*))
  (rt.po.edit-form/set-date-field-value "Due date" (make-local-date 2016 11 11))
  (rt.po.edit-form/set-lookup "Assigned to" "Adam")
  (rt.po.edit-form/set-choice-value-v2 "Task status" "Not started")
  (rt.po.edit-form/save)
  
  
  (def ActivatedPlanName (first (rt.po.report-view/get-column-values-in-relationship "IT Plans Being Tested" "ITDR-Test")))
(rt.po.report-view/double-click-row-by-text-in-relationship "IT Plans Being Tested" ActivatedPlanName)
(def ImmRecActName (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "IT Recovery Action"))
(rt.po.report-view/double-click-row-by-text-in-relationship "IT Recovery Actions" ImmRecActName)
  (rt.po.edit-form/click-edit-button)
  (rt.po.edit-form/set-number-value-v2 "% Complete" 100)
  (rt.po.edit-form/set-choice-value-v2 "Status" "Completed")
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (def OngoingRecActName (rt.po.report-builder/get-grouped-row-content "Ongoing" 0 "Recovery Action"))
  (rt.po.report-view/double-click-row-by-text-in-relationship "IT Recovery Actions" OngoingRecActName)
  (rt.po.edit-form/click-edit-button)
  (rt.po.edit-form/set-number-value-v2 "% Complete" 100)
  (rt.po.edit-form/set-choice-value-v2 "Status" "Completed")
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (expect-equals "100%" (first (clojure.string/split (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "% Complete") #"\n")))
  (expect-equals "Completed" (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "Status"))
  (expect-equals "100%" (first (clojure.string/split (rt.po.report-builder/get-grouped-row-content "Ongoing" 0 "% Complete") #"\n")))
  (expect-equals "Completed" (rt.po.report-builder/get-grouped-row-content "Ongoing" 0 "Status"))
  (rt.po.view-form/set-search-text-for-relationship "Call Tree" "Not Called")
  (rt.po.report-view/double-click-row-by-text-in-relationship "Call Tree" "Not Called")
  (rt.po.edit-form/click-edit-button)
  (rt.po.edit-form/set-choice-value-v2 "Status" "Contacted")
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (rt.po.edit-form/click-edit-button)
  (rt.po.edit-form/set-number-value-v2 "% Complete" 100)
  (rt.po.edit-form/set-choice-value-v2 "Execution Status" "Completed")
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  (expect-equals "100%" (first (clojure.string/split (first (rt.po.report-view/get-column-values-in-relationship "IT Plans Being Tested" "% Complete")) #"\n")))
  (expect-equals "Completed" (first (rt.po.report-view/get-column-values-in-relationship "IT Plans Being Tested" "Execution Status")))
  (rt.po.edit-form/click-edit-button)
  (rt.po.edit-form/set-number-value-v2 "% Complete" 100)
  (rt.po.edit-form/set-choice-value-v2 "Test Status" "Complete")
  (rt.po.edit-form/set-choice-value-v2 "IT Test Outcome" "Pass")
  (rt.po.edit-form/set-date-field-value "End_Date_Time" (make-local-date 2016 11 11))
  (rt.po.edit-form/save)
  (rt.po.app/open-nav-tree-node "Reports")
  (rt.po.app/select-navigator-item "Test History")
  (rt.po.report-view/refresh-now)
  (rt.po.report-view/set-search-text (:rehearsal-name *tc*))
  (expect-equals "Pass" (rt.po.report-view/get-report-cell-text-content 0 "IT Test Outcome"))
  
  
  
  
  
  
  
  
  
  (rn.common/start-app-and-login)
    (rt.po.app/navigate-to "CRM")
    (rt.po.form-builder-config/get-calculation-error)
    (rt.po.form-builder-config/set-calculation "[Name]+")
    (rt.po.form-builder-config/ok-disabled?)
    (rt.po.form-builder/add-from-field-menu-to-container "Calculation" 0)
  
  
  (do
    ;;existing access rules are enabled

    )
  
  
  )