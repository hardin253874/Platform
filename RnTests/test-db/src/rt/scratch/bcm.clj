(comment

  (do
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all]))

  (println "hello")
  (select-keys (rt.setup/get-settings) [:app-url :tenant :username
                                        :password :test :server])

  ;; set the default test context

  (rt.test.core/merge-tc (rt.test.core/get-default-tc))

  (rt.test.core/merge-tc {:target :chrome
                          :tenant "EDC"})

  (rt.test.core/merge-tc {:username "Administrator"
                          :password "tacoT0wn"})


  (do
    (rn.common/start-app-and-login)

    (rt.test.core/merge-tc {:creds (rn.services.security/ensure-test-accounts ["BIA User"] "BIAUSER" 100)})
    (rt.test.core/merge-tc {:login-creds (get-random-tc-item :creds)})

    (rt.test.core/merge-tc {:bu-name (make-test-name "BU")
                            :bf-name (make-test-name "BF")
                            :br-name (make-test-name "BR")})

    (rt.test.core/merge-tc {:owner-name (rn.services.security/get-account-holder-name (-> *tc* :login-creds :username))})

    (rt.test.core/merge-tc {:people (rn.common/get-record-names-for-type "Employee")})

    (rn.common/start-app-and-login (-> *tc* :login-creds))

    (rt.test.core/merge-tc {:levels      (rn.common/get-choice-values "Level of Risk")
                            :drivers     (rn.common/get-choice-values "Risk Type (Source)")
                            :impacts     (rn.common/get-record-names-for-type "Risk Impact")
                            :likelihoods (rn.common/get-record-names-for-type "Risk Likelihood Rating")})

    (rt.test.core/merge-tc {:op-impacts          (rn.common/get-choice-values "Function Operational Impact" {:app-name "ReadiBCM"})
                            :products-or-services (rn.common/get-record-names-for-type "Product or Service")
                            :critical-periods (rn.common/get-record-names-for-type "Business Critical Periods")
                            :divisions (rn.common/get-record-names-for-type "Division")})

    (rt.test.core/merge-tc {:fill-bf-form (fn [bf-name]
                                         (rt.po.edit-form/set-text-field-value "Business Function" bf-name)
                                         (rt.po.edit-form/set-choice-value-v2 "Op Impact Day 1" (get-random-tc-item :op-impacts))
                                         (rt.po.edit-form/set-choice-value-v2 "Op Impact Day 2-5" (get-random-tc-item :op-impacts))
                                         (rt.po.edit-form/set-choice-value-v2 "Op Impact Week 1+" (get-random-tc-item :op-impacts))
                                         (rt.po.edit-form/set-number-value-v2 "Fin Impact Day 1" (str (rand-int 10000)))
                                         (rt.po.edit-form/set-number-value-v2 "Fin Impact Day 2-5" (str (rand-int 100000)))
                                         (rt.po.edit-form/set-number-value-v2 "Fin Impact Week 1" (str (rand-int 1000000))))})

    ;; do only if wanting to elevate
    (rn.services.security/ensure-account-in-roles (-> *tc* :login-creds :username) ["Administrators"])

    (rn.common/start-app-and-login (-> *tc* :login-creds))

    (rt.po.app/navigate-to "Business Continuity Planning")
    (rt.po.app/select-app-tab "Business Units")
    (rt.po.app/select-navigator-item "Business Units (Full)")

    (rt.po.report-view/open-action-menu)
    (rt.po.app/choose-context-menu "New")

    (rt.po.edit-form/set-text-field-value "Name" (:bu-name *tc*))
    (rt.po.edit-form/set-multiline "Description" (str "Description for " (:bu-name *tc*)))

    (rt.po.edit-form/set-dropdown-control "Division" (get-random-tc-item :divisions))

    (comment "#knownissue - need to set the owner to the current user")
    (rt.po.edit-form/set-lookup "Owner" (:owner-name *tc*))

    (comment "#knownissue - need to save the BU before adding related records")
    ;(rt.po.edit-form/save)
    ;(rt.po.report-view/set-search-text (:bu-name *tc*))
    ;(rt.po.report-view/double-click-row-by-text (:bu-name *tc*))
    ;(rt.po.view-form/click-edit)

    (rt.po.view-form/open-tab-action-menu "Business Function" "New")
    ((:fill-bf-form *tc*) (str (:bf-name *tc*) "-a"))
    (rt.po.edit-form/save)

    (rt.po.edit-form/set-bool-field-value "Completed" true)

    (rt.po.view-form/open-tab-action-menu "Recovery Requirements" "New")
    (def buildings (rn.common/get-record-names-for-type "Building" {:app-name "ReadiBCM"}))
    (def sites (rn.common/get-record-names-for-type "Building" {:app-name "ReadiBCM"}))
    (rt.po.edit-form/set-lookup "Building" (rand-nth buildings))
    (rt.po.edit-form/set-lookup "Recovery Site" (rand-nth sites))
    (rt.po.edit-form/set-number-value-v2 "People Day 1" (str (rand-int 10)))
    (rt.po.edit-form/set-number-value-v2 "People Day 2-5" (str (rand-int 100)))
    (rt.po.edit-form/set-number-value-v2 "People Week 1" (str (rand-int 100)))
    (rt.po.edit-form/set-number-value-v2 "Positions Day 1" (str (rand-int 10)))
    (rt.po.edit-form/set-number-value-v2 "Positions Day 2-5" (str (rand-int 100)))
    (rt.po.edit-form/set-number-value-v2 "Positions Week 1" (str (rand-int 100)))
    (rt.po.edit-form/save)

    (rt.po.edit-form/set-bool-field-value "Completed" true)

    ;;debug
    (->> (rt.lib.wd-rn/get-entities-of-type
           "record"
           "name"
           {:filter "[Resource is of type].Name = 'Technology'
       or [Resource is of type].[Type inherits type].Name = 'Technology'"}))
    (->> (rt.lib.wd-rn/get-entities-of-type
           "definition"
           "name,derivedTypes*.instancesOfType.name,instancesOfType.name"
           {:filter "Name = 'Technology'"}))

    (rt.po.view-form/open-tab-action-menu "IT Recovery Requirements" "New")
    (rt.po.edit-form/set-lookup "Technology" (rand-nth (rn.common/get-record-names-for-type "PC")))
    (rt.po.edit-form/set-bool-field-value "Required in 2-5 days" true)
    (rt.po.edit-form/set-bool-field-value "Required in 1 week" true)
    (rt.po.edit-form/click-save-plus-button)
    (rt.po.edit-form/set-lookup "Technology" (rand-nth (rn.common/get-record-names-for-type "Printer")))
    (rt.po.edit-form/set-bool-field-value "Required in 2-5 days" true)
    (rt.po.edit-form/set-bool-field-value "Required in 1 week" true)
    (rt.po.edit-form/click-save-plus-button)
    (rt.po.edit-form/set-lookup "Technology" (rand-nth (rn.common/get-record-names-for-type "Server Application")))
    (rt.po.edit-form/set-bool-field-value "Required in 2-5 days" true)
    (rt.po.edit-form/set-bool-field-value "Required in 1 week" true)
    (rt.po.edit-form/click-save-button)

    (rt.po.edit-form/set-bool-field-value "Completed" true)

    (rt.po.view-form/open-tab-action-menu "Products & Services Dependency" "Link")
    (rt.po.edit-form/choose-in-entity-picker-dialog (get-random-tc-item :products-or-services))
    (rt.po.view-form/open-tab-action-menu "Products & Services Dependency" "Link")
    (rt.po.edit-form/choose-in-entity-picker-dialog (get-random-tc-item :products-or-services))

    (rt.po.edit-form/set-bool-field-value "Completed" true)

    (rt.po.view-form/open-tab-action-menu "Business Critical Periods" "Link")
    (rt.po.edit-form/choose-in-entity-picker-dialog (get-random-tc-item :critical-periods))
    (rt.po.edit-form/set-bool-field-value "Completed" true)

    (rt.po.edit-form/select-form-tab "Business Unit Plan")
    (rt.po.edit-form/set-bool-field-value "Completed" true)

    (rt.po.edit-form/select-form-tab "Approval")
    (rt.po.edit-form/set-lookup "Approver" (get-random-tc-item :people))

    (rt.po.view-form/open-tab-action-menu "Employees" "Link")


    ;; do following if previously saved BU
    ;(rt.po.view-form/form-nav-back)
    (rt.po.report-view/set-search-text (:bu-name *tc*))

    (comment "TODO - check % complete is 14%")


    (commment "temp login as admin until I get the user story straightened out")
    (rt.po.app/logout)
    (rn.common/start-app-and-login (-> *tc* :creds :admin))

    (rt.po.app/navigate-to "Risk Management")
    (rt.po.app/select-app-tab "Operational Risk")
    (rt.po.app/select-navigator-item "Business Unit Risks")

    (rt.po.screen/open-report-action-menu-on-screen "Business Unit Risk Report")
    (rt.po.app/choose-context-menu "New")

    (rt.po.edit-form/set-text-field-value "Name" (:br-name *tc*))

    (rt.po.edit-form/set-choice-value-v2 "Level of Risk" (get-random-tc-item :levels))
    (rt.po.edit-form/set-choice-value-v2 "Risk Driver" (get-random-tc-item :drivers))

    (rt.po.edit-form/set-lookup "Inherent Impact" (get-random-tc-item :impacts))
    (rt.po.edit-form/set-lookup "Inherent Likelihood" (get-random-tc-item :likelihoods))
    (rt.po.edit-form/set-lookup "Residual Impact" (get-random-tc-item :impacts))
    (rt.po.edit-form/set-lookup "Residual Likelihood" (get-random-tc-item :likelihoods))
    (rt.po.edit-form/set-lookup "Target Impact" (get-random-tc-item :impacts))
    (rt.po.edit-form/set-lookup "Target Likelihood" (get-random-tc-item :likelihoods))

    (comment "TODO - fix up for actual user name")
    (rt.po.edit-form/set-lookup "Owner" (:owner-name *tc*))
    (rt.po.edit-form/set-lookup "Business Unit" (:bu-name *tc*))

    (rt.po.view-form/open-tab-action-menu "Mitigating Control" "Link")
    (rt.po.edit-form/choose-in-entity-picker-dialog (get-random-tc-item :mitigating-controls))

    (rt.po.edit-form/save)

    (rt.po.app/select-app-tab "Mitigating Controls")

    (rt.po.app/select-navigator-item "Mitigating Controls")
    (let [mc (get-random-tc-item :mitigating-controls)]
      (rt.po.report-view/set-search-text mc)
      (rt.po.report-view/double-click-row-by-text mc))

    (comment "TODO - verify the Residual Rating has been filled in")
    ;(rt.po.report-view/refresh-now)



    (rt.po.edit-form/save)

    (rt.po.report-view/set-search-text (:bu-name *tc*))
    (expect-equals 1 (count (rt.po.report-view/get-loaded-grid-values)))

    (rt.po.report-view/refresh-now)

    (comment "TODO - check % complete is 85%")
    (comment "TODO - check Max Risk rating is High")

    )

  )

