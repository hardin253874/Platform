(ns rt.app
  (:require-macros [reagent.ratom :refer [reaction]]
                   [rt.lib.cljs-macros :refer [handler-fn with-subs]])
  (:require rt.source
            rt.run
            rt.report
            rt.edit
            [rt.common :refer [reset-server-button]]
            [rt.basic-components :refer [simple-props-table]]
            [rt.store :as store]
            [rt.util :refer [url-encode get-datetime-str id->str]]
            [reagent.core :as r :refer [atom]]
            [re-frame.core :refer [subscribe dispatch dispatch-sync register-sub register-handler]]
            [secretary.core :as secretary :include-macros true :refer-macros [defroute]]
            [goog.events :as events]
            [goog.history.EventType :as EventType]
            [goog.string :as gstring]
            [goog.string.format]
            [clojure.string :as string]
            [clairvoyant.core :as trace :include-macros true])
  (:import goog.History))

(defn time-now []
  (.now js/Date))

;; -------------------------
;; Queries

(register-sub
  :get-in
  (fn [db [_ path]]
    (reaction (get-in @db path))))

(register-sub
  :route
  (fn [db _]
    (reaction (get-in @db [:route]))))

(register-sub
  :show-help
  (fn [db _]
    (reaction (get-in @db [:show-help]))))

(register-sub
  :app-settings
  (fn [db _]
    (reaction (get-in @db [:app-settings]))))

(register-sub
  :app-status
  (fn [db _]
    (reaction (get-in @db [:app-status]))))

(register-sub
  :app-alert
  (fn [db _]
    (reaction (get-in @db [:app-alert]))))

(register-sub
  :app-busy?
  (fn [db _]
    (reaction (let [first-busy (first (get @db :app-busy))]
                ;(println "busy time" (and first-busy (- (time-now) first-busy)))
                (and first-busy
                     (> (- (time-now) first-busy)
                        400))))))

(register-sub
  :step-summary
  (fn [db _]
    (reaction (get-in @db [:step-summary :value]))))

;; -------------------------
;; Event handlers

(register-handler
  :update-in
  (fn [db [_ path value]]
    (println "update-in" path value "old=" (pr-str (get-in db path)))
    (assoc-in db path value)))

(register-handler
  :route
  (fn [db [_ page params]]
    (assoc db :route {:page page :params params})))

(register-handler
  :show-help
  (fn [db [_ feature]]
    (assoc-in db [:show-help] feature)))

(register-handler
  :update-app-settings
  (fn [db [_ key value]]
    (println "update app settings" key value)
    (let [db (if key (assoc-in db [:app-settings key] value)
                     (assoc-in db [:app-settings] value))]
      ;; now only saving to server with a save button
      #_(store/put-app-settings (:app-settings db))
      db)))

(register-handler
  :app-status
  (fn [db [_ status]]
    (assoc db :app-status status)))

(register-handler
  :app-alert
  (fn [db [_ status]]
    (assoc db :app-alert status)))

(register-handler
  :push-busy
  (fn [db [_]]
    (println "push-busy" (:app-busy db))
    (when (empty? (:app-busy db))
      (.setTimeout js/window #(dispatch [:tickle-busy]) 500))
    (assoc db :app-busy (conj (vec (:app-busy db)) (time-now)))))

(register-handler
  :pop-busy
  (fn [db [_]]
    (println "pop-busy" (:app-busy db))
    (assoc db :app-busy (drop-last (:app-busy db)))))

(register-handler
  :tickle-busy
  (fn [db [_]]
    (let [busy (:app-busy db)]
      ;; tickle the busy state to trigger a is-busy? check
      (println "tickle-busy" busy)
      (cond-> db
              (not (empty? busy)) (do
                                    (dispatch [:pop-busy])
                                    (assoc db :app-busy (conj (vec busy) (time-now))))))))

(register-handler
  :watch-step-summary
  (fn [db [_]]
    (println "watching step-summary")
    (or (when-not (#{:requesting :poll-wait} (get-in db [:step-summary :state]))
          (dispatch [:request-step-summary]))
        db)))

(register-handler
  :cancel-watch-step-summary
  (fn [db [_]]
    (println "cancel watching step-summary")
    (assoc db :step-summary {:state :idle})))

(register-handler
  :request-step-summary
  (fn [db [_]]
    (or (when-not (#{:requesting} (get-in db [:step-summary :state]))
          (store/get-step-summary #(dispatch [:update-step-summary %]))
          (assoc-in db [:step-summary :state] :requesting))
        db)))

(register-handler
  :update-step-summary
  (fn [db [_ value]]
    (or (when (#{:requesting} (get-in db [:step-summary :state]))
          (.setTimeout js/window #(dispatch [:request-step-summary]) 2000)
          (assoc db :step-summary {:value value :state :poll-wait}))
        db)))


;; ---------------------------
;; Views

(defn bgc-str [& [c]]
  (let [c (or c (map rand-int (repeat 3 255)))
        pad #(if (= 1 (count %)) (str "0" %) %)
        int->hex #(.toString % 16)]
    (str "#" (apply str (map (comp pad int->hex) c)))))

;(trace/trace-forms
;  {:tracer trace/default-tracer}
; stuff goes here
;}

(defn comp2 []
  (with-subs [v [:some-value]]
             [:div v]))


(defn comp1 [_ text]
  (let [s (r/atom text)]
    (fn [_ text]
      [:div @s "." text])))

(defn home-page []
  (let [n (r/atom 0)]
    (fn []
      [:div {:style {:flex           "1"
                     :display        :flex
                     :flex-direction :column}}

       [:div {:style {:flex             "1 1 0"
                      :background-color (bgc-str)}}
        #_(apply str (repeat 100 "Section A "))
        [:button {:onClick (handler-fn (reset! n (rand-int 3)))} "next"]
        [:div @n]
        [comp1 {} "hello"]
        [comp1 {} "out"]
        [comp1 {} "there"]
        (condp = @n 0 [comp1 {:key 1} "hello"]
                    1 [comp1 {:key 2} "out"]
                    2 [comp1 {:key 3} "there"]
                    [comp1 {} "default"])]
       [:div {:style {:flex             "1 1 0"
                      :background-color (bgc-str)}}
        (apply str (repeat 200 "Section B "))]
       [:div {:style {:flex             "1 1 0"
                      :background-color (bgc-str)}}
        (apply str (repeat 10 "Section C "))]
       [:div {:style {:flex             "1 1 0"
                      :background-color (bgc-str)}}
        (apply str (repeat 100 "Section D "))]])))

(defn settings-page []
  (let [app-settings (subscribe [:app-settings])
        app-url (reaction (:app-url @app-settings))
        test-ids (reaction (:test @app-settings))]
    (store/get-app-settings #(dispatch [:update-app-settings nil %]))
    (fn []
      [:div.settings-page
       [:h1 "Settings"]
       [:div
        [:div "to be styled... obviously..."]
        [:label "App URL"
         [:input {:value @app-url :onChange #(dispatch-sync [:update-app-settings :app-url (.. % -target -value)])}]]
        [:label "Test"
         [:input {:value @test-ids :onChange #(dispatch-sync [:update-app-settings :test (.. % -target -value)])}]]
        [:button.button {:onClick #(store/put-app-settings @app-settings)} "save"]
        [reset-server-button]]
       #_[:pre.wrap \newline \newline (pr-str @app-settings)]
       [:div {:style {:margin-top "10px"}}
        "for debugging..."
        [simple-props-table (map pr-str (mapcat identity @app-settings))]]])))

(defn- format-float [n]
  (if n (gstring/format "%.1f" n) ""))

(defn- pct [v1 v2]
  (* (/ (- v2 v1) v1) 100))

(defn- trunc
  [s n]
  (subs s 0 (min (count s) n)))

(defn- get-session-data [data]
  (let [active-filter #(let [t (when (not-empty (:last-reported %))
                                 (.parse js/Date (:last-reported %)))]
                        (when t (< (- (long (js/Date.)) t) (* 5 60 1000))))]
    (->> (vals (:sessions data))
         (mapcat vals)
         (filter active-filter)
         (sort-by :session-id)
         (sort-by :host))))

(defn- get-steps-data [data declutter? metricsonly?]
  (let [tdata (->> (vals (:tests data))
                   (mapcat vals)
                   (sort-by :index)
                   (sort-by :test-id))]
    (cond-> tdata
            declutter? (->> (remove #(or (< (:wavg %) 0.5)
                                         (some-> % :expr (.startsWith "(think"))
                                         (neg? (:index %))))
                            (map #(assoc % :expr (some-> % :expr (string/replace #"(rt\..*?/)|(rn\..*?/)" "")))))
            metricsonly? (->> (filter #(or (:metric-id %) (:target-msec %) (:errors %)))
                              (map #(assoc % :expr (some-> % :expr (string/replace #"(rt\..*?/)|(rn\..*?/)" ""))))))))

(defn get-durations [xs]
  (map #(/ (:last-test-duration %) 1000) xs))

(defn avg-duration [xs]
  (let [t (reduce + 0 (get-durations xs))
        n (count xs)]
    (if (pos? n) (/ t n) 0)))

(defn min-duration [xs]
  (reduce min (get-durations xs)))

(defn max-duration [xs]
  (reduce max (get-durations xs)))

(defn step-summary-page [route]
  (let [data (subscribe [:step-summary])
        declutter (reaction (get-in @route [:params :query-params :declutter]))
        metricsonly (reaction (get-in @route [:params :query-params :metricsonly]))
        app-settings (subscribe [:app-settings])
        request-settings (fn [] (store/get-app-settings #(dispatch [:update-app-settings nil %])))
        show-steps (r/atom true)]
    (request-settings)
    (fn []
      (let [session-data (get-session-data @data)
            metrics-data (->> (vals (:metrics @data)) (sort-by :index) (sort-by :test-id))
            steps-data (get-steps-data @data @declutter @metricsonly)]

        [:div.status-page
         [:div.status-header
          [:div.status-buttons {:style {:float :right :margin-top "3px"}}
           [:label "show steps"
            [:input {:type      :checkbox
                     :checked   @show-steps
                     :on-change (handler-fn
                                  (println "help " (pr-str (.. event -target -value)))
                                  (swap! show-steps not))}]]
           [:button {:on-click #(store/eval-expr "(rt.server/write-step-summary-csv)"
                                                 (handler-fn (println "saved")))
                     :title    "save to run-summary.csv file in the RT server's folder"} "save to csv"]
           [:button {:on-click #(store/eval-expr "(reset! rt.server/posted-step-summary {})"
                                                 (handler-fn (println "reset")))
                     :title    "reset stats"} "reset"]
           [:button {:on-click #(store/eval-expr "(rt.server/take-step-stats-baseline)"
                                                 (handler-fn (println "done")))
                     :title    "save to run-summary.csv file in the RT server's folder"} "take baseline"]
           [:button {:on-click #(do (store/eval-expr "(rt.setup/update-settings {:repeat (not (:repeat (rt.setup/get-settings)))})"
                                                     (handler-fn (println "done")))
                                    (.setTimeout js/window request-settings 2000))
                     :title    "toggle repeat setting - if on tells the runners to repeat their tests"}
            (if (:repeat @app-settings) "turn repeat off" "turn repeat on")]]
          [:div.status-metric
           [:label "Active users:"] [:span (count session-data)]]
          [:div.status-metric {:style {:font-size :smaller}}
           [:label "Updates:"] [:span (:update-count @data)]]]
         [:div.status-content

          (when (and (not @declutter) (not-empty session-data))
            [:table.stats
               [:thead [:tr
                        [:th "user id"]
                        [:th "test"]
                        [:th "step"]
                        [:th "updated"]]]
               (apply vector :tbody
                      (map #(vector :tr
                                    [:td (str (:host %) "-" (or (:session-id %) ""))]
                                    [:td (:last-test-id %)]
                                    [:td (:last-step-index %)]
                                    [:td (:last-reported %)])
                           session-data))])

          (when (not-empty session-data)
            (let [tdata (group-by :last-test-id session-data)]
              [:table.stats
               [:thead [:tr
                        [:th "test"]
                        [:th "active users"]
                        [:th "avg duration"]
                        [:th "min duration"]
                        [:th "max duration"]]]
               (apply vector :tbody
                      (map #(vector :tr
                                    [:td (str (first %))]
                                    [:td (count (second %))]
                                    [:td (format-float (avg-duration (second %)))]
                                    [:td (format-float (min-duration (second %)))]
                                    [:td (format-float (max-duration (second %)))])
                           tdata))]))

          (when (not-empty metrics-data)
            [:table.stats
             [:thead [:tr
                      [:th "metric"]
                      [:th "expr"]
                      [:th.align-right "count"]
                      [:th.align-right "min"]
                      [:th.align-right "max"]
                      [:th.align-right "avg"]
                      [:th.align-right "wavg"]]]
             (apply vector :tbody
                    (map #(vector :tr
                                  [:td (:metric-id %)]
                                  [:td (or (:expr %) "")]
                                  [:td.align-right (:count %)]
                                  [:td.align-right (:min %)]
                                  [:td.align-right (:max %)]
                                  [:td.align-right (Math/floor (:avg %))]
                                  [:td.align-right (Math/floor (:wavg %))])
                         metrics-data))])

          (when (and @show-steps (not-empty steps-data))
            [:table.stats
             [:thead [:tr
                      [:th "test"]
                      [:th "index"]
                      [:th "expr"]
                      [:th.align-right "count"]
                      [:th.align-right "min"]
                      [:th.align-right "max"]
                      [:th.align-right "avg"]
                      (when-not @declutter [:th.align-right "wavg"])
                      (when-not @declutter [:th.align-right "cavg"])
                      (when-not @declutter [:th.align-right "rate"])
                      [:th.align-right "target"]
                      [:th.align-right "errors"]
                      (when-not @declutter [:th.align-right "base"])
                      (when-not @declutter [:th.align-right "delta"])]]
             (apply vector :tbody
                    (map #(let [baseline (or (:baseline %) (-> % :values first :time))
                                expr (or (:expr %) "")
                                target (:target-msec %)
                                time (* 1000 (:cavg %))]
                           (vector :tr
                                   [:td (some-> (:test-id %) (string/split #"/") last)]
                                   [:td (:index %)]
                                   [:td {:title expr} (trunc expr (if @declutter 50 200))]
                                   [:td.align-right (:count %)]
                                   [:td.align-right (format-float (:min %))]
                                   [:td.align-right (format-float (:max %))]
                                   (when-not @declutter [:td.align-right (format-float (:avg %))])
                                   (when-not @declutter [:td.align-right (format-float (:wavg %))])
                                   [:td.align-right
                                    {:title (str "target: " target)
                                     :class (cond
                                              (nil? target) ""
                                              (< time target) "time-great"
                                              (< time (* target 1.2)) "time-ok"
                                              (< time (* target 2)) "time-just"
                                              :default "time-unacceptable")}
                                    (format-float (:cavg %))]
                                   (when-not @declutter [:td.align-right (format-float (:rate %))])
                                   [:td.align-right (when target (format-float (/ target 1000)))]
                                   [:td.align-right (:errors %)]
                                   (when-not @declutter [:td.align-right (format-float baseline)])
                                   (when-not @declutter [:td.align-right (format-float (pct baseline (:cavg %)))])))
                         steps-data))])]]))))


(def pages {"report"        #'rt.report/report-page
            "edit"          #'rt.edit/edit-page
            "new"           #'rt.edit/edit-page
            "source"        #'rt.source/source-edit-page
            "source-list"   #'rt.source/source-list-page
            "run"           #'rt.run/run-page
            "run-test-view" #'rt.run/run-page-test-view
            "run-step-view" #'rt.run/run-page-step-view
            "settings"      #'settings-page
            "home"          #'home-page
            "step-summary"  #'step-summary-page
            "default"       #'rt.report/report-page})

(defn app-page [route]
  (println "render - app-page")
  (dispatch [:app-alert nil])
  (dispatch [:app-status nil])
  [:div.app-page
   (if-let [page (get pages (:page @route))]
     [page route]
     "")])

(defn app-links [route]
  (let [entity-list (subscribe [:entity-list])
        selected-type (reaction (:selected-entity-type @entity-list))
        filter-text (reaction (or (get-in @route [:params :query-params :q])
                                  (:filter-text @entity-list)))]
    (fn []
      [:div.app-links
       [:ul
        [:li [:a.app-link {:href (str "#/report"
                                      (when @selected-type (str "/" (name @selected-type)))
                                      (when @filter-text (str "?q=" @filter-text)))} "Manager"]]
        [:li [:a.app-link {:href "#/run"} "Runner"]]
        [:li [:a.app-link {:href "#/source"} "Source Editor"]]
        [:li [:a.app-link {:href "#/settings"} "Settings"]]
        [:li [:a.app-link {:href "#/step-summary?declutter&metricsonly"} "Status"]]
        [:li [:a.app-link {:href "http://spwiki.sp.local/x/CgCgBQ" :target "readitest-help"} "Help"]]]])))

(defn app-header [route]
  [:div.app-header
   [:div.logo [:a {:href "#/"} [:img {:src "images/logo_M_w.png" :height "23"}]]]
   [app-links route]])

(defn app-footer []
  (let [app-settings (subscribe [:app-settings])
        app-url (reaction (:app-url @app-settings))
        app-status (subscribe [:app-status])]
    (fn []
      [:div.app-footer
       [:span @app-url]
       [:span.right @app-status]])))

(defn app-help []
  [:div.app-help
   "help goes here...."
   [:button {:onClick #(dispatch [:show-help nil])} "ok"]])

(defn app []
  (let [route (subscribe [:route])
        show-help (subscribe [:show-help])
        app-alert (subscribe [:app-alert])
        app-busy? (subscribe [:app-busy?])]
    ;; prefetch various entity types
    (doseq [t [:driverfn :test :testfixture :testsuite]]
      (dispatch [:push-busy])
      (store/request-entity-list t #(do (dispatch [:update-entity-list t %])
                                        (dispatch [:pop-busy]))))
    (store/get-app-settings #(dispatch [:update-app-settings nil %]))
    (dispatch [:watch-step-summary])
    ;(dispatch [:push-busy])
    ;(.setTimeout js/window #(dispatch [:pop-busy]) 10000)
    (fn []
      [:div.app-container
       [app-header route]
       (when @app-alert [:div.warning @app-alert])
       (when @show-help [app-help])
       (when @app-busy? [:div.loading "Loading"])
       [app-page route]
       #_[:div.app-debug
          [:pre.wrap (pr-str (-> @re-frame.db/app-db
                                 (dissoc :source-file-list)
                                 (dissoc :source-file)))]]
       [app-footer route]])))

;; ------------------------
;; Routing

(secretary/set-config! :prefix "#")
(defroute "/source/:ns" {:as params} (dispatch [:route "source" params]))
(defroute "/source" {:as params} (dispatch [:route "source-list" params]))
(defroute "/run/:test-id/:index" {:as params} (dispatch [:route "run-step-view" params]))
(defroute "/run/:test-id" {:as params} (dispatch [:route "run-test-view" params]))
(defroute "/run" {:as params} (dispatch [:route "run" params]))
(defroute "/:page/:id" {page :page :as params} (dispatch [:route page params]))
(defroute "/:page" {page :page :as params} (dispatch [:route page params]))
(defroute "*" {:as params} (dispatch [:route "default" params]))

;; -------------------------
;; History
;; must be called after routes have been defined

(defn hook-browser-navigation! []
  (doto (History.)
    (events/listen
      EventType/NAVIGATE
      (fn [event]
        (secretary/dispatch! (.-token event))))
    (.setEnabled true)))

;; -------------------------
;; Initialize app

(defn mount-root []
  (r/render [app] (.getElementById js/document "app")))

(defn init! []
  (hook-browser-navigation!)
  (mount-root))

;; some once off setup....

(enable-console-print!)
(init!)
