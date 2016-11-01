(ns rt.scratch.kun
  (:require [clj-webdriver.taxi :refer :all]
            [clj-webdriver.core :refer [->actions move-to-element]]
            rt.scripts.qa-daily
            [rt.scripts.common :refer [start-app-and-login]]
            [rt.po.app :as app]
            [rt.po.app-toolbox :as tb]
            [rt.po.form-builder :as fb]
            [rt.po.report-builder :as rb]
            [rt.po.report-view :as rv]
            [rt.po.edit-form :as ef]
            [rt.test.core :refer :all]
            [rt.test.db :refer :all]
            [rt.lib.util :refer :all]
            [rt.lib.wd :refer :all]
            [rt.lib.wd-rn :refer :all]
            [rt.lib.wd-ng :refer :all]
            [rt.setup :refer :all]
            [rt.app :refer [setup-environment]]
            [clojure.repl :refer :all]
            [clojure.pprint :refer [pprint pp print-table]]
            [clojure.string :as string]
            [clojure.walk :as walk]
            [clojure.data.json :as json]
            [datomic.api :as d]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.test.core :refer [*tc* *test-context*]]
            [clj-webdriver.taxi :as taxi]
            [clj-time.core :as t]
            [clj-time.coerce :as tc]))
(comment

  (* 2 2),

  (println "hello")

  (defn myfn [] (println "hey"))
  (rt.app/setup-environment)
  (myfn)
  ; (set-input-value ".reportPropertyDialog-view [ng-model*=reportName]" "RT Report 1")
  ;  (set-input-value ".reportPropertyDialog-view [ng-model*=reportDesc]" "Report created by ReadiTest")
  ; (rt.po.app-toolbox/set-report-object "AA_All Fields")
  
  (clj-webdriver.taxi/find-element-under (clj-webdriver.taxi/element (rt.po.report-view/find-analyzer-field "User Account")) (clj-webdriver.core/by-css ".inlineRelPicker-button"))
  
  
  (rt.po.report-view/click-analyser-field-picker-report "User Account")
  
  
  (let [elem-str (str (rt.po.report-view/find-analyzer-field "User Account") "button.inlineRelPicker-button")]
           (clj-webdriver.taxi/element elem-str))
  
  (let [elem-str (clj-webdriver.taxi/find-element-under (clj-webdriver.taxi/element (rt.po.report-view/find-analyzer-field "User Account")) (clj-webdriver.core/by-css ".inlineRelPicker-button"))]
           (clj-webdriver.taxi/click elem-str))
  
  
  (rt.po.report-format/click-highlight-condition-picker-report 0)
  (rt.lib.wd/set-input-value ".entityReportPickerDialog .sp-search-control input" "Adam")
  
  (clj-webdriver.taxi/exists? ".structureItem:contains('Adam')")
  
  (if(clj-webdriver.taxi/exists? ".structureItem:contains('Adadm')")
          (clj-webdriver.taxi/click (clj-webdriver.taxi/element ".structureItem:contains('Adadm')")))
  
  
  
  (rt.po.report-view/select-row-by-text "Adam" ".entityReportPickerDialog .dataGrid-view")
  
  (clj-webdriver.taxi/element ".structureViewPickerNodes")
  
  
  (clj-webdriver.taxi/exists? (clj-webdriver.taxi/element ".structureViewPickerNodes"))
  
  
  (cond
    (= (clj-webdriver.taxi/exists? (clj-webdriver.taxi/element ".structureViewPickerNodes")) true) "Up"
    :else (rt.po.report-view/select-row-by-text "Adam" ".entityReportPickerDialog .dataGrid-view"))  
  
  (rt.po.report-format/set-highlight-conditions [{:oper "Any below"  :value "Adam Foster" :colour "Black on Red"}  {:oper "" :value "" :colour "Red text"}])
  (rt.po.report-view/get-format-icons-in-report "Tenure" "Red Cross Format Icon1")
  (clojure.core/contains? #{:a :b :c} :a)
  (contains? #{:a :b :c} :a)
  )

(comment

  (do
    (rt.scripts.qa-daily/get-default-data)
    (identity {:target :chrome})
    (rt.scripts.common/start-app-and-login)
    )
  )

(comment

  (do

    (setup-environment)

    (alter-var-root (var *test-context*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *test-context*)
    (start-app-and-login)
    (rt.po.app/navigate-to "Test Solution" )
    (rt.po.app/enable-config-mode)
    (rt.po.app/navigate-to-item "Test Solution" "Test Solution/AA_Automation/AA_AutomationReport")
    (click ".report-config-panel-button")
    (click (find-element {:tag :span, :text "Modify Report"}))

    )
  )

(comment
  (do

    (setup-environment)

    (alter-var-root (var *test-context*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *test-context*)
    (rt.scripts.common/start-app-and-login)
    (rt.po.app/navigate-to "Home" )
    (rt.po.app/enable-config-mode)
    (println (str "adding report using .client-nav-panel"))
    (rt.po.app/click-nav-builder-tool)
    (drag-n-drop (str ".nb-entry:contains('Report')") ".client-nav-panel")
    (rt.po.report-new/set-name (str "RT Report " (rand 10000)))
    (rt.po.report-new/set-description "RT Report Description")
    (rt.po.report-new/set-report-base-on "AA_All Fields")
    (rt.po.report-new/set-report-form "[Default]")
    (rt.po.report-new/set-icon "Black Circle Format Icon")
    (rt.po.report-new/set-style "Default")
    (rt.po.report-new/set-default-display-report true)
    (rt.po.report-new/set-default-display-report false)
    (rt.po.report-new/set-default-picker-report true)
    (rt.po.report-new/set-default-picker-report false)
    (rt.po.report-new/set-hide-action-bar true)
    (rt.po.report-new/set-hide-action-bar false)
    (rt.po.report-new/set-hide-report-header true)
    (rt.po.report-new/set-hide-report-header false)
    (rt.po.report-new/click-ok)
    (rt.po.report-builder/set-filter-field-name "Currency")
    (rt.po.report-builder/set-in-report-checkbox-field-value "Currency" true)
    (rt.po.report-builder/set-filter-field-name "DateTime")
    (rt.po.report-builder/set-in-report-checkbox-field-value "DateTime" true)
    (rt.po.report-builder/set-in-analyzer-checkbox-field-value "DateTime" true)
    (rt.po.report-builder/set-filter-field-name "")
    (rt.po.report-builder/click-add-relationship-botton)
    (rt.po.report-add-relationship/set-filter-relationship-name "AA_Herb")
    (rt.po.report-add-relationship/add-relationship "AA_Herb")
    (rt.po.report-add-relationship/add-relationship "AA_Herb")
    (rt.po.report-add-relationship/remove-relationship "AA_Herb")
    (rt.po.report-add-relationship/set-filter-relationship-name "AA_Truck")
    (rt.po.report-add-relationship/add-relationship "AA_Truck")
    (rt.po.report-add-relationship/set-filter-relationship-name "")
    (rt.po.report-add-relationship/set-type "Lookups")
    (rt.po.report-add-relationship/add-relationship "AA_Drinks")
    (rt.po.report-add-relationship/click-ok)
    (rt.po.report-builder/select-treenode "AA_Herb")
    (rt.po.report-builder/remove-relationship "AA_Herb")
    (rt.po.report-builder/select-treenode "AA_Truck")
    (rt.po.report-builder/click-advanced-botton)
    (rt.po.report-advanced/set-join-type "Record must exist")
    (rt.po.report-advanced/set-follow-recursion true)
    (rt.po.report-advanced/set-recursion "Include Self")
    (rt.po.report-advanced/click-ok)

    (rt.po.report-builder/click-advanced-botton)
    (rt.po.report-advanced/set-join-type "Automatic")
    (rt.po.report-advanced/set-follow-recursion false)
    (rt.po.report-advanced/click-ok)

    (rt.po.report-builder/select-treenode "AA_All Fields")
    (rt.po.report-builder/click-advanced-botton)
    (rt.po.report-advanced/set-exact-type true)
    (rt.po.report-advanced/click-ok)
    (rt.po.report-builder/click-calculation-botton)
    (rt.po.report-calculated/set-column-name "Calculated column 1")
    (rt.po.report-calculated/select-hint "Number")
    (rt.po.report-calculated/click-ok)
    (rt.po.report-builder/click-calculation-botton)
    (rt.po.report-calculated/set-column-name "Calculated column 2")
    (rt.po.report-calculated/set-calculation-script "[Number] * 2")
    (rt.po.report-calculated/click-ok)
    (rt.po.report-builder/click-summarise-botton)
    (rt.po.report-summarise/click-cancel)
    (rt.po.report-view/sort-column-by-click-header "" "AA_All Fields")
    (rt.po.report-builder/choose-column-menu-item "Calculated column 1" "Remove")
    (rt.po.report-builder/choose-column-menu-item "AA_All Fields" "Sort A-Z")
    (rt.po.report-builder/choose-column-menu-item "AA_All Fields" "Sort Z-A")
    (rt.po.report-builder/choose-column-menu-item "AA_All Fields" "Sort Options")
    (rt.po.report-sort-option/click-ok)
    (rt.po.report-builder/choose-column-menu-item "AA_All Fields" "Rename Column")
    (rt.po.report-builder/set-rename-column-name "AA_All Fields1")
    (rt.po.report-builder/click-ok)
    (rt.po.report-builder/choose-column-menu-item "DateTime" "Format Column")
    (rt.po.report-format/click-value-formatting)
    (rt.po.report-format/set-datetime-format "Day Month Time")
    (rt.po.report-format/click-conditional-formatting)

    )


  (comment

    (do

      (setup-environment)

      (alter-var-root (var *test-context*)
                      (constantly {:tenant   "EDC"
                                   :username "Administrator"
                                   :password "tacoT0wn"
                                   :target :chrome}))

      (clojure.pprint/pprint *test-context*)
      (rt.scripts.common/start-app-and-login)
      (rt.po.app/navigate-to "Home" )
      (rt.po.app/enable-config-mode)
      (println (str "adding report using .client-nav-panel"))
      (rt.po.app/click-nav-builder-tool)
      (drag-n-drop (str ".nb-entry:contains('Report')") ".client-nav-panel")
      (rt.po.report-new/set-name (str "RT Report " (rand 10000)))
      (rt.po.report-new/set-description "RT Report Description")
      (rt.po.report-new/set-report-base-on "AA_All Fields")
      (rt.po.report-new/set-report-form "[Default]")
      (rt.po.report-new/click-ok)
      (rt.po.report-builder/set-in-report-checkbox-field-value "DateTime" true)
      (rt.po.report-builder/choose-column-menu-item "DateTime" "Sort Options")
      (rt.po.report-sort-option/click-add-sorting)
      (rt.po.report-sort-option/click-add-sorting)
      ;(Thread/sleep 100)
      (rt.po.report-sort-option/set-sortings [{:colname "AA_All Fields"  :dir "Ascending"} {:colname "DateTime" :dir "Descending"}])
      ;;(rt.po.report-builder/click-save-report-botton)
      ;;(rt.po.report-builder/click-close-botton)
      ;(Thread/sleep 1000)
      (rt.po.report-sort-option/click-ok)
      ;;(rt.po.report-format/click-value-formatting)
      ;;(rt.po.report-format/set-datetime-format "Day Month Time")
      ;;(rt.po.report-format/click-conditional-formatting)
      ;;(rt.po.report-format/set-format-type "Highlight")
      ;;(rt.po.report-format/set-format-scheme "2 step - Red Highlight")
      ;;(rt.po.report-format/click-add-rule)
      )
    )

  (comment

    (do

      (setup-environment)

      (alter-var-root (var *test-context*)
                      (constantly {:tenant   "EDC"
                                   :username "Administrator"
                                   :password "tacoT0wn"
                                   :target :chrome}))

      (clojure.pprint/pprint *test-context*)
      (rt.scripts.common/start-app-and-login)
      (rt.po.app/navigate-to "Home" )
      (rt.po.app/enable-config-mode)
      (println (str "adding report using .client-nav-panel"))
      (rt.po.app/click-nav-builder-tool)
      (drag-n-drop (str ".nb-entry:contains('Report')") ".client-nav-panel")
      (rt.po.report-new/set-name (str "RT Report " (rand 10000)))
      (rt.po.report-new/set-description "RT Report  Description")
      (rt.po.report-new/set-report-base-on "AA_All Fields")
      (rt.po.report-new/set-report-form "[Default]")
      (rt.po.report-new/click-ok)
      (rt.po.report-builder/set-in-report-checkbox-field-value "Boolean" true)
      (rt.po.report-builder/click-add-relationship-botton)
      (rt.po.report-add-relationship/set-filter-relationship-name "AA_Herb")
      (rt.po.report-add-relationship/add-relationship "AA_Herb")
      (rt.po.report-add-relationship/add-relationship "AA_Herb")
      (rt.po.report-add-relationship/remove-relationship "AA_Herb")
      (rt.po.report-add-relationship/remove-relationship "AA_Herb")
      ;;(rt.po.report-add-relationship/set-filter-relationship-name "AA_Truck")
      ;;(rt.po.report-add-relationship/add-relationship "AA_Truck")
      ;;(rt.po.report-add-relationship/set-filter-relationship-name "")
      ;;(rt.po.report-add-relationship/set-type "Lookups")
      ;;(rt.po.report-add-relationship/add-relationship "AA_Drinks")
      ;;(rt.po.report-add-relationship/click-ok)
      ;(rt.po.report-builder/set-in-report-checkbox-field-value "Currency" true)
      ;(rt.po.report-builder/choose-column-menu-item "Boolean" "Group By")
      ;(wait-for-angular)
      ;(rt.po.report-builder/choose-column-menu-item "Currency" "Show Totals")
      ;(wait-for-angular)
      ;(rt.po.report-total/set-show-grand-total true)
      ;(wait-for-angular)
      ;(rt.po.report-total/set-show-sub-total true)
      ;(wait-for-angular)
      ;(rt.po.report-total/set-summarise-option-value "Count" true)
      ;(rt.po.report-total/set-summarise-option-value "Max" true)
      ;(rt.po.report-total/set-summarise-option-value "Min" true)
      ; (rt.po.report-total/click-ok)
      ;(rt.po.report-builder/click-save-report-botton)
      ;(rt.po.report-builder/click-close-botton)
      ;;(rt.po.report-builder/click-summarise-botton)
      ;;(rt.po.report-summarise/set-summarise-option "AA_All Fields" "Count all" true)
      ;;(rt.po.report-summarise/click-ok)
      )
    )

  (comment

    (do

      (setup-environment)

      (alter-var-root (var *test-context*)
                      (constantly {:tenant   "EDC"
                                   :username "Administrator"
                                   :password "tacoT0wn"
                                   :target :chrome}))

      (clojure.pprint/pprint *test-context*)
      (rt.scripts.common/start-app-and-login)
      (rt.po.app/navigate-to "Home" )
      (rt.po.app/enable-config-mode)
      (println (str "adding report using .client-nav-panel"))
      (rt.po.app/click-nav-builder-tool)
      (drag-n-drop (str ".nb-entry:contains('Report')") ".client-nav-panel")
      (rt.po.report-new/set-name (str "RT Report " (rand 10000)))
      (rt.po.report-new/set-description "RT Report Description")
      (rt.po.report-new/set-report-base-on "AA_All Fields")
      (rt.po.report-new/set-report-form "[Default]")
      (rt.po.report-new/set-icon "Black Circle Format Icon")
      (rt.po.report-new/set-style "Default")
      (rt.po.report-new/set-default-display-report true)
      (rt.po.report-new/set-default-picker-report true)
      (rt.po.report-new/set-hide-action-bar true)
      (rt.po.report-new/set-hide-report-header true)
      (rt.po.report-new/set-default-display-report false)
      (rt.po.report-new/set-default-picker-report false)
      (rt.po.report-new/set-hide-action-bar false)
      (rt.po.report-new/set-hide-report-header false)
      (rt.po.report-new/click-ok)
      (rt.po.report-builder/set-filter-field-name "Currency")
      (rt.po.report-builder/set-in-report-checkbox-field-value "Currency" true)
      (rt.po.report-builder/set-filter-field-name "DateTime")
      (rt.po.report-builder/set-in-report-checkbox-field-value "DateTime" true)
      (rt.po.report-builder/set-in-analyzer-checkbox-field-value "DateTime" true)
      (rt.po.report-builder/set-filter-field-name "")
      (rt.po.report-builder/set-in-report-checkbox-field-value "Autonumber" true)
      (rt.po.report-builder/set-in-report-checkbox-field-value "Currency" true)
      (rt.po.report-builder/set-in-report-checkbox-field-value "Number" true)
      (rt.po.report-builder/set-in-report-checkbox-field-value "Boolean" true)
      (rt.po.report-builder/set-in-report-checkbox-field-value "AA_Employee" true)
      (rt.po.report-builder/click-add-relationship-botton)
      (rt.po.report-add-relationship/set-filter-relationship-name "AA_Herb")
      (rt.po.report-add-relationship/add-relationship "AA_Herb")
      (rt.po.report-add-relationship/add-relationship "AA_Herb")
      (rt.po.report-add-relationship/remove-relationship "AA_Herb")
      (rt.po.report-add-relationship/remove-relationship "AA_Herb")
      (rt.po.report-add-relationship/set-filter-relationship-name "AA_Truck")
      (rt.po.report-add-relationship/add-relationship "AA_Truck")
      (rt.po.report-add-relationship/set-filter-relationship-name "")
      (rt.po.report-add-relationship/set-type "Lookups")
      (rt.po.report-add-relationship/add-relationship "AA_Drinks")
      (rt.po.report-add-relationship/click-ok)
      (rt.po.report-builder/select-treenode "AA_Herb")
      (rt.po.report-builder/remove-relationship "AA_Herb")
      (rt.po.report-builder/select-treenode "AA_Truck")
      (rt.po.report-builder/click-advanced-botton)
      (rt.po.report-advanced/set-join-type "Record must exist")
      (rt.po.report-advanced/set-follow-recursion true)
      (rt.po.report-advanced/set-recursion "Include Self")
      (rt.po.report-advanced/click-ok)

      (rt.po.report-builder/click-advanced-botton)
      (rt.po.report-advanced/set-join-type "Automatic")
      (rt.po.report-advanced/set-follow-recursion false)
      (rt.po.report-advanced/click-ok)

      (rt.po.report-builder/select-treenode "AA_All Fields")
      (rt.po.report-builder/click-advanced-botton)
      (rt.po.report-advanced/set-exact-type true)
      (rt.po.report-advanced/click-ok)
      (wait-for-angular)
      (rt.po.report-builder/choose-column-menu-item "DateTime" "Sort Options")
      (rt.po.report-sort-option/click-add-sorting)
      (rt.po.report-sort-option/click-add-sorting)

      (rt.po.report-sort-option/set-sortings [{:colname "AA_All Fields"  :dir "Ascending"} {:colname "DateTime" :dir "Descending"}])

      (rt.po.report-sort-option/click-ok)
      (rt.po.report-builder/click-calculation-botton)
      (rt.po.report-calculated/set-column-name "Calculated column 1")
      (rt.po.report-calculated/select-hint "Number")
      (rt.po.report-calculated/click-ok)
      (rt.po.report-builder/click-calculation-botton)
      (rt.po.report-calculated/set-column-name "Calculated column 2")
      (rt.po.report-calculated/set-calculation-script "[Currency] * 2")
      (rt.po.report-calculated/click-ok)


      (rt.po.report-builder/choose-column-menu-item "Calculated column 1" "Remove Column")
      (wait-for-angular)
      (rt.po.report-builder/choose-column-menu-item "Autonumber" "Sort A-Z")
      (rt.po.report-builder/choose-column-menu-item "Autonumber" "Sort Z-A")
      (rt.po.report-builder/choose-column-menu-item "Number" "Rename Column")
      (rt.po.report-builder/set-rename-column-name "Number 1")
      (rt.po.report-builder/click-ok)
      (wait-for-angular)
      (rt.po.report-builder/choose-column-menu-item "DateTime" "Format Column")
      (rt.po.report-format/click-value-formatting)
      (rt.po.report-format/set-datetime-format "Day Month Time")
      (rt.po.report-format/click-ok)
      (wait-for-angular)
      (rt.po.report-builder/click-summarise-botton)
      (rt.po.report-summarise/set-summarise-option "AA_All Fields" "Count all" true)
      (rt.po.report-summarise/set-summarise-option "Number 1" "Max" true)
      (rt.po.report-summarise/set-summarise-option "Autonumber" "Sum" true)
      (rt.po.report-summarise/click-ok)
      ;(rt.po.report-builder/click-summarise-botton)
      ;(rt.po.report-summarise/click-remove-summarise)
      ;;(rt.po.report-builder/choose-column-menu-item "DateTime" "Sort Options")
      ;;(rt.po.report-sort-option/click-add-sorting)
      ;;(rt.po.report-sort-option/click-add-sorting)
      ;;(rt.po.report-sort-option/set-sortings [{:colname "AA_All Fields"  :dir "Ascending"} {:colname "DateTime" :dir "Descending"}])

      (rt.po.report-builder/choose-column-menu-item "Boolean" "Group By")
      (wait-for-angular)
      (wait-for-angular)
      (wait-for-angular)
      (rt.po.report-builder/choose-column-menu-item "Currency" "Show Totals")
      (wait-for-angular)
      (rt.po.report-total/set-show-grand-total true)

      (wait-for-angular)
      (rt.po.report-total/set-show-sub-total true)

      (wait-for-angular)
      (rt.po.report-total/set-summarise-option-value "Count" true)
      (rt.po.report-total/set-summarise-option-value "Max" true)
      (rt.po.report-total/set-summarise-option-value "Min" true)
      (rt.po.report-total/click-ok)



      (rt.po.report-builder/click-save-report-botton)
      (rt.po.report-builder/click-close-botton)
      )
    )



  )

(comment

  (do

    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (rt.scripts.common/start-app-and-login)
    (rt.po.app/navigate-to "Test Solution")
    (rt.po.app/navigate-to-item "Test Solution" "Test Solution/AA_Automation/AA_AutomationReport")
    (rt.po.app/enable-config-mode)
    (rt.po.report-builder/access-build-mode)
    (rt.po.report-builder/click-property-report-botton)
    )
  )

(comment

  (do

    (setup-environment)

    (alter-var-root (var *test-context*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *test-context*)
    (rt.scripts.common/start-app-and-login)
    (rt.po.app/navigate-to "Test Solution")
    (rt.po.report-builder/create-report "Test Solution" "new report" "new report description" "AA_All Fields")

    )
  )

(comment

  (do

    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (rt.scripts.common/start-app-and-login)
    (rt.po.app/navigate-to "Test Solution")
    )
  )

(comment

  (do

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (rt.scripts.common/start-app-and-login)
    (rt.po.app/navigate-to "Test Solution")
    )
  )