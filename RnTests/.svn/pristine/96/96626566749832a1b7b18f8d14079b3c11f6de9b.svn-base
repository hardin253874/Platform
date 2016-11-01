(ns rt.scratch.alex
  (:require [clj-webdriver.core :refer [->actions move-to-element move-by-offset click-and-hold release]]
            [clj-webdriver.taxi :as taxi]            
            [clojure.repl :refer :all]
            [clojure.pprint :refer [pprint pp print-table]]
            [clojure.string :as string]
            [clojure.walk :as walk]
            [clojure.data.json :as json]
            [datomic.api :as d]
            [clj-time.core :as t]
            [clj-time.coerce :as tc]
            [rt.lib.wd :as wd]
            [rt.lib.wd-ng :as wd-ng]
            [rt.test.core :refer [*tc* *test-context*]]
            [rt.test.expects :refer :all]
            [clojure.pprint :as pp]))

(comment

  (* 2 2)

  )

(comment

  ;; Creating new instance on iPad/Tablet using Student report
  (do
    ;; NORMAL
    (alter-var-root (var *tc*)
                    (constantly (merge {:tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                       {:target :chrome})))

    ;; MOBILE
    ;(start-browser :chrome {:device "Apple iPhone 5" :width 400 :height 800})
    (alter-var-root (var *tc*)
                    (constantly (merge {:tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                       {:target :chrome}
                                       {:target-device "Apple iPhone 5"
                                        :target-width  400
                                        :target-height 800})))

    ;; TABLET
    ;(start-browser :chrome {:device "Apple iPad 3 / 4" :width 1024 :height 768})
    ;(app/start-app)
    ;(app/login)
    (alter-var-root (var *tc*)
                    (constantly (merge {:tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                       {:target :chrome}
                                       {:target-device "Apple iPad"
                                        :target-width  1024
                                        :target-height 768})))


    (clojure.pprint/pprint *tc*)

    (rn.common/start-app-and-login)

    (rt.app/setup-environment)
    
    ; driver for boards
    (do
      (rt.po.app/navigate-to-item "Test Solution" "")
      (rt.po.app/enable-config-mode)
      (rt.po.app/add-board "")
      (expect-equals "New Board" (rt.po.common/get-string "Name"))
      (rt.po.common/set-string "Name" "Elements!")
      (rt.po.common/set-multiline "Description" "ReadiTest generated periodic board of elements.")
      (rt.po.common/set-lookup "Report" "Elements (All Fields)")
      (expect-equals true (rt.po.common/get-bool "Show quick add"))
      (expect-equals false (rt.po.common/options-expanded?))
      (rt.po.common/options-expand)
      (expect-equals "Test Solution" (rt.po.common/get-lookup "Applications"))
      (rt.po.common/click-tab-heading "Format")
      (expect-equals "" (rt.po.common/get-lookup "Icon"))
      (rt.po.common/set-lookup "Icon" "Calendar.svg")
      (rt.po.common/click-ok)
      (expect-equals "Elements!" (rt.po.board/get-board-header-text))      
      (expect-equals "" (rt.po.board/get-quick-add))
      (expect-equals true (rt.po.board/board-header-icon-exist?))
      (expect-equals {:r 82, :g 135, :b 202, :a 255} (rt.po.board/get-board-header-icon-background-colour))      
      (rt.po.app/disable-config-mode)
      (expect-equals false (rt.po.board/settings-open?))
      (rt.po.board/click-settings)
      (expect-equals true (rt.po.board/settings-open?))
      (expect-equals "Name & description" (rt.po.board/get-card-template))
      (expect-equals "Standard state" (rt.po.board/get-column-source))
      (expect-equals "" (rt.po.board/get-row-source))
      (expect-equals "Bonding type" (rt.po.board/get-color-source))
      (rt.po.board/set-column-source "Metal type")
      (rt.po.board/set-row-source "Bonding type")
      (rt.po.board/set-color-source "Standard state")
      (expect-equals "Metal type" (rt.po.board/get-column-source))
      (expect-equals "Bonding type" (rt.po.board/get-row-source))
      (expect-equals "Standard state" (rt.po.board/get-color-source))
      (rt.po.board/set-row-source-value "Undefined" false)
      (expect-equals false (rt.po.board/row-source-value-checked? "Undefined"))
      (rt.po.board/set-color-source-value "Undefined" false)
      (expect-equals false (rt.po.board/color-source-value-checked? "Undefined"))
      (rt.po.board/click-settings-dialog-save)
      (rt.po.board/click-refresh)
      (expect-equals 3 (count (rt.po.board/get-legend)))
      (expect-equals true (rt.po.board/legend-contains? "Solid"))
      (expect-equals true (rt.po.board/legend-contains? "Liquid"))
      (expect-equals true (rt.po.board/legend-contains? "Gas"))
      (expect-equals 86 (rt.po.board/get-legend-item-count "Solid"))
      (expect-equals 2 (rt.po.board/get-legend-item-count "Liquid"))
      (expect-equals 11 (rt.po.board/get-legend-item-count "Gas"))
      (expect-equals 10 (rt.po.board/get-column-count))
      (expect-equals 4 (count (rt.po.board/get-rows)))
      (rt.po.board/scroll-reset)
      (expect-equals 6 (rt.po.board/get-column-item-count "Halogen"))
      (expect-equals 5 (count (rt.po.board/get-card-elements "Halogen")))
      (expect-equals 4 (count (rt.po.board/get-card-elements-in-row "Halogen" "Covalent network")))
      (expect-equals 0 (count (rt.po.board/get-card-elements-in-row "Actinoid" "Atomic")))
      (rt.po.board/set-quick-add "Engelhardtium")
      (rt.po.board/click-quick-add)
      (expect-equals 1 (count (rt.po.board/get-card-elements-in-row "Actinoid" "Atomic")))
      (expect-equals true (rt.po.board/card-exists? "Actinoid" "Engelhardtium"))
      (expect-equals true (rt.po.board/card-exists-in-row? "Atomic" "Actinoid" "Engelhardtium"))
      (expect-equals 1 (count (rt.po.board/get-card-elements-in-row "Nonmetal" "Metallic")))
      (expect-equals false (rt.po.board/card-exists-in-row? "Metallic" "Nonmetal" "Engelhardtium"))
      (expect-equals true (rt.po.board/card-exists-in-row? "Metallic" "Nonmetal" "Selenium"))
      (rt.po.board/drag-card-to-dimension-and-row "Engelhardtium" "Actinoid" "Nonmetal" "Atomic" "Metallic")
      (expect-equals 0 (count (rt.po.board/get-card-elements-in-row "Actinoid" "Atomic")))
      (expect-equals false (rt.po.board/card-exists? "Actinoid" "Engelhardtium"))
      (expect-equals false (rt.po.board/card-exists-in-row? "Atomic" "Actinoid" "Engelhardtium"))
      (expect-equals 2 (count (rt.po.board/get-card-elements-in-row "Nonmetal" "Metallic")))
      (expect-equals true (rt.po.board/card-exists-in-row? "Metallic" "Nonmetal" "Engelhardtium"))
      (expect-equals true (rt.po.board/card-exists-in-row? "Metallic" "Nonmetal" "Selenium"))      
      (rt.po.board/right-click-card-in-row "Metallic" "Nonmetal" "Engelhardtium")
      (rt.po.app/choose-context-menu "Delete")
      (rt.po.edit-form/click-confirm-delete-ok-button)
      (rt.po.app/enable-config-mode)
      (rt.po.app/delete-nav-item "Elements!")
      (rt.po.app/disable-config-mode)

      
      
      (rt.po.app/navigate-to-item "Test Solution" "")
      (rt.po.app/enable-config-mode)
      (rt.po.app/add-board "")
      (expect-equals "New Board" (rt.po.common/get-string "Name"))
      (rt.po.common/set-string "Name" "Science!")
      (rt.po.common/set-multiline "Description" "ReadiTest generated board... of Science!")
      (rt.po.common/set-lookup "Report" "Scientists")
      (expect-equals true (rt.po.common/get-bool "Show quick add"))
      (expect-equals false (rt.po.common/options-expanded?))
      (rt.po.common/options-expand)
      (expect-equals "Test Solution" (rt.po.common/get-lookup "Applications"))
      (rt.po.common/click-tab-heading "Deploy")
      (expect-equals true (rt.po.common/enabled-on-desktop?))
      (expect-equals false (rt.po.common/enabled-on-tablet?))
      (expect-equals false (rt.po.common/enabled-on-mobile?))
      (rt.po.common/click-tablet)
      (rt.po.common/click-mobile)      
      (rt.po.common/click-ok)
      (expect-equals "Science!" (rt.po.board/get-board-header-text))      
      (expect-equals "" (rt.po.board/get-quick-add))
      (rt.po.app/disable-config-mode)
      (rt.po.board/set-quick-add "Blaise Pascal")
      (rt.po.board/click-quick-add)      
      (rt.po.board/set-search "Pascal")
      (rt.po.board/clear-search)
      (expect-equals true (rt.po.board/card-exists? "Mathematics" "Blaise Pascal"))
      (expect-equals false (rt.po.board/settings-open?))
      (rt.po.board/click-settings)
      (expect-equals true (rt.po.board/settings-open?))
      (rt.po.board/click-settings-dialog-close)
      (rt.po.board/click-settings)
      (expect-equals "Name & description" (rt.po.board/get-card-template))
      (rt.po.board/set-card-template "All values & labels")
      (expect-equals "Discipline" (rt.po.board/get-column-source))      
      (expect-equals false (rt.po.board/column-source-value-checked? "Undefined"))
      (expect-equals true (rt.po.board/column-source-value-checked? "Physics"))
      (expect-equals true (rt.po.board/column-source-value-checked? "Psychology"))
      (rt.po.board/set-column-source-value "Biology" false)
      (expect-equals "" (rt.po.board/get-row-source))      
      (expect-equals "" (rt.po.board/get-color-source))
      (rt.po.board/set-color-source "Discipline")
      (expect-equals false (rt.po.board/color-source-value-checked? "Undefined"))
      (expect-equals true (rt.po.board/color-source-value-checked? "Physics"))
      (expect-equals false (rt.po.board/color-source-value-checked? "Biology"))
      (rt.po.board/set-color-source-value "Psychology" false)
      (expect-equals false (rt.po.board/column-source-value-checked? "Psychology"))
      (rt.po.board/click-settings-dialog-save)
      (rt.po.board/click-refresh)
      (expect-equals 10 (count (rt.po.board/get-legend)))
      (expect-equals true (rt.po.board/legend-contains? "Astronomy"))
      (expect-equals 3 (rt.po.board/get-legend-item-count "Inventor"))
      (expect-equals 10 (rt.po.board/get-column-count))
      (expect-equals true (rt.po.board/column-exists? "Climatology"))
      (expect-equals 1 (count (rt.po.board/get-rows)))
      (rt.po.board/scroll-reset)
      (expect-equals 7 (rt.po.board/get-column-item-count "Physics"))
      (expect-equals 7 (count (rt.po.board/get-card-elements "Physics")))
      
      (rt.po.board/right-click-card "Mathematics" "Mary Somerville")
      (rt.po.app/choose-context-menu "Edit")
      (rt.po.edit-form/cancel)
      (rt.po.board/click-card-link "Mathematics" "Blaise Pascal")
      (rt.po.edit-form/click-back-button)
      
      
      
      (rt.po.board/drag-card-to-dimension "Blaise Pascal" "Mathematics" "Astronomy")
      (rt.po.board/drag-card-to-legend "Astronomy" "Blaise Pascal" "Inventor")
      (rt.po.board/drag-legend-to-card "Botany" "Inventor" "Blaise Pascal")
      
      (rt.po.board/right-click-card "Botany" "Blaise Pascal")
      (rt.po.app/choose-context-menu "Delete")
      (rt.po.edit-form/click-confirm-delete-ok-button)
      (rt.po.app/enable-config-mode)
      (rt.po.app/delete-nav-item "Science!")
      (rt.po.app/disable-config-mode)

      ;(rt.po.common/exists-present? "button[title='Refresh']")
      )
    
    ; driver for security tabs
    (do
      (rn.app/open-security "User Roles")
      (rt.po.report-view/set-search-text "Foster Common")
      (rt.po.report-view/choose-report-row-action "Foster Common" "View")
      (rt.po.view-form/select-form-tab "Record Access")
      (expect-equals true (rt.po.access-rules/row-exists? "University" "View" "University"))
      (expect-equals "University" (:query (rt.po.access-rules/get-row "University" "View" "Uni")))
      (expect-equals false (rt.po.access-rules/exact-row-exists? "University" "View" "Uni"))
      (rt.po.access-rules/add-new-access-rule)
      (rt.po.access-rules-new/set-object "University")
      (rt.po.access-rules-new/click-ok)
      (rt.po.edit-form/click-edit-button)
      (rt.po.access-rules/double-click-row (rt.po.access-rules/get-last-row "University" "View" "University"))
      (rt.po.report-builder/rename "Uni")
      (rt.po.report-builder/save)
      (rt.po.report-builder/close)
      (expect-equals true (rt.po.access-rules/exact-row-exists? "University" "View" "University"))
      (expect-equals true (rt.po.access-rules/exact-row-exists? "University" "View" "Uni"))
      (expect-equals false (rt.po.access-rules/row-enabled? (rt.po.access-rules/get-last-exact-row "University" "View" "Uni")))
      (rt.po.access-rules/set-row-enabled (rt.po.access-rules/get-last-exact-row "University" "View" "Uni") true)
      (rt.po.access-rules/set-row-operation (rt.po.access-rules/get-last-exact-row "University" "View" "Uni") "Full (Create, View, Edit and Delete)")
      (rt.po.edit-form/click-save-button)
      (expect-equals false (rt.po.app/page-has-error-alert?))
      (expect-equals true (rt.po.access-rules/row-exists? "University" "" "University"))
      (expect-equals true (rt.po.access-rules/exact-row-exists? "University" "Full (Create, View, Edit and Delete)" "Uni"))
      (expect-equals true (rt.po.access-rules/row-enabled? (rt.po.access-rules/get-exact-row "University" "Full (Create, View, Edit and Delete)" "Uni")))
      (expect-equals "Full (Create, View, Edit and Delete)" (:permissions (rt.po.access-rules/get-last-row "University" "" "Uni")))
      (rt.po.access-rules/right-click-row (rt.po.access-rules/get-last-exact-row "University" "View" "Uni"))
      (rt.po.app/choose-context-menu "Delete")
      (rt.po.edit-form/click-confirm-delete-ok-button)
      (expect-equals false (rt.po.app/page-has-error-alert?))
      (expect-equals true (rt.po.access-rules/exact-row-exists? "University" "View" "University"))
      (expect-equals false (rt.po.access-rules/exact-row-exists? "University" "Full (Create, View, Edit and Delete)" "Uni"))
      (rt.po.view-form/select-form-tab "Navigation Access")
      (rt.po.access-rules/set-application "Foster University")
      (expect-equals false (rt.po.access-rules/node-checked? "Staff screen - all three"))
      (rt.po.edit-form/click-edit-button)
      (rt.po.access-rules/set-node-value "Staff screen - all three" true)
      (rt.po.edit-form/click-save-button)
      (expect-equals true (rt.po.access-rules/node-checked? "Staff screen - all three"))
      (rt.po.edit-form/click-edit-button)
      (rt.po.access-rules/set-node-value "Staff screen - all three" false)
      (rt.po.edit-form/click-save-button)
      (expect-equals false (rt.po.app/page-has-error-alert?))
      

      )
    
    ; driver for survey
    (do
      (rt.po.app/navigate-to-item "Administration" "Surveys")
      (rt.po.app/select-non-toggle-navigator-item "Surveys" 1)
      (rt.po.report-view/right-click-row-by-text "Application survey")
      (rt.po.app/choose-context-menu "View")
      (rt.po.view-form/select-form-tab "Sections")
      
      (rt.po.view-form/click-edit)
      (rt.po.survey/click-question-allows-notes "AAA" "What is your?")
      (rt.po.survey/click-question-allows-attachments "AAA" "What is your?"))
    
    ; driver for workflow run input
    (do
      (rt.po.workflow-user-input/set-workflow-input-text "InputString" "Foo")
      (rt.po.workflow-user-input/set-workflow-input-text "InputGuid" "9ECEE6D8-DBA2-4D07-AFFC-2A7EF25C323F")
      (rt.po.workflow-user-input/set-workflow-input-text "InputCurrency" "4.56")
      (rt.po.workflow-user-input/set-workflow-input-text "InputDecimal" "1.230")
      (rt.po.workflow-user-input/set-workflow-input-text "InputNumber" "67")      
      (rt.po.workflow-user-input/set-workflow-input-text "InputDate" "24/10/1977")
      (rt.po.workflow-user-input/set-workflow-input-record "InputRecord" "German Shepherd")
      (rt.po.workflow-user-input/set-workflow-input-record "InputRecordList" "Aero")      
      (rt.po.workflow-user-input/set-workflow-input-checkbox "InputBool" false)
      (rt.po.workflow-user-input/set-workflow-input-time "VarDateTime" "2" "40" "PM")
      (rt.po.workflow-user-input/set-workflow-input-date "VarDate" "24/10/1977")
      
      (rt.po.workflow-user-input/wait-for-message "Prompt 1")
      (rt.po.workflow-user-input/get-workflow-input-text "InputString")
      
      
        
      ;(let [row (filter #(.contains "InputRecord" (taxi/text %)) rows)]
      ;(clojure.pprint/pprint row)
      ;(let [i (taxi/find-element-under row {:css ".workflow-input-field"})])
      ;(filter #(= (taxi/text %) "InputRecord") rows)
      ;(set-run-input-select "InputRecord" "Australian Cattle Dog")
      )
    

    (do
      (rt.po.app/navigate-via-tiles "Foster University")
      (rt.po.app/enable-app-toolbox )
      (rt.po.app-toolbox/create-object {:name "New Test Object3" :description "New Test Object Description"})
      (rt.po.form-builder/add-display-option-from-toolbox-to-form "Tabbed Container")
      ;(rt.po.form-builder/add-field-to-form "Choice")
      (rt.po.form-builder/open-add-field-menu)
      (rt.lib.wd-rn/drag-n-drop (rt.po.form-builder/get-field-menu-item-element "Choice") (rt.po.form-builder/get-default-form-drop-target))
      (rt.po.form-builder/handle-choice-field-form)
      (rt.lib.wd/set-input-value (first (taxi/elements (rt.po.edit-form/string-field "Name"))) "New 'Choice' field")
      (rt.lib.wd/set-input-value (second (taxi/elements (rt.po.edit-form/string-field "Name"))) "New 'Choice' field")
      (rt.po.form-builder/set-choice-values-in-choice-dialog ["Option 1"])
      (rt.po.common/click-modal-dialog-button-and-wait ".modal-footer button[ng-click*=ok]")
      )


    ; Report and Form on Screen for Mobile and iPad.mp4 (tablet)
    (do
      (rn.tablet.app/navigate-to-app-launcher)
      (rn.tablet.app/navigate-to-application "Foster University")
      (rn.tablet.app/open-navigator)
      (rn.tablet.app/navigate-to-item nil "Student screen - form and report")
      (rt.test.expects/expect-equals true (rn.tablet.screen/is-report-on-screen "Student report"))
      (rt.test.expects/expect-equals true (rn.tablet.screen/is-form-on-screen "Student"))
      (rn.tablet.screen/expect-report-row-count-in-screen 9 "Student report")
      (rn.tablet.screen/expect-report-column-count-in-screen 8 "Student report")
      (rn.tablet.report/sort-column-descending "Student")
      (rn.tablet.report/scroll-to-last-record)
      (rn.tablet.report/expect-row-by-text "Abraham Mcdonald")
      ;(rn.tablet.report/select-row-by-text "Abraham Mcdonald")
      (rn.tablet.report/double-click-row-by-text "Abraham Mcdonald")
      (rt.test.expects/expect-equals (rn.tablet.form/get-field-value "StudentID") "1136")
      (rt.test.expects/expect-equals (rn.tablet.form/get-field-value "Title") "Mr.")
      (rt.test.expects/expect-equals (rn.tablet.form/get-field-value "Full name") "Abraham Mcdonald")
      (rn.tablet.form/form-nav-back)
      (rn.tablet.app/navigate-to-app-launcher)
      (rn.tablet.app/logout))

    ; Report and Form on Screen for Mobile and iPad.mp4 (mobile)
    (do
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Foster University")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Student screen - form and report")
      (rt.test.expects/expect-equals true (rn.mobile.screen/is-report-on-screen "Student report"))
      (rt.test.expects/expect-equals false (rn.mobile.screen/is-form-on-screen "Student"))
      (rn.mobile.screen/expect-report-row-count-in-screen 6 "Student report")
      (rn.mobile.screen/expect-report-column-count-in-screen 3 "Student report")
      (rn.mobile.screen/view-full-report)
      (rn.mobile.report/expect-report-row-count 20)
      (rn.mobile.report/expect-report-column-count 3)
      (rn.mobile.report/sort-column-descending "Student")
      (rn.mobile.report/scroll-to-last-record)
      (rn.mobile.report/expect-row-by-text "Abraham Mcdonald")
      (rn.mobile.report/double-click-row-by-text "Abraham Mcdonald")
      (rn.mobile.form/expect-form-title "Abraham Mcdonald")
      (rn.mobile.form/back)
      (rn.mobile.report/expect-report-column-count 3)
      (rn.mobile.report/back)
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/logout))

    ; Modify existing instance on Mobile.mp4
    (do
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Foster University")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Student report")
      (if (rn.mobile.app/navigator-open?) (rn.mobile.app/close-navigator))
      (rn.mobile.report/expect-row-by-text "1221")
      (rn.mobile.report/double-click-row-by-text "1221")
      (rn.mobile.form/expect-form-title "Yoshi Joseph")     ; ADD!
      (rn.mobile.form/edit)
      (rn.mobile.form/set-choice-value "Title" "Mrs.")
      (rn.mobile.form/set-text-field-value "Full name" "John Mathew")
      (rn.mobile.form/set-date-field-value "DOB" "10/07/1988") ; October 7th
      ;(rn.mobile.form/set-lookup "User Account" "Adam.Foster")
      (rn.mobile.form/clear-lookup "User Account")
      (rn.mobile.form/set-multi-select-choice-value "Club" "Sports" false)
      (rn.mobile.form/set-multi-select-choice-value "Club" "Dance" false)
      (rn.mobile.form/set-multi-select-choice-value "Club" "Chess")
      (rn.mobile.form/set-number-field-value "Balance" 67.70)
      (rn.mobile.form/select-page 4)
      (rn.mobile.report/click-add)
      (rn.mobile.report/choose-picker-row "C10066")
      (rn.mobile.report/close-picker-ok)
      (when (= (rn.mobile.report/count-report-rows) 2)
        (rn.mobile.report/right-click-row-by-text "C10069")
        (rn.mobile.report/choose-context-menu "Remove Link"))
      (rn.mobile.form/save)
      (rn.mobile.form/back)
      ;; Verify the details just saved
      (rn.mobile.report/expect-row-by-text "1221")
      (rn.mobile.report/double-click-row-by-text "1221")
      (rn.mobile.form/expect-form-title "John Mathew")      ; ADD!
      (rt.test.expects/expect-equals (rn.mobile.form/get-field-value "Title") "Mrs.")
      (rt.test.expects/expect-equals (rn.mobile.form/get-field-value "Full name") "John Mathew")
      (rt.test.expects/expect-equals (rn.mobile.form/get-field-value "DOB") "10/7/1988")
      (rt.test.expects/expect-equals (rn.mobile.form/get-field-value "User Account") "")
      (rt.test.expects/expect-equals (rn.mobile.form/get-field-value "Club") "Chess")
      (rt.test.expects/expect-equals (rn.mobile.form/get-field-value "Balance") "$ 67.70")
      (rn.mobile.form/select-page 4)
      (rt.test.expects/expect-equals (rn.mobile.report/count-report-rows) 1)
      (rt.test.expects/expect-equals (rn.mobile.report/get-report-cell-text-content 0 "Qualification Code") "C10066")
      ;; Undo... in reverse!!!
      (rn.mobile.form/edit)
      (rn.mobile.report/click-add)
      (rn.mobile.report/choose-picker-row "C10069")
      (rn.mobile.report/close-picker-ok)
      (when (= (rn.mobile.report/count-report-rows) 2)
        (rn.mobile.report/right-click-row-by-text "C10066")
        (rn.mobile.report/choose-context-menu "Remove Link"))
      (rn.mobile.form/select-page 1)
      (rn.mobile.form/set-number-field-value "Balance" 237.5)
      (rn.mobile.form/set-multi-select-choice-value "Club" "Sports")
      (rn.mobile.form/set-multi-select-choice-value "Club" "Dance")
      (rn.mobile.form/set-multi-select-choice-value "Club" "Chess" false)
      (rn.mobile.form/set-lookup "User Account" "Yoshi.Joseph")
      (rn.mobile.form/set-date-field-value "DOB" "10/13/1988") ; October 13th
      (rn.mobile.form/set-text-field-value "Full name" "Yoshi Joseph")
      (rn.mobile.form/set-choice-value "Title" "Mr.")
      (rn.mobile.form/save)
      (rn.mobile.form/back)
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/logout))

    ; General Navigation on Mobile.mp4
    (do
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Foster University")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Staff Report")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Student Report")
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Test Solution")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Area - NSW Population")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Bar - Internet Access")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "1. Scientists Screen")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "2. Dog Breeds")
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Shared")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "People")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Activities")
      (rn.mobile.report/expect-report-column-count 3)
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Approvals")
      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/logout))

    ; Create new instance on Ipad.mp4
    (do
      (rn.tablet.app/navigate-to-app-launcher)
      (rn.tablet.app/navigate-to-application "Foster University")
      (rn.tablet.app/open-navigator)
      (rn.tablet.app/navigate-to-item nil "Student Report")
      (rn.tablet.report/click-new)
      (rn.tablet.form/set-choice-value "Title" "Mrs.")
      (rn.tablet.form/set-text-field-value "Full name" "Marie John")
      (expect-equals "M/d/yyyy" (rn.mobile.form/get-date-field-placeholder "DOB"))
      (rn.tablet.form/set-date-field-value "DOB" "2/23/2002")
      (comment "getting the date in the mobile emulation of a date input returns universal format")
      (expect-equals "2002-02-23" (rn.mobile.form/get-date-field-value "DOB"))
      (rn.tablet.form/set-lookup "User Account" "Amery.Palmer")
      (rn.tablet.form/set-multi-select-choice-value "Club" "Dance")
      (rn.tablet.form/set-multi-select-choice-value "Club" "Drama")
      (comment "*** TODO select Babbage for the image field ***")
      (expect-match #"Dance" (rn.mobile.form/get-multi-select-choice-value "Club"))
      (expect-match #"Drama" (rn.mobile.form/get-multi-select-choice-value "Club"))
      (rn.tablet.form/set-number-field-value "Balance" 67.7)
      (expect-equals 67.7 (rn.tablet.form/get-number-field-value "Balance")))



    ;; OLDER STUFF

    ;(rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (let [fu (element ".app-launcher-tile:contains(Foster University)")]
      (click fu))

    (rt.po.report-view/open-new-menu)
    (rt.po.edit-form/set-choice-value "Title" "Mrs.")
    (rt.po.edit-form/set-text-field-value "Full name" "marie john")
    ;(rt.po.edit-form/set-date-field-value "DOB" (tc/to-date (t/date-time 2002 02 06))) ; Feb 6th
    ;(rt.po.edit-form/set-lookup-value "User Account" "Amery.Palmer")

    ;; Select an image. Add to driver?
    (let [el (rt.po.edit-form/get-field-control-element "Photo")]
      (clojure.pprint/pprint el)
      (click (find-element-under el {:css "button[name=contextMenuButton"}))
      (wait-until #(rt.po.common/exists-present? ".menuItem:contains(Find Existing)") 200)
      (click (str ".menuItem:contains(Find Existing)"))
      (rt.po.edit-form/choose-in-entity-picker-dialog "babbage"))

    (rt.po.edit-form/select-multi-select-choice-value "Club" "Dance")
    (rt.po.edit-form/select-multi-select-choice-value "Club" "Drama")
    (rt.po.edit-form/set-number-field-value "Balance" "67.70")

    ;; Add a new subject
    (do
      (rt.po.view-form/select-form-tab "Subjects")
      (let [tab (element ".tab-pane.active")]

        (clojure.pprint/pprint tab)
        (let [el (last (find-elements-under tab {:xpath "//button[@title='New']"}))]
          (clojure.pprint/pprint el)
          (click el)))

      (rt.po.edit-form/set-text-field-value "Subject name" "Diploma in nursing")
      (rt.po.edit-form/click-save-button))

    ;; Add an existing subject
    (do
      (rt.po.view-form/select-form-tab "Subjects")
      (let [tab (element ".tab-pane.active")]
        (let [el (find-element-under tab {:xpath "//button[@title='Add Existing']"})]
          (click el))))
    (rt.po.edit-form/choose-in-entity-picker-dialog "41062")

    ;; Save and validate
    (rt.po.edit-form/click-save-button)

    ;; Clean up
    (rt.po.report-view/delete-record "marie john")
    (rt.po.report-view/set-search-text "")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Subjects Report")
    (rt.po.report-view/delete-record "Diploma in nursing")
    (rt.po.report-view/set-search-text "")

    ;; OTHER PEOPLE'S STUFF

    (rn.common/start-app-and-login)

    ; Sri 2
    (do
      ;(identity {:object-name (rt.lib.util/make-test-name "RT-Choice-Test-Object")})
      (rt.po.app/enable-app-toolbox)
      (rt.po.common/wait-until (taxi/exists? ".fb-screen-add-button") 10000)
      (rt.po.app-toolbox/create-object {:name "foo"})
      (rt.po.form-builder/add-container-to-form)
      (rt.po.form-builder/add-field-to-form "Choice")
      (rt.po.form-builder/run-field-properties-dialog "Choice" {:name "Choice 1" :choice-values ["Monday" "Tuesday" "Wednesday"]})
      (rt.po.form-builder/save)
      (rt.po.form-builder/show-field-properties-dialog "Choice")
      (expect-equals #{"Monday" "Tuesday" "Wednesday"} (set (rt.po.form-builder/get-choice-values)))
      (rt.po.common/click-modal-dialog-button-and-wait ".modal-footer button[ng-click*='cancel']")
      (rt.po.form-builder/close)
      (rt.po.app/disable-app-toolbox)
      (rt.po.app/disable-config-mode)
      (rn.common/clear-all-alerts))

    ; Sri
    (do
      (rt.po.app/navigate-to-item "Home" "")
      (rt.po.app/enable-config-mode)
      (rt.po.app/add-new-nav-item "New Object" "")
      (rt.po.common/set-string "Object name" "_choice_mandatory_object")
      (rt.po.common/click-ok)
      (expect-equals "_choice_mandatory_object Form" (rt.po.form-builder/get-name))
      (rt.po.form-builder/add-field-from-toolbox-to-form "Name")
      (rt.po.form-builder/add-from-field-menu-to-form "Choice")
      (rt.scripts.common/sleep 3000)
      (rt.po.form-builder-config/set-name "New choice")
      (rt.po.form-builder-config/add-new-choice-value)
      (rt.po.form-builder-config/add-new-choice-value)
      (rt.po.form-builder-config/add-new-choice-value)
      (rt.po.form-builder-config/add-new-choice-value)
      (rt.po.form-builder-config/set-choice-value "New Value" "Value 1")
      (rt.po.form-builder-config/set-choice-value "New Value1" "Value 2")
      (rt.po.form-builder-config/set-choice-value "New Value2" "Value 3")
      (rt.po.form-builder-config/set-choice-value "New Value3" "Value 4")
      (rt.po.form-builder-config/expand-section "Options")
      (rt.po.form-builder-config/set-control-mandatory true)
      (rt.po.form-builder-config/click-ok)
      (rt.po.form-builder/save)
      (rt.po.form-builder/close)
      (rt.po.report-view/expect-report-visible "_choice_mandatory_object Report")
      (expect-equals true (rt.po.report-view/report-visible? "_choice_mandatory_object Report"))
      (rt.po.report-view/open-new-menu)
      (expect-equals true (rt.po.edit-form/field-mandatory-indicator-visible? "New choice"))
      (rt.po.edit-form/set-text-field-value "Name" "test choice1")
      (rt.po.edit-form/save)
      (expect-equals "A value is required." (rt.po.edit-form/field-error-message "New choice"))
      (rt.po.app/clear-alerts)
      (rt.po.edit-form/set-choice-value "New choice" "Value 2")
      (rt.po.edit-form/save)
      (expect-equals true (rt.po.report-view/report-visible? "_choice_mandatory_object Report"))
      (rt.po.report-view/view-record "test choice1")
      (expect-equals "test choice1" (rt.po.view-form/get-field-value "Name"))
      (expect-equals "Value 2" (rt.po.view-form/get-field-value "New choice"))
      (rt.po.view-form/click-edit)
      (rt.po.edit-form/set-choice-value "New choice" "[Select]")
      (rt.po.edit-form/save)
      (expect-equals "A value is required." (rt.po.edit-form/field-error-message "New choice"))
      (rt.po.app/clear-alerts)
      (rt.po.edit-form/set-choice-value "New choice" "Value 3")
      (rt.po.edit-form/save)
      (rt.po.view-form/form-nav-back)
      )
    )
  )

