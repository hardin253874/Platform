(ns rt.lib.wd-remote
  (:require [clojure.java.io :refer [as-url]]
            [clj-webdriver.util :as util]
            [clj-webdriver.driver :refer [init-driver]]
            [clj-webdriver.core :refer [get-url]]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import clj_webdriver.ext.remote.RemoteWebDriverExt
           [org.mortbay.jetty Connector Server]
           org.mortbay.jetty.nio.SelectChannelConnector
           org.mortbay.jetty.security.SslSocketConnector
           org.mortbay.jetty.webapp.WebAppContext
           javax.servlet.Servlet
           org.openqa.selenium.remote.server.DriverServlet
           [org.openqa.selenium.remote
            DesiredCapabilities
            HttpCommandExecutor]))

(defprotocol IRemoteServer
  "Functions for managing a RemoteServer instance."
  (start [server] "Start the server. Will try to run stop if a bind exception occurs.")
  (stop [server] "Stop the server")
  (address [server] "Get address of the server")
  (new-remote-driver [server browser-spec] "Instantiate a new RemoteDriver record.")
  (start-remote-driver [server browser-spec target-url] "Start a new RemoteDriver record and go to `target-url`."))

(defn new-remote-webdriver*
  "Internal: wire up the `RemoteWebDriverExt` object correctly with a command executor and capabilities."
  ([remote-server browser-spec] (new-remote-webdriver* remote-server
                                                       browser-spec
                                                       {}))
  ([remote-server browser-spec capabilities]

   (let [path (as-url (address remote-server))
         http-cmd-exec (HttpCommandExecutor. path)
         {:keys [browser]} browser-spec
         desired-caps (if (seq capabilities)
                        (DesiredCapabilities. (util/java-keys capabilities))
                        (util/call-method DesiredCapabilities browser nil nil))
         _ (debug "DESIRED caps" desired-caps)
         remote-webdriver (RemoteWebDriverExt. http-cmd-exec desired-caps)]
     [remote-webdriver, desired-caps])))

(defrecord RemoteServer [connection-params webdriver-server]
  IRemoteServer
  (stop [remote-server]
    (some-> (:webdriver-server remote-server) .stop))

  (start [remote-server]
    (let [port (get-in remote-server [:connection-params :port])
          path-spec (get-in remote-server [:connection-params :path-spec])]
      (try
        (let [server (Server.)
              context (WebAppContext.)
              connector (doto (SelectChannelConnector.)
                          (.setPort port))]
          (.setContextPath context "")
          (.setWar context ".")
          (.addHandler server context)
          (.addServlet context DriverServlet path-spec)
          (.addConnector server connector)
          (.start server)
          server)
        (catch java.net.BindException _
          (error "bind exception in server start, stopping..." remote-server)
          (stop remote-server)
          (info "bind exception in server start, retrying...")
          (start (assoc-in remote-server [:connection-params :port] (inc port)))))))

  (address [remote-server]
    (let [{:keys [host port path-spec existing]} (:connection-params remote-server)]
      (str "http://"
           host
           ":"
           port
           (apply str (drop-last path-spec))
           (when existing
             "hub"))))

  (new-remote-driver
    [remote-server browser-spec]
    (let [{:keys [browser profile capabilities cache-spec]
           :or   {browser :firefox cache-spec {}}} browser-spec
          _ (debug "new driver with caps:" capabilities)
          ;; WebDriver object, DesiredCapabilities object based on capabilities map passed in
          [webdriver desired-caps] (new-remote-webdriver* remote-server
                                                          {:browser browser
                                                           :profile profile}
                                                          capabilities)
          _ (debug "desired caps:" desired-caps)
          ;; DesiredCapabilities as a Clojure map
          desired-capabilities (util/clojure-keys (into {} (.asMap desired-caps)))
          ;; actual capabilities (Java object) supported by the driver, despite your desired ones
          caps (.. webdriver getCapabilities)
          ;; actual capabilities as Clojure map, since once capabilities are
          ;; assigned to a driver you can't do anything but read them
          capabilities (util/clojure-keys (into {} (.asMap caps)))]
      (init-driver {:webdriver    webdriver
                    :capabilities {:desired     desired-capabilities
                                   :desired-obj desired-caps
                                   :actual      capabilities
                                   :actual-obj  caps}
                    :cache-spec   cache-spec})))

  (start-remote-driver
    [remote-server browser-spec url]
    (let [driver (new-remote-driver remote-server browser-spec)]
      (get-url driver url)
      driver)))

(defn init-remote-server
  "Initialize a new RemoteServer record, optionally starting the
  server automatically (enabled by default)."
  [connection-params]
  (let [{:keys [host port path-spec existing]
         :or   {host      "127.0.0.1"
                port      4444
                path-spec "/wd/*"
                existing  false}} connection-params
        server-record (RemoteServer. {:host      host
                                      :port      port
                                      :path-spec path-spec
                                      :existing  existing}
                                     nil)]
    (if (get-in server-record [:connection-params :existing])
      server-record
      (assoc server-record :webdriver-server (start server-record)))))

(defn remote-server?
  [rs]
  (:existing rs)
  ;(= (class rs) RemoteServer)
  )

(defn new-remote-session
  "Start up a server, start up a driver, return both in that
order. Pass a final falsey arg to prevent the server from being
started for you."
  ([] (new-remote-session {}))
  ([connection-params] (new-remote-session connection-params {:browser :firefox}))
  ([connection-params browser-spec]
   (let [new-server (init-remote-server connection-params)
         new-driver (new-remote-driver new-server browser-spec)]
     [new-server new-driver])))
