(ns rt.edit
  (:require-macros [reagent.ratom :refer [reaction]]
                   [rt.lib.cljs-macros :refer [handler-fn]])
  (:require [rt.codemirror :refer [cm-input]]
            [rt.common :refer [entity-link run-link save-and-run-link]]
            [rt.basic-components :refer [tabs]]
            [rt.store :as store]
            [rt.model :as model]
            [rt.report :as report]
            [rt.dnd :as dnd]
            [rt.driverfn-chooser :refer [driverfn-chooser]]
            [rt.util :as util :refer [url-encode get-datetime-str id->str filter-records
                                      ensure-valid-id ensure-keyword]]
            [reagent.core :as r]
            [re-frame.core :refer [subscribe dispatch dispatch-sync register-sub register-handler]]
            [clojure.string :as string]
            [cljs.reader :refer [read-string]]))

;; -------------------------
;; Queries

(register-sub
  :entity
  (fn [db _]
    (reaction (get-in @db [:entity]))))

(register-sub
  :pristine-entity
  (fn [db _]
    (reaction (get-in @db [:pristine-entity]))))

(register-sub
  :editing-step
  (fn [db _]
    (reaction (get-in @db [:editing-step]))))


;; -------------------------
;; Event handlers

;; todo  all this new and request logic is not quite right ... mainly due to it
;; being kicked off by render fns... need to look at

(register-handler
  :request-entity
  (fn [db [_ id]]
    (println "request-entity:" id)
    (store/request-entity id #(dispatch [:update-entity (model/prepare-entity-for-edit %)]))
    ;; not doing this right now.... too much data, too slow, slightly different format....
    ;(store/request-entity-graph id #(dispatch [:update-in :entity-graph %]))
    (store/get-suite-tests id #(dispatch [:suite-tests id %]))
    (assoc db :entity {:id id}
              :pristine-entity {:id id}
              :explicit-suite-tests nil)))

(register-handler
  :update-entity
  (fn [db [_ entity]]
    (println "update-entity:" (:id entity))
    (-> db
        (assoc :entity entity
               :pristine-entity entity
               :explicit-suite-tests (:tests entity)))))

(register-handler
  :reset-entity
  (fn [db [_]]
    (let [entity (get db :pristine-entity)]
      (-> db
          (assoc :entity entity
                 :pristine-entity entity
                 :explicit-suite-tests (:tests entity))))))

(register-handler
  :new-entity
  (fn [db [_ type-id]]
    (let [new-id (keyword (str "new-folder/new-" (id->str type-id) "-" (util/get-datetime-str)))]
      (println "new-entity:" type-id new-id)
      (-> db
          (assoc :entity (model/prepare-entity-for-edit {:type type-id :id new-id})
                 :pristine-entity nil
                 :explicit-suite-tests nil)))))

(register-handler
  :save-entity
  (fn [db [_]]
    (let [entity (:entity db)]
      (println "save-entity:" (:id entity))
      (when (and (:id entity) (not= (get db :pristine-entity) (get db :entity)))
        (store/save-entity entity #(do (dispatch [:request-entity (:id entity)])
                                       (dispatch [:refresh-entity-list (:type entity)]))))
      db)))

;;todo - the following will add -n even if already have -n.... rather it should parse out n and inc it
(defn get-saveas-entity-id [db {type-id :type id :id}]
  (let [entity-ids (->> (get-in db [:entity-list type-id]) (map :id) set)
        make-new-id #(util/ensure-keyword (str id "-" %))]

    (loop [id id n 1]
      (println "new id" id n type-id entity-ids)

      (if-not (contains? entity-ids id)
        id
        (recur (make-new-id n) (inc n))))))

(register-handler
  :save-as-prompt
  (fn [db [_]]
    (let [entity (:entity db)]
      (dispatch [:update-entity-prop :id (get-saveas-entity-id db entity)]))
    db))

(register-handler
  :suite-tests
  (fn [db [_ id tests]]
    (-> db
        (assoc :suite-tests {:id id :tests tests}))))

(register-handler
  :update-entity-prop
  (fn [db [_ key value]]
    (assoc-in db [:entity key] value)))

(register-handler
  :add-related-entities
  (fn [db [_ key values]]
    (let [entities (get-in db [:entity key])]
      (println "added new rels... now=" (concat entities values))
      (assoc-in db [:entity key] (vec (concat entities values))))))

(register-handler
  :remove-related-entity
  (fn [db [_ key value]]
    (let [entities (get-in db [:entity key])]
      (println "removing" value "from" entities)
      (assoc-in db [:entity key] (vec (remove (partial = value) entities))))))

(register-handler
  :remove-related-entity-at-index
  (fn [db [_ key index]]
    (let [entities (get-in db [:entity key])]
      (println "removing index" index "from" entities)
      (assoc-in db [:entity key] (vec (concat (subvec entities 0 index) (subvec entities (inc index))))))))

(register-handler
  :move-related-entity
  (fn [db [_ key src-index dest-index]]
    (println "moving" key (pr-str src-index) (pr-str dest-index) (get-in db [:entity key]))
    (let [ids (get-in db [:entity key])
          id (get ids src-index)
          ids (vec (concat (subvec ids 0 src-index) (subvec ids (inc src-index))))
          ids (vec (concat (subvec ids 0 dest-index) [id] (subvec ids dest-index)))]
      (assoc-in db [:entity key] ids))))

(register-handler
  :edit-step
  (fn [db [_ steps-key index]]
    (assoc-in db [:editing-step] {:entity-id (get-in db [:entity :id]) :steps-key steps-key :index index})))

(register-handler
  :done-edit-step
  (fn [db [_ steps-key index]]
    (println "done edit step" steps-key index)
    (let [steps (-> (get-in db [:entity steps-key])
                    (model/expand-step-at-index index)
                    (model/prepare-step-list-for-edit))]
      (-> db
          (assoc-in [:entity steps-key] steps)
          (assoc-in [:editing-step] nil)))))

(register-handler
  :cancel-edit-step
  (fn [db [_ steps-key index step]]
    (-> db
        (assoc-in [:entity steps-key index] step)
        (assoc-in [:editing-step] nil))))

(register-handler
  :close-edit-step
  (fn [db [_]]
    (-> db
        (assoc-in [:editing-step] nil))))

(register-handler
  :delete-step
  (fn [db [_ steps-key index]]
    (let [steps (get-in db [:entity steps-key])
          ;; clearing the expression in the step and then using the 'prepare....' function will remove it
          steps (assoc steps index {:script ""})
          steps (model/prepare-step-list-for-edit steps)]
      (assoc-in db [:entity steps-key] steps))))

(register-handler
  :move-step
  (fn [db [_ {source-steps-key :steps-key source-index :index} {dest-steps-key :steps-key dest-index :index}]]
    (println "moving" source-index dest-index)
    (let [steps (get-in db [:entity source-steps-key])
          step (get steps source-index)
          db (assoc-in db [:entity source-steps-key] (model/delete-step steps source-index))
          steps (get-in db [:entity dest-steps-key])
          steps (model/insert-step steps dest-index step)
          db (assoc-in db [:entity dest-steps-key] steps)]
      db)))

;; ---------------------------
;; Views

(defn- edit-step [steps-key index]
  (dispatch [:edit-step steps-key index]))

(defn- update-step [steps-key index expr]
  (dispatch-sync [:update-in [:entity steps-key index :script] expr]))

(defn- done-edit [steps-key index]
  (dispatch [:done-edit-step steps-key index]))

(defn- close-edit []
  (dispatch [:close-edit-step]))

(defn- cancel-edit [steps-key index step]
  (dispatch [:cancel-edit-step steps-key index step]))

(defn- delete-step [steps-key index]
  (dispatch [:delete-step steps-key index]))

(defn drag-handle []
  [:span.edit-drag-handle {:dangerouslySetInnerHTML {:__html "&#x2a29;"}}])

(defn delete-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x2717;"}}])

(defn edit-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x270D;"}}])

(defn tick-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x2713;"}}])

(defn cross-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x2717;"}}])

(defn box-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x2610;"}}])

(defn ticked-box-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x2611;"}}])

(defn fn-button []
  [:span.edit-step-button {:dangerouslySetInnerHTML {:__html "&#x0192;"}}])

(defn- step-link [steps-key index expr]
  (let [expr (if (not-empty expr) expr "(comment -- click here to edit --)")
        comment? (zero? (.indexOf expr "(comment"))
        comment? (or comment? (zero? (.indexOf expr ";")))]
    [(if comment? :span.comment :span) {:onClick #(edit-step steps-key index)} expr]))

(defn- script-link [script-id]
  (let []
    [:span "Script:"
     [entity-link script-id script-id]]))

(defn- step-editor [steps-key index]
  (let [step (subscribe [:get-in [:entity steps-key index]])
        expr (reaction (or (:script-id @step) (:script @step)))
        step0 (r/atom @step)
        show-driver-chooser (r/atom false)]
    (println "creating step editor" steps-key index)
    (fn []
      ;(println "rendering cm-input" steps-key index (pr-str @step) (pr-str @expr))
      [:div.step-editor-container
       [:div.step-editor
        [:div.step-input
         [cm-input {:value expr :onChange #(update-step steps-key index %) :onCtrlEnter #(done-edit steps-key index)}]]
        [:div.step-editor-buttons
         [:button {:onClick #(done-edit steps-key index) :title "Ctrl-Enter"} (tick-button)]
         [:button {:onClick #(cancel-edit steps-key index @step0)} (cross-button)]
         [:button {:onClick (handler-fn (reset! show-driver-chooser (not @show-driver-chooser)))} (fn-button)]]]
       (when @show-driver-chooser [:div.step-editor-chooser (when @show-driver-chooser [driverfn-chooser])])])))

;; assume same entity for now, but TODO allow cross entity dnd
;; todo - support move or copy operations
(defn- step-dropped [dest source tfr-data]
  (println "dropped" source tfr-data "on" dest)
  (dispatch [:move-step source dest]))

(defn- step-table-row []
  (let []
    (fn [steps-key {expr :script index :__index script-id :script-id} editing-step]
      (let [{editing-index :index editing-steps-key :steps-key} @editing-step
            editing? (and (= editing-index index) (= editing-steps-key steps-key))
            step-ref {:steps-key steps-key :index index}]
        [:tr
         [:td (dnd/drag-target-attrs #(step-dropped step-ref %1 %2))
          [:span (dnd/drag-source-attrs #(identity step-ref)) [drag-handle]]
          [:span {:onClick #(edit-step steps-key index)} (edit-button)]]
         [:td index]
         [:td (cond editing? [step-editor steps-key index]
                    script-id [script-link script-id]
                    :default [step-link steps-key index expr])]
         [:td
          (when (and (not-empty expr) (not script-id))
            [:span {:onClick #(delete-step steps-key index)} (delete-button)])]]))))

(defn steps-table []
  (let [editing-step (subscribe [:editing-step])]
    (fn [steps-key entity label]
      (println "render steps table:" (pr-str @editing-step))
      [:div.list-section
       ;[:label (or label (string/capitalize (id->str steps-key)))]
       [:table.steps
        [:thead
         [:tr
          [:th {:style {:width "3rem"}} ""]
          [:th {:style {:width "3rem"}} ""]
          [:th ""]
          [:th {:style {:width "3rem"}} ""]]]
        (apply vector :tbody
               (map #(vector step-table-row steps-key % editing-step) (steps-key entity)))]])))

(defn- entity-onchange [key event]
  (dispatch-sync [:update-entity-prop key (.. event -target -value)]))

(defn entity-form-input [entity key {:keys [label placeholder]}]
  (let [label (or label (string/capitalize (id->str key)))]
    [:label (str label ":") [:input {:value       (key entity)
                                     :onChange    (partial entity-onchange key)
                                     :placeholder (or placeholder (id->str key))}]]))

(defn- entity-keyword-value-onchange [key event]
  (dispatch-sync [:update-entity-prop key (ensure-keyword (.. event -target -value))]))

(defn entity-form-keyword-input [entity key {:keys [label placeholder]}]
  (let [label (or label (string/capitalize (id->str key)))]
    (println "render keyword-input" key (pr-str (key entity)))
    [:label (str label ":") [:input {:value       (id->str (key entity))
                                     :onChange    (partial entity-keyword-value-onchange key)
                                     :placeholder (or placeholder (id->str key))}]]))

#_(defn atom-backed-input [{:keys [value onChange placeholder]}]
    (let [value-atom (r/atom value)]
      (fn []
        [:input {:value       @value-atom
                 :onChange    (handler-fn (reset! value-atom (onChange (.. event -target -value))))
                 :placeholder placeholder}])))

(defn- entity-list-value-onchange [key event]
  (let [value (.. event -target -value)
        value (filter not-empty (string/split value #"[, ]"))]
    (println "updating list value" key (pr-str value))
    (dispatch-sync [:update-entity-prop key (vec value)])))

(defn entity-form-list-input [entity key {:keys [label placeholder]}]
  (println "init list-input" key (pr-str (key entity)))
  (fn [entity key {:keys [label placeholder]}]
    (let [label (or label (string/capitalize (id->str key)))
          value (apply str (interpose "," (map str (key entity))))]
      (println "render list-input" key (pr-str (key entity)) value)
      [:label (str label ":")
       [:input {:value       value
                :onChange    (partial entity-list-value-onchange key)
                :placeholder (or placeholder (id->str key))}]])))

(defn- record-table-row [index {:keys [id type name doc tags explicit?] :as r}
                         selected?
                         {:keys [onSelect onDelete onDropped canDelete? onRun]}]
  ;(println "render record-table-row:" (pr-str r) selected?)
  (let [id-str (id->str id)]
    [:tr {:key id-str}
     [:td (when onDropped (dnd/drag-target-attrs (fn [[src-index _]] (onDropped index src-index))))
      (when onRun [:span.link {:onClick #(onRun index r)} "run"])
      [:span (when onDropped (dnd/drag-source-attrs #(identity [index r])) [drag-handle])]
      (when onSelect [:span {:onClick #(onSelect index r)} (if selected? [ticked-box-button] [box-button])])]
     [:td index]
     [:td (entity-link id-str id-str)]
     [:td {:title (str doc " " tags)} (str name (when explicit? " *"))]
     ;[:td (apply str (interpose "," tags))]
     [:td (when (and onDelete (canDelete? r)) [:span {:onClick #(onDelete index r)} [delete-button]])]]))

(defn record-table [records selected-ids opts]
  ;(println "render record-table:" (type records))
  ;(println "render record-table:" (some-> records first keys))
  [:table.entity
   [:thead [:tr
            [:th {:style {:width "3em"}} ""]
            [:th ""]
            [:th ""]
            [:th ""]
            ;[:th ""]
            [:th {:style {:width "3em"}} ""]]]
   (apply vector :tbody
          (map #(vector record-table-row %2 %1 (@selected-ids (:id %1)) opts)
               records (range)))])

(defn record-list-view [{:keys [onRunTest]}]
  (let [selected-ids (r/atom #{})
        onSelect (fn [_ {id :id}] (swap! selected-ids #(if (% id) (disj % id) (conj % id)) id))
        onRun (when onRunTest (fn [_ {id :id}] (onRunTest id)))]
    (fn [{:keys [key records onDone onDelete onDropped]}]
      (println "render record-list-view" key (count records) (first records))
      [:div.record-list
       [:div
        (when onDone [:span.button {:onClick #(onDone @selected-ids)} "add selected"])
        (when onDone [:span.button {:onClick #(onDone [])} "cancel"])]
       [record-table records selected-ids {:onSelect   (when onDone onSelect)
                                           :onDelete   #(onDelete %1)
                                           :canDelete? :explicit?
                                           :onDropped  onDropped
                                           :onRun      (when onRun onRun)}]])))

(defn- id->entity [id]
  (if (map? id) id {:id (ensure-keyword id)}))

(defn- run-suite-test [suite-id test-id]
  (dispatch [:create-testrun suite-id test-id])
  (util/set-location "#/run"))

(defn related-entities [{:keys [type-id rel-id]}]
  (let [adding? (r/atom false)
        avail-entities (reaction (get @(subscribe [:entity-list]) type-id))
        find-entity (fn [id] (first (filter #(= id (:id %)) @avail-entities)))
        onDone #(do (reset! adding? false)
                    (dispatch [:add-related-entities rel-id %]))
        onDelete #(dispatch [:remove-related-entity-at-index rel-id %])
        onDropped #(dispatch [:move-related-entity rel-id %2 %1])]
    (fn [{:keys [entity entities type-id rel-id label]}]
      (println "render related-entities" type-id (count entities) rel-id label (count @avail-entities))
      (let [label (or label (string/capitalize (id->str rel-id)))]
        [:div.list-section
         [:div.list-toolbar
          (when-not @adding? [:span.button {:onClick (handler-fn (swap! adding? not))} "add"])]
         (if-not @adding?
           [record-list-view {:key       "1"
                              :records   (->> entities
                                              (map #(or (find-entity %) %))
                                              (map id->entity))
                              :onDelete  onDelete
                              :onDropped onDropped
                              :onRunTest (when (#{:test} type-id) (partial run-suite-test (:id entity)))
                              }]
           [record-list-view {:key     "2"
                              :records @avail-entities
                              :onDone  onDone}])]))))

(defn- reconcile-tests [entity {:keys [tests id]}]
  (let [test-ids (set (:tests entity))]
    (if (not= id (:id entity))
      (:tests entity)
      (map #(assoc % :explicit? (test-ids (:id %))) tests))))

(defn edit-page []
  (let [entity (subscribe [:entity])
        fixture-data (reaction (pr-str (:data @entity)))
        pristine-entity (subscribe [:pristine-entity])
        suite-tests (subscribe [:get-in [:suite-tests]])
        editing-state (subscribe [:editing-step])]
    (fn [route]
      (let [entity @entity
            pristine-entity @pristine-entity
            new-mode (not pristine-entity)
            saveas-mode (and (not new-mode) (not= (:id entity) (:id pristine-entity)))
            {{req-id :id {new-type-id :type} :query-params} :params} @route
            req-id (util/ensure-keyword req-id)
            new-type-id (util/ensure-keyword new-type-id)
            do-save #(do (close-edit)
                         (dispatch [:save-entity])
                         (util/set-location (str "#/edit/" (util/url-encode (id->str (:id entity))))))
            do-cancel #(do (close-edit)
                           (dispatch [:reset-entity]))]
        ;(println "edit - render: prist=" (:id pristine-entity) "req=" req-id)
        ;(println "edit - render: types=" (type (:id pristine-entity)) (type req-id))
        ;(println "edit - render: modes new" new-mode "saveas" saveas-mode)
        (when (and (not saveas-mode) req-id (not= (:id pristine-entity) req-id))
          (println "requesting")
          (dispatch [:request-entity req-id]))
        ;(println "edit - render: ent type=" (:type entity) "type-id=" new-type-id "ent id" (:id entity))
        (when (and new-type-id (or pristine-entity (not= (:type entity) new-type-id)))
          (dispatch [:new-entity new-type-id]))
        (when (not= (:id entity) (:entity-id @editing-state))
          (close-edit))
        [:div.edit-page
         [:div.edit-page-toolbar
          [:button.button {:onClick  do-save
                           :disabled (or (not (:id entity)) (= entity pristine-entity))} "save"]
          [:button.button {:onClick #(dispatch [:save-as-prompt])
                           :title   "to be implemented"} "save as"]
          [:button.button {:onClick do-cancel
                           :title   "discard changes"} "revert"]
          (when (and (#{:test :testsuite} (:type entity)) (:id entity))
            [save-and-run-link req-id "run"])]
         ;; Id
         (if-not pristine-entity
           [entity-form-keyword-input entity :id]
           (if (not= (:id entity) (:id pristine-entity))
             [entity-form-keyword-input entity :id]
             [:div
              [:div
               [:label "Id:"
                [:input {:key (:id entity) :readOnly "readOnly" :value (id->str (:id entity))}]]]
              ;; Props
              [entity-form-input entity :name {:placeholder "short and meaningful name"}]
              [entity-form-list-input entity :tags {:placeholder "tags such as :area/form-builder or :user/karen"}]
              [entity-form-input entity :doc {:placeholder "documentation"}]
              (when (#{:testsuite} (:type entity))
                [entity-form-input entity :test-filter {:label       "Test filter"
                                                        :placeholder "e.g. (has-tags? [\"mobile\" \"prod\"] %)"}])
              ;; Lists
              (when (#{:testsuite} (:type entity))
                [tabs
                 (let [tests (reconcile-tests entity @suite-tests)]
                   ["Tests" related-entities {:entity entity :entities tests :type-id :test :rel-id :tests}])
                 ["Fixtures" related-entities {:entity entity :entities (:each-fixtures entity) :type-id :testfixture :rel-id :each-fixtures}]
                 ["Fixtures (once)" related-entities {:entity entity :entities (:once-fixtures entity) :type-id :testfixture :rel-id :once-fixtures}]])
              (when (#{:testscript} (:type entity))
                [tabs
                 ["Steps" steps-table :steps entity]])
              (when (#{:test} (:type entity))
                [tabs
                 ["Steps" steps-table :steps entity]
                 ["Fixtures" related-entities {:entity entity :entities (:fixtures entity) :type-id :testfixture :rel-id :fixtures}]
                 ["Setup" steps-table :setup entity]
                 ["Teardown" steps-table :teardown entity]])
              (when (#{:testfixture} (:type entity))
                [tabs
                 ["Setup" steps-table :setup entity]
                 ["Teardown" steps-table :teardown entity]
                 (when (:data entity)
                   ["Properties (data)"
                    :div.fixture-data
                    [cm-input {:value    fixture-data
                               :onChange #(dispatch [:update-entity-prop :data (read-string %)])}]])])]))
         ;; Debug
         #_[:pre.wrap \newline \newline (pr-str entity)]
         #_[:pre.wrap \newline \newline (pr-str @suite-tests)]
         #_[:pre.wrap \newline (pr-str @dnd/state)]]))))

