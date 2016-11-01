(ns rt.agent
  (:require [rt.setup :refer [get-settings running-in-teamcity?]]
            [rt.lib.util :refer [get-browser-position id->str]]
            [clj-http.client :as http]
            [clojure.data.json :as json]
            [clojure.java.io :as io]
            [clojure.string :as string]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

(def sessions (atom []))
(def quitting (atom false))

(defn host-name []
  (System/getenv "COMPUTERNAME"))

(defn- get-register-agent-url [server max-sessions]
  (str server "/api/registeragent?host=" (host-name) "&max=" max-sessions))

(defn- get-register-session-url [server id]
  (str server "/api/registersession?host=" (host-name) "&id=" id))

(defn- get-report-event-url [server]
  (str server "/api/reportevent"))

(defn- register-agent [server max-sessions]
  (try
    ;;todo add some kind of identifier for this session/runner
    (-> (http/get (get-register-agent-url server max-sessions))
        :body
        json/read-json
        (or nil))
    (catch Exception ex
      (error "Exception in web call to registeragent" ex)
      nil)))

(defn- register-session [server index]
  (try
    ;;todo add some kind of identifier for this session/runner
    (-> (http/get (get-register-session-url server index))
        :body
        json/read-json
        (or nil))
    (catch Exception ex
      (error "Exception in web call to registerserver" ex)
      nil)))

(defn report-event [server index event]
  (try
    ;;todo add some kind of identifier for this session/runner
    (clj-http.client/post (get-report-event-url server)
                          {:form-params  {:host (host-name) :index index :event event}
                           :content-type :json})
    (catch Exception ex
      (error "Exception in web call to reportevent" ex)
      nil)))

(defn- list->csv [xs]
  (->> xs
       (map id->str)
       (interpose ",")
       (apply str)))

;; TODO - this is getting tedious!!! ... all these command line params
(defn- shell-rt [cmd {:keys [server session-id test top left app-url no-repeat tenant
                             username password pace ramp-up no-thinking stop-on-error
                             max-test-count from-test-index dry-run verbose test-retry-limit
                             test-pkg shared-db-dir]}]
  (let [jarfile "rt-standalone.jar"
        jarfile (if-not (.exists (io/file jarfile))
                  ;; may be running in leiningen project so assume we can use the jar in ./target
                  (str "target/" jarfile)
                  jarfile)
        cmd-args (cond
                   ;; on teamcity agents we often don't have java on the path, but there is a JDK
                   (running-in-teamcity?) [(str "\"" (get (System/getenv) "JAVA_HOME") "\\bin\\java" "\"")]
                   ;; using java directly is quieter.... (and no cmd on os x)
                   (not (.contains (System/getProperty "os.name") "Windows")) ["java"]
                   ;; using cmd gives us a cmd window so we can see what happens
                   ;:else ["cmd" "/c" "start" "java"]
                   ;; or don't use cmd....
                   :else ["java"]
                   )
        args (concat cmd-args
                     ["-Xmx192m" "-jar" jarfile cmd]
                     (if test ["--test" (str test)] [])
                     (if no-repeat ["--no-repeat"] [])
                     (if no-thinking ["--no-thinking"] [])
                     (if dry-run ["--dry-run"] [])
                     (if verbose ["--verbose"] [])
                     (if stop-on-error ["--stop-on-error"] [])
                     (if session-id ["--session-id" (str session-id)] [])
                     (if app-url ["--app-url" (str app-url)] [])
                     (if server ["--server" (str server)] [])
                     (if tenant ["--tenant" (str tenant)] [])
                     (if test-pkg ["--test-pkg" (str test-pkg)] [])
                     (if shared-db-dir ["--shared-db-dir" (str shared-db-dir)] [])
                     (if pace ["--pace" (str pace)] [])
                     (if ramp-up ["--ramp-up" (str ramp-up)] [])
                     (if test-retry-limit ["--test-retry-limit" (str test-retry-limit)] [])
                     (if username ["--username" (str username)] [])
                     (if password ["--password" (str password)] [])
                     (if from-test-index ["--from-test-index" (str from-test-index)] [])
                     (if max-test-count ["--max-test-count" (str max-test-count)] [])
                     (if left ["--left" (str left)] [])
                     (if top ["--top" (str top)] [])
                     ;[(format "> session-%s.log" (or session-id 0))]
                     ;[:dir (str (System/getProperty "user.dir"))]
                     )]
    (info "Starting test run for:" test "using jarfile" jarfile "with" (pr-str args))
    (apply clojure.java.shell/sh args)))

(comment
  (rt.agent/shell-rt "list" {})
  )

(defn- run-test [test]
  (let [testrun-id (rt.test.core/create-testrun test)]
    (info "RT Agent  - created testrun" testrun-id)
    (rt.test.core/run-to-end testrun-id)
    (rt.test.junit-report/write-junit-report testrun-id)
    #_(with-out-str (rt.repl/print-run testrun-id))))

(defn- inproc-session [{:keys [test server index]}]
  {:pre [server index]}
  (loop []
    (let [{:keys [action] :as cmd} (register-session server index)]
      (condp = action
        :run-test (run-test test)
        :snooze (Thread/sleep 5000)
        :default nil))))

(defn- dummy-session [{:keys [test server index]}]
  {:pre [server index]}
  (loop []
    (let [{:keys [action] :as cmd} (register-session server index)]
      (condp = action
        :run-test (doseq [step-index (range 5)]
                    (let [t (rand-int 5000)]
                      (Thread/sleep t)
                      (report-event server step-index
                                    {:type :step :test-id test :index step-index
                                     :expr (str "step " step-index) :time t})))
        :snooze (Thread/sleep 5000)
        :default nil))))

(defn run-test-session [cmd {:keys [test mode index] :as options}]
  (infof "RT Session - running %s with opts %s\n" test (pr-str options))
  (let [result (case mode
                 :shell (shell-rt cmd options)
                 :inproc (inproc-session options)
                 :dummy-run (dummy-session options)
                 (warn "RT Session - session" index "- unknown running mode:" mode))]
    (info "RT Session - session" index "terminated.")
    result))

(defn- start-session [index server]
  (let [options {:host   (host-name)
                 :index  index
                 :server server
                 ;:mode   :inproc
                 :mode   :dummy-run
                 }
        options (merge options (get-browser-position index))]
    (future (run-test-session "agent" options))))

(defn- prstr-session [session]
  (cond
    (not session) "nil"
    (realized? session) @session
    :default "running"))

(defn- start-sessions [n server]
  {:pre [(number? n) server]}
  (info "RT Agent  - starting" n "sessions")
  (info "RT Agent  - sessions before" (map prstr-session @sessions))
  ;; ensure at least n sessions are running
  (doseq [index (range n)]
    (cond
      (>= index (count @sessions)) (do (info "RT Agent  - session" n "- starting new")
                                       (swap! sessions conj (start-session (count @sessions) server))
                                       ;; sleep a little, mainly for debugging and console output reasons
                                       (Thread/sleep 100))
      (realized? (get @sessions index)) (do (info "RT Agent  - session" n "- restarting")
                                            (swap! sessions assoc index (start-session index server))
                                            ;; sleep a little, mainly for debugging and console output reasons
                                            (Thread/sleep 100))
      :default nil))
  (info "RT Agent  - sessions after" (map prstr-session @sessions)))

(defn run-agent [{:keys [server sessions] :or [sessions 1] :as options}]
  (try
    (info "RT Agent  - starting with options" options)
    (loop []
      (when-not @quitting
        (let [{:keys [action] :as cmd} (register-agent server sessions)]
          (debug "RT Agent  - received cmd" cmd)
          (when (= action "start-sessions")
            (start-sessions (:count cmd) server))
          (when (not= action "quit")
            (debug "RT Agent  - waiting")
            (Thread/sleep 5000)
            (recur)))))
    (catch Exception e
      (error "RT Agent  - Exception" e)))
  (info "RT Agent  - agent terminating"))

(comment

  (def options {})

  @quitting

  (def f1 (future (run-agent {:server "http://localhost:3000"})))

  (def f2 (future (do
                    (reset! quitting false)
                    (Thread/sleep 60000)
                    (reset! quitting true))))

  )