(ns rt.scratch.grc)

(in-ns 'rt.scripts)

(comment

  (rt.app/setup-environment {})

  ;; use this... => output to repl
  (let [id :grc/tests/set-test-data]
    (println ";; setup")
    (println (rt.test.core/get-test-steps-as-source id :setup))
    (println ";; steps")
    (println (rt.test.core/get-test-steps-as-source id)))

  ;; get the output from the repl and paste here
  ;; - copy from repl to here
  ;; - replace any identity calls in the setup steps with rt.test.core/merge-tc

  (rt.setup/update-settings {:app-url "https://sg-mbp-2013.local"})

  ;; testing against the qa 'prod like' server
  (rt.setup/update-settings {:app-url "https://qa.readinow.info" :tenant "GRC" :username "admin" :password "Readi911"})
  (rt.setup/update-settings {:app-url "https://spdevfe03.sp.local" :tenant "EDC" :username "Administrator" :password "tacoT0wn"})

  ;; run this to initialise the test context
  (rt.test.core/merge-tc (rt.test.core/get-default-tc))

  ;; to debug
  (println *ns*)
  (println *tc*)
  (select-keys (rt.setup/get-settings) [:app-url :tenant :username :password :test :server])

  ;; setup
  (rn.common/start-app-and-login) ; as admin
  (set-test-data :grc-creds rn.services.security/ensure-test-accounts ["BCP Manager" "Compliance Manager" "Risk Manager"] "GRC" 50)
  (set-test-data :bcp-manager-creds rn.services.security/ensure-test-accounts ["BCP Manager"] "BCP" 50)
  (set-test-data :bia-user-creds rn.services.security/ensure-test-accounts ["BIA User"] "BIAUSER" 50)

  (rn.common/start-app-and-login (first (get-test-data :bcp-manager-creds)))
  (set-test-data :business-units rn.common/get-record-names-for-type-via-report "Business Unit")
  (set-test-data :risk-impacts rn.common/get-record-names-for-type "Risk Impact" {:app-name "ReadiBCM"})
  (set-test-data :likelihoods (rn.common/get-record-names-for-type "Risk Likelihood Rating" {:app-name "ReadiBCM"}))

  (rn.common/start-app-and-login (first (get-test-data :bia-user-creds)))
  (set-test-data :employees-for-bia-user rn.common/get-record-names-for-type-via-report "Employee")
  (set-test-data :divisions-for-bia-user rn.common/get-record-names-for-type "Division")

  (comment "Assuming security for the following are the same for all GRC roles")
  (set-test-data :buildings (rn.common/get-record-names-for-type "Building" {:app-name "ReadiBCM"}))
  (comment "TODO - sites is actually a subset of buildings .. so temp hardcoding")
  (comment (set-test-data :sites rn.common/get-record-names-for-type "Building" {:app-name "ReadiBCM"}))
  (set-test-data :sites ["Box Hill" "Maitland" "Ryde" "Pookaka"])
  (set-test-data :op-impacts (rn.common/get-choice-values "Function Operational Impact" {:app-name "ReadiBCM"}))
  (set-test-data :risk-drivers (rn.common/get-choice-values "Risk Type (Source)" {:app-name "ReadiBCM"}))
  (set-test-data :risk-levels (rn.common/get-choice-values "Level of Risk" {:app-name "ReadiBCM"}))
  (set-test-data :critical-periods (rn.common/get-record-names-for-type "Business Critical Periods"))
  (set-test-data :products-or-services (rn.common/get-record-names-for-type "Product or Service"))
  (set-test-data :tech-pcs (rn.common/get-record-names-for-type "PC"))
  (set-test-data :tech-sw rn.common/get-record-names-for-type "Server Application")
  (set-test-data :tech-printers rn.common/get-record-names-for-type "Printer")
  => nil
  )

