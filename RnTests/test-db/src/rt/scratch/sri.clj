(ns rt.scratch.sri
  "The scripts namespace contains handwritten scripts for use in test cases,
  and in some cases the definition of tests based on the scripts."
  (:require [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.app :refer [setup-environment]]
            [clj-webdriver.taxi :refer :all]
            [rt.scripts.common :refer [start-app-and-login]]
            [rt.repl :refer :all]

            [rt.test.core :refer [*test-context* *tc*]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser]]
            [rt.lib.wd-rn :refer [run-query query-results-as-objects]]
            [rt.lib.util :refer :all]
            [rt.scripts.common]
            [rt.scripts.edit-form]
            [rt.scripts.samples]
            [rt.scripts.qa-daily]
            [rt.scripts.perf]
            [rt.scripts.rn]))

(comment

  (do
    ;;existing access rules are enabled

    )
  )

(comment
  ;;Student can modify his own record.
  (do
    (setup-environment)

    
    (rt.po.edit-form/click-edit-button)
    (rt.po.edit-form/save)    
    (rt.po.edit-form/click-back-button)
    
    (rt.po.edit-form/set-number-value-v2 "% Complete" 100)
    (rt.po.edit-form/set-choice-value-v2 "Status" "Completed")
    (rt.po.edit-form/save)
    (rt.po.edit-form/click-back-button)
    
    
    
    
    (
      (def ActivatedPlanName (rt.po.report-builder/get-grouped-row-content "Business Continuity Plan" 0 "Recovery Plan"))
    (rt.po.report-view/double-click-row-by-text-in-relationship "Activated Plans" ActivatedPlanName)
     )
    
    (
      (def ImmRecActName (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "Recovery Action"))
    (rt.po.report-view/double-click-row-by-text-in-relationship "Recovery Actions" ImmRecActName)
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
    
	(rt.po.view-form/set-search-text-for-relationship "Recovery Actions" "Immediate")
    (expect-equals "100%" (first (clojure.string/split (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "% Complete") #"\n")) )
    (expect-equals "Completed" (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "Status") )
    
    (rt.po.view-form/set-search-text-for-relationship "Recovery Actions" "Ongoing")
    (expect-equals "100%" (first (clojure.string/split (rt.po.report-builder/get-grouped-row-content "Ongoing" 0 "% Complete") #"\n")) )
    (expect-equals "Completed" (rt.po.report-builder/get-grouped-row-content "Ongoing" 0 "Status") )
    
    (expect-equals "100%" (first (clojure.string/split (rt.po.report-builder/get-grouped-row-content "Immediate" 0 "% Complete") #"\n")) )
    
  
    
    
    
    
    	(rt.po.edit-form/save)
    
    
    
    
    
    
    
    
   

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (expect-equals true (rt.po.access-rules/access-rule-enabled? "Students (User Role)" "Staff" "Staff in my faculty") )
    (expect-equals true (rt.po.access-rules/access-rule-enabled? "Students (User Role)" "Student" "Staff in my faculty") )
    (expect-equals true (rt.po.access-rules/access-rule-enabled? "Students (User Role)" "Qualifications" "My qualifications") )
    (expect-equals true  (rt.po.access-rules/access-rule-enabled? "Students (User Role)" "Library card" "Own library card") )
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/view-record "Nelle Odom")
    (rt.po.view-form/select-form-tab "Qualifications")
    (expect-equals 2 (rt.po.report-view/count-report-row))
    (rt.po.edit-form/click-edit-button)
    (rt.po.edit-form/set-string-field-value "First name" "Nelle1")
    (rt.po.edit-form/save)
    (expect-equals "Nelle1" (rt.po.view-form/get-field-value "First name"))
    (rt.po.edit-form/click-edit-button)
    (rt.po.edit-form/set-string-field-value "First name" "Nelle")
    (rt.po.edit-form/save)
    (rt.po.view-form/form-nav-back)
    (rt.po.app/navigate-to-item "Foster University" "Reports/Qualification Report")
    (expect-equals 2 (rt.po.report-view/count-report-row))
    )
  )

(comment
  
  
  (defn get-grouped-row-content-in  [group-by-value index column-name & [grid-locator]]
  (try (let [col-index (get-report-column-index column-name grid-locator)]
         (let [row (get-grouped-row-by-index-in-relationship group-by-value index grid-locator)]
           (get (mapv #(text %) (find-elements-under row (by-css "[sp-data-grid-row-col-scope]"))) (+ col-index 1))))
       (catch Exception e "")))
  
  (defn get-grouped-row-content-in  [group-by-value index column-name & [grid-locator]]
  (try 
    (let [col-index (get-report-column-index column-name grid-locator)]
         (let [row (get-grouped-row-by-index-in-relationship group-by-value index grid-locator)]
           (get (mapv #(text %) (find-elements-under row (by-css "[sp-data-grid-row-col-scope]"))) (+ col-index 1))
           )
      )
       (catch Exception e "")
    ))
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  ;;Student can only see his related data.
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Staff Report")
    (expect-equals 3 (rt.po.report-view/count-report-row))
    (rt.po.app/navigate-to-item "Foster University" "Reports/Deans of University report")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/view-record "Alec")
    (rt.po.view-form/select-form-tab "Direct Reports")
    (expect-equals 2 (rt.po.report-view/count-report-row))
    (rt.po.app/navigate-to-item "Foster University" "Reports/Library cards")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/view-record "11111")
    (expect-equals "11111" (rt.po.view-form/get-field-value "Library Card #"))
    (rt.po.view-form/form-nav-back)
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (rt.po.report-view/view-record "Nelle Odom")
    (expect-equals "11111" (rt.po.view-form/get-field-value "Library card #"))
    (rt.po.view-form/form-nav-back)
    )
  )

(comment
  ;;Ensure existing access rule works for university Administrator
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (expect-equals true (rt.po.access-rules/access-rule-enabled? "University Administrators (User Role)" "Staff" "Staff") )
    (rt.po.app/logout)
    (rt.po.app/login "Uma.Crawford" "Uma.Crawford1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Staff Report")
    (expect (> (rt.po.report-view/count-report-row) 0))
    (rt.po.report-view/view-record "Ina Harmon")
    (rt.po.edit-form/click-edit-button)
    (rt.po.edit-form/set-text-field-value "First name" "Ina1")
    (rt.po.edit-form/save)
    (expect-equals "Ina1" (rt.po.view-form/get-field-value "First name"))
    (rt.po.edit-form/click-edit-button)
    (rt.po.edit-form/set-text-field-value "First name" "Ina")
    (rt.po.edit-form/save)
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/open-new-menu)
    (rt.po.app/choose-context-menu "Staff")
    (rt.po.edit-form/set-text-field-value "Full name" "Mary Qian")
    (rt.po.edit-form/save)
    (rt.po.report-view/delete-record "Mary Qian")
    )
  )

(comment
  ;;Ensure security flag works.
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/view-record "Nelle Odom")
    (expect-equals "11111" (rt.po.view-form/get-field-value "Library card #"))
    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/disable-access-rule "Students (User Role)" "Library card" "Own library card")
    (rt.po.access-rules/click-save-button)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/view-record "Nelle Odom")
    (expect-equals "" (rt.po.view-form/get-field-value "Library card #"))
    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Resources/Relationships")
    (rt.po.report-view/view-record "Student - Library card")
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-bool-field-value "Secures 'to' type" true)
    (rt.po.edit-form/save)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/view-record "Nelle Odom")
    (expect-equals "11111" (rt.po.view-form/get-field-value "Library card #"))
    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/enable-access-rule "Students (User Role)" "Library card" "Own library card")
    (rt.po.access-rules/click-save-button)
    (rt.po.app/navigate-to-item "Administration" "Resources/Relationships")
    (rt.po.report-view/view-record "Student - Library card")
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-bool-field-value "Secures 'to' type" false)
    (rt.po.edit-form/save)
    )
  )


(comment
  ;;Ensure multiple access rules works.
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/add-new-access-rule)
    (expect-equals true (rt.po.access-rules-new/new-dialog-visible?))
    (rt.po.access-rules-new/set-role "Students (User Role)")
    (rt.po.access-rules-new/set-object "Student")
    (rt.po.access-rules-new/click-ok)
    (rt.po.access-rules/enable-access-rule "Students (User Role)" "Student" "Student")
    (rt.po.access-rules/click-save-button)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect (> (rt.po.report-view/count-report-row) 0))

    (rt.po.report-view/right-click-row-by-text "Ezekiel Cooley")
    (expect-equals true (rt.po.app/context-menu-exists? "View"))
    (expect-equals false (rt.po.app/context-menu-exists? "Edit"))
    (rt.po.report-view/right-click-row-by-text "Ezekiel Cooley")

    (rt.po.report-view/right-click-row-by-text "Gray Byrd")
    (expect-equals true (rt.po.app/context-menu-exists? "View"))
    (expect-equals false (rt.po.app/context-menu-exists? "Edit"))
    (rt.po.report-view/right-click-row-by-text "Gray Byrd")

    (rt.po.report-view/set-search-text "Nelle")
    (rt.po.report-view/right-click-row-by-text "Nelle Odom")
    (expect-equals true (rt.po.app/context-menu-exists? "View"))
    (expect-equals true (rt.po.app/context-menu-exists? "Edit"))
    (rt.po.report-view/right-click-row-by-text "Nelle Odom")

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/delete-access-rule "Students (User Role)" "Student" "Student")
    (rt.po.access-rules/click-save-button)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    )
  )


(comment
  ;; ensure access rules work for user
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/logout)
    (rt.po.app/login "Maite.Walls" "Maite.Walls1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Deans of University report")
    (expect-equals 0 (rt.po.report-view/count-report-row))
    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/add-new-access-rule)
    (expect-equals true (rt.po.access-rules-new/new-dialog-visible?))
    (rt.po.access-rules-new/set-include-user true)
    (rt.po.access-rules-new/set-role "Maite.Walls (User Account)")
    (rt.po.access-rules-new/set-object "Dean")
    (rt.po.access-rules-new/click-ok)
    (rt.po.access-rules/enable-access-rule "Maite.Walls (User Account)" "Dean" "Dean")
    (rt.po.access-rules/click-save-button)

    (rt.po.app/logout)
    (rt.po.app/login "Maite.Walls" "Maite.Walls1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Deans of University report")
    (expect (> (rt.po.report-view/count-report-row) 0))

    (rt.po.app/logout)
    (rt.po.app/login "Nolan.Horne" "Nolan.Horne1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Deans of University report")
    (expect-equals 0 (rt.po.report-view/count-report-row))

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/delete-access-rule "Maite.Walls (User Account)" "Dean" "Dean")
    (rt.po.access-rules/click-save-button)
    )
  )


(comment
  ;;ensure access rules work for data change
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/add-new-access-rule)
    (expect-equals true (rt.po.access-rules-new/new-dialog-visible?))
    (rt.po.access-rules-new/set-role "Students (User Role)")
    (rt.po.access-rules-new/set-object "Student")
    (rt.po.access-rules-new/click-ok)
    (rt.po.access-rules/edit-access-rule "Students (User Role)" "Student" "Student")
    (rt.po.report-builder/select-field-checkboxes "Balance" true true)
    (rt.po.report-builder/set-name "Students with balance = 0")

    (rt.po.report-view/open-analyzer)
    (expect-equals true (rt.po.report-view/analyzer-field-exists? "Balance"))

    (rt.po.report-view/set-analyzer-field-oper "Balance" "=")
    (rt.po.report-view/set-analyzer-string "Balance" "0")
    (rt.po.report-view/apply-analyzer)
    (rt.po.report-builder/save)
    (rt.po.access-rules/enable-access-rule "Students (User Role)" "Student" "Students with balance = 0")
    (rt.po.access-rules/click-save-button)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 2 (rt.po.report-view/count-report-row))

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (rt.po.report-view/right-click-row-by-text "Selma Terrell")
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.edit-form/set-number-field-value "Balance" "0")
    (rt.po.edit-form/save)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 3 (rt.po.report-view/count-report-row))

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/Record Access")
    (rt.po.access-rules/click-edit-button)
    (rt.po.access-rules/delete-access-rule "Students (User Role)" "Student" "Students with balance = 0")
    (rt.po.access-rules/click-save-button)

    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (rt.po.report-view/right-click-row-by-text "Selma Terrell")
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.edit-form/set-number-field-value "Balance" "-79.17")
    (rt.po.edit-form/save)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))

    )
  )


(comment
  ;;ensure access rules work for data change - relationship

  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Qualification Report")
    (expect-equals 2 (rt.po.report-view/count-report-row))
    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (rt.po.report-view/set-search-text "Nelle")
    (rt.po.report-view/right-click-row-by-text "Nelle Odom")
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.edit-form/select-form-tab "Qualifications")
    (expect-equals 2 (rt.po.view-form/get-report-row-count))
    (rt.po.view-form/open-tab-action-menu "Qualifications" "Add Existing")
    (rt.po.edit-form/choose-in-entity-picker-dialog "C04255")
    (rt.po.edit-form/save)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Qualification Report")
    (expect-equals 3 (rt.po.report-view/count-report-row))

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (rt.po.report-view/set-search-text "Nelle")
    (rt.po.report-view/right-click-row-by-text "Nelle Odom")
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.edit-form/select-form-tab "Qualifications")
    (rt.po.report-view/set-search-text "C04255")
    (rt.po.view-form/remove-selected-report-item)
    (rt.po.edit-form/save)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Qualification Report")
    (expect-equals 2 (rt.po.report-view/count-report-row))
    )
  )


(comment
  ;;Ensure user access changes as add or remove from role.
  (do
    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/User Accounts")
    (rt.po.report-view/set-search-text "Nelle.Odom")
    (rt.po.report-view/right-click-row-by-text "Nelle.Odom")
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.view-form/open-tab-action-menu "Security roles" "Add Existing")
    (rt.po.edit-form/choose-in-entity-picker-dialog "Chancellors")
    (rt.po.edit-form/save)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect (> (rt.po.report-view/count-report-row) 10))

    (rt.po.app/logout)
    (rt.po.app/login "Administrator" "tacoT0wn")
    (rt.po.app/navigate-to-item "Administration" "Security/User Accounts")
    (rt.po.report-view/set-search-text "Nelle.Odom")
    (rt.po.report-view/right-click-row-by-text "Nelle.Odom")
    (rt.po.app/choose-context-menu "Edit")
    (rt.po.report-view/set-search-text "Chancellors")
    (rt.po.view-form/remove-selected-report-item)
    (rt.po.edit-form/save)

    (rt.po.app/logout)
    (rt.po.app/login "Nelle.Odom" "Nelle.Odom1")
    (rt.po.app/navigate-to-item "Foster University" "Reports/Student report")
    (expect-equals 1 (rt.po.report-view/count-report-row))

    )
  )
(comment
  ;;Ensure the document versioning works.
  (do

    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Documents" "Document Library/Icons")
    (expect (> (rt.po.report-view/count-report-row) 10))
    (rt.po.app/navigate-to-item "Documents" "Document Library/Documents")
    (rt.po.document-library/add-new-document)
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path "Test 1.txt"))
    (rt.po.edit-form/save)
    (rt.po.report-view/view-record "Test 1")
    (rt.po.document-library/download-document "Test 1.txt")
    (expect-equals "This is the first document" (rt.po.common/get-text-file-string (rt.po.common/get-download-file-path "Test 1.txt")))
    (expect-equals "1.0" (rt.po.view-form/get-field-value "Current revision"))
    (expect-equals "Administrator" (rt.po.view-form/get-field-value "Created by"))
    (expect-equals "Administrator" (rt.po.view-form/get-field-value "Modified by"))
    (expect-equals 1 (rt.po.view-form/get-report-row-count))
    (rt.po.view-form/click-edit)
    (expect-equals "1.0" (rt.po.edit-form/get-lookup "Current revision"))
    (expect-equals "Administrator" (rt.po.edit-form/get-lookup "Created by"))
    (expect-equals "Administrator" (rt.po.edit-form/get-lookup "Modified by"))
    (expect-equals true (rt.po.edit-form/inline-lookup-read-only? "Current revision"))
    (expect-equals true (rt.po.edit-form/inline-lookup-read-only? "Created by"))
    (expect-equals true (rt.po.edit-form/inline-lookup-read-only? "Modified by"))
    (rt.po.edit-form/click-cancel-button)
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/view-record "Test 1")
    (rt.po.view-form/click-edit)
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path "Test 2.txt"))
    (rt.po.edit-form/save)
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/view-record "Test 2")
    (rt.po.document-library/download-document "Test 2.txt")
    (expect-equals "This is the second document" (rt.po.common/get-text-file-string (rt.po.common/get-download-file-path "Test 2.txt")))
    (expect-equals "2.0" (rt.po.view-form/get-field-value "Current revision"))
    (expect-equals 2 (rt.po.view-form/get-report-row-count))

    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/view-record "Test 2")
    (rt.po.view-form/click-edit)
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path "Test 3.txt"))
    (rt.po.edit-form/save)
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/view-record "Test 3")
    (rt.po.document-library/download-document "Test 3.txt")
    (expect-equals "This is the third document" (rt.po.common/get-text-file-string (rt.po.common/get-download-file-path "Test 3.txt")))
    (expect-equals "3.0" (rt.po.view-form/get-field-value "Current revision"))
    (expect-equals 3 (rt.po.view-form/get-report-row-count))
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/right-click-row-by-text "Test 3")
    (rt.po.app/choose-context-menu "Delete")
    (rt.po.app/choose-modal-ok)
    )
  )

(comment
  ;;Ensure object can be related to document.
  (do

    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Documents" "Document Library/Documents")
    ;;Steps to create new object and form with lookup and relationship fields to Document.
    (rt.po.app/enable-config-mode)
    (rt.po.app/add-new-nav-item "New Object" "Report Templates")
    (rt.po.common/set-string "Object name" "_A_DocObject")
    (rt.po.common/click-ok)
    (expect-equals "_A_DocObject Form" (rt.po.form-builder/get-form-title))
    (rt.po.form-builder/add-field-from-toolbox-to-form "Name")
    (rt.po.form-builder/add-from-field-menu-to-form "Lookup")
    (expect-equals true (rt.po.form-builder-config/config-dialog-visible?))
    (rt.po.form-builder-config/set-object "Document")
    (rt.po.form-builder-config/set-name "Document Lookup")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/expand-toolbox-section "Display Options")
    (rt.po.form-builder/add-display-option-from-toolbox-to-form "Tabbed Container")
    (rt.po.form-builder/add-from-field-menu-to-container "Relationship" 0)
    (expect-equals true (rt.po.form-builder-config/config-dialog-visible?))
    (rt.po.form-builder-config/section-expanded? "Options")
    (rt.po.form-builder-config/set-object "Document")
    (rt.po.form-builder-config/set-name "Document Rel")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/save)
    (rt.po.form-builder/close)
    ;;Add various type documents to document library
    (rt.po.app/navigate-to-item "Documents" "Document Library/Documents")
    (rt.po.document-library/add-new-document)
    (expect-equals true (rt.po.edit-form/field-visible? "File"))
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path "Test Excel Document.xlsx"))
    (rt.po.edit-form/save)
    (expect-equals true (rt.po.report-view/report-visible? "Document Library"))
    (rt.po.document-library/add-new-document)
    (expect-equals true (rt.po.edit-form/field-visible? "File"))
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path "Test PDF Document.pdf"))
    (rt.po.edit-form/save)
    (expect-equals true (rt.po.report-view/report-visible? "Document Library"))
    (rt.po.document-library/add-new-document)
    (expect-equals true (rt.po.edit-form/field-visible? "File"))
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path ""))
    (rt.po.edit-form/save)
    (expect-equals true (rt.po.report-view/report-visible? "Document Library"))
    (rt.po.document-library/add-new-document)
    (expect-equals true (rt.po.edit-form/field-visible? "File"))
    (rt.po.document-library/upload-document (rt.po.common/get-data-file-path "Test Publisher Document.pub"))
    (rt.po.edit-form/save)
    (expect-equals true (rt.po.report-view/report-visible? "Document Library"))
    (rt.po.app/navigate-to-item "Documents" "Document Library/_A_DocObject Report")
    (rt.po.report-view/open-new-menu)
    (rt.po.edit-form/set-string-field-value "Name" "Test1")
    (rt.po.edit-form/set-lookup "Document Lookup" "Test Excel Document")
    (rt.po.view-form/open-tab-action-menu "Document Rel" "Add Existing")
    (rt.po.edit-form/choose-in-entity-picker-dialog "Test PDF Document")
    (rt.po.edit-form/save)
    (rt.po.report-view/view-record "Test1")
    (expect-equals "Test Excel Document" (rt.po.view-form/get-lookup-link "Document Lookup"))
    (rt.po.view-form/click-lookup-link "Document Lookup")
    (rt.po.document-library/download-document "Test Excel Document.xlsx")
    (expect-equals true (rt.po.common/file-exist? (rt.po.common/get-download-file-path "Test Excel Document.xlsx")))
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/double-click-row-by-text "Test PDF Document")
    (rt.po.document-library/download-document "Test PDF Document.pdf")
    (expect-equals true (rt.po.common/file-exist? (rt.po.common/get-download-file-path "Test PDF Document.pdf")))
    (rt.po.view-form/form-nav-back)
    (rt.po.view-form/form-nav-back)

    (rt.po.report-view/open-new-menu)
    (rt.po.edit-form/set-string-field-value "Name" "Test2")
    (rt.po.edit-form/set-lookup "Document Lookup" "Test Powerpoint Document")
    (rt.po.view-form/open-tab-action-menu "Document Rel" "Add Existing")
    (rt.po.edit-form/choose-in-entity-picker-dialog "Test Publisher Document")
    (rt.po.edit-form/save)
    (rt.po.report-view/view-record "Test2")
    (expect-equals "Test Powerpoint Document" (rt.po.view-form/get-lookup-link "Document Lookup"))
    (rt.po.view-form/click-lookup-link "Document Lookup")
    (rt.po.document-library/download-document "Test Powerpoint Document.ppt")
    (expect-equals true (rt.po.common/file-exist? (rt.po.common/get-download-file-path "Test Powerpoint Document.ppt")))
    (rt.po.view-form/form-nav-back)
    (rt.po.report-view/double-click-row-by-text "Test Publisher Document")
    (rt.po.document-library/download-document "Test Publisher Document.pub")
    (expect-equals true (rt.po.common/file-exist? (rt.po.common/get-download-file-path "Test Publisher Document.pub")))
    (rt.po.view-form/form-nav-back)
    (rt.po.view-form/form-nav-back)

    ;;Delete the report

    ;;Delete the newly created object
    ;;Delete all the documents added
    (rt.po.app/navigate-to-item "Documents" "Document Library/Documents")

    )
  )

(comment
;;Ensure we can view report and report instance
  (do

    (setup-environment)

    (alter-var-root (var *tc*)
                    (constantly {:tenant   "EDC"
                                 :username "Administrator"
                                 :password "tacoT0wn"
                                 :target :chrome}))

    (clojure.pprint/pprint *tc*)
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Foster University" "Reports/Staff Report")
    (expect (> (rt.po.report-view/count-report-row) 20))
    (rt.po.report-view/select-row-by-text "Indigo Huffman")
    (rt.po.report-view/open-action-menu)
    (expect-equals true (rt.po.app/context-menu-exists? "View 'Indigo Huf"))
    (expect-equals true (rt.po.app/context-menu-exists? "Edit 'Indigo Huf"))
    (rt.po.report-view/close-action-menu)
    (rt.po.report-view/select-row-by-text "Timon Valentine")
    (rt.po.report-view/open-action-menu)
    (expect-equals true (rt.po.app/context-menu-exists? "View 'Timon Val"))
    (expect-equals true (rt.po.app/context-menu-exists? "Edit 'Timon Val"))
    (rt.po.app/choose-context-menu "View")
    (expect-equals "Timon.Valentine" (rt.po.view-form/get-lookup-link "User Account"))
    (expect-equals "Cooper Griffith" (rt.po.view-form/get-lookup-link "Manager"))
    (expect (> (rt.po.view-form/get-report-row-count) 1))
    (rt.po.view-form/click-lookup-link "User Account")
    (expect-equals "Timon.Valentine" (rt.po.view-form/get-field-value "Name"))
    (rt.po.view-form/form-nav-back)
    (rt.po.view-form/click-lookup-link "Manager")
    (expect-equals "Cooper.Griffith" (rt.po.view-form/get-field-value "User Account"))
    (rt.po.view-form/form-nav-back)
    (expect (> (rt.po.view-form/get-report-row-count) 1))
    (rt.po.report-view/view-record "Ifeoma Woodard")
    (expect-equals "Ifeoma Woodard" (rt.po.view-form/get-field-value "Full name"))
    (rt.po.view-form/form-nav-back)
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-text-field-value "Last name" "Danielle")
    (rt.po.edit-form/set-choice-value "Title" "Mr.")
    (rt.po.edit-form/set-lookup "Manager" "Ina Harmon")
    (rt.po.report-view/view-record "Ifeoma Woodard")
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-text-field-value "Last name" "Smith")
    (rt.po.edit-form/set-text-field-value "Full name" "Ifeoma Smith")
    (rt.po.edit-form/save)
    (rt.po.view-form/form-nav-back)
    (rt.po.edit-form/save)
    ;;Check if the changes have been saved
    (expect-equals "Danielle" (rt.po.view-form/get-field-value "Last name"))
    (expect-equals "Mr." (rt.po.view-form/get-field-value "Title"))
    (expect-equals "Ina Harmon" (rt.po.view-form/get-lookup-link "Manager"))
    (rt.po.report-view/view-record "Ifeoma Smith")
    (expect-equals "Ifeoma Smith" (rt.po.view-form/get-field-value "Full name"))
    (rt.po.view-form/form-nav-back)
    ;; revert all the changes back resume the environment
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-text-field-value "Last name" "Valentine")
    (rt.po.edit-form/set-choice-value "Title" "Dr.")
    (rt.po.edit-form/set-lookup "Manager" "Cooper Griffith" 1)
    (rt.scripts.common/sleep 2000)
    (rt.po.edit-form/save)
    (rt.po.report-view/view-record "Ifeoma Smith")
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-text-field-value "Last name" "Woodard")
    (rt.po.edit-form/set-text-field-value "Full name" "Ifeoma Woodard")
    (rt.po.edit-form/save)
    (rt.po.view-form/form-nav-back)
    (rt.po.view-form/form-nav-back)
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
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Foster University" "Screens/Student screen - chart (p)")
    (rt.po.report-view/open-analyzer)
    (rt.po.report-view/set-analyzer-choice-option "State" "Any of" "WA")
    (expect-equals "WA" (rt.po.report-view/get-report-cell-text-content 0 "State"))
    (expect-equals "WA" (rt.po.report-view/get-report-cell-text-content 1 "State"))
    (expect-equals "WA" (rt.po.report-view/get-report-cell-text-content (- (rt.po.report-view/count-report-row) 1) "State"))
    (rt.po.app/navigate-to-item "Foster University" "Screens/Student screen - chart (np)")
    (rt.po.report-view/open-analyzer)
    (rt.po.report-view/set-analyzer-field-oper "Balance" "<")
    (rt.po.report-view/set-analyzer-string "Balance" "0")
    (rt.po.report-view/apply-analyzer)
    (expect-equals true (.startsWith (rt.po.report-view/get-report-cell-text-content 0 "Balance") "-$"))
    (expect-equals true (.startsWith (rt.po.report-view/get-report-cell-text-content 1 "Balance") "-$"))
    (expect-equals true (.startsWith (rt.po.report-view/get-report-cell-text-content (- (rt.po.report-view/count-report-row) 1) "Balance") "-$"))
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
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Home" "")
    (rt.po.app/enable-config-mode)
    (rt.po.app/add-new-nav-item "New Object" "")
    (rt.po.common/set-string "Object name" "_choice_field_object")
    (rt.po.common/click-ok)
    (expect-equals "_choice_field_object Form" (rt.po.form-builder/get-name))
    (rt.po.form-builder/add-field-from-toolbox-to-form "Name")
    (rt.po.form-builder/expand-toolbox-section "Display Options")
    (expect-equals true (rt.po.form-builder/toolbox-section-expanded? "Display Options"))
    (rt.po.form-builder/add-display-option-from-toolbox-to-form "Container")
    (rt.po.form-builder/open-container-configure-dialog 0)
    (rt.po.form-builder-config/set-name "Container1")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "New choice")
    (rt.po.form-builder-config/add-new-choice-value)
    (rt.po.form-builder-config/add-new-choice-value)
    (rt.po.form-builder-config/add-new-choice-value)
    (rt.po.form-builder-config/add-new-choice-value)
    (rt.po.form-builder-config/set-choice-value "New Value" "Value 1")
    (rt.po.form-builder-config/set-choice-value "New Value1" "Value 2")
    (rt.po.form-builder-config/set-choice-value "New Value2" "Value 3")
    (rt.po.form-builder-config/set-choice-value "New Value3" "Value 4")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "Existing choice")
    (rt.po.form-builder-config/set-choice-field-option "Use Existing")
    (rt.po.form-builder-config/set-choice-field-lookup "AA_Nationality")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "Mandatory choice")
    (rt.po.form-builder-config/set-choice-field-option "Use Existing")
    (rt.po.form-builder-config/set-choice-field-lookup "AA_Condiments")
    (rt.po.form-builder-config/expand-section "Options")
    (rt.po.form-builder-config/set-control-mandatory true)
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "Read only choice")
    (rt.po.form-builder-config/set-choice-field-option "Use Existing")
    (rt.po.form-builder-config/set-choice-field-lookup "AA_Status")
    (rt.po.form-builder-config/expand-section "Options")
    (rt.po.form-builder-config/set-control-readonly true)
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "Multi select choice")
    (rt.po.form-builder-config/set-choice-field-option "Use Existing")
    (rt.po.form-builder-config/set-choice-field-lookup "AA_Language")
    (rt.po.form-builder-config/expand-section "Options")
    (rt.po.form-builder-config/select-tab "Object Detail")
    (rt.po.form-builder-config/set-choice-field-type "Multi select")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "Default choice")
    (rt.po.form-builder-config/set-choice-field-option "Use Existing")
    (rt.po.form-builder-config/set-choice-field-lookup "AA_Title")
    (rt.po.form-builder-config/expand-section "Options")
    (rt.po.form-builder-config/select-tab "Object Detail")
    (rt.po.form-builder-config/set-choice-field-default-value "Benevolent Dictator")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/add-from-field-menu-to-container "Choice" 0)
    (rt.po.form-builder-config/set-name "Format choice")
    (rt.po.form-builder-config/set-choice-field-option "Use Existing")
    (rt.po.form-builder-config/set-choice-field-lookup "AA_Weekday")
    (rt.po.form-builder-config/expand-section "Options")
    (rt.po.form-builder-config/select-tab "Format")
    (rt.po.form-builder-config/set-background-color "Brown")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/save)
    (rt.po.form-builder/close)
    (expect-equals true (rt.po.report-view/report-visible? "_choice_field_object Report"))
     (rt.po.report-view/open-new-menu)
     (expect-equals true (rt.po.edit-form/field-mandatory-indicator-visible? "Mandatory choice"))
     (rt.po.edit-form/set-text-field-value "Name" "test choice1")
     (rt.po.edit-form/save)
     (expect-equals "A value is required." (rt.po.edit-form/field-error-message "Mandatory choice"))
     (rt.po.app/clear-alerts)
     ;;set all the fields
     (rt.po.edit-form/set-choice-value "New choice" "Value 2")
     (rt.po.edit-form/set-choice-value "Existing choice" "AUS")
     (rt.po.edit-form/set-choice-value "Mandatory choice" "Chili")
     (rt.po.edit-form/set-choice-value "Default choice" "Mr.")
     (rt.po.edit-form/select-multi-select-choice-value "Multi select choice" "English")
     (rt.po.edit-form/select-multi-select-choice-value "Multi select choice" "Shona")
     (expect-equals "English, Shona" (rt.po.edit-form/get-multi-select-choice-value "Multi select choice"))
     (rt.po.edit-form/set-choice-value "Format choice" "Tuesday")
     (rt.po.edit-form/save)
     (expect-equals true (rt.po.report-view/report-visible? "_choice_field_object Report"))
     (rt.po.report-view/view-record "test choice1")
     ;;Check for the saved values
     (expect-equals "test choice1" (rt.po.view-form/get-field-value "Name"))
     (expect-equals "Value 2" (rt.po.view-form/get-field-value "New choice"))
     (expect-equals "Chili" (rt.po.view-form/get-field-value "Mandatory choice"))
     (expect-equals "English, Shona" (rt.po.view-form/get-field-value "Multi select choice"))
     (expect-equals "Mr." (rt.po.view-form/get-field-value "Default choice"))
     (expect-equals "Tuesday" (rt.po.view-form/get-field-value "Format choicee"))
     (rt.po.view-form/form-nav-back)
     ;;Delete the object and report to resume the environment
     (rt.po.app/remove-nav-item "_choice_field_object Report")
     (rt.po.app/enable-app-toolbox)
     (rt.po.app-toolbox/select-application-filter "ReadiNow Console")
     (rt.po.app-toolbox/choose-object-menu "_choice_field_object" "Delete")
     (rt.po.app/choose-modal-ok)
     (rt.po.app/navigate-to-item "Administration" "Resources/Reports")
     (rt.po.report-view/delete-record "_choice_field_object Report")
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
    (start-app-and-login)
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
    ;;Delete the object and report to resume the environment
    (rt.po.app/remove-nav-item "_choice_mandatory_object Report")
    (rt.po.app/enable-app-toolbox)
    (rt.po.app-toolbox/select-application-filter "ReadiNow Console")
    (rt.po.app-toolbox/choose-object-menu "_choice_mandatory_object" "Delete")
    (rt.po.app/choose-modal-ok)
    (rt.po.app/navigate-to-item "Administration" "Resources/Reports")
    (rt.po.report-view/delete-record "_choice_mandatory_object Report")
    (rt.po.app/navigate-to-item "Administration" "Resources/Choice Fields")
    (rt.po.report-view/delete-record "New choice")
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
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Home" "")
    (rt.po.app/enable-config-mode)
    (rt.po.app/add-new-nav-item "New Screen" "")
    (rt.po.common/set-string "Name" "_screen_testing")
    (rt.po.common/click-ok)
    (rt.po.app-toolbox/select-application-filter "Test Solution")
    (rt.po.screen-builder/add-report-from-toolbox "AA_All Fields" "AF_Choice")
    (rt.po.screen-builder/add-form-from-toolbox "AA_All Fields" "Screen Layout - All Relationships Form")
    (rt.po.screen-builder/show-form-config-menu "Screen Layout - All Relationships Form")
    (rt.po.app/choose-context-menu "Assign Parent")
    (rt.po.common/set-combo "Parent Element" "AF_Choice (AA_All Fields Report)")
    (rt.po.common/click-ok)
    (rt.po.screen-builder/save-screen)
    (rt.po.screen-builder/close)
    (expect-equals true (rt.po.screen/is-report-on-screen "AF_Choice"))
    (expect-equals true (rt.po.screen/is-form-on-screen "Screen Layout - All Relationships Form"))
    (rt.po.report-view/select-row-by-text "Friday")
    (rt.po.screen/click-form-edit-button "Screen Layout - All Relationships Form")
    (rt.po.edit-form/set-string-field-value "Name" "testing friday edit")
    (rt.po.screen/click-form-save-button "Screen Layout - All Relationships Form")
    (expect-equals "testing friday edit" (rt.po.view-form/get-field-value "Name"))
    (rt.po.report-view/select-row-by-text "Tuesday")
    (rt.po.report-view/select-row-by-text "Friday")
    (expect-equals "testing friday edit" (rt.po.view-form/get-field-value "Name"))
    (rt.po.app/delete-nav-item"_screen_testing")
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
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Test Solution" "")
    (rt.po.app/enable-config-mode)
    (rt.po.app/enable-app-toolbox)
    (choose-screen-element-menu "AA_All Fields" :form "AA_All Fields Form" "Modify")
    (rt.po.form-builder/open-form-control-configure-dialog "Number")
    (expect-equals true (rt.po.form-builder-config/config-dialog-visible?))
(rt.po.form-builder-config/expand-section "Option")
    (rt.po.form-builder-config/select-tab "Object Detail")
    (rt.po.form-builder-config/set-field-minimum-value "10")
    (rt.po.form-builder-config/set-field-maximum-value "50")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/open-form-control-configure-dialog "Number")
    (rt.po.form-builder-config/expand-section "Option")
    (rt.po.form-builder-config/select-tab "Object Detail")
    (expect-equals "10"  (rt.po.form-builder-config/get-field-minimum-value))
    (expect-equals "50"  (rt.po.form-builder-config/get-field-maximum-value))
    (rt.po.form-builder-config/set-field-minimum-value "")
    (rt.po.form-builder-config/set-field-maximum-value "")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/save)
    (rt.po.form-builder/close)
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
    (start-app-and-login)
    (rt.po.app/navigate-to-item "Foster University" "Reports/Textbooks")
    (rt.po.app/enable-config-mode)
    (rt.po.app/modify-nav-item "Textbooks")
    (rt.po.report-builder/click-saveas-report-button)
    (rt.po.report-builder/set-report-new-name "Textbooks Copy Testing")
    (rt.po.report-builder/double-click-saveas-ok)
    (rt.po.report-builder/close)
    (expect-equals 1  (rt.po.app/count-matching-nav-items "Textbooks Copy Testing"))
   (rt.po.app/remove-nav-item "Textbooks Copy Testing")
    (rt.po.app/navigate-to-item "Administration" "Resources/Reports")
    (rt.po.report-view/set-search-text "Textbooks Copy Testing")
    (expect-equals 1 (rt.po.report-view/count-report-row))
    (rt.po.report-view/delete-record "Textbooks Copy Testing")
    )
  )

(comment
  (rt.server/start-server)
)