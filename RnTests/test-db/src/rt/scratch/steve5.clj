(comment

  ;; get some symbols in place that we normally have when running
  (do
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all])
    (require '[clojure.set :refer [subset?]])
    (require '[clj-webdriver.taxi :as taxi]))

  ;; set the default test context
  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :chrome}
                                     {:form-name (make-test-name "RT-Student Form")
                                      :report-name (make-test-name "RT-Student Report")})))

  ;; print to output window the steps for a test
  (println (rt.test.core/get-test-steps-as-source
             "rn/calculation/calculated-field-on-report---dataChange"
             :steps))
  (println (rt.test.core/get-test-steps-as-source
             "rn/calculation/calculated-field-on-report---dataChange"
             :teardown))

  ;; paste those steps here and get to it....
  (do
    (identity {:form-name (make-test-name "RT-Student Form")
               :report-name (make-test-name "RT-Student Report")})

    (rn.common/start-app-and-login (:admin-creds *tc*))
    (comment "==================================== Adding a new form for Dean with calcualted field (Yes/No used for group by)for test purpose. ==================================")
    (rn.app/open-report "Deans of University report" {:app-name "Foster University" :folder-name "Reports"})
    (rt.po.app/enable-app-toolbox)
    (rt.po.app-toolbox/set-application-filter "Foster University")
    (rt.po.app-toolbox/create-form-via-toolbox-object "Student")
    (expect-equals "New 'Student' Form" (rt.po.form-builder/get-form-title))
    (rt.po.form-builder/set-name (:form-name *tc*))
    (rt.po.form-builder/add-field-from-toolbox-to-form "Name")
    (rt.po.form-builder/add-field-from-toolbox-to-form "Balance")
    (rt.po.form-builder/add-field-to-form "Number")
    (rt.po.form-builder/add-field-to-form "Calculation")
    (expect-equals "Calculation" (rt.po.form-builder-config/get-name))
    (rt.po.form-builder-config/set-name "Cal-Decimal")
    (expect-equals "Text" (rt.po.form-builder-config/get-calculation-field-type))
    (rt.po.form-builder-config/set-calculation "[Number]+[Balance]")
    (expect (not (rt.po.form-builder-config/ok-disabled?)))
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/open-form-control-configure-dialog "Cal-Decimal")
    (expect (rt.po.form-builder-config/config-dialog-visible?))
    (expect (rt.po.form-builder-config/calculation-field-type-read-only?))
    (rt.po.form-builder-config/click-cancel)
    (rt.po.form-builder/save)
    (rt.po.form-builder/close)
    (rt.po.app-toolbox/set-application-filter "Foster University")
    (comment "==================================== Adding a new report for student with caluculated date field to test data/formula change. ==================================")
    (rn.app/open-report "Student report" {:app-name "Foster University", :folder-name "Reports"})
    (rt.po.app/enable-config-mode)
    (rt.po.report-builder/access-build-mode)
    (rt.po.report-builder/click-saveas-report-button)
    (rt.po.report-builder/set-report-new-name (:report-name *tc*))
    (rt.po.report-builder/click-saveas-ok)
    (rt.po.report-builder/select-fields-in-report "Cal-Decimal" "Number")
    (rt.po.report-builder/deselect-fields-in-report "DOB" "Email address" "Address line1" "State" "Gender" "Balance")
    (expect-equals 1 (rt.po.report-builder/get-count-matched-columns-name "Cal-Decimal"))
    (rt.po.report-builder/click-property-report-button)
    (rt.po.report-new/set-report-form (:form-name *tc*))
    (rt.po.report-new/click-ok)
    (rt.po.report-builder/click-save-report-button)
    (rt.po.report-builder/click-close-button)
    (comment "==================================== Following is to test if data change is reflected in calculated field. ==================================")
    (rn.app/open-report (:report-name *tc*) {:app-name "Foster University", :folder-name "Reports"})
    (expect-equals "" (rt.po.report-view/get-value-for-row-and-column "1145" "Number"))
    (expect-equals "" (rt.po.report-view/get-value-for-row-and-column "1145" "Cal-Decimal"))
    (rt.po.report-view/double-click-row-by-text "Selma Terrell")
    (expect (rt.po.edit-form/edit-button-exists?))
    (expect-equals "" (rt.po.view-form/get-field-value "Number"))
    (expect-equals "" (rt.po.view-form/get-field-value "Cal-Decimal"))
    (rt.po.view-form/click-edit)
    (rt.po.edit-form/set-number-field-value "Number" "1")
    (rt.po.edit-form/click-save-button)
    (expect-equals "-78.17" (rt.po.view-form/get-field-value "Cal-Decimal"))
    (rt.po.edit-form/click-back-button)
    (expect-equals "$-78.170" (rt.po.report-view/get-value-for-row-and-column "1145" "Cal-Decimal"))
    (rt.po.report-view/double-click-row-by-text "Selma Terrell")
    (comment "==================================== Following is to test if formula change is reflected in calculated field. ==================================")
    (rt.po.app/enable-app-toolbox)
    (rt.po.app-toolbox/set-application-filter "Foster University")
    (rt.po.app-toolbox/choose-object-menu "Student" "Modify Object")
    (rt.po.form-builder/open-field-configure-dialog "Cal-Decimal")
    (rt.po.form-builder-config/set-calculation "[Number]*2+[Balance]")
    (rt.po.form-builder-config/click-ok)
    (rt.po.form-builder/save)
    (rn.app/open-report (:report-name *tc*) {:app-name "Foster University", :folder-name "Reports"})
    (expect-equals "$-77.170" (rt.po.report-view/get-value-for-row-and-column "1145" "Cal-Decimal"))
    (rt.po.report-view/double-click-row-by-text "Selma Terrell")
    (expect-equals "-77.17" (rt.po.view-form/get-field-value "Cal-Decimal"))
    (comment "==================================== Forced env. reset. ==================================")
    (rt.po.app/enable-app-toolbox)
    (rt.po.app-toolbox/set-application-filter "Foster University")
    (rt.po.app-toolbox/choose-object-menu "Student" "Modify Object")
    (expect-equals "Students Form" (rt.po.form-builder/get-form-title))
    (rt.po.form-builder/delete-fields-and-save "Cal-Decimal" "Number")
    (expect-equals false (rt.po.form-builder/toolbox-field-exist? "Cal-Decimal"))
    (rn.app/delete-form (:form-name *tc*))
    (rn.app/delete-report (:report-name *tc*))

    )
  )
