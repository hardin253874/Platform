(ns rt.scripts.common
  (:require [rt.test.core :refer [*tc* *testrun-cursor* next-file-count
                                  make-artifact-file-path get-test-id-str]]
            [rt.test.expects :refer [expect expect-equals]]
            [rt.lib.wd :refer [start-browser stop-browser get-browser get-browser-logs
                               is-webdriver-initialised?]]
            [rt.lib.wd-rn :refer [run-query query-results-as-objects nudge-top-dialog get-authenticated-identity notify-loggedin]]
            [rt.lib.wd-ng :refer [wait-for-angular]]
            [rt.lib.util :refer :all]
            [rt.po.app :as app]
            [rt.po.app-toolbox :as tb]
            [rt.po.form-builder :as fb]
            [rt.po.screen-builder :as sb]
            [rt.po.report-builder :as rb]
            [rt.po.edit-form :as ef]
            [rt.po.view-form :as vf]
            [rt.po.report-view :as rv]
            [clj-webdriver.taxi :as taxi]
            [clojure.string :as string]))

(defn sleep [msec]
  "Sleep for the given number of milliseconds where msecs is a number.

  #Usage
  (rt.scripts.common/sleep msecs)

  #Example
  (rt.scripts.common/sleep 2000)
  "
  (Thread/sleep msec))

(defn wait-until-settled []
  (wait-for-angular))

(defn clear-all-alerts
  "Clear any dirty messages and alerts"
  []
  (app/clear-all-alerts))

(defn- showing-modal? []
  (try
    (or (rt.po.common/exists-present? ".modal-backdrop")
        (rt.po.common/exists-present? ".contextmenu-backdrop"))
    (catch Exception _
      ;; ignore exceptions ... can happen if we are logging in just after a logout
      ;; as a logout recycles the app
      false)))

(defn ensure-browser [tc]
  ;; note - we only restart if the wrong browser, we don't check emulation mode
  (let [current (get-browser)
        target (keyword (:target tc))
        options (merge (select-keys (rt.setup/get-settings) [:left :top :remote])
                       (when (:target-width tc) {:width (:target-width tc)})
                       (when (:target-height tc) {:height (:target-height tc)})
                       (when (:target-device tc) {:device (:target-device tc)}))]
    (when (not= current target)
      (println "start-app-and-login: Current session is not the target browser so restarting. Current=" current ", target=" target)
      (stop-browser)
      (start-browser target options))))

(defn ensure-logged-in [{:keys [tenant username password]} & [skip-setup?]]
  (let [auth (get-authenticated-identity)]

    (if (and auth (= (:username auth) username) (= (:tenant auth) tenant))
      ;; already logged in correctly so ensure we are at the right place
      (app/open-sp-client)

      ;; else either logout or start the app, and then login
      (do
        (println "start-app-and-login: Logging in using configured credentials")
        (if auth (app/logout-direct) (app/start-app tenant skip-setup?))
        (app/login-direct username password)))))

(defn start-app-and-login
  "Start the specified browser if not already running, then start ReadiNow and login.
  All parameters are read from the current test data context.

  This is typically called as a setup step and so the returned properties will
  be added to the running test context.

  If the current browser type is not the desired type then stop it and start a new.
  If the current authenticated login is not as desired then log out and in again.

  Note - this does not check other browser target attributes like size or device.
  If you wish to alter these then you need an explicit stop-browser before calling this function.
  "
  {:test-script true}
  ([tc]
    ;; this is a bit ugly and needs refactoring, but the short of it is that the
    ;; no arg version is in widespread use and I want a way to call it with some overrides
   (binding [*tc* (merge *tc* tc)]
     (start-app-and-login)))
  ([]
   {:pre [(:target *tc*)]}

   (when (and (get-browser) (showing-modal?))
     (println "app-start-and-login: Detected modal so stopping browser")
     (stop-browser))

   (ensure-browser *tc*)
   (ensure-logged-in *tc*)

   (app/clear-all-alerts)
   (app/navigate-to-landing-home)
   (app/navigate-to-landing-home-mobile)
   (app/assert-on-launcher-page)
   (notify-loggedin)

   (println "Client version" (app/get-client-version))
   (println "Server version" (app/get-server-version))
   (println "Client App Settings:" (app/get-client-app-settings))

   {:app {:client-version (app/get-client-version)
          :server-version (app/get-server-version)
          :host-name      (System/getenv "COMPUTERNAME")
          :browser        (:target *tc*)
          :client-url     (app/make-app-url "/")}}))

 (defn start-app-and-login2
  "Start the specified browser if not already running, then start ReadiNow and login.
  All parameters are read from the current test data context.

  This is typically called as a setup step and so the returned properties will
  be added to the running test context.

  If the current browser type is not the desired type then stop it and start a new.
  If the current authenticated login is not as desired then log out and in again.

  Note - this does not check other browser target attributes like size or device.
  If you wish to alter these then you need an explicit stop-browser before calling this function.
  "
  {:test-script true}
  ([tc]
    ;; this is a bit ugly and needs refactoring, but the short of it is that the
    ;; no arg version is in widespread use and I want a way to call it with some overrides
   (binding [*tc* (merge *tc* tc)]
     (start-app-and-login2)))
  ([]
   {:pre [(:target *tc*)]}

   (when (and (get-browser) (showing-modal?))
     (println "app-start-and-login: Detected modal so stopping browser")
     (stop-browser))

   (ensure-browser *tc*)
   
   ;; pass skip-setup  param as true
   (ensure-logged-in *tc* true)

   (app/clear-all-alerts)
   (app/navigate-to-landing-home)
   (app/navigate-to-landing-home-mobile)
   (app/assert-on-launcher-page)
   (notify-loggedin)

   (println "Client version" (app/get-client-version))
   (println "Server version" (app/get-server-version))
   (println "Client App Settings:" (app/get-client-app-settings))

   {:app {:client-version (app/get-client-version)
          :server-version (app/get-server-version)
          :host-name      (System/getenv "COMPUTERNAME")
          :browser        (:target *tc*)
          :client-url     (app/make-app-url "/")}}))

(defn stop-all
  "Stop the current browser session"
  {:test-script true}
  []
  (stop-browser))

(defn navigate-to-nav-item [report-name analyzer-name resource-name action-name]
  (app/navigate-to-item "Administration" (str "Resources/" report-name))
  (wait-for-angular)
  (rv/set-search-exact analyzer-name resource-name)
  (wait-for-angular)
  (rv/right-click-row-by-text "")
  (wait-for-angular)
  (app/choose-context-menu action-name)
  (wait-for-angular))

(defn view-chart [name] (navigate-to-nav-item "Charts" "Chart" name "Open"))
(defn edit-chart [name] (navigate-to-nav-item "Charts" "Chart" name "Edit"))
(defn view-report [name] (navigate-to-nav-item "Reports" "Report" name "Open"))
(defn edit-report [name] (navigate-to-nav-item "Reports" "Report" name "Edit"))
(defn view-screen [name] (navigate-to-nav-item "Screens" "Screen" name "Open"))
(defn edit-screen [name] (navigate-to-nav-item "Screens" "Screen" name "Edit"))
(defn edit-form [name] (navigate-to-nav-item "Forms" "Form" name "Edit"))

(defn take-screenshot []
  (when (get-browser)
    (when-let [{{:keys [dir-name]} :testrun} *testrun-cursor*]
      (let [base-name (make-artifact-file-path)]
        (taxi/take-screenshot :file (str base-name ".png"))
        ;;don't nudge anymore.... mainly had this to track down a double dialog
        #_(when (nudge-top-dialog)
            (taxi/take-screenshot :file (str base-name "-nudged.png")))))))

(defn capture-browser-logs
  "Call to capture and save the browser's logs to the test run output folder.
  By default it saves logs and if there are any errors then save those separately.
  If you pass :errors-only keyword then only errors, if any, are catpured and saved."
  [& opts]
  (when (get-browser)
    (when-let [{{:keys [dir-name]} :testrun} *testrun-cursor*]
      (let [name (make-artifact-file-path)
            logs-file-name (str name "-logs.txt")
            errors-file-name (str name "-errors.txt")
            logs (get-browser-logs)
            errors (filter #(re-find #"\[(ERROR)|(SEVERE)\]" %) logs)
            ;; we know about the following... exclude it to stop lots of noise
            ;;errors (remove #(re-find #"preserveAspectRatio: Trailing garbage" %) errors)
            ]
        (when-not (= (first opts) :errors-only)
          (->> logs
               (interpose \newline)
               (apply str)
               (spit logs-file-name)))
        (when-not (empty? errors)
          (->> errors
               (interpose \newline)
               (apply str)
               (spit errors-file-name)))))))

(defn clear-browser-logs []
  (get-browser-logs)
  ;; don't return anything
  nil)

(defn safe-wait-for-angular []
  (timeit "safe-wait-for-angular"
          (try
            (get-browser)
            (wait-for-angular)

            (catch Exception e
              (println "Ignoring exception in safe-wait-for-angular" e)))))

(comment
  ;; the following don't work as they'll be in the captured test out rather than
  ;; the std out of the test runner...

  (defn send-teamcity-test-started []
    (println (format-tc-message :testStarted {:name (get-test-id-str) :captureStandardOutput "true"})))

  (defn send-teamcity-test-finished []
    (println (format-tc-message :testFinished {:name (get-test-id-str)})))

  (defn send-teamcity-test-failed []
    (println (format-tc-message :testFailed {:name (get-test-id-str)}))))

(defn init []
  )

(defn expect-equals-colour
  [selector property-name colourName]
  (expect-equals
    (rt.po.common/get-colour-from-css-colour (rt.po.common/get-element-css-value selector property-name))
    (rt.po.common/get-colour-from-css-colour (rt.lib.wd-rn/get-css-color-from-name colourName))))
