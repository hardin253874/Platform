(comment

  (:require [rt.lib.wd :refer [right-click debug-click set-input-value double-click cancel-editable-edit-mode
                               wait-for-jq find-element-with-text wait-until-displayed get-repeated-elements
                               has-class]]
            [rt.lib.wd-ng :refer [wait-for-angular evaluate-angular-expression execute-script-on-element]]
            [rt.lib.wd-rn :refer [drag-n-drop test-id-css set-click-to-edit-value navitem-isdirty?]]
            [rt.lib.util :refer [timeit]]
            [rt.po.common :refer [exists-present? click-modal-dialog-button-and-wait]]
            [rt.po.edit-form :as ef]
            [rt.po.report-view :as rv]
            [rt.po.common :refer [safe-text exists-present? set-search-text-input-value wait-until]]
            [clj-webdriver.taxi :refer [text attribute send-keys elements element exists? displayed? find-element-under *driver* value input-text clear]]
            [clj-webdriver.core :refer [->actions move-to-element]]
            [clojure.string :as string]
            [rt.po.app :as app]
            [clj-webdriver.taxi :as taxi]
            [clj-webdriver.taxi :as taxi :refer [to text exists? click value selected? find-element select-by-text selected-options select-option find-element-under find-elements-under element elements element attribute visible? *driver*]]            
            [clj-webdriver.core :refer [by-css by-xpath ->actions move-to-element]])
  
  (comment 
  
  (def TestIconPath (rt.po.common/get-data-file-path "TestRegressionIcon.png"))
  (rt.po.edit-form/upload-image "Conditional format image" TestIconPath)
  
  (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "List: Recipients") )
  (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Notification") )
  (expect-equals nil (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Time Sent") )  )
  (rt.po.report-view/click-send-notification-button-in-relationship "Rehearsal Notifications")
  
  (expect (not (= "" (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Time Sent")))))
  
  (expect (not (= "" (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Email")))))
  
  (first (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Email") )
  
    
  (rt.po.report-view/get-column-values-in-relationship "Rehearsal Notifications" "Email")
  
  
    
    
    
    
    (rt.po.chart-view/select-pie-slice nil "Amended Regulation")
    
   (rt.po.chart-view/select-pie-slice "Plans by Status" "In Review")
    
    (rt.po.chart-view/select-data-point "By Source" "ASIC"  )
    
    
    (rt.po.view-form/open-action-menu-for-relationship "ACTIVE REGULATORY IMPACTS")
    (rt.po.app/choose-context-menu "New")
    
    (rt.po.edit-form/set-lookup-value "Regulatory Event" "Reg-Eve")
    
    (rt.po.edit-form/open-lookup "Organisational Levels")
    	(rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Org Level East" "")
    (rt.po.common/click-ok)
    
        (rt.po.edit-form/open-lookup "Corporate Obligations")
        (rt.po.common/select-picker-dialog-grid-row-by-text "ISO Guidance")
    (rt.po.common/click-ok)
    	
    
    (rt.po.edit-form/set-lookup-value "Products/Services" "External Product 1")
        (rt.po.edit-form/set-lookup-value "Processes" "Process 1")
            (rt.po.edit-form/set-lookup-value "Controls" "Control 3")
    (rt.po.edit-form/open-lookup "Sites")
    	(rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Site South" "")
    	(rt.po.common/click-ok)
    
    
    (rt.po.view-form/open-action-menu-for-relationship "ACTION PLAN")
     (rt.po.app/choose-context-menu "New")
    
    (rt.po.edit-form/set-lookup-value "Controls" "Control 3")
    (rt.po.edit-form/set-lookup-value "Controls" "Control 3")
    (rt.po.common/set-lookup-field "Controls" "Control 3")
    
    (rt.po.edit-form/click-lookup-button "Controls ")
    
    (rt.po.common/set-lookup-field "Controls" "Control 3")
    
    (rt.po.edit-form/set-text-field-value "Controls" "Control 3")
    
    
    	(rt.po.view-form/open-action-menu-for-relationship "EXEMPTION REQUESTS")
    	(rt.po.app/choose-context-menu "New")
    
    (rt.po.edit-form/set-date-field-value "Expiry Date" (make-local-date 2017 2 2))
    
    
    
    )
  
  
  (clj-webdriver.taxi/flash 
  
  (rt.po.report-view/get-report-cell-element-in-relationship  0 "Email" (str ".tab-relationship-render-control:has(.structure-control-header:contains('Rehearsal Notifications'))") ))
  
  ".tab-relationship-render-control:has(.structure-control-header:contains('Rehearsal Notifications'))"
  
  
  (clj-webdriver.taxi/flash  rt.po.report-view/get-grid-row-element-by-text-in-relationship "Crisis Notifications" "TestNotif-20160930-155242-15ead" )
  
  
  (clj-webdriver.taxi/flash (str ".tab-relationship-render-control:has(.structure-control-header:contains('Rehearsal Notifications'))") )
  
  
  
  
  
  
  
  (defn get-column-text-content-in-relationship [rel-title row-index col-name]
  (let [rel-locator  (str ".tab-relationship-render-control:has(.structure-control-header:contains('" rel-title "'))")]
      e (get-report-cell-element-in-relationship row-index column-name rel-locator)
    (text e)
   ))



(
  defn get-report-cell-element-in-relationship [row-index column-name & [grid-locator]]
  (
    let [col-index (rt.po.report-view/get-report-column-index column-name grid-locator)
        grid-locator (or grid-locator ".spreport-view")
        col-query (str ".dataGridCell.col" (or col-index 0))
        cells (find-elements-under grid-locator (by-css col-query)
                                   )
         ]
    (when (and (>= col-index 0) (< col-index (count cells))
               )
      (nth cells col-index)
      )
    )
  )
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  (rt.po.view-form/set-search-text-for-relationship "Crisis Notifications" "TestNotif-20161004-094345-50a5")
  (rt.po.report-view/click-send-notification-button-in-relationship "Crisis Notifications")
  (rt.po.report-view/click-refresh-now-in-relationship "Crisis Notifications")
  (rt.po.report-view/double-click-row-by-text-in-relationship "Crisis Notifications" "TestNotif-20161004-094345-50a5")
  
  (expect-equals "User Action" (first (rt.po.report-view/get-active-tab-column-values "Time Last Sent")))
  
  (first (rt.po.report-view/get-active-tab-column-values "Team Leader"))
  
  
  (clj-webdriver.taxi/flash (str ".tab-relationship-render-control:has(.structure-control-header:contains('Crisis Notifications'))"))
  (rt.po.report-view/get-column-values-in-relationship "Crisis Notifications" "Subject")
  (rt.po.report-view/get-loaded-column-values "Time Last Sent" (str ".tab-relationship-render-control:has(.structure-control-header:contains('Crisis Notifications'))"))
  (rt.po.report-view/get-loaded-column-values "Message" (str ".tab-relationship-render-control:has(.structure-control-header:contains('Crisis Notifications'))"))
  
  (rt.po.report-view/get-column-values-in-relationship "Crisis Notifications" "Time Last Sent")
  
  (+ 4 5)
  
  (str ".tab-relationship-render-control:has(.structure-control-header:contains('" "Crisis Notifications" "'))")
  
   
   (expect (not (= "" (first (rt.po.report-view/get-column-values-in-relationship "Crisis Notifications" "Time Last Sent"))  ) ))
  
  
  
  (rt.po.report-builder/get-grouped-row-content "Business Continuity Plan" 0 "Recovery Plan")
  
  (expect-equals "This Staff has been approved" (nth (rt.po.report-view/get-active-tab-column-values "User Action Log Entry Action summary") 0))
  
  (nth (rt.po.report-view/get-active-tab-column-values "Team Leader") 0)
  
  
  
  (clj-webdriver.taxi/flash  rt.po.report-view/get-grid-row-element-by-text-in-relationship "Crisis Notifications" "TestNotif-20160930-155242-15ead" )
  
  (rt.po.report-view/double-click-row-by-text-in-relationship "Crisis Notifications" "TestNotif-20160930-155242-15ead")
  
  (rt.po.view-form/click-task-action-v2 "Send Notification")
 
  
  
  (rt.po.edit-form/set-text-field-value "Crisis" "testing" )
  (rt.po.edit-form/set-date-field-value "Exp Resolution Date" (make-local-date 2016 10 10))
  (rt.po.edit-form/set-date-field-value "Start Date/Time" "9/9/2016")
  (rt.po.edit-form/set-date-value-v2 "Start Date/Time" "9/9/2016")
  (rt.po.edit-form/set-lookup "Discovered By" "Business Continuity Owner")
  (rt.po.edit-form/set-lookup "Reported By" "Business Continuity Owner")
  (rt.po.edit-form/set-choice-value-v2 "Severity" "Critical")
  
  (rt.po.edit-form/open-lookup "Impacted Sites")
  (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "China" "")
  (rt.po.common/click-ok).
  (rt.po.edit-form/save)
  
  
  (rt.po.view-form/click-task-action-v2 "Activate Plans")
  (rt.po.edit-form/set-workflow-lookup "Select Plans to Activate" "RecoveryPlan")
  (rt.po.edit-form/click-workflow-done)
  
  
  
  (
  	(def ImmRecActName (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "Recovery Action"))
(rt.po.report-view/double-click-row-by-text-in-relationship "Recovery Actions" ImmRecActName)
(rt.po.edit-form/click-edit-button)
(rt.po.edit-form/set-number-value-v2 "% Complete" 100)
(rt.po.edit-form/set-choice-value-v2 "Status" "Completed")
(rt.po.edit-form/save)
(rt.po.edit-form/click-back-button)
  
  )
  
  (
  (def OngoingRecActName (rt.po.report-builder/get-grouped-row-content "Ongoing" 0 "Recovery Action"))
  (rt.po.report-view/double-click-row-by-text-in-relationship "Recovery Actions" OngoingRecActName)
  (rt.po.edit-form/click-edit-button)
(rt.po.edit-form/set-number-value-v2 "% Complete" 100)
(rt.po.edit-form/set-choice-value-v2 "Status" "Completed")
(rt.po.edit-form/save)
(rt.po.edit-form/click-back-button)
  )
  
  (rt.po.edit-form/set-date-field-value "Due date" (make-local-date 2016 11 11))
  (rt.po.edit-form/set-choice-value-v2 "Task status" "Not started")
  
  (expect-equals "100%" (first (clojure.string/split (rt.po.report-view/get-column-values-in-relationship "Plans Being Tested" "% Complete") #"\n")))
  
  (expect-equals "100%" (first (clojure.string/split (first (rt.po.report-view/get-column-values-in-relationship "Plans Being Tested" "% Complete") ) #"\n" )) )
  (expect-equals "Completed" (first (rt.po.report-view/get-column-values-in-relationship "Plans Being Tested" "Execution Status")))
  
  
  
  
    
  
  
  (rt.po.view-form/open-action-menu-for-relationship "ADHOC CORRECTIVE ACTIONS")
  (rt.po.app/choose-context-menu "New")
  (rt.po.edit-form/set-text-field-value "Subject" "Test notification message")

  (rt.po.edit-form/set-date-field-value "Due_date" (make-local-date 2016 10 10))
  
  
  
  
  (rt.po.edit-form/set-bool-field-value "Email" true)
  (rt.po.edit-form/set-bool-field-value "SMS" true)  
  (rt.po.view-form/open-action-menu-for-relationship "RECIPIENTS")
  (rt.po.app/choose-context-menu "Link")
  (rt.po.edit-form/choose-in-entity-picker-dialog "Adam")  
  (rt.po.edit-form/save)
  
  
  
  ((rt.po.report-view/get-quick-search "CRISIS NOTIFICATIONS" )
  (rt.po.report-view/set-search-text "bbbbbb") )
  (rt.po.report-view/apply-quick-search "Crisis Notifications" "bbbbbb")
  	(rt.po.report-view/set-search-text (:disrupRisk-name *tc*))
  (expect-equals "" (rt.po.report-view/get-report-cell-text-content "0" "Time Last Sent"))
  
  (rt.po.view-form/click-task-action-v2 "Send Notification")
  	(rt.po.edit-form/select-form-tab "Crisis Notifications")
  (expect-equals "bbbbbb" (first (rt.po.report-view/get-active-tab-column-values "Subject")))
  
  (rt.po.view-form/click-task-action-v2 "Activate Plans")
  
  (rt.po.view-form/set-search-text-for-relationship "Crisis Notifications" "bbbbbb")
  
  (rt.po.view-form/click-send-notification-button-in-relationship "Crisis Notifications")
  
  (expect-equals "9/30/2016 2:08 PM" (rt.po.report-view/get-report-cell-text-content "Crisis Notifications" "Time Last Sent"))
    
  
  
  
  (expect-equals "Recovery Action" (rt.po.edit-form/get-form-title))
  (rt.po.edit-form/set-number-value-v2 "Sequence Number" "2")
  (rt.po.edit-form/set-choice-value-v2 "Stage" "Ongoing")
  (rt.po.edit-form/set-text-field-value "Recovery Action" "Make Ongoing Action 2")
  (rt.po.edit-form/set-choice-value-v2 "Priority" "High")
  (rt.po.edit-form/set-bool-field-value "Milestone?" true)
  (rt.po.edit-form/save)
  (rt.po.edit-form/click-back-button)
  
  (rt.po.edit-form/set-number-value-v2 "Financial Impact" 10000)
  (rt.po.edit-form/set-lookup "Key People" "Adam")
  (rt.po.edit-form/set-lookup "Key People" "Aide")
  (rt.po.edit-form/set-lookup "Recovery Site" "China")
  
  
  (rt.po.edit-form/select-multi-select-choice-value "Impact Type(s)" "Customer")
  (rt.po.edit-form/select-multi-select-choice-value "Impact Type(s)" "Legal")
  (rt.po.edit-form/set-choice-value-v2 "1 Hour" "Medium")
(rt.po.edit-form/set-choice-value-v2 "1 Day" "Medium")
(rt.po.edit-form/set-choice-value-v2 "1 Week" "Medium")
(rt.po.edit-form/set-choice-value-v2 "1 Month" "Medium")

  
  
  ((rt.po.edit-form/open-lookup "Recovery Site")
   (rt.po.view-form/expand-structure-view-item-node "All Sites")
  (rt.po.view-form/expand-structure-view-item-node "China")
  	(rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Hong Kong" "")
  (rt.po.common/click-ok))
  
  ((rt.po.edit-form/open-lookup "Recovery Site")
   (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "China" "")
  (rt.po.common/click-ok))
  
  (rt.po.report-view/set-search-text  "BIM-20160928-110735-2835")
  
  (rt.po.app/select-navigator-item "Disruptive Risk Register")
  (rt.po.view-form/open-action-menu-for-relationship "DISRUPTIVE RISK REGISTER")
  (rt.po.app/choose-context-menu "New")
  (expect-equals "Disruptive Risk" (rt.po.edit-form/get-form-title))  
  (rt.po.edit-form/set-text-field-value "Name" (:disrupRisk-name *tc*))
  (rt.po.edit-form/set-lookup "Owner" "Business Continuity Owner")
  (rt.po.edit-form/select-multi-select-choice-value "Areas of Risk" "Financial")
  (rt.po.edit-form/select-multi-select-choice-value "Areas of Risk" "Regulatory") 
  (rt.po.edit-form/open-lookup "Organisation Levels")
  (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Manufacturing" "")
  (rt.po.common/click-ok)
  (rt.po.edit-form/set-choice-value-v2 "Impact" "Minor")
  (rt.po.edit-form/set-choice-value-v2 "Likelihood" "Possible")
  	(rt.po.edit-form/set-date-field-value "Next Review Date" (make-local-date 2016 11 11))
  (rt.po.edit-form/save)
  
  (rt.po.edit-form/set-text-field-value "Name" (:issue-name *tc*))
  (rt.po.edit-form/set-lookup "Owner" "Business Continuity Owner")
  (rt.po.edit-form/set-date-field-value "Exp Resolution Date" (make-local-date 2016 11 11))
  (rt.po.edit-form/save)
  
  (rt.po.app/select-navigator-item "Disruptive Risk Register")
  (rt.po.report-view/set-search-text (:disrupRisk-name *tc*))
  (expect (= (rt.po.report-view/count-report-row) 1))
  
  (rn.app/open-board "BIA Status Board" {:app-name "Business Continuity Application", :folder-name "Reports"})
  (expect-equals "BIA Status Board" (rt.po.board/get-board-header-text))
  (expect (rt.po.board/card-exists-in-row? "" "Not Started" (:bim-name *tc*)))
  (rt.po.board/drag-card-to-dimension (:bim-name *tc*) "Not Started" "Approved")
  (expect (rt.po.board/card-exists-in-row? "" "Approved" (:bim-name *tc*)))
  
  (rt.po.edit-form/set-date-field-value "Exp Resolution Date" "11/11/2016")
  
  (rt.po.edit-form/set-date-field-value "Act. Resolution Date" (make-local-date 2016 11 11))
   


  
  
     
  (rn.app/open-board "Plan Status Board" {:app-name "Business Continuity Application",  :folder-name "Reports"})
    
  (rn.app/open-board "Plan Status Board" {:app-name "Business Continuity Application", :screen-name "Recovery Planning" })
  (rn.app/open-screen "Recovery Planning"  {:app-name "Business Continuity Application", :screen-name "Recovery Planning"})
  
  
  
  

  
  
  
  
  (do
    ;; set some namespaces
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all])
    (require '[clojure.set :refer [subset?]])
    (require '[clj-webdriver.taxi :as taxi])
    (require '[rt.po.import-spreadsheet :as is])
    (require '[rt.po.report-builder])
    
    ;; set the default test context 
    (rt.test.core/merge-tc {:target   :chrome
                            :tenant   "EDC"
                            :username "Administrator" :password "tacoT0wn"})

    
    )

  (do
    ;; write test script here 
    (rn.common/start-app-and-login)
    (rt.po.app/navigate-to "CRM")
    (rt.po.form-builder-config/get-calculation-error)
    (rt.po.form-builder-config/set-calculation "[Name]+")
    (rt.po.form-builder-config/ok-disabled?)
    (rt.po.form-builder/add-from-field-menu-to-container "Calculation" 0)
    
    (rt.po.report-builder/save-as "Test123")
    )

  (defn save-as [name]
  (click ".report-Builder-ToolBar button[ng-click*=saveAsReportEntity]")
  (set-input-value ".reportSaveAsDialog-view [ng-model*=reportName]" name)
  (taxi/click "button:contains(OK)")
  (wait-for-angular)
  (when (rt.po.common/exists-present? ".reportSaveAsDialog-view input")
    (throw (Exception. "Did not expect dialog to be present."))))
  
   (defn save-as [name]
  (taxi/click ".report-Builder-ToolBar button[ng-click*=saveAsReportEntity]")
  (set-input-value ".reportSaveAsDialog-view input" name)
  (app/choose-modal-ok)
  (wait-for-angular)
  (when (rt.po.common/exists-present? ".reportSaveAsDialog-view input")
    (throw (Exception. "Did not expect dialog to be present."))))
  
  )




