(ns rt.scratch.anurag
  (:require [clj-webdriver.taxi :refer :all]
            [rt.scripts.common :refer [start-app-and-login]]
            [rt.app :refer [setup-environment]]
            [rt.po.edit-form :as ef]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [clojure.repl :refer :all]
            [clojure.pprint :refer [pprint]]
            [clojure.string :as string]
            [clojure.walk :as walk]
            [clojure.data.json :as json]
            [clj-time.core :as t]
            [clj-time.coerce :as tc]
            [clj-time.format :as tf]
            [rt.test.core :refer [*test-context*]]))

(comment
  (clj-webdriver.taxi/flash (rt.po.form-builder-config/get-action-enabled-checkbox "Report"))
  (rt.po.form-builder-config/get-action-enabled-checkbox "Report") 
  
  (clj-webdriver.taxi/element(str ".ngRow:contains('1Location WF') input[type='checkbox']"))
  
  (clj-webdriver.taxi/click (str ".sp-Form-Builder-Action button[ng-click*='showActionsDialog()']"))
  
  (clj-webdriver.taxi/flash (str "input "))
  
  (clj-webdriver.taxi/flash (str ".ngCanvas :contains('1Location WF')" ))
  
  (clj-webdriver.taxi/flash (str ".ngRow .ngCell:contains('1Location WF')" ))

 (rt.po.report-view/select-row-by-text "NSW")
(rt.po.edit-form/get-today-date-us-format)  
(rt.po.edit-form/get-today-date-aus-format)  
(rt.po.edit-form/get-today-date "yyyy/MM/dd")  
(rt.po.edit-form/get-today-date "dd/MM/yyyy") 
(quot (System/currentTimeMillis) 1000)
  
  (rt.po.edit-form/get-now-date-time-utc)
  (clj-time.core/now)
  
  ;;Unix timestamp 
(defn get-unix-date-from-long []
  (tc/from-long (quot (System/currentTimeMillis) 1000))
  )

(defn get-now-date-time-utc []
  (tc/from-long (tc/to-long  (t/now)))
  )
  
(expect-equals "Darren Jacobs" (rt.po.report-view/get-report-cell-text-content "0" "Employee")) 
  (rt.po.report-view/get-report-cell-text-content "0" "Date and Time")
 
(index-of s value from-index)
  
  (clojure.string/index-of "Hello world" " " 0)
  (subs "Hello world" 0 (clojure.string/index-of "Hello world" " " 0))
  ************************************************************************************
   (rt.po.report-view/report-header-icon-exist?)
(rt.po.report-view/get-report-header-icon-background-colour)
(expect-equals {:r 255, :g 162, :b 0, :a 255} (rt.po.report-view/get-report-header-icon-background-colour))
  
  (rt.po.board/board-header-icon-exist?)
 (rt.po.board/get-board-header-icon-background-colour)
 (expect-equals {:r 182, :g 121, :b 50, :a 255} (rt.po.board/get-board-header-icon-background-colour))
  
  (rt.po.app/node-icon-in-nav-tree-exists? "Student report")
(rt.po.app/get-node-icon-background-colour "Student report")
(expect-equals {:r 255, :g 162, :b 0, :a 255} (rt.po.app/get-node-icon-background-colour "Student report"))
  
  (rt.po.edit-form/form-title-icon-exists?)
(rt.po.edit-form/get-form-header-icon-background-colour)
(expect-equals {:r 255, :g 162, :b 0, :a 255} (rt.po.edit-form/get-form-header-icon-background-colour))

  
  (* 2 2)
  
  (clj-webdriver.taxi/text (str ".relControl:contains('1Location') a[ng-click*=handleLinkClick]:contains('NSW')"))  
  (clj-webdriver.taxi/exists? (str ".relControl:contains('1Location') a[ng-click*=handleLinkClick]:contains('NSW')"))  
  (clj-webdriver.taxi/click (str ".relControl:contains('1Location') a[ng-click*=handleLinkClick]:contains('Sydney')"))
  
  
  
  (count(clj-webdriver.taxi/elements  (str ".multilineRel:contains('1Location') a[ng-click*=handleLinkClick]")))  
  (count(clj-webdriver.taxi/elements  (str ".expanderDialog a[ng-click*=linkClicked]")))
  
  (rt.po.view-form/get-count-lookup-link-values "1Location")
  (rt.po.view-form/get-count-expander-dialog-lookup-link-values)
  
  
  
  (clj-webdriver.taxi/count (str ".multilineRel:contains('1Location') button[ng-click*=spExpander_modal]"))
  
  (clj-webdriver.taxi/exists? (str ".multilineRel:contains('1Location') button[ng-click*=spExpander_modal]"))
  (clj-webdriver.taxi/click (str ".multilineRel:contains('1Location') button[ng-click*=spExpander_modal]"))
  
  (clj-webdriver.taxi/exists? (str ".expanderDialog button[ng-click*=closeDetail]"))
  (clj-webdriver.taxi/click (str ".expanderDialog button[ng-click*=closeDetail]"))
  
  (clj-webdriver.taxi/exists? (str ".expanderDialog a[ng-click*=linkClicked]:contains('NSW')")) 
  (clj-webdriver.taxi/click (str ".expanderDialog a[ng-click*=linkClicked]:contains('NSW')"))
  
  
  ****
  (rt.po.view-form/multi-lookup-link-value-exists? "1Location" "VIC")  
  (rt.po.view-form/click-multi-lookup-link-value "1Location" "VIC")
  (rt.po.view-form/get-count-lookup-link-values "1Location")
  
  (rt.po.view-form/multi-lookup-link-expander-button-exists? "1Location")
  (rt.po.view-form/click-multi-lookup-link-expander-button "1Location")
  
  (rt.po.view-form/multi-lookup-expander-dialog-link-value-exists? "NSW")
  (rt.po.view-form/click-multi-lookup-expander-dialog-link-value "NSW")
  (rt.po.view-form/get-count-expander-dialog-lookup-link-values)
  
  (rt.po.view-form/close-multi-lookup-expander-dialog)
  
  
  (str ".relControl:contains('1Location') a[ng-click*=handleLinkClick]")
  
  (rt.po.form-builder-config/set-display-as "Report") 
  (rt.po.form-builder-config/set-display-as "Inline")
  
  (rt.po.edit-form/get-lookup "1Location")
  (rt.po.view-form/get-lookup-link  "1Location")
  
  (rt.po.report-view/open-analyser-field-configure-dialog "1Location")
  
  (rt.po.report-format/click-value-formatting )
  
  (rt.po.report-format/get-alignment)
  (rt.po.report-format/set-hierarchy-report "1Location Hierarchy")
  (rt.po.report-format/get-hierarchy-report)
  
  (clj-webdriver.taxi/flash (str ".spConditionalFormattingDialog div[ng-show*='valueFormatting.showStructureView']  select[ng-model*=selectedEntity]"))
  
  
  
  (clj-webdriver.taxi/flash (str ".entityCompositePicker span[class*='matched']:contains('Sydney')"))
  
  (clj-webdriver.taxi/find-elements-under (clj-webdriver.taxi/element (rt.po.view-form/find-structure-view-item 'Australia')) )
  
  
  (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "Australia" "NSW")
  (rt.po.view-form/select-structure-view-item-with-ctrl-key-by-text "VIC" "")
  (rt.po.view-form/select-item-with-ctrl-key-by-text "Ballarat" "NSW")
  
  (rt.po.report-view/select-row-with-ctrl-key (clj-webdriver.taxi/element (rt.po.view-form/find-structure-view-item 'VIC')) )
  
(rt.po.view-form/find-structure-view-item 'Sydney')
  (rt.po.view-form/structure-view-item-exists? 'TAS')
  (rt.po.view-form/find-structure-view-item 'TAS')
  (clj-webdriver.taxi/flash (clj-webdriver.taxi/element (rt.po.view-form/find-structure-view-item 'TAS')))
  (clj-webdriver.taxi/element (rt.po.view-form/find-structure-view-item 'NSW'))
  (rt.po.view-form/find-structure-view-item 'NSW')
  
  (clj-webdriver.taxi/flash (str ".entityCompositePicker span:contains('NSW')"))
  (clj-webdriver.taxi/exists? (str ".entityCompositePicker span:contains('NSW')"))
  (rt.po.view-form/structure-view-item-exists? "TAS")
  
  (rt.po.view-form/structure-view-matched-item-exists? "Sydney")
  (rt.po.view-form/select-structure-view-item "TAS")
  
  (rt.po.view-form/get-count-all-search-matched-structure-view-items)
  (rt.po.view-form/get-count-search-matched-structure-view-items "20")  
  (rt.po.view-form/get-count-structure-view-items "20")
  (rt.po.view-form/get-count-all-structure-view-items)
  (rt.po.view-form/get-count-all-selected-structure-view-items)
  
  (count (clj-webdriver.taxi/elements ".entityCompositePicker div[class*='structureItem']"))
  
  (count (clj-webdriver.taxi/elements ".entityCompositePicker div[class*='structureItem']:has(span[class*='matched'])"))
  
  (count (clj-webdriver.taxi/elements ".entityCompositePicker div[class*='structureItem']"))
  )


(comment

  (do
    ;; setup and login
    (do
      (setup-environment)

      (alter-var-root (var *test-context*)
                      (constantly {:tenant   "EDC"
                                   :username "Administrator"
                                   :password "tacoT0wn"
                                   :target :chrome}))

      (clojure.pprint/pprint *test-context*)
      (start-app-and-login)
      (wait-for-angular)
      )

    ;; create 'Forms Regression' app if it doesn't exist
    (if-not (element (str ".app-launcher-tile img[alt='Forms Regression']"))
      (do
        (rt.po.app-toolbox/create-app {:name "Forms Regression" :description ""})
        ;;(rt.po.app/enable-app-toolbox )
        ;(rt.po.app/enable-config-mode )
        ;(click(str "div[ng-click*=addNewApplication]"))
        )
      )

    ;; enable toolbox and select 'Forms Regression' app
    (do
      (rt.po.app/enable-app-toolbox )
      (rt.po.app-toolbox/set-app-combo "Forms Regression")
      (wait-for-angular)
      )
    
   
    ;; new access rule dialog
    (clj-webdriver.taxi/options "#roles")    
    (clj-webdriver.taxi/select-by-text "#roles" "Aidan.Mcclure (User Account)")    
    (rt.po.access-rules-new/set-user-or-role "Aidan.Mcclure (User Account)")    
    (rt.po.access-rules-new/set-object "AA_All Fields")    
    (rt.po.access-rules-new/get-include-users-value)    
    (clj-webdriver.taxi/exists? (str "#roles" " option[label='Aidan.Mcclure (User Account)']"))    
    (clj-webdriver.taxi/exists? (str "#roles" " option[label='Aidan.Mcclure (User Account)']")) 
    (rt.po.access-rules-new/user-or-role-option-exists? "Aidan.Mcclure (User Account)")    
    (rt.po.access-rules-new/object-option-exists? "Actor")    
    (str ".edit-form-control-container:contains('Users') .edit-form-value input[type=checkbox]")

    ;; create 'Text Field Object' and its form
    (do
      (rt.po.app-toolbox/create-object {:name "Text Field Object" :description "New Test Object Description"})
      (rt.po.form-builder/add-container-to-form )

      ;; mandatory field
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "Mandatory Field")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Form Detail")
        (rt.po.form-builder-config/set-control-mandatory true)
        (rt.po.form-builder-config/click-ok )
        )

      ;; readonly field
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "ReadOnly Field")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Form Detail")
        (rt.po.form-builder-config/set-control-readonly true)
        (rt.po.form-builder-config/click-ok )
        )

      ;; Min Character Set
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "Min Character Set")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Object Detail")
        (rt.po.form-builder-config/set-field-minimum-value "5")
        (rt.po.form-builder-config/click-ok )
        )

      ;; Max Character Set
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "Max Character Set")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Object Detail")
        (rt.po.form-builder-config/set-field-maximum-value "10")
        (rt.po.form-builder-config/click-ok )
        )

      ;; Default Value Set
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "Default Value Set")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Object Detail")
        (rt.po.form-builder-config/set-field-default-value "This is a Text Field")
        (rt.po.form-builder-config/click-ok )
        )

      ;; Format Control
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "Format Control")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Format")
        (rt.po.form-builder-config/set-background-color "Light Red")
        (rt.po.form-builder-config/click-ok )
        )

      ;; Pattern Field
      (do
        (rt.po.form-builder/add-from-field-menu-to-container "Text" 0)
        (rt.po.form-builder/open-form-control-configure-dialog "Text")
        (rt.po.form-builder-config/set-name "Pattern Field")
        (rt.po.form-builder-config/expand-section "Options")
        (rt.po.form-builder-config/select-tab "Object Detail")
        ;;(wait-for-angular)
        ;;(rt.po.form-builder-config/set-text-pattern "Email Address")
        (rt.po.form-builder-config/click-ok )
        )

      ;; Save and Close to go back to toolbox
      (do
        (rt.po.form-builder/save)
        (rt.po.form-builder/close)
        )
      )

    )
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;


  (do





    (setup-environment)

    (alter-var-root (var *test-context*)
                    (constantly {:tenant   "EDC"
                                :username "Administrator"
                                :password "tacoT0wn"
                                :target :chrome}))

    (clojure.pprint/pprint *test-context*)
    (start-app-and-login)
    ;;(rt.po.app/navigate-to "Home" )
    ;;(rt.po.app/navigate-to-item "Home" "r1")
    ;;(rt.po.report-view/select-row-by-text "Test 01")
    ;; (rt.po.report-view/right-click-row-by-text "Test 01")
    ;; (rt.po.app/choose-context-menu "View")
    ;; (rt.po.edit-form/edit-button-exists?)
    ;; (rt.po.edit-form/back-button-exists?)

    (defn app-tile-exist? [app-name]
      (let [q (element (str ".app-launcher-tile img[alt='" app-name "']"))]
        (exists-present? q)))

    ;; open AA_All Fields report
    (rt.po.app/navigate-to "Test Solution")
    (rt.po.app/open-nav-tree-node "Test Solution")
    ;(rt.po.app/open-nav-tree-node "1A")
    (rt.po.app/open-nav-tree-node "AA_Reports")
    (rt.po.app/open-nav-tree-node "AA_All Fields")

    (rt.po.report-view/select-row-by-text "Test 01")
    (rt.po.report-view/right-click-row-by-text "Test 01")
    (rt.po.app/choose-context-menu "View")
    ;(rt.po.edit-form/click-edit-button)
    ;(rt.po.edit-form/field-multi-line? "Single Line")
    (rt.po.edit-form/field-multiline? "Description")
    ;(rt.po.edit-form/field-mandatory-indicator-visible? "Single Line")
    (rt.po.edit-form/click-multiline-expander "Description")
    (rt.po.edit-form/get-modal-multiline)
    ;(rt.po.app/choose-modal-ok)
    ;(str " :focus")
    ;field-has-focus?
    ;(flash (str " :focus"))
    ;(rt.po.view-form/get-lookup-title "AA_Drinks")
    ;(rt.po.view-form/get-lookup-link "AA_Drinks")
    ;;(rt.po.view-form/lookup-link-exists? "AA_Drinks")
    ;(rt.po.edit-form/button-exists? "Edit")
    ;; (rt.po.edit-form/field-exists? "AA_Drinks")
    ;(rt.po.common/get-lookup "AA_Drinks")
    ;(rt.po.common/clear-lookup "AA_Drinks")
    ;(rt.po.common/get-lookup "AA_Drinks")
    ;(rt.po.edit-form/open-lookup "AA_Employee")    ;
    ;(rt.po.edit-form/click-new-button)
    ;(rt.po.edit-form/click-new-button)
    ;(rt.po.edit-form/create-option-visible? "AA_Manager")
    ;(rt.po.edit-form/click-create-option "AA_Manager")
    ;(rt.po.common/find-field "AA_Drinks")
    ;(rt.po.edit-form/get-lookup "AA_Drinks")
    ;;(rt.po.edit-form/clear-lookup "AA_Drinks")
    ;;(rt.po.edit-form/form-title-exists?)
    ;;(rt.po.edit-form/get-form-title)
    ;; (rt.po.edit-form/click-lookup-link "AA_Drinks")
    ;;(rt.po.common/field-visible? "Multiline")
    ;(rt.po.edit-form/open-lookup "AA_Drinks")
    ;(rt.po.edit-form/string-field-value "Name")
    ;(rt.po.edit-form/click-edit-button)
    ;(rt.po.edit-form/string-field-value "Name")
    ;(rt.po.edit-form/set-string-field-value "Name" "Anurag Test 1")
    ;(rt.po.edit-form/click-cancel-button)
    ;(rt.po.edit-form/page-dirty-check-exists?)
    ;(rt.po.edit-form/page-dirty-cancel-button-exists?)
    ;(rt.po.edit-form/page-dirty-continue-button-exists?)
    ;(rt.po.edit-form/click-page-dirty-cancel-button)
    ;(rt.po.edit-form/click-cancel-button)
    ;(rt.po.edit-form/click-page-dirty-continue-button)
    ; todo: click calander of a field
    ;;(rt.po.edit-form/back-button-exists?)
    ;;(rt.po.edit-form/find-button "Back")
    ;;(rt.po.report-view/open-action-menu)
  )


  )
