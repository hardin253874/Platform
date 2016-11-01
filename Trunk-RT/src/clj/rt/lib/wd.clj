(ns rt.lib.wd
  (:require [rt.lib.wd-remote :refer [new-remote-session]]
            [rt.lib.util :refer [->int get-browser-position]]
            [clj-webdriver.driver :refer [init-driver driver?]]
            [clj-webdriver.core :refer [find-elements ->actions move-to-element]]
            [clj-webdriver.taxi :as taxi :refer [*driver* set-driver! *finder-fn* set-finder! window-resize
                                                 attribute execute-script send-keys input-text exists? displayed?
                                                 text clear element elements wait-until find-element-under
                                                 click window-reposition]]
            [clj-webdriver.element :refer [element-like? init-elements]]
            [clojure.java.io :as io]
            [clojure.string :as string]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import [org.openqa.selenium.remote DesiredCapabilities CapabilityType RemoteWebDriver]
           [org.openqa.selenium.chrome ChromeDriver ChromeOptions]
           org.openqa.selenium.firefox.FirefoxDriver
           org.openqa.selenium.phantomjs.PhantomJSDriver
           org.openqa.selenium.ie.InternetExplorerDriver
           [org.openqa.selenium Keys StaleElementReferenceException WebDriverException WebElement WebDriver]
           [org.openqa.selenium.logging LogEntries LogEntry LogType LoggingPreferences]
           java.util.concurrent.TimeUnit
           java.util.logging.Level
           [java.io File PushbackReader]
           java.text.SimpleDateFormat
           java.util.Date
           [java.util HashMap]
           [clj_webdriver.element Element]
           (java.awt Toolkit)
           (org.openqa.selenium.phantomjs PhantomJSDriver PhantomJSDriverService)))

(def ^:dynamic *grid-server* nil)

(defn- set-server!
  [new-server]
  (alter-var-root (var *grid-server*)
                  (constantly new-server)
                  (when (thread-bound? (var *grid-server*))
                    (set! *grid-server* new-server))))

(defn start-remote-session [caps & [existing?]]
  (let [existing? ((comp not not) existing?)
        [server driver] (new-remote-session {:existing existing?} caps)]
    (set-server! server)
    (set-driver! driver)))

(defn start-remote-session2 [connection-opts caps]
  (let [[server driver] (new-remote-session connection-opts caps)]
    (set-server! server)
    (set-driver! driver)))

(defn- build-logging-prefs []
  (let [prefs (LoggingPreferences.)]
    (.enable prefs LogType/BROWSER Level/ALL)
    prefs))

(defn- get-chrome-driver-path [& [default]]
  ;; TODO: generalise to checking a list of possible locations
  (let [fname (if (.contains (System/getProperty "os.name") "Windows")
                "chromedriver.exe"
                "chromedriver")
        fpath (or default (str "resources/bin/" fname))
        fpath (if (.exists (io/as-file fpath)) fpath (str "bin/" fname))
        fpath (if (.exists (io/as-file fpath)) (.getAbsolutePath (io/as-file fpath)) nil)]
    (debug "Found chromedriver at:" fpath)
    fpath))

(defn- set-chrome-driver []
  (let [chrome-driver (get-chrome-driver-path)]
    (when-not chrome-driver
      (throw (Exception. "Failed to find chromedriver.exe")))
    (debug "Setting chromedriver path: " chrome-driver)
    (System/setProperty "webdriver.chrome.driver" chrome-driver)
    chrome-driver))

(defn get-ie-driver-path [& [default]]
  (let [fname (or default "resources/bin/IEDriverServer.exe")
        fname (if (.exists (io/as-file fname)) fname "bin/IEDriverServer.exe")
        fname (if (.exists (io/as-file fname)) (.getAbsolutePath (io/as-file fname)) nil)]
    (debug "Found iedriver at:" fname)
    fname))

(defn set-ie-driver []
  (let [ie-driver (get-ie-driver-path)]
    (when-not ie-driver
      (throw (Exception. "Failed to find IEDriverServer.exe")))
    (debug "Setting iedriver path: " ie-driver)
    (System/setProperty "webdriver.ie.driver" ie-driver)
    ie-driver))

(defn get-phantomjs-driver-path [& [default]]
  (let [fname (or default "resources/bin/phantomjs.exe")
        fname (if (.exists (io/as-file fname)) fname "bin/phantomjs.exe")
        fname (if (.exists (io/as-file fname)) (.getAbsolutePath (io/as-file fname)) nil)]
    (debug "Found phantomjs at:" fname)
    fname))

(defn set-phantomjs-driver []
  (let [driver (get-phantomjs-driver-path)]
    (when-not driver
      (throw (Exception. "Failed to find phantomjs.exe")))
    (debug "Setting phantomjs path: " driver)
    (System/setProperty "phantomjs.binary.path" driver)
    driver))

(defn set-script-timeout []
  (.setScriptTimeout (.. (:webdriver *driver*) manage timeouts) (* 10 60 1000) TimeUnit/MILLISECONDS))

(defn start-chrome
  ([] (start-chrome nil))
  ([{:keys [width height device top left remote]
     :or   {width 1024 height 768 top 0 left 0}}]

   (if (not-empty remote)
     ;; then
     (let [[server driver] (new-remote-session {:existing true :host remote}
                                               {:browser :chrome})]
       (set-server! server)
       (set-driver! driver))
     ;; else
     (do
       (set-chrome-driver)
       (let [options (doto (ChromeOptions.) (.addArguments ["--lang=en-AU"]))
             m1 (doto (HashMap.) (.put "deviceName" device))
             m2 (doto (HashMap.) (.put "mobileEmulation" m1))
             caps (doto (DesiredCapabilities/chrome)
                    (.setCapability CapabilityType/LOGGING_PREFS (build-logging-prefs))
                    (.setCapability ChromeOptions/CAPABILITY options))
             caps (if-not device caps (doto caps (.setCapability ChromeOptions/CAPABILITY m2)))
             driver (ChromeDriver. caps)]
         (set-driver! (init-driver driver)))))

   (window-resize {:width width :height height})
   (window-reposition {:x (->int left) :y (->int top)})
    ;(window-reposition (get-browser-position))
   (set-script-timeout)
    ;(taxi/to "chrome://version")
    ;(info "Started Chrome version: %s" (taxi/text ".version"))
    ))

;; not complete....
(defn start-chrome-remote
  ([] (start-chrome-remote nil))
  ([{:keys [width height device]
     :or   {width 1024 height 768}}]
   (let [opts (if device {:mobileEmulation {:deviceName device}} {})]
     (start-remote-session {:capabilities {:browserName   "chrome"
                                           :chromeOptions opts}}))
   (window-resize {:width width :height height})
   (set-script-timeout)))

(comment
  ;; playing with Selenium Grid

  ;; the following return [server driver]

  (new-remote-session {:port     4444
                       :host     "127.0.0.1"
                       :existing true}
                      {:browser :firefox})

  (new-remote-session {:port     4444
                       :host     "127.0.0.1"
                       :existing true}
                      {:browser :chrome})

  ;; the following create the session and then bind to the clj-webdriver vars

  (start-remote-session {:browser      :chrome
                         :capabilities {:chromeOptions {:mobileEmulation {:deviceName "Google Nexus 5"}}}}
                        false)

  (start-remote-session {:browser      :chrome
                         :capabilities {:browserName "chrome"}})

  (start-remote-session {:browser      :chrome
                         :capabilities {:browserName   "chrome"
                                        :chromeOptions {:mobileEmulation {:deviceName "Google Nexus 5"}}}})

  ;; more tests

  (start-chrome {:remote "spdev24tc01.sp.local"})


  (set-chrome-driver)
  (start-remote-session {:browser :chrome})
  (start-remote-session {:browser :firefox} false)

  (start-remote-session {:browser :chrome} false)

  (start-remote-session2 {:existing true}
                         {:browser :chrome})

  (start-remote-session2 {:port     4444
                          :host     "127.0.0.1"
                          :existing true}
                         {:browser :chrome})

  (start-remote-session {:browser :firefox} true)
  (start-remote-session {:browser :internetExplorer})

  (taxi/close)
  (do (some-> *grid-server* .stop) (set-server! nil))

  (taxi/window-resize {:width 800 :height 600})
  (rt.po.app/start-app "EDC")
  (rt.po.app/login "Administrator" "tacoT0wn")
  (taxi/take-screenshot :bytes "./temp.png")


  (let [webdriver (:webdriver *driver*)
        logs (.getAll (.get (.. webdriver manage logs) LogType/BROWSER))]
    (count logs))
  )

;; todo - add options map as arg for things like window size
(defn start-firefox
  ([] (start-firefox nil))
  ([{:keys [width height]
     :or   {width 1024 height 768}}]
   (set-driver!
     (init-driver
       (FirefoxDriver.
         (doto (DesiredCapabilities/firefox)
           (.setCapability CapabilityType/LOGGING_PREFS (build-logging-prefs))))))
   (window-resize {:width width :height height})
   (set-script-timeout)))

(defn start-phantomjs
  ([] (start-phantomjs nil))
  ([{:keys [width height]
     :or   {width 1024 height 768}}]
   (set-phantomjs-driver)
   (set-driver!
     (init-driver
       (PhantomJSDriver.
         (doto (DesiredCapabilities/phantomjs)
           (.setCapability CapabilityType/LOGGING_PREFS (build-logging-prefs))
           (.setCapability PhantomJSDriverService/PHANTOMJS_CLI_ARGS
                           ["--ignore-ssl-errors=true"])))))
   (window-resize {:width width :height height})
   (set-script-timeout)))

(comment
  (start-phantomjs)
  (taxi/to "https://sg-mbp-2013.local/sp/#/EDC")
  (rt.po.app/login)
  (taxi/html {:css "body"})
  (let [file-name "phantom.png"
        path-str (str (System/getProperty "user.dir") "/" file-name)]
    (taxi/take-screenshot :file (.getCanonicalPath (io/file path-str))))
  )

(defn start-ie
  ([] (start-ie nil))
  ([{:keys [width height]
     :or   {width 1024 height 768}}]
   (set-ie-driver)
   (set-driver!
     (init-driver
       (InternetExplorerDriver.
         (doto (DesiredCapabilities/internetExplorer)
           (.setCapability CapabilityType/LOGGING_PREFS (build-logging-prefs))))))
   (window-resize {:width width :height height})
   (set-script-timeout)))

(defn execute-async-script
  ([js] (execute-async-script *driver* js))
  ([driver-or-js js-or-args] (if (driver? driver-or-js)
                               (execute-async-script driver-or-js js-or-args [])
                               (execute-async-script *driver* driver-or-js js-or-args)))
  ([driver js js-args]
   (when-not (:webdriver driver)
     (throw (Exception. "Cannot execute script: web driver is not initialised.")))
   (.executeAsyncScript (:webdriver driver) js (to-array js-args))))

(def find-by-jq-js "
  console.log('find-by-jq arguments:', arguments.length, arguments);
  var selector = arguments[0];
  var result = $(selector);
  result = _.map(result, _.identity); // firefox hangs if we don't do this
  console.log('find-by-jq ' + arguments.length + ' ' + arguments[0] + ', returned ' + result.length);
  return result;
  ")

(defn find-by-jq
  ([q] (find-by-jq *driver* q))
  ([driver q]
   (try
     (let [elements (execute-script driver find-by-jq-js [q])]
       ;;todo - check that we aren't creating WebElements on nil webelement
       (init-elements elements))
     (catch Exception e
       (error (str "Exception finding \"" q "\". Exception=" (.getMessage e)))
       (throw e)))))

(comment
  ;; not tested....

  (def find-by-css-containing-text-js "
  console.log('find-by-css-containing-text arguments:', arguments.length, arguments);
  var selector = arguments[0], text = arguments[1];
  var result = $(selector).filter(function() {
    return $(this).text() === text;
  });
  result = _.map(result, _.identity); // firefox hangs if we don't do this
  console.log('find-by-css-containing-text ' + arguments.length + ' ' + arguments[0] + ', returned ' + result.length);
  return result;
  ")

  (defn by-css-containing-text [q text]
    (try
      (let [elements (execute-script find-by-css-containing-text-js [q text])]
        (debug (str "by-css-containing-text \"" q "\" => count = " (count elements)))
        (init-elements elements))
      (catch Exception e
        (error (str "Exception finding \"" q "\". Exception=" (.getMessage e)))
        (throw e))))

  )

(defn jq-finder
  "default to find-by-jq, but can pass map to say whether css, xpath or jq"
  ([q] (jq-finder *driver* q))
  ([driver q]
   (if q
     (let [t0 (.getTime (Date.))
           elements (cond
                      (element-like? q) (vector q)
                      (map? q) (if (= (first (keys q)) :jq)
                                 (find-by-jq driver (first (vals q)))
                                 (find-elements driver q))
                      :else (find-by-jq driver q))
           t (- (.getTime (Date.)) t0)]
       (debugf "%s: jq-finder %s => found %d elements, took %d msecs"
               (.format (SimpleDateFormat. "HH:mm:ss.SSS") (Date.)) (pr-str q) (count elements) t)
       elements)
     (do (error "jq-finder called on falsy query:" q)
         (throw (Exception. "jq-finder called on falsy query"))))))

(defn wait-for-jq
  ([q] (wait-for-jq q 60000))
  ([q t]
    ;{:pre [q (string? q)]}
   (let [t0 (.getTime (java.util.Date.))
         first-exists? (fn [a] (-> (jq-finder a) first (#(and % (exists? %)))))]
     #_(trace "waiting...")
     (wait-until (partial first-exists? q) t 200)
     #_(trace "wait took " (- (.getTime (java.util.Date.)) t0)))))

(defn wait-until-displayed
  ([q] (wait-until-displayed q 20000))
  ([q t] (let [t0 (.getTime (java.util.Date.))
               first-displayed? (fn [q] (-> (jq-finder q) first (#(and % (exists? %) (displayed? %)))))]
           #_(trace "waiting...")
           (wait-until (partial first-displayed? q) t 20)
           #_(trace "wait-until-displayed: wait took " (- (.getTime (java.util.Date.)) t0)))))

(defn right-click
  ([el] (right-click *driver* el))
  ([driver el] (-> (:actions driver)
                   (.contextClick (:webelement el))
                   (.perform))))

(defn double-click
  ([el] (double-click *driver* el))
  ([driver el] (let [el (if (element-like? el) el (element el))]
                 (-> (:actions driver)
                     (.moveToElement (:webelement el))
                     (.doubleClick)
                     (.perform)))))

(defn press-and-hold
  ([el] (double-click *driver* el))
  ([driver el] (let [el (if (element-like? el) el (element el))]
                 (-> (:actions driver)
                     (.pressAndHold (:webelement el))
                     .release
                     .perform))))

;; failing to click on something is a common problem .... this is to help see the issue
(defn debug-click [q]
  ;(debug "debug-click: clicking" (pr-str q))
  (if-let [e (element q)]
    (try
      (->actions *driver* (move-to-element e))
      (wait-until-displayed e)
      (click e)

      (catch WebDriverException ex
        (.printStackTrace ex)
        (throw (Exception. (str "WebDriver Exception when clicking on " (pr-str q) ". " \newline "Exception=" ex)))))
    (throw (Exception. (str "Failed to find element:" (pr-str q))))))

(defn send-enter-key [e]
  (send-keys e Keys/ENTER))

(defn cancel-editable-edit-mode [e]
  (try
    (send-enter-key e)
    (Thread/sleep 100)
    (catch StaleElementReferenceException e
      ;; ignore this one as it sometimes occurs due to the target INPUT being
      ;; removed from the DOM once we send the ENTER
      (warn "Ignoring StaleElementReferenceException when leaving input edit mode"))))

(defn input-clear-all [q]
  "Clear the text of the INPUT with the given selector query.
  FIXME this isn't working"
  (send-keys q (str Keys/CONTROL "a" Keys/NULL Keys/BACK_SPACE)))

(defn set-input-value-old [s value]
  (when-not (exists? s)
    (throw (Exception. (str "set-input-value: Failed to find element: " (pr-str s)))))
  (try
    (clear s)
    (input-clear-all s)
    (input-text s value)

    (catch Exception e
      (error "Exception in set-input-value on element: " (pr-str s) ", exception:" (pr-str e))
      (throw e))))

(defn set-input-value [s value]
  {:pre [(string? value)]}
  (let [e (element s)]
    (when-not (and e (exists? e))
      (throw (Exception. (str "set-input-value: Failed to find element: " (pr-str s)))))
    (try
      (clear e)
      (input-clear-all e)
      (input-text e value)

      ;; return nil to avoid returning webelements that try to be printed....
      nil

      (catch Exception ex
        (error "Exception in set-input-value on element: " (pr-str s) ", exception:" (pr-str ex))
        (throw ex)))))

(defn find-element-with-text
  "Find the element for the given query with the given text.
  Note that this only compares the text with the first line text
  the element may contain... todo: explain this better"
  [q text-value]
  (->> (elements q)
       (filter #(= text-value (string/trim (re-find #".*" (text %)))))
       (first)))

(defn find-partial-tree
  "Find an return a partial tree of elements based on a set of queries.
  To be explained better, but an example is where you need to find
  say a button based on a sibling node's text. Use this with queries
  for the common parent, the text node, and the button node where
  the text and button nodes are relative to the parent.
  This will return a collection of vectors, one for each parent found
  and each vector is the parent element and an element for each child query.
  For now the child queries must be css (not jquery or xpath or other)"
  [root-q & child-queries]
  ;; todo - generalise for more than two child queries
  (let [root-elems (elements root-q)
        tree (map #(vector %
                           (find-element-under % {:css (first child-queries)})
                           (find-element-under % {:css (second child-queries)}))
                  root-elems)]
    tree))

(defn get-repeated-elements
  "Find a set of elements that are repeated as a group.
  Provide an element query map where the keys are anything you like and the values
  are element queries to the desired child element and relative to the container element.
  An equivalent map is returned with the values replaced with the web element objects.
  For now the child queries must be css (not jquery or xpath or other)"
  [container-q element-q-map]
  (map (fn [e] (let [result (assoc element-q-map :__container e)]
                 (reduce #(assoc %1 %2 (find-element-under e {:css (%2 %1)}))
                         result (keys element-q-map))))
       (elements container-q)))

(defn console-log [& args]
  (clj-webdriver.taxi/execute-script
    "
    console.log(arguments[0]);
    "
    [(apply str args)]))

(defn is-webdriver-initialised? []
  (not (or (nil? clj-webdriver.taxi/*driver*)
           (instance? clojure.lang.Var$Unbound clj-webdriver.taxi/*driver*))))

(defn get-browser []
  (try
    (let [b (cond
              (not (is-webdriver-initialised?)) nil
              (not (:webdriver *driver*)) nil
              (instance? ChromeDriver (:webdriver *driver*)) :chrome
              (instance? FirefoxDriver (:webdriver *driver*)) :firefox
              (instance? PhantomJSDriver (:webdriver *driver*)) :phantomjs
              (instance? InternetExplorerDriver (:webdriver *driver*)) :ie
              (instance? RemoteWebDriver (:webdriver *driver*)) (some-> *driver*
                                                                        :capabilities :actual :browser-name)
              :else nil)]
      ;; call the following to ensure all is still good with the driver
      ;; i think it does anyway
      ;(debug "checking browser" b)
      (clj-webdriver.taxi/window)
      b)
    (catch Exception ex
      ;(debug "exception while checking browser:" ex)
      nil)))

(defn start-browser [browser & [options]]
  (info "starting" browser "with options" options)
  (condp = (keyword browser)
    :chrome (start-chrome options)
    :firefox (start-firefox options)
    :ie (start-ie options)
    :phantomjs (start-phantomjs options)
    nil (throw (Exception. "browser argument is missing"))
    (throw (Exception. (str "browser \"" browser "\" not supported"))))
  (info "start-browser: =>" (some-> *driver* :webdriver pr-str))
  *driver*)

(defn stop-browser []
  (try
    (clj-webdriver.taxi/close)
    (catch Exception e
      ;; ignore exception
      ))
  (try
    (clj-webdriver.taxi/quit)
    (catch Exception e
      ;; ignore exception
      ))
  true)

(defn get-browser-logs []
  (try
    (if (is-webdriver-initialised?)
      (let [webdriver (:webdriver *driver*)
            logs (.getAll (.get (.. webdriver manage logs) LogType/BROWSER))]
        (map str logs))
      (do
        (warn "no logs")
        []))
    (catch Exception e
      (error "exception getting logs" e)
      [])))

;; todo - refactor to use get-browser-logs
(defn save-browser-logs [name]
  (let [webdriver (:webdriver *driver*)
        logs (.getAll (.get (.. webdriver manage logs) LogType/BROWSER))]
    (->> logs
         (interpose \newline)
         (apply str)
         (spit name))))

(defn prepare-script-arg
  "The script arg can be a string or a WebElement. If we receive a
  clj-webdriver element wrapper then extract the underlying webelement"
  [arg]
  (if (instance? Element arg)
    (:webelement arg)
    arg))

(defn has-class [s c]
  (>= (.indexOf (attribute s "className") c) 0))

(defn checked-find-element [query-string]
  (or (element query-string)
      (throw (Exception. (str "Failed to find element: " query-string)))))


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; Overrides of the print-method for some objects

(defmethod print-method WebDriver
  [q w]
  (try
    (let [caps (.getCapabilities q)]
      (print-simple
        (str "#<" "Title: " (.getTitle q) ", "
             "URL: " (clj-webdriver.util/first-n-chars (.getCurrentUrl q)) ", "
             "Browser: " (.getBrowserName caps) ", "
             "Version: " (.getVersion caps) ", "
             "JS Enabled: " (.isJavascriptEnabled caps) ", "
             "Native Events Enabled: " (boolean (re-find #"nativeEvents=true" (str caps))) ", "
             "Object: " q ">") w))
    (catch Exception _ (print "error"))))

(defmethod print-method WebElement
  [q w]
  (try
    (let [tag-name (.getTagName q)
          text (.getText q)
          id (.getAttribute q "id")
          class-name (.getAttribute q "class")
          name-name (.getAttribute q "name")
          value (.getAttribute q "value")
          href (.getAttribute q "href")
          src (.getAttribute q "src")
          obj q]
      (print-simple
        (str "#<"
             (clj-webdriver.util/when-attr tag-name
                                           (str "Tag: " "<" tag-name ">" ", "))
             (clj-webdriver.util/when-attr text
                                           (str "Text: " (-> text clj-webdriver.util/elim-breaks clj-webdriver.util/first-n-chars) ", "))
             (clj-webdriver.util/when-attr id
                                           (str "Id: " id ", "))
             (clj-webdriver.util/when-attr class-name
                                           (str "Class: " class-name ", "))
             (clj-webdriver.util/when-attr name-name
                                           (str "Name: " name-name ", "))
             (clj-webdriver.util/when-attr value
                                           (str "Value: " (-> value clj-webdriver.util/elim-breaks clj-webdriver.util/first-n-chars) ", "))
             (clj-webdriver.util/when-attr href
                                           (str "Href: " href ", "))
             (clj-webdriver.util/when-attr src
                                           (str "Source: " src ", "))
             "Object: " q ">") w))
    (catch Exception _ (print "error"))))
