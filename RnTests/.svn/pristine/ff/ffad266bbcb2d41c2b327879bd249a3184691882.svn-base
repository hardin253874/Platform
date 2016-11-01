(ns rt.scratch.steve
  (:require [clj-webdriver.taxi :refer :all]
            [clj-webdriver.core :refer [->actions move-to-element]]
            [rt.test.expects :refer :all]
            rt.scripts.qa-daily
            [rt.scripts.common :refer [start-app-and-login]]
            [rt.po.app :as app]
            [rt.po.app-toolbox :as tb]
            [rt.po.form-builder :as fb]
            [rt.po.report-builder :as rb]
            [rt.po.report-view :as rv]
            [rt.po.edit-form :as ef]
            [rt.test.core :refer :all]
            [rt.test.db :refer :all]
            [rt.lib.util :refer :all]
            [rt.lib.wd :refer :all]
            [rt.lib.wd-rn :refer :all]
            [rt.lib.wd-ng :refer :all]
            [rt.setup :refer :all]
            [rt.app :refer [setup-environment]]
            [clojure.repl :refer :all]
            [clojure.pprint :refer [pprint pp print-table]]
            [clojure.string :as string]
            [clojure.walk :as walk]
            [clojure.data.json :as json]
            [datomic.api :as d]
            [clj-webdriver.taxi :as taxi]))

(comment
  (defn- get-loaded-nav-items
    "Get a seq of maps, each with the nav item element and some useful details,
    and sorted by the item's text."
    []
    (let [q ".client-nav-panel div[class*=nav-type]"]
      (->> (taxi/elements q)
           (map #(hash-map :e %
                           :class (taxi/attribute % "class")
                           :text (taxi/text %)))
           (map #(assoc % :level (get (re-find #"nav-level-(\d)" (:class %)) 1)))
           (sort-by :text))))

  (let [s "(println 99) 88 (+ 3 4)"
        forms (rt.test.core/read-expressions s)]
    (reduce #(do (println (pr-str %2))
                 (eval %2))
            nil forms))


  (let [table-q ".ngreport-view"
        row-q ".ngRow"
        cell-q ".ngCell"
        table (taxi/find-element-under table-q {:css ".ngGrid"})
        rows (taxi/find-elements-under table {:css row-q})
        cells (map #(taxi/find-elements-under % {:css cell-q}) rows)
        table (hash-map :table table
                        :rows (map #(hash-map :row %1
                                              :cells %2) rows cells))]
    (-> table
        first
        :cells
        second
        taxi/text))


  (let [table-q ".spreport-view"
        row-q ".ngRow"
        cell-q ".ngCell"
        table (taxi/find-element-under table-q {:css ".ngGrid"})
        rows (taxi/find-elements-under table {:css row-q})
        cells (mapv #(taxi/find-elements-under % {:css cell-q}) rows)
        table (hash-map :table table
                        :rows (mapv #(hash-map :row %1
                                               :cells %2) rows cells))]
    (->> table
         :rows
         (map (comp taxi/text first :cells))))


  (->
    (->> (rt.lib.wd/get-repeated-elements ".edit-form-control-container"
                                          {:title-elem ".edit-form-title"
                                           :value-elem ".edit-form-value"})
         (map #(assoc % :title-label (taxi/text (:title-elem %))))
         (filter #(re-find #"Division" (:title-label %)))
         (map :value-elem)
         first)
    (taxi/find-element-under {:css "select"})
    (select-option {:text "Finance"}))

  )

(comment

  (expect (> 400 (rt.lib.util/get-time #(Thread/sleep %) 300)))
  (expect-max-time 400 (Thread/sleep 400))

  )

(comment
  ;; repro a admin toolbox bug

  ;(rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})
  (rt.app/setup-environment)
  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :firefox})))

  (timeit "login" (start-app-and-login))

  (set-finder! (add-angular-wait-to-finder jq-finder))
  (set-finder! (add-angular-wait-to-finder taxi/css-finder))
  (set-finder! taxi/css-finder)
  (set-finder! jq-finder)
  (set-finder! rt.lib.wd-ng/jq-finder-wait-ng)

  (timeit "a" (execute-async-script "var callback = arguments[0] || function () {};callback(null);"))
  (timeit "a"
          (clj-webdriver.taxi/execute-script
            "console.log('hello from RT ' + window.navigator.language);
                return window.navigator.language;"))

  (timeit "a"
          (clj-webdriver.taxi/execute-script
            "var a = angular.element('body').injector().get('spAppSettings');
             return JSON.stringify(a)"))

  (timeit "a" (count (rt.lib.wd/jq-finder {:css "div.app-launcher-page"})))
  (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-tile"})))
  (timeit "a" (count (taxi/elements {:css "div.app-launcher-page"})))
  (timeit "a" (count (taxi/elements "div.app-launcher-page")))
  (timeit "a" (count (taxi/*finder-fn* {:css "div.app-launcher-page"})))

  (timeit "total"
          (doseq [n (repeat 5 1)]
            (timeit "" (println (taxi/text {:css "div.app-launcher-tile"})))))

  (timeit "total"
          (do
            ;(timeit "XX" (wait-for-angular))
            (timeit "a" (count (rt.lib.wd/jq-finder {:css "div.app-launcher-page"})))
            (timeit "a" (count (rt.lib.wd/jq-finder {:css "div.app-launcher-page"})))
            ;            (Thread/sleep 50)
            (timeit "a" (count (rt.lib.wd/jq-finder {:css "div.app-launcher-page"})))
            (timeit "a" (count (rt.lib.wd/jq-finder {:css "div.app-launcher-page"})))
            ;            (Thread/sleep 50)
            (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-page"})))
            (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-page"})))
            ;            (Thread/sleep 50)
            (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-page"})))
            (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-page"})))
            ;            (Thread/sleep 50)
            (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-page"})))
            (timeit "a" (count (clj-webdriver.core/find-element *driver* {:css "div.app-launcher-page"})))))

  (do
    (timeit "XX" (wait-for-angular))
    (apply f args))

  (timeit "a"
          (count
            (apply clj-webdriver.core/find-element
                   [*driver*
                    {:css "div.app-launcher-page"}])))

  )

(comment

  (rt.app/setup-environment)

  (rt.test.core/merge-tc {:target :firefox})
  (rt.test.core/merge-tc {:target :chrome})

  (rt.test.core/merge-tc {:tenant   "EDC"
                          :username "Administrator"
                          :password "tacoT0wn"})

  (rt.test.core/merge-tc {:target-device "Apple iPhone 5"
                          :target-width  400
                          :target-height 800})

  (rt.lib.wd/stop-browser)

  ;; running this
  (println (rt.test.core/get-test-steps-as-source :rn/mobile/general-navigation-on-mobile))
  ;(println (rt.test.core/get-test-steps-as-source :rn/console/create-and-apply-console-theme :setup))

  ;; gives a bunch of steps we can paste below

  (do
    (rt.po.app/logout)
    (rt.po.app/login))


  (do

    ;(rt.test.core/merge-tc {:new-subject (make-test-name "Nursing") :new-student (make-test-name "Marie John")})

    (do
      (rn.common/start-app-and-login)

      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Foster University")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Staff Report")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Student Report")

      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Test Solution")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Area - NSW Population")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Bar - Internet Access")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "1. Scientists Screen")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "2. Dog Breeds")

      (rn.mobile.app/navigate-to-app-launcher)
      (rn.mobile.app/navigate-to-application "Shared")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "People")
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Activities")
      (rn.mobile.report/expect-report-column-count 3)
      (rn.mobile.app/open-navigator)
      (rn.mobile.app/navigate-to-item nil "Approvals")
      (rn.mobile.app/navigate-to-app-launcher)

      )

    )
  )

(comment
  (.printStackTrace *e)
  (pp)

  ;; use this if running on non entdata or sp domain machine
  ;; or if wanting to target other than your host
  (rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})

  ;; at work use this
  (rt.app/setup-environment)

  ;; run a test or suite
  (rt.repl/init-test-run :dev/steve/firefox)
  (rt.repl/run-to-error)

  (rt.repl/run-tests :steve)

  ;; use the following to run bits of tests

  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :chrome}
                                     {:target-device "Apple iPhone 5"
                                      :target-width  400
                                      :target-height 800})))

  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :firefox})))

  (alter-var-root (var *tc*)
                  (constantly (merge {:tenant   "EDC"
                                      :username "Administrator"
                                      :password "tacoT0wn"}
                                     {:target :chrome})))


  (start-app-and-login)

  (start-app-and-login {:tenant "XYZ" :username "Joe" :password "p1"})

  (do
    (app/logout)
    ;; safe-wait-for-angular is killing the browser after a logout
    (do
      (get-browser)
      (wait-for-angular)))

  (app/logout)
  (stop-browser)
  (clj-webdriver.taxi/quit)

  (start-browser :chrome {:device "Google Nexus 5" :width 400 :height 800})
  (start-browser :chrome {:device "Apple iPhone 5" :width 400 :height 800})
  (start-browser :firefox {:device "Google Nexus 5" :width 400 :height 800})
  (start-browser :ie {:device "Google Nexus 5" :width 400 :height 800})
  (start-browser :ie)
  (app/start-app)
  (app/login)

  (do
    (app/logout)

    ;; safe-wait-for-angular is killing the browser after a logout
    (do
      (get-browser)
      (wait-for-angular))

    (app/login "Nelle.Odom" "Nelle.Odom1"))

  (get-authenticated-identity)

  (app/navigate-to "Home")

  (app/enable-config-mode)
  (app/enable-app-toolbox)

  (app/logout)
  (stop-browser)

  (do-client-ready-check)

  (clojure.pprint/pp)

  (get-settings)
  @test-db
  (get-test-list)
  (run-tests [:qa-daily])
  (run-tests [:navigation-builder-smoke-test])
  (run-tests [:screen-builder-smoke-test-1])
  (run-tests [:edit-form/edit-form-4])

  (get-test-list)
  (map #(select-keys % [:id :name :doc]) (vals @test-db))
  (map #(-> % :id name) (get-test-list))

  (do
    (println "Suites:")
    (doseq [t (->> (get-test-list :testsuite)
                   (map #(-> % :id name)))]
      (println "  " t))
    (println "Tests:")
    (doseq [t (->> (get-test-list :test)
                   (map #(-> % :id name)))]
      (println "  " t)))

  (->> [["Suites" :testsuite] ["Tests" :test]]
       (map (fn [x]
              (println (first x) ":")
              (doseq [t (->> (get-test-list (second x))
                             (map #(-> % :id name)))]
                (println "  " t)))))

  (->> [["Suites" :testsuite] ["Test cases" :test]]
       (reduce (fn [c x]
                 (str c
                      (first x) ":" \newline
                      (->> (get-test-list (second x))
                           (map #(-> % :id name))
                           (interpose \newline)
                           (apply str))
                      \newline))
               ""))

  (->> (get-test-list :testscenario)
       (map #(-> % :id name))
       (interpose \newline)
       (apply str))
  )

(comment

  (-> (execute-async-script
        "
        var done = arguments[arguments.length -1];

        if (!window.angular) return 'null';

        var spReportService = angular.element('body').injector().get('spReportService');

        if (!spReportService) return 'null';

        spReportService.runPickerReport('resources', { resourceType: { value: 'test:condimentsEnum' }})
                  .then(function (result) {
                    done(JSON.stringify({result: result}));
                  }, function (err) {
                    done(JSON.stringify({error: err}));
                  });
        ")
      (json/read-json))

  (timeit "all" (do (-> (execute-async-script
                          "
                          var done = arguments[arguments.length -1];

                          if (!window.angular) return 'null';

                          var spReportService = angular.element('body').injector().get('spReportService');

                          if (!spReportService) return 'null';

                          spReportService.runPickerReport('resources', { resourceType: { value: 'core:resource' } })
                                    .then(function (result) {
                                      //done(JSON.stringify({result: result}));
                                      done(JSON.stringify({result: 1}));
                                    }, function (err) {
                                      done(JSON.stringify({error: err}));
                                    });
                          ")
                        (json/read-json))
                    1))

  )

(comment

  (app/logout)
  (stop-browser)

  (do
    (start-browser "chrome")
    (app/start-app)
    (app/login))

  (rt.scripts.edit-form/test-view-form)

  (app/enable-app-toolbox)

  (tb/set-object-filter "rt")

  (tb/get-object-names)

  (tb/create-report {:name        "RT-Report 1"
                     :object-type (first (tb/get-object-names))})

  (tb/create-report-via-toolbox-object {:name        "RT-Report 2"
                                        :object-type (first (tb/get-object-names))})

  (rb/select-field-in-report "Owned by" (not (rb/field-in-report? "Owned by")))
  (rb/field-in-report? "Created by")

  (rb/select-field-in-analyser "Owned by" (not (rb/field-in-analyser? "Owned by")))
  (rb/field-in-analyser? "Created by")

  (rb/get-analyser-fields-in-popup)
  (rb/get-analyser-fields-in-toolbox)
  (rb/get-fields-in-report-view)
  (rb/get-selected-fields-in-toolbox)

  (rb/show-column-context-menu "Owned by")
  (rb/choose-column-menu-item "Owned by" "Sort Z-A")
  (rb/choose-column-menu-item "Owned by" "Show Totals")

  (rb/select-field-in-analyser "Text 1")
  (rb/field-in-analyser? "Text 1")

  (tb/open-report-builder "AA_Drink" "AA_Drinks")
  (tb/choose-report-menu "AA_Drink" "AA_Drinks" "Chart")

  (rb/add-relationship "Rel 1")

  )

(comment

  (pprint (resolve-entity "test:employee" "id,name,isOfType.{alias,name},instancesOfType.{id,alias,name}"))
  (pprint (run-query {:root    {:id "test:employee"}
                      :selects [{:field "name"}]
                      :conds   []}))


  (timeit "hey"
          (clj-webdriver.taxi/execute-script
            "
            console.log('hello from RT');
            "))

  (clj-webdriver.taxi/execute-script
    "
    var svc = angular.element('body').injector().get('spDialogService');
    svc.showMessageBox('ReadiNow Automated Testing', 'La la la...');
    ")

  (element "div:first")
  (reduce (fn [xs x] (conj xs (:webelement x))) [] (elements ".navbar"))

  (let [args (reduce (fn [xs x] (conj xs (:webelement x))) [] (elements ".navbar"))
        args [".navbar" (first args)]]
    (count
      (clj-webdriver.taxi/execute-script
        "
        var q_or_e = arguments.length > 0 && arguments[0];
        var using = q_or_e || document;
        console.log('script called, args=', arguments);
        console.log('script called, using=', using);
        console.log('script called, args=', _.isString(q_or_e));
        console.log('script called, doc imgs=', $('img', document));
        console.log('script called, imgs=', $('img', q_or_e));
        return $('img', q_or_e);
        "
        args)))

  ;; attempt to get this working but no dice
  (let [from (first (filter #(= (text %) "Text") (elements "tr.fb-field div:contains(Text)")))
        to ".sp-form-builder-container-content"]
    (drag-n-drop from to {:preferredTarget "#insertIndicator"}))


  ;; hover to show configure tool, then choose it
  ;; turns out I only need the click... go figure
  (let [e (element ".fieldControl[test-id=New__Text__field]")
        actions (:actions clj-webdriver.taxi/*driver*)]
    (-> actions
        (.moveToElement (:webelement e))
        (.perform))
    (click (element ".fieldControl[test-id=New__Text__field] .sp-form-builder-adornment img[src*=icon_configure]")))

  (clj-webdriver.core/->actions
    clj-webdriver.taxi/*driver*
    (clj-webdriver.core/move-to-element (element ".fieldControl[test-id=New__Text__field]")))

  (click ".sp-form-builder-container.sp-form-builder-field-control:contains('Number') [ng-click*=Config]")

  )

(comment

  ;; exploring ways to find functions in a namespace based on metadata

  (let [ns 'rt.po.app]
    (->> (ns-publics (symbol ns))
         (filter #(:rt-action (meta (val %))))
         (map #(first %))))

  (type (first (keys (ns-publics 'rt.po.app))))

  (pprint (meta (var rt.po.app/top-menu-name)))
  (meta app/top-menu-name)
  (meta 'app/top-menu-name)
  (meta #'app/top-menu-name)

  (str (:ns (meta (second (first (ns-publics 'rt.po.app))))))

  (let [fns (ns-publics 'rt.po.app)
        x (first fns)]
    (meta (ns-resolve 'rt.po.app (key x)))
    #_(pprint (meta (var (symbol (first fns))))))

  (let [fns (ns-publics 'rt.po.app)
        reducer #(let [m (meta (ns-resolve 'rt.po.app (key %2)))]
                  (conj %1 m))]
    (map #(hash-map (str (:ns %) "/" (:name %)))
         (reduce reducer [] fns)))

  (let [rt-namespace? (fn [ns] (= 0 (.indexOf (str ns) "rt.po.app")))
        get-fn-meta (fn [ns] (map #(meta (ns-resolve ns (key %))) (ns-publics ns)))
        has-rt-key? (fn [m] (some #(= 0 (.indexOf (name %) "rt-")) (keys m)))]
    (->> (all-ns)
         (filter rt-namespace?)
         (mapcat get-fn-meta)
         #_(filter has-rt-key?)
         ;; ensure all fields are in all objects for the write csv function
         (map #(merge {:rt-action false :rt-query false :rt-completed nil :rt-tags nil} %))
         ;; the conj bit is needed so we get the keys in the expected order
         (map #(conj {} (select-keys % [:ns :name :doc :arglists :rt-action :rt-query :rt-completed :rt-tags])))
         (sort-by :name)
         (sort-by #(str :ns %))
         (write-csv-objects "app-driver-functions.csv")
         #_(clojure.pprint/print-table)))

  (filter #(:rt-action (meta %)) (vals (ns-publics 'rt.po.app)))

  (->> (ns-publics 'rt.po.app)
       (vals)
       (filter #(:rt-action (meta %)))
       (count))

  (->> (ns-publics 'rt.po.app)
       (filter #(:rt-action (meta (val %))))
       (map #(first %))
       (pprint))

  (let [ns "rt.po.app"]
    (->> (ns-publics (symbol ns))
         (filter #(:rt-action (meta (val %))))
         (map #(hash-map :name (first %) :meta (meta (second %))))
         (pprint)))

  (let [ns "rt.po.app"]
    (->> (ns-publics (symbol ns))
         (filter #(:rt-action (meta (val %))))
         (map #(first %))
         (pprint)))

  (pprint (get-rt-registry))

  (let [ns "rt.po.app"
        publics (ns-publics (symbol ns))
        f (first publics)]
    (pprint (meta (var-get (val f))))
    (pprint (meta (val f))))

  (defn foo "hey" {:test-query true
                   :parameters ["username" "password"]} [p1 p2] 99)
  (meta (var foo))

  (macroexpand '(defn foo "hey" [] 99))

  )

(comment

  (do
    (start-browser "chrome")
    (app/start-app)
    (app/login))

  (clj-webdriver.taxi/execute-script
    "
    console.time('listenAllEvents');
    window.rats = window.rats || {};
    var elements = document.querySelectorAll('*'),
        i, length = elements.length,
        eventListener = window.rt.eventListener;
    if (eventListener) {
      console.log('removing handler on node count', length);
      for (i = 0; i < length; i += 1) {
        elements[i].removeEventListener('click', eventListener);
      }
    }
    var cssPath = function(el) {
        if (!(el instanceof Element))
            return;
        var path = [];
        while (el.nodeType === Node.ELEMENT_NODE) {
            var selector = el.nodeName.toLowerCase();
            if (el.id) {
                selector += '#' + el.id;
                path.unshift(selector);
                break;
            } else {
                var sib = el, nth = 1;
                while (sib = sib.previousElementSibling) {
                    if (sib.nodeName.toLowerCase() == selector)
                       nth++;
                }
                if (nth != 1)
                    selector += ':nth-of-type('+nth+')';
            }
            path.unshift(selector);
            el = el.parentNode;
        }
        return path.join(' > ');
    }
    eventListener = window.rt.eventListener = function (event) {
      console.log('event',
        'type', event.type,
        'target', event.target,
        'which', event.which,
        'src', event.srcElement === event.target || event.srcElement,
        'to', event.toElement === event.target || event.toElement,
        'event', event);
      console.log('event', event.type, cssPath(event.target));
    }
    console.log('adding handler on node count', length);
    for (i = 0; i < length; i += 1) {
      elements[i].addEventListener('click', eventListener);
    }
    console.timeEnd('listenAllEvents');
    ")

  (do
    (console-log "clicking....")
    (click ".sp-tool-box:first")
    (console-log "click done"))

  )

(comment

  (pprint (run-query {:root    {:id      "console:screen"
                                :related []}
                      :selects [{:field "name"}]
                      :conds   [{:expr {:field "name"} :oper "equal" :val "2. Dog Breeds"}]}))

  (-> {:root    {:id "console:screen"}
       :selects [{:field "name"}]
       :conds   []}
      (run-query)
      (query-results-as-objects)
      (pprint))

  (let [rr {:result
            {:cols
             [{:title "_Id", :type "Identifier"} {:title "Name", :type "String"}],
             :data
             [{:id 3736, :item [{:value "3736"} {:value "Demo Screen - Chart/Detail"}]}
              {:id 3780, :item [{:value "3780"} {:value "Demo Screen - Scientists"}]}
              {:id 4149, :item [{:value "4149"} {:value "Demo Screen - Master/Detail"}]}
              {:id 4363, :item [{:value "4363"} {:value "Demo Screen - Chart on form"}]}
              {:id 6354, :item [{:value "6354"} {:value "2. Dog Breeds"}]}
              {:id 8100, :item [{:value "8100"} {:value "1. Scientists Screen"}]}
              {:id 14670, :item [{:value "14670"} {:value "Screen-20140804-084134"}]}
              {:id 14677, :item [{:value "14677"} {:value "Screen-20140804-091412"}]}]}}
        cols (get-in rr [:result :cols])
        cols (map :title cols)
        rows (get-in rr [:result :data])
        rows (map (fn [x] (map :value (:item x))) rows)]
    (pprint cols)
    (pprint rows)

    (let [keys (map #(keyword (string/trim %)) cols)]
      (reduce (fn [result data]
                (conj result (zipmap keys (map #(string/trim %) data))))
              []
              rows)))

  )

(comment

  (let [events [{:type :start-suite}
                {:type :start-test, :test {:id :screen-builder-smoke-test-1}}
                {:type :pass, :message nil}
                {:type :end-test, :test {:id :screen-builder-smoke-test-1}, :summary {:error 0, :fail 0, :pass 1}}
                {:type :start-test, :test {:id :screen-builder-smoke-test-2}}
                {:type :end-test, :test {:id :screen-builder-smoke-test-2}, :summary {:error 0, :fail 0, :pass 2}}
                {:type :end-suite, :summary {:fail 0, :error 1, :pass 8, :test 6}}
                {:type :start-suite :id 123} {:type :end-suite}]]
    (pprint (reduce (fn [a b]
                      ;;(println "-------------") (pprint a) (pprint b)
                      (condp = (:type b)
                        :start-suite (assoc a :suites (vec (conj (:suites a) b)))
                        :start-test (let [suites (:suites a)
                                          suite (-> suites (last))
                                          tests (-> suite (:tests))]
                                      (->> b (conj tests) (vec) (assoc suite :tests)
                                           (conj (butlast suites)) (vec) (assoc a :suites)))
                        a)) {} events)))

  )

(comment

  (require '[datomic.api :as d])
  (def uri "datomic:dev://localhost:4334/hello"  #_"datomic:mem://hello")
  (def uri "datomic:mem://hello")
  (d/create-database uri)
  (def conn (d/connect uri))
  (def tx-result (d/transact conn
                             [[:db/add
                               (d/tempid :db.part/user)
                               :db/doc
                               "Hello world"]]))
  (def dbval (d/db conn))
  (def q-result (d/q '[:find ?e
                       :where [?e :db/doc "Hello world"]]
                     dbval))
  (def ent (d/entity dbval (ffirst q-result)))
  (d/touch ent)

  )

;; questions on the schema
;; - should i use the same attribute for multiple entities, for example have a :entity/doc attr
;; rather than a doc for each? Ditto id

(def step-schema

  [{:db/doc                "index of the script"
    :db/ident              :step/index
    :db/valueType          :db.type/long
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   ;; is either a script ref

   {:db/doc                "a step of a script or test"
    :db/ident              :step/script
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   ;; OR the expressions that is clojure code to eval

   {:db/doc                "text of the script"
    :db/ident              :step/expr
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "description of the script"
    :db/ident              :step/doc
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "target time for the step"
    :db/ident              :step/target-msecs
    :db/valueType          :db.type/long
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(def script-schema
  [{:db/doc                "script id ... optional"
    :db/ident              :script/id
    :db/valueType          :db.type/keyword
    :db/cardinality        :db.cardinality/one
    :db/unique             :db.unique/identity
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "description of the script"
    :db/ident              :script/doc
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "script steps"
    :db/ident              :script/steps
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/isComponent        true
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(def fixture-schema
  [{:db/doc                "fixture id"
    :db/ident              :fixture/id
    :db/valueType          :db.type/keyword
    :db/cardinality        :db.cardinality/one
    :db/unique             :db.unique/identity
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "name of the fixture"
    :db/ident              :fixture/name
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "description of the fixture"
    :db/ident              :fixture/doc
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "data of the fixture - should be simple map of name value pairs"
    :db/ident              :fixture/data
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "steps to run before each test"
    :db/ident              :fixture/beforeEach
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/isComponent        true
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "steps to run after each test"
    :db/ident              :fixture/afterEach
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/isComponent        true
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(def test-schema
  [{:db/doc                "test fixture ref"
    :db/ident              :testFixture/fixture
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "test fixture position"
    :db/ident              :testFixture/index
    :db/valueType          :db.type/long
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "test id"
    :db/ident              :test/id
    :db/valueType          :db.type/keyword
    :db/cardinality        :db.cardinality/one
    :db/unique             :db.unique/identity
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "name of the test"
    :db/ident              :test/name
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "description of the test"
    :db/ident              :test/doc
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "fixtures to run this test in - refs to testFixtures"
    :db/ident              :test/fixtures
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/isComponent        true
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "the test steps"
    :db/ident              :test/steps
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/isComponent        true
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(def suite-schema
  [{:db/doc                "suite id"
    :db/ident              :suite/id
    :db/valueType          :db.type/keyword
    :db/cardinality        :db.cardinality/one
    :db/unique             :db.unique/identity
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "name of the suite"
    :db/ident              :suite/name
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "description of the suite"
    :db/ident              :suite/doc
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "fixtures to apply once for all contained tests"
    :db/ident              :suite/onceFixtures
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "fixtures to apply for each of the contained tests"
    :db/ident              :suite/eachFixtures
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "the contained test and suites"
    :db/ident              :suite/tests
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(def testevent-schema
  [{:db/doc                "date and time of the event"
    :db/ident              :testevent/when
    :db/valueType          :db.type/instant
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "related entity"
    :db/ident              :testevent/entity
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "result"
    :db/ident              :testevent/result
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "output"
    :db/ident              :testevent/output
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(def testplan-schema
  [{:db/doc                "testplan id"
    :db/ident              :testplan/id
    :db/valueType          :db.type/keyword
    :db/cardinality        :db.cardinality/one
    :db/unique             :db.unique/identity
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "created date of the testplan"
    :db/ident              :testplan/created
    :db/valueType          :db.type/instant
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "output folder name"
    :db/ident              :testplan/dirName
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "the tests and suites"
    :db/ident              :testplan/tests
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "test events, mainly the results of executing scripts"
    :db/ident              :testplan/events
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(defn make-step-tx [step]
  (cond
    ;; a script id
    ;; todo - need to fixup any steps that have :step/script so these attrs
    ;; are the :db/id rather than :script/id
    (keyword? step) {:db/id       (d/tempid :db.part/user)
                     :step/script step}
    ;; the map with the expr
    (map? step) (merge {:db/id     (d/tempid :db.part/user)
                        :step/expr (:script step)}
                       (when (:doc step) {:step/doc (:doc step)})
                       (when (:target-msecs step) {:step/target-msecs (:target-msecs step)}))
    ;; a plain string that is the expr
    (string? step) {:db/id     (d/tempid :db.part/user)
                    :step/expr step}
    :else nil))

(defn default-on-nil [v d]
  (if (nil? v) d v))

(defn make-steps-tx [steps]
  (->> steps
       (map make-step-tx)
       (filter identity)                                    ; remove nil entries
       (map-indexed #(assoc %2 :step/index %1))))

(defn make-script-tx [{:keys [id doc steps]}]
  (let [steps-tx (make-steps-tx steps)]
    (conj steps-tx (merge {:db/id        (d/tempid :db.part/user)
                           :script/id    id
                           :script/steps (mapv :db/id steps-tx)}
                          (when doc {:script/doc doc})))))

(defn make-fixture-tx [{:keys [id name doc data setup teardown]}]
  (let [setup-steps-tx (make-steps-tx setup)
        teardown-steps-tx (make-steps-tx teardown)]
    (concat setup-steps-tx
            teardown-steps-tx
            [(merge {:db/id              (d/tempid :db.part/user)
                     :fixture/id         id
                     :fixture/name       (default-on-nil name "")
                     :fixture/data       (prn-str data)
                     :fixture/beforeEach (mapv :db/id setup-steps-tx)
                     :fixture/afterEach  (mapv :db/id teardown-steps-tx)}
                    (when doc {:fixture/doc doc}))])))

(defn make-test-tx [{:keys [id name doc fixtures steps]}]
  ;; note - need to fix up the fixtures with their :db/id rather than our :fixture/id
  (let [steps-tx (make-steps-tx steps)
        test-fixtures (map #(identity {:db/id               (d/tempid :db.part/user)
                                       :testFixture/fixture %1
                                       :testFixture/index   %2})
                           fixtures (range))]
    (concat steps-tx
            test-fixtures
            [(merge {:db/id         (d/tempid :db.part/user)
                     :test/id       id
                     :test/name     (default-on-nil name "")
                     :test/steps    (mapv :db/id steps-tx)
                     :test/fixtures (mapv :db/id test-fixtures)}
                    (when doc {:test/doc doc}))])))

(defn make-suite-tx [{:keys [id name doc once-fixtures each-fixtures tests]}]
  ;; note - need to fix up the fixtures with their :db/id rather than our :fixture/id
  ;; ditto for tests
  (let []
    (concat []
            [(merge {:db/id              (d/tempid :db.part/user)
                     :suite/id           id
                     :suite/name         (default-on-nil name "")
                     :suite/tests        tests
                     :suite/onceFixtures once-fixtures
                     :suite/eachFixtures each-fixtures}
                    (when doc {:suite/doc doc}))])))

(defn get-ref-for-id [scripts-tx id-key id]
  (if (keyword? id)
    (if-let [tx-data (first (filter #(= (id-key %) id) scripts-tx))]
      (:db/id tx-data)
      nil)
    id))

(defn get-tx-entity-type [e] (cond
                               (:script/id e) :script
                               (:fixture/id e) :fixture
                               (:test/id e) :test
                               (:suite/id e) :suite
                               (or (:step/expr e) (:step/script e)) :step
                               (:testFixture/fixture e) :test-fixture
                               :default nil))

(defmulti fixup-refs #(get-tx-entity-type %2))

(defmethod fixup-refs :step [scripts-tx step]
  (if-let [script-id (:step/script step)]
    (if-let [script-tx (first (filter #(= (:script/id %) script-id) scripts-tx))]
      (assoc step :step/script (:db/id script-tx))
      (-> step
          (assoc :step/expr (str "(comment failed to find script \"" script-id "\")"))
          (dissoc :step/script)))
    step))

(defmethod fixup-refs :script [scripts-tx script]
  script)

(defmethod fixup-refs :fixture [scripts-tx fixture]
  fixture)

(defmethod fixup-refs :test-fixture [scripts-tx test-fixture]
  (assoc test-fixture :testFixture/fixture (get-ref-for-id scripts-tx :fixture/id (:testFixture/fixture test-fixture))))

(defmethod fixup-refs :test [scripts-tx test]
  test)

(defmethod fixup-refs :suite [scripts-tx suite]
  (assoc suite :suite/tests (mapv #(get-ref-for-id scripts-tx :test/id %) (:suite/tests suite))
               :suite/onceFixtures (mapv #(get-ref-for-id scripts-tx :fixture/id %) (:suite/onceFixtures suite))
               :suite/eachFixtures (mapv #(get-ref-for-id scripts-tx :fixture/id %) (:suite/eachFixtures suite))))

(defmethod fixup-refs nil [_ e]
  e)

(comment

  (def uri "datomic:mem://readiTest")
  (d/delete-database uri)
  (do
    (d/create-database uri)
    (def conn (d/connect uri))
    (d/transact conn (concat step-schema script-schema
                             fixture-schema test-schema suite-schema
                             testevent-schema testplan-schema)))

  (rt.app/setup-environment {:app-url "https://sg-mbp-2013.local"})
  (rt.app/setup-environment)

  (do
    (init-test-db)
    (init-test-runs-db)
    (add-tests [{:id    :script1
                 :type  :testscript
                 :steps ["(println \"hey from script1\")"]}
                {:id    :fixture1
                 :type  :testfixture
                 :data  {:tenant "EDC"}
                 :setup ["(println \"fixture1 setup\")"
                         :script1]}
                {:id       :fixture2
                 :type     :testfixture
                 :teardown ["(println \"fixture2 teardown\")"]}
                {:id    :t1
                 :type  :test
                 :data  {:tenant "EDC"}
                 :setup ["(println \"SETUP SCRIPT\")"
                         "(identity {:a 1 :b 2})"]
                 :steps [{:script "(println \"STEP 1\")" :doc "first step in my test"}
                         :script1
                         "(expect-equals 1 2)"]}
                {:id       :t2
                 :type     :test
                 :fixtures [:fixture1 :fixture2]
                 :steps    ["(println \"STEP a\")"
                            "(samples/sample-test-script-1 \"STEP b\")"
                            "(samples/sample-test-query-1 \"STEP c\")"
                            "(expect-equals 1 1)"]}
                {:id       :s1
                 :name     "suite1"
                 :type     :testsuite
                 :tests    [:t1 :t2]
                 :teardown []}]))

  (do
    (def scripts-tx
      (->> (vals @test-db)
           (filter #(= :testscript (:type %)))
           (mapcat make-script-tx)))

    (def fixtures-tx
      (->> (vals @test-db)
           (filter #(= :testfixture (:type %)))
           (mapcat make-fixture-tx)))

    (def tests-tx
      (->> (vals @test-db)
           (filter #(= :test (:type %)))
           (mapcat make-test-tx)))

    (def suites-tx
      (->> (vals @test-db)
           (filter #(= :testsuite (:type %)))
           (mapcat make-suite-tx)))

    (def all-entities-tx (concat scripts-tx fixtures-tx tests-tx suites-tx))
    (def fixup-entities (partial fixup-refs all-entities-tx))
    (def all-fixed-up (map fixup-entities all-entities-tx)))

  (pprint scripts-tx)
  (pprint (map fixup-entities scripts-tx))
  (pprint fixtures-tx)
  (pprint tests-tx)
  (pprint (map fixup-entities tests-tx))
  (pprint suites-tx)
  (pprint (map fixup-entities suites-tx))
  (pprint all-entities-tx)
  (pprint all-fixed-up)

  (def tx-result @(d/transact conn all-fixed-up))

  (->> (d/q '[:find ?id ?expr ?i
              :where
              [?s :step/expr ?expr]
              [?s :step/index ?i]
              [?sc :script/steps ?s]
              [?sc :script/id ?id]]
            (d/db conn))
       (map #(hash-map :id (first %) :expr (second %) :index (get % 2)))
       (take 10)
       print-table)

  (->> (d/q '[:find ?id
              :where [_ :script/id ?id]]
            (d/db conn))
       (map #(hash-map :id (first %)))
       (take 10)
       print-table)

  (->> (d/q '[:find ?id ?expr ?index
              :where
              [?t :test/id ?id]
              [?t :test/steps ?step]
              [?step :step/expr ?expr]
              [?step :step/index ?index]]
            (d/db conn))
       (map #(hash-map :id (first %) :expr (nth % 1) :index (nth % 2)))
       (take 10)
       print-table)

  ;; get test entity with its steps included inline, but do not fully expand script or fixtures


  (->> (d/q '[:find ?id ?expr ?index
              :in $ ?id
              :where
              [?t :test/id ?id]
              [?t :test/steps ?step]
              [?step :step/expr ?expr]
              [?step :step/index ?index]]
            (d/db conn) :t2)
       (map #(hash-map :id (first %) :expr (nth % 1) :index (nth % 2)))
       (sort-by :index)
       (take 10)
       print-table)

  (let [db (d/db conn)
        test-id :t2
        result (d/q '[:find ?t :in $ ?id :where [?t :test/id ?id]] db test-id)
        test (d/entity db (ffirst result))
        test (d/touch test)
        ]
    ;(pprint test)
    (->
      (assoc (into {} test) :test/steps (->> test :test/steps (sort-by :step/index) (mapv identity))
                            :test/fixtures (->> test :test/fixtures
                                                (sort-by :testFixture/index)
                                                (mapv #(get-in % [:testFixture/fixture :fixture/id]))))
      (pprint)))

  #_(->> (d/q '[:find ?step
                         :in $ ?id
                         :where
                         [?t :test/id ?id]
                         [?t :test/steps ?step]]
                       db test-id)
                  ;(map #(hash-map :id (first %) :expr (nth % 1) :index (nth % 2)))
                  ;(sort-by :index)
                  ;(take 10)
                  ;print-table
                  pprint
                  )
  (pp)
  )


(comment

  ;; playing with loading drivers at runtime

  (require '[clojure.tools.namespace.find :as f])
  (f/find-namespaces-in-dir (clojure.java.io/file "test-db"))
  (f/find-clojure-sources-in-dir (clojure.java.io/file "test-db"))
  (f/clojure-sources-in-jar (java.util.jar.JarFile. "rn-drivers-0.1.0.jar"))

  )