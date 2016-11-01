(ns rt.scripts.edit-form
  "Tests for the edit form."
  (:require [rt.scripts.common :refer [start-app-and-login]]
            [rt.test.core :refer [*test-context*]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.test.db :refer [add-tests add-tests-for-scripts-in-ns]]
            [rt.lib.util :refer [make-test-name]]
            [rt.lib.wd :refer [set-input-value stop-browser]]
            [rt.po.app :refer :all]
            [rt.po.view-form :as vf]
            [rt.po.edit-form :as ef]
            [clj-webdriver.taxi :as taxi]
            [clj-time.core :as t]
            [clj-time.coerce :as tc]
            [rt.scripts.common :as common]))

(defn test-view-form
  "script to open a form in view mode"
  {:testcase true}
  []
  (vf/to-view-form "test:af30" "test:allFieldsForm")
  (expect (= "Test 30" (vf/string-field-value "Name")))
  (expect (vf/field-exists? "Autonumber")))

(defn test-can-open-edit-on-all-fields
  "can open a form in edit mode"
  {:testcase true}
  []
  (ef/to-edit-form "test:af30" "test:allFieldsForm")
  (expect (= "Test 30" (ef/string-field-value "Name"))))

(defn test-create-all-fields-record
  "create, edit, save, reopen"
  {:testcase true}
  []
  (let [new-name (make-test-name "AF")]
    (ef/to-create-form "test:allFieldsTestType" "test:allFieldsForm")
    (expect (empty? (ef/string-field-value "Name")))
    (ef/set-string-field-value "Name" new-name)
    (expect (= new-name (ef/string-field-value "Name")))
    (ef/save)
    ;; should have returned to view mode
    (expect (= new-name (vf/string-field-value "Name")))))

(defn test-edit-fields-in-all-fields
  "create, edit, save, reopen and more"
  {:testcase true}
  []
  (let [new-name (make-test-name "AF")
        new-text "some test string data"
        new-number 12345
        new-float 123.45
        new-date (tc/to-date (tc/to-local-date-time (t/date-time 2014 07 25)))
        new-date-time (tc/to-date-time (tc/to-local-date-time (t/date-time 2014 07 25 16 56)))]

    ;; create a new All Fields record
    (ef/to-create-form "test:allFields" "test:allFieldsForm")
    (expect (empty? (ef/string-field-value "Name")))

    ;; name it
    (ef/set-string-field-value "Name" new-name)
    (expect (= new-name (ef/string-field-value "Name")))

    (ef/set-string-field-value "Single_Line" new-text)
    (ef/set-string-field-value "Multiline" new-text)

    (ef/set-number-field-value "Decimal" new-float)
    (ef/set-number-field-value "Currency" new-float)
    (ef/set-number-field-value "Number" new-number)

    (ef/set-date-field-value "Date" new-date)

    ;; issues with the time value sticking... it seems to change then it changes back to 12:00
    ;(ef/set-time-field-value "Time" new-date-time)
    ;(ef/date-field-value! "DateTime" new-date-time)
    ;(ef/set-time-field-value "DateTime" new-date-time)

    (ef/set-choice-value "Weekday" "Monday")

    ;; save (obvious comment, for punctuation purposes)
    (ef/save)

    ;; should have returned to view mode
    (expect (= new-name (vf/string-field-value "Name")))

    (expect (= new-text (vf/string-field-value "Single_Line")))
    (expect (= new-text (vf/multiline-field-value "Multiline")))

    (expect (= new-float (vf/number-field-value "Decimal")))
    (expect (= new-float (vf/number-field-value "Currency")))
    (expect (= new-number (vf/number-field-value "Number")))

    (expect (= new-date (vf/date-field-value "Date")))

    ;; need to fix up to work with dates not strings
    ;(expect (= (ef/time-string new-date-time) (vf/date-field-value "Time")))
    ;(expect (= (str (ef/date-string new-date-time) " " (ef/time-string new-date-time))
    ;       (vf/date-field-value "DateTime")))

    (expect (= "Monday" (vf/choice-field-value "Weekday")))))

(defn test-cancelling-an-edit
  "more edit form tests"
  {:testcase true}
  []

  (vf/to-view-form "test:af30" "test:allFieldsForm")
  (expect (= "Test 30" (vf/string-field-value "Name")))

  (vf/click-edit)
  (expect (= "Test 30" (ef/string-field-value "Name")))

  (ef/set-string-field-value "Name" "xxx")

  (ef/cancel)

  (expect (are-changes-pending?))
  (expect (= "xxx" (ef/string-field-value "Name")))

  (choose-to-continue-navigation)
  (expect (not (are-changes-pending?)))

  (expect (= "Test 30" (vf/string-field-value "Name")))

  )

(defn test-autogen-form-boolean
  "can open default form for a known entity"
  {:testcase true}
  []

  (vf/to-view-form "core:boolField")

  (expect (= "Boolean" (vf/string-field-value "Database_type")))

  )

(defn test-mandatory-field [finder-fn id value]
  ;; clear the mandatory field
  (set-input-value (finder-fn id) "")
  (expect (empty? (taxi/attribute (finder-fn id) "value")) (str "field is cleared: " id))

  ;; attempt to save
  (ef/save)

  ;; saving should be blocked and we should still be in edit mode and have an error
  (expect (taxi/present? (finder-fn id)) (str "still in edit mode: " id))
  (expect (= 1 (count (ef/get-error-indicators))) (str "expect a field error: " id))

  ;; set a value to clear the error
  (set-input-value (finder-fn id) value)
  (expect (= 0 (count (ef/get-error-indicators))) (str "expect no field errors: " id))

  ;; clear any alerts that may have popped up
  (clear-alerts)
  )

(defn test-mandatory-datetime-field [id date-value time-value]
  (let [finder-fn (if date-value ef/date-field (partial ef/time-control-input))]

    ;; if both date and time then note that once the date is cleared the time
    ;; controls cannot be interacted with, so clear the time first
    (when time-value
      (ef/set-time-field-value id nil))
    (when date-value
      (ef/set-date-field-value id nil))

    (expect (empty? (taxi/attribute (finder-fn id) "value")) (str "field is cleared: " id))

    ;; attempt to save
    (ef/save)

    ;; saving should be blocked and we should still be in edit mode and have an error
    (expect (taxi/present? (finder-fn id)) (str "still in edit mode: " id))
    (expect (= 1 (count (ef/get-error-indicators))) (str "expect a field error: " id))

    ;; set a value to clear the error
    ;; - note that we need to set the date first to enable the time controls
    ;; (for the case we have both)
    (when date-value
      (ef/set-date-field-value id date-value))
    (when time-value
      (ef/set-time-field-value id time-value))

    (expect (= 0 (count (ef/get-error-indicators))) (str "expect no field errors: " id))

    ;; clear any alerts that may have popped up
    (clear-alerts))
  )

(defn test-mandatory-fields-on-various
  "mandatory fields"
  {:testcase true}
  []

  ;;"should open in create mode and mark mandatory fields as mandatory"
  (ef/to-create-form "test:edtFrmMandatoryOnDef")
  (expect (= 9 (count (ef/get-mandatory-indicators))) "visible mandatory markers")

  ;;"should open in edit mode and mark mandatory fields as mandatory"
  (vf/to-view-form "test:edtFrmTest2")
  (vf/click-edit)
  (expect (= 9 (count (ef/get-mandatory-indicators))) "visible mandatory markers")

  ;;"can save"
  (ef/save)
  (expect (= "Edt Frm Test 2" (vf/string-field-value "Name")))

  (ef/to-edit-form "test:edtFrmTest2")
  (expect (= 0 (count (ef/get-error-indicators))) "expect no field errors")

  ;;"should detect missing string and numeric mandatory fields"
  (test-mandatory-field ef/string-field "String" "E2ETest_SomeString")
  (test-mandatory-field ef/string-field "Multiline" "E2ETest_SomeString")
  (test-mandatory-field ef/number-field "Number" "99")
  (test-mandatory-field ef/number-field "Decimal" "1.234")
  (test-mandatory-field ef/number-field "Currency" "1.23")

  ;;"should detect missing date and time mandatory fields"
  ;(test-mandatory-datetime-field "Date" (java.util.Date.) nil)
  ; running this a second time fails... the edit form seems to switch back to view mode
  ; to be investigated... not sure if the tests or the edit form.
  ;(test-mandatory-datetime-field "Date" (java.util.Date.) nil)

  ; and these just aren't working... to come back to
  ;(test-mandatory-datetime-field "Time" nil (java.util.Date.))
  ;(test-mandatory-datetime-field "DateTime" (java.util.Date.) (java.util.Date.))

  )

(defn init []
  #_(add-tests-for-scripts-in-ns 'rt.scripts.edit-form
                               {:data  {:target   :chrome
                                        :tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                :setup ["(common/start-app-and-login)"]}
                               {:id       :edit-form-test-suite
                                :name     "Edit form tests brought over from prev e2e tests"
                                :teardown ["(common/stop-all)"]}))

(comment

  (rt.app/setup-environment {:app-url "https://spst09.sp.local"})
  (rt.app/setup-environment {:app-url "https://syd1dev26.entdata.local"})

  (rt.app/setup-environment)
  (alter-var-root (var *test-context*) (constantly {:target   :chrome
                                                    :tenant   "EDC"
                                                    :username "Administrator"
                                                    :password "tacoT0wn"}))
  (common/start-app-and-login)
  (test-autogen-form-boolean)
  (test-edit-fields-in-all-fields)

  (do
    (common/start-app-and-login)
    (ef/to-edit-form "test:edtFrmTest2")

    ;(test-mandatory-field ef/string-field "Multiline" "E2ETest_SomeString")

    (let [finder-fn ef/string-field id "Multiline" value "E2ETest_SomeString"]
      ;; clear the mandatory field
      (set-input-value (finder-fn id) "")
      (expect (empty? (taxi/attribute (finder-fn id) "value")) (str "field is cleared: " id))

      ;; attempt to save
      (ef/save)

      ;; saving should be blocked and we should still be in edit mode and have an error
      (expect (taxi/present? (finder-fn id)) (str "still in edit mode: " id))

      #_(expect (= 1 (count (ef/get-error-indicators))) (str "expect a field error: " id))

      ;; set a value to clear the error
      #_(set-input-value (finder-fn id) value)
      #_(expect (= 0 (count (ef/get-error-indicators))) (str "expect no field errors: " id))

      ;; clear any alerts that may have popped up
      #_(clear-alerts)
      ))

  )
