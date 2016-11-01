(ns rt.run
  (:require-macros [reagent.ratom :refer [reaction]]
                   [cljs.core.async.macros :refer [go alt!]]
                   [rt.lib.cljs-macros :refer [handler-fn]])
  (:require [reagent.core :as r]
            [re-frame.core :refer [subscribe dispatch register-sub register-handler dispatch-sync]]
            [rt.basic-components :refer [simple-props-table]]
            [rt.codemirror :refer [cm-input]]
            [rt.store :as store]
            [rt.util :refer [url-encode get-datetime-str]]
            [clojure.string :as string]
            [secretary.core :as secretary]
            [cljs-http.client :as http]
            [cljs.core.async :refer [put! <! >! chan timeout close!]]))

;; -------------------------
;; Misc

#_(defn test-ids-in-testrun [testrun]
    (->> testrun :steps (map :test-id) distinct))

#_(defn testrun-matches-requested [{:keys [test-id suite-id]} testrun]
    (let [testrun-main-id (-> testrun :tests first :id)
          testrun-tests (test-ids-in-testrun testrun)]
      (and (= suite-id testrun-main-id)
           (or (nil? test-id)
               ((set testrun-tests) test-id)))))

(defn- get-last-run-step-index [{:keys [steps]}]
  (dec (count (take-while #(not (#{"not-done" "skipped"} (:status %))) steps))))

(defn- index-testrun-steps [testrun]
  (assoc testrun :steps (mapv #(assoc %1 :step-index %2) (:steps testrun) (range))))

;; -------------------------
;; Queries

(defn- get-run-test-id [db]
  (get-in db [:route :params :test-id]))

(defn- get-run-step-index [db]
  (get-in db [:route :params :index]))

(register-sub
  :run
  (fn [db _]
    (reaction (get-in @db [:run]))))

(register-sub
  :run-test-id
  (fn [db _]
    (reaction (if (= (get-in @db [:route :page]) "run-test-view") (get-run-test-id @db)))))

(register-sub
  :run-step-index
  (fn [db _]
    (reaction (if (= (get-in @db [:route :page]) "run-step-view") (get-run-step-index @db)))))

;; -------------------------
;; Event handlers

;; todo - if a request is outstanding but the details are different ...
;; a new test for example,
;; then we need to immediately issue a new request and ensure the response
;; for the previous is discarded

(comment
  (defn run-next-with-expr [_ expr save?]
    (when (not-empty expr)
      (go (<! (http/post "/api/reset-step" {}))
          (<! (http/post "/api/step" {:json-params {:expr expr :save? save?}})))))
  )

(defn- run [db]
  (http/post "/api/run" {})
  db)

(defn- run-next [db]
  (http/post "/api/step" {})
  db)

(defn- run-to-error [db]
  (http/post "/api/run-to-error" {})
  db)

(defn- run-to-step [db path]
  (http/post "/api/run-to-step" {:json-params {:path path}})
  db)

(defn- reset-step [db]
  (http/post "/api/reset-step" {})
  db)

(defn- reset-testrun [db]
  (when-let [suite (get-in db [:run :testrun :tests 0])]
    ;; if there is a single test in the suite then specify that one again
    (let [tests (get-in db [:run :testrun :test-records])]
      (if-let [test (and (= 1 (count tests)) (first tests))]
        (store/initiate-test-run (:id suite) (:id test))
        (store/initiate-test-run (:id suite) nil))))
  (assoc-in db [:run :testrun] nil))

(register-handler
  :watch-testrun
  (fn [db [_]]
    (println "watching")
    (or (when-not (#{:requesting :poll-wait} (get-in db [:run :state]))
          (dispatch [:request-testrun]))
        db)))

(register-handler
  :cancel-watch-testrun
  (fn [db [_]]
    (println "cancel watching")
    (assoc db :run {:state :idle})))

(register-handler
  :request-testrun
  (fn [db [_]]
    (or (when-not (#{:requesting} (get-in db [:run :state]))
          (store/get-testrun-report (get-run-test-id db) #(dispatch [:update-testrun %]))
          (assoc-in db [:run :state] :requesting))
        db)))

(register-handler
  :update-testrun
  (fn [db [_ testrun]]
    (or (when (#{:requesting} (get-in db [:run :state]))
          (.setTimeout js/window #(dispatch [:request-testrun]) 2000)
          (assoc db :run {:testrun (index-testrun-steps testrun) :state :poll-wait}))
        db)))

(register-handler
  :create-testrun
  (fn [db [_ suite-id test-id]]
    (println "creating testrun" suite-id test-id)
    (when suite-id
      (store/initiate-test-run suite-id test-id))
    db))

(register-handler
  :run
  (fn [db [_ run-type]]
    (condp = run-type
      :to-end (run db)
      :next-step (run-next db)
      :to-error (run-to-error db)
      :reset-step (reset-step db)
      :reset (reset-testrun db))))

(register-handler
  :run-to-step
  (fn [db [_ {:keys [path]}]]
    (run-to-step db path)))

;; -------------------------
;; Views

(defn run-to-here-icon []
  [:span.run-to-here {:dangerouslySetInnerHTML {:__html "&#x21e8;"}}])

(defn- run-to-here-link [step]
  [:span {:onClick #(dispatch [:run-to-step step]) :title "run to here"} (run-to-here-icon)])

(defn- step-link [test-id index text]
  [:a {:href (str "#/run/" (url-encode test-id) "/" index)} text])

(defn- step-table-row [{:keys [test-id script step-index owner status time] :as step}]
  [:tr
   [:td (step-link test-id step-index step-index)]
   [:td {:class (str "status-" status) :style {:text-align "right"}} status]
   [:td.align-right (or time [run-to-here-link step])]
   [:td script]])

(defn- test-table-row [{:keys [id status time]}]
  [:tr
   [:td [:a {:href (str "#/run/" (url-encode id))} id]]
   [:td {:class (str "status-" status) :style {:text-align "right"}} status]
   [:td time]])

(defn testrun-toolbar []
  (fn []
    [:div.testrun-toolbar
     [:button {:onClick #(dispatch [:run :reset]) :title "restart the test run"} "reset"]
     [:button {:onClick #(dispatch [:run :to-end]) :title "run to end, starting with the next step"} "run to end"]
     [:button {:onClick #(dispatch [:run :to-error]) :title "run to error, starting with the next step"} "run to error"]
     [:button {:onClick #(dispatch [:run :next-step]) :title "run the next step"} "step"]
     [:button {:onClick #(dispatch [:run :reset-step]) :title "reset the last step"} "reset last"]]))

(defn- tests-table [r]
  [:div.test-list
   [:table.testrun
    [:thead [:tr
             [:th "Test"]
             [:th.align-right {:style {:width "6em"}} "Status"]
             [:th {:style {:width "4em"}} "Time"]]]
    (apply vector :tbody (map test-table-row (get-in @r [:testrun :test-records])))]])

(defn- steps-table [steps]
  [:table.testrun
   [:thead [:tr
            [:th {:style {:width "3em"}} "Index"]
            [:th.align-right {:style {:width "6em"}} "Status"]
            [:th.align-right {:style {:width "4em"}} "Time"]
            [:th "Expression"]]]
   (apply vector :tbody
          (map step-table-row steps))])

(defn run-page []
  (let [r (subscribe [:run])
        render (fn []
                 (let [suite-list (->> (get-in @r [:testrun :tests])
                                       (map :id)
                                       (interpose ", ")
                                       (apply str))]
                   (if-not (:testrun @r)
                     [:div.run-page "loading...."]
                     [:div.run-page
                      [testrun-toolbar]
                      [:div "Test run on suite/tests:" suite-list ", created: " (str (get-in @r [:testrun :created]))]
                      [tests-table r]
                      #_[:pre.wrap (pr-str (get-in @r [:testrun :test-records]))]])))]
    ;(add-watch r :watcher #(println "watch fired" %1 %2 %3 %4))
    (r/create-class
      {:reagent-render         render
       :component-will-mount   #(dispatch [:watch-testrun])
       :component-will-unmount #(dispatch [:cancel-watch-testrun])})))

(defn repl-control []
  (let [content (r/atom "")
        result (r/atom "")
        output (r/atom "")
        update-content (partial reset! content)
        handle-eval-result #(do (reset! result %2) (reset! output %3))
        eval-selection #(store/eval-expr @content handle-eval-result)
        run-with-save #(store/rerun-with-expr @content true)]
    (fn []
      [:div.repl
       [:label "Try expression: "
        [:button {:onClick eval-selection :title "Ctrl+Enter"} "eval"]
        [:button {:onClick run-with-save :title "Replace the current step and run"} "run with save"]]
       [:div.editor
        [cm-input {:value       content
                   :onChange    update-content
                   :onCtrlEnter eval-selection}]]
       [:div.result [:label "Result"]
        [:pre.wrap @result]]
       [:div.output [:label "Output"]
        [:pre.wrap @output]]])))

(defn run-page-test-view []
  (let [test-id (subscribe [:run-test-id])
        r (subscribe [:run])
        get-steps #(get-in @r [:testrun :steps])
        render (fn []
                 [:div.run-page.run-page-test-view
                  [:div.run-page-header
                   [testrun-toolbar]
                   [:label "Steps for test:" [:span @test-id]]
                   #_[:label "Last updated:" [:span (str (js/Date.))]]]
                  [:div.run-page-content
                   [steps-table (get-steps)]
                   #_[repl-control]
                   #_[:pre.wrap (pr-str (get-steps))]]])]
    (r/create-class
      {:reagent-render         render
       :component-will-mount   #(dispatch [:watch-testrun])
       :component-will-unmount #(dispatch [:cancel-watch-testrun])})))

(defn run-page-step-view []
  (let [index (subscribe [:run-step-index])
        r (subscribe [:run])
        render (fn []
                 (let [{:keys [testrun]} @r
                       index (js/parseInt (str @index))
                       index (if (>= index 0) index (get-last-run-step-index testrun))
                       step (first (filter #(= index (:step-index %)) (:steps testrun)))
                       {:keys [test-id script doc result out status events test-context]} step]
                   [:div.run-page.run-page-step-view
                    [testrun-toolbar]
                    #_[:div (pr-str index) " Last rendered: " (str (js/Date.))]
                    [:div.step-props
                     (simple-props-table ["Expression" script
                                          "Test" test-id
                                          "Index" index
                                          "Status" status
                                          "Result" (pr-str result)
                                          "Output" [:pre.wrap out]
                                          "Events" (pr-str events)
                                          "Context" (pr-str test-context)
                                          "Doc" doc])]
                    #_[:pre.wrap (pr-str step)]]))]
    (r/create-class
      {:reagent-render         render
       :component-will-mount   #(dispatch [:watch-testrun])
       :component-will-unmount #(dispatch [:cancel-watch-testrun])})))
