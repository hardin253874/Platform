(ns rt.model
  (:require [clojure.string :as string]
            [cljs.reader :refer [read-string]]
            [rt.util :as util]))

(defn prepare-step-list-for-edit [steps]
  ;; clear existing empty steps, then add an empty one at the end
  ;; plus add an index so we can disguish between steps with the same details
  ;; Warning - refs to scripts are not maps and so we'll still have issues with
  ;; multiple steps with same script ref
  (let [empty-script? #(and (:script %) (empty? (:script %)))
        add-step-index #(if (map? %1) (assoc %1 :__index %2) %1)]
    (mapv add-step-index (conj (vec (remove empty-script? steps)) {:script ""}) (range))))

(defn prepare-entity-for-edit [entity]
  (let [entity-type (:type entity)
        entity-type (if (keyword? entity-type) entity-type (keyword entity-type))
        add-empty-step (fn [entity step-key]
                         (assoc entity step-key (prepare-step-list-for-edit (step-key entity))))
        entity (condp = entity-type
                 :test (reduce add-empty-step entity [:steps :setup :teardown])
                 :testfixture (reduce add-empty-step entity [:setup :teardown])
                 :testscript (reduce add-empty-step entity [:steps])
                 :testsuite (reduce add-empty-step entity [:setup :teardown])
                 entity)
        ;; ensure the list exists, even if empty, and ensure any ids are keywords
        ;; and make sure any ids are keywords, not strings
        fixup-lists (fn [entity list-key]
                      (->> (list-key entity)
                           (map #(if (string? %) (keyword %) %))
                           (into [] )
                           (assoc entity list-key)))
        entity (condp = entity-type
                 :test (reduce fixup-lists entity [:fixtures])
                 :testsuite (reduce fixup-lists entity [:once-fixtures :each-fixtures :tests])
                 entity)]
    entity))

(defn insert-step [steps i & [step]]
  (let [step (or step {:script "(comment -- click here to edit --)"})]
    (-> (concat (subvec steps 0 i) [step] (subvec steps i))
        prepare-step-list-for-edit)))

(defn delete-step [steps i]
  (prepare-step-list-for-edit (assoc steps i {:script ""})))

(defn driverfn-call-syntax [{:keys [id arglists]}]
  (str "("
       (->> (map pr-str (first arglists))
                (concat [(util/id->str id)])
                (interpose " ")
                (apply str))
       ")"))

(defn driverfn-doc [{:keys [doc] :as e}]
  #_(or doc (str "Generated hint (see development team or edit yourself for better documentation)"
                  \newline \newline "#Syntax"
                  \newline \newline (driverfn-call-syntax e) \newline \newline))
  doc)

(defn- code->exprs [expr]
  (try
    (let [forms (read-string (str "(" expr ")"))]
      (map pr-str forms))
    (catch js/Error e
      (println "error parsing expression" (pr-str expr))
      [expr])))

(defn expand-step-at-index [steps index]
  (if-let [expr (get-in steps [index :script])]
    (let [step (get steps index)
          exprs (code->exprs expr)
          steps (delete-step steps index)
          steps (reduce #(insert-step %1 index (assoc step :script %2))
                        steps (reverse exprs))]
      (println "expanding" steps expr)
      steps)
    steps))

