(comment

  (let [set-stage #(clj-http.client/post "http://localhost:3000/api/context"
                                         {:form-params  {:stage-number %}
                                          :content-type :json})
        get-stage #(-> (clj-http.client/get "http://localhost:3000/api/context")
                       :body
                       clojure.data.json/read-json
                       :stage-number)
        wait-stage #(do (loop [retries-remaining 50]
                          (when (and (< (get-stage) %) (> retries-remaining 0))
                            (Thread/sleep 100)
                            (recur (dec retries-remaining))))
                        (>= (get-stage) %))]
    (set-stage 2)
    (println (get-stage))
    (wait-stage 2))

  )

(comment
  ;; doing some mobile tests

  (do
    (require '[rt.test.core :refer [*tc*]])
    (require '[rt.test.expects :refer :all])
    (require '[rt.scripts :refer :all])
    (require '[rt.lib.util :refer :all]))

  (rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})
  (rt.app/setup-environment)

  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :chrome}
                                     {:target-device "Apple iPhone 5"
                                      :target-width  400
                                      :target-height 800}
                                     {:new-subject (make-test-name "Nursing")
                                      :new-student (make-test-name "Marie John")})))

  (println 99)
  (println *ns*)
  (println *tc*)

  (do
    (rn.common/start-app-and-login)
    ;; add new account we can use in our test
    (rt.lib.wd-rn/put-entity {"typeId" "core:userAccount"
                              "name"   (:new-student *tc*)})
    )

  (do
    (rn.common/start-app-and-login)
    (rn.mobile.app/navigate-to-app-launcher)
    (rn.mobile.app/navigate-to-application "Foster University")
    (rn.mobile.app/open-navigator)
    (rn.mobile.app/navigate-to-item nil "Student Report")
    (rn.mobile.report/click-new)
    (rn.mobile.form/set-choice-value "Title" "Mrs.")
    (rn.mobile.form/set-text-field-value "Full name" (:new-student *tc*))
    (expect-equals "M/d/yyyy" (rn.mobile.form/get-date-field-placeholder "DOB"))
    (rn.mobile.form/set-date-field-value "DOB" "2/23/2002")
    (comment "getting the date in the mobile emulation of a date input returns universal format")
    (expect-equals "2002-02-23" (rn.mobile.form/get-date-field-value "DOB"))
    (rn.mobile.form/set-lookup "User Account" (:new-student *tc*))
    (rn.mobile.form/set-multi-select-choice-value "Club" "Dance")
    (rn.mobile.form/set-multi-select-choice-value "Club" "Drama")
    (comment "*** TODO select Babbage for the image field ***")
    (expect-match #"Dance" (rn.mobile.form/get-multi-select-choice-value "Club"))
    (expect-match #"Drama" (rn.mobile.form/get-multi-select-choice-value "Club"))
    (rn.mobile.form/set-number-field-value "Balance" 67.7)
    (expect-equals 67.7 (rn.mobile.form/get-number-field-value "Balance"))
    )

  (do
    (comment "*** Select Subjects tab ***")
    (rn.mobile.form/select-page 4)
    (expect (not (contains-match #"Nursing" (rn.mobile.report/get-loaded-column-values "Subjects"))))
    (rn.mobile.report/click-new)
    (rn.mobile.form/set-text-field-value "Subject name" (:new-subject *tc*))
    (rn.mobile.form/save)
    (expect-equals 2 (rn.mobile.form/get-selected-page))
    (expect-contains-match #"Nursing" (rn.mobile.report/get-loaded-column-values "Subjects"))
    )

  (do
    (comment "*** Select Subjects tab ***")
    (rn.mobile.form/select-page 2)
    (expect (not (contains-match #"Practical" (rn.mobile.report/get-loaded-column-values "Subjects"))))
    (rn.mobile.report/click-add)
    (rn.mobile.report/choose-picker-row "Practical")
    (rn.mobile.report/close-picker-ok)
    (expect-equals 2 (rn.mobile.form/get-selected-page))
    (expect-contains-match #"Practical" (rn.mobile.report/get-loaded-column-values "Subjects"))
    )

  (do
    (comment "*** save it ***")
    (rn.mobile.form/save)
    (comment "*** delete it ***")
    (rn.mobile.report/delete-record (:new-student *tc*))
    )

  )
