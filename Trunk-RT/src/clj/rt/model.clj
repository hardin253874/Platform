(ns rt.model
  "Data model"
  (require [datomic.api :as d]
           [rt.test.db :refer [init-test-db add-tests test-db]]
           [rt.app :refer [setup-environment]]
           [rt.setup :refer [get-test-runs-dir]]
           [clojure.pprint :refer [pprint print-table pp]])
  (:import (java.util Date)
           (java.io File StringWriter)))

(def uri "datomic:mem://readiTest")

;; questions on the schema
;; - should i use the same attribute for multiple entities, for example have a :entity/doc attr
;; rather than a doc for each? Ditto id.. YES - should use common id that is an identity as
;; we are using a list of ids in a suite where those ids can be suites or tests

;; todo move the schema to data files

(def ordered-ref-schema
  [{:db/doc                "ordered ref"
    :db/ident              :orderedRef/entity
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "ordered ref position"
    :db/ident              :orderedRef/index
    :db/valueType          :db.type/long
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

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

   {:db/doc                "name of the script"
    :db/ident              :script/name
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
  [{:db/doc                "test id"
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

   {:db/doc                "fixtures to run this test - refs to orderedRefs to fixtures"
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
  [
   {:db/doc                "suite id"
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

   {:db/doc                "fixtures to apply once for all contained tests - refs to orderedRefs"
    :db/ident              :suite/onceFixtures
    :db/valueType          :db.type/ref
    :db/isComponent        true
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "fixtures to apply for each of the contained tests - refs to orderedRefs"
    :db/ident              :suite/eachFixtures
    :db/valueType          :db.type/ref
    :db/isComponent        true
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "the contained test and suites - refs to orderedRefs"
    :db/ident              :suite/tests
    :db/valueType          :db.type/ref
    :db/isComponent        true
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

(def testrun-schema
  [{:db/doc                "testrun id"
    :db/ident              :testrun/id
    :db/valueType          :db.type/keyword
    :db/cardinality        :db.cardinality/one
    :db/unique             :db.unique/identity
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "created date of the testrun"
    :db/ident              :testrun/created
    :db/valueType          :db.type/instant
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "output folder name"
    :db/ident              :testrun/dirName
    :db/valueType          :db.type/string
    :db/cardinality        :db.cardinality/one
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "the tests and suites - orderedRefs"
    :db/ident              :testrun/entities
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}

   {:db/doc                "test events, mainly the results of executing scripts"
    :db/ident              :testrun/events
    :db/valueType          :db.type/ref
    :db/cardinality        :db.cardinality/many
    :db/id                 #db/id[:db.part/db]
    :db.install/_attribute :db.part/db}])

(defn default-on-nil [v d]
  (if (nil? v) d v))

(defn get-ref-for-id [scripts-tx id-key id]
  (if (keyword? id)
    (if-let [tx-data (first (filter #(= (id-key %) id) scripts-tx))]
      (:db/id tx-data)
      nil)
    id))

(defn get-tx-entity-type [e]
  (cond
    (:script/id e) :script
    (:fixture/id e) :fixture
    (:test/id e) :test
    (:suite/id e) :suite
    (or (:step/expr e) (:step/script e)) :step
    (:orderedRef/entity e) :ordered-ref
    :default nil))

(defn init-database []
  (d/create-database uri)
  (d/transact (d/connect uri)
              (concat ordered-ref-schema
                      step-schema script-schema
                      fixture-schema test-schema suite-schema
                      testevent-schema testrun-schema)))

(defn type->id-attr [t]
  (t {:test    :test/id
      :suite   :suite/id
      :fixture :fixture/id
      :script  :script/id}))

(defn get-ordered-ref-ids [refs id-attr]
  (->> refs
       (sort-by :orderedRef/index)
       (mapv #(get-in % [:orderedRef/entity id-attr]))))

(defn make-step [step]
  (if-let [script-ref (:step/script step)]
    (assoc (into {} step) :step/script (:script/id script-ref))
    step))

(defmulti make-entity get-tx-entity-type)

(defmethod make-entity :script [e]
  (assoc (into {} e) :script/steps (->> e :script/steps (sort-by :step/index) (mapv make-step))))

(defmethod make-entity :fixture [e]
  (assoc (into {} e) :fixture/beforeEach (->> e :fixture/beforeEach (sort-by :step/index) (mapv make-step))
                     :fixture/afterEach (->> e :fixture/afterEach (sort-by :step/index) (mapv make-step))))

(defmethod make-entity :test [e]
  (assoc (into {} e) :test/steps (->> e :test/steps (sort-by :step/index) (mapv make-step))
                     :test/fixtures (get-ordered-ref-ids (:test/fixtures e) :fixture/id)))

(defmethod make-entity :suite [e]
  (assoc (into {} e) :suite/tests (get-ordered-ref-ids (:suite/tests e) :test/id)
                     :suite/onceFixtures (get-ordered-ref-ids (:suite/onceFixtures e) :fixture/id)
                     :suite/eachFixtures (get-ordered-ref-ids (:suite/eachFixtures e) :fixture/id)))

(defmethod make-entity :default [e]
  (into {} e))

(defn get-raw-entity [id]
  (let [db (d/db (d/connect uri))
        result (d/q '[:find ?e :in $ ?id
                      :where (or [?e :test/id ?id] [?e :suite/id ?id]
                                 [?e :fixture/id ?id] [?e :script/id ?id])]
                    db id)
        e (d/entity db (ffirst result))]
    e))

(defn get-entity [id]
  (make-entity (d/touch (get-raw-entity id))))

(defn get-entities [t]
  (let [id-attr (type->id-attr t)
        db (d/db (d/connect uri))
        result (d/q '[:find ?e :in $ ?id-attr :where [?e ?id-attr]] db id-attr)]
    (map (comp make-entity d/touch (partial d/entity db) first) result)))

(defn update-steps
  "update the steps in the script, fixture or test"
  ([id steps dest-attr id-attr] (update-steps (d/connect uri) id steps dest-attr id-attr))
  ([conn id steps dest-attr id-attr]
    ;; upsert any scripts we reference in our steps
   (d/transact conn (mapv #(identity {:db/id (d/tempid :db.part/user) :script/id %})
                          (filter keyword? steps)))

    ;; clean out existing steps to replace them.
    ;; todo - do a diff and update rather than replace
   (let [e (get-raw-entity id)
         make-step-tx (fn [step index]
                        (cond
                          ;; a script id - assume it already exists
                          (keyword? step) {:step/script [:script/id step]
                                           :step/index  index}
                          ;; the map with the expr and others
                          (map? step) (merge {:step/expr  (:script step)
                                              :step/index index}
                                             (when (:doc step) {:step/doc (:doc step)})
                                             (when (:target-msecs step) {:step/target-msecs (:target-msecs step)}))
                          ;; a plain string that is the expr
                          (string? step) {:step/expr  step
                                          :step/index index}
                          :else (throw (Exception. (str "Invalid step in " id " => " (pr-str step))))))]
     ;; remove the existing
     (when (and (:db/id e) (not-empty (dest-attr e)))
       (d/transact conn (mapv #(identity [:db.fn/retractEntity (:db/id %)]) (dest-attr e))))
     ;; add given steps
     (d/transact conn [{:db/id [id-attr id] dest-attr (map make-step-tx steps (range))}]))))

(defn update-fixture-refs
  "update the fixtures in the test or suite"
  ([id fixtures dest-attr id-attr] (update-fixture-refs (d/connect uri) id fixtures dest-attr id-attr))
  ([conn id fixtures dest-attr id-attr]
    ;; upsert any fixtures we reference
   (d/transact conn (mapv #(identity {:db/id (d/tempid :db.part/user) :fixture/id %})
                          fixtures))

    ;; replace the existing fixtures
    ;; todo - do a diff and update rather than replace
   (let [e (get-raw-entity id)
         make-ordered-refs-tx (fn [id index]
                                {:db/id             (d/tempid :db.part/user)
                                 :orderedRef/index  index
                                 :orderedRef/entity [:fixture/id id]})]
     ;; remove the existing
     (when (and (:db/id e) (not-empty (dest-attr e)))
       (d/transact conn (mapv #(identity [:db.fn/retractEntity (:db/id %)]) (dest-attr e))))
     ;; add given steps
     (d/transact conn [{:db/id [id-attr id] dest-attr (map make-ordered-refs-tx fixtures (range))}]))))

(defn update-test-refs
  "update the tests in the suite"
  ([id tests dest-attr id-attr] (update-test-refs (d/connect uri) id tests dest-attr id-attr))
  ([conn id tests dest-attr id-attr]
    ;; upsert any tests we reference
   (d/transact conn (mapv #(identity {:db/id (d/tempid :db.part/user) :test/id %})
                          tests))

    ;; replace the existing tests
    ;; todo - do a diff and update rather than replace
   (let [e (get-raw-entity id)
         make-ordered-refs-tx (fn [id index]
                                {:db/id             (d/tempid :db.part/user)
                                 :orderedRef/index  index
                                 :orderedRef/entity [:test/id id]})]
     ;; remove the existing
     (when (and (:db/id e) (not-empty (dest-attr e)))
       (d/transact conn (mapv #(identity [:db.fn/retractEntity (:db/id %)]) (dest-attr e))))
     ;; add given steps
     (d/transact conn [{:db/id [id-attr id] dest-attr (map make-ordered-refs-tx tests (range))}]))))

(defn update-script [{:keys [id name doc steps]}]
  (let [conn (d/connect uri)
        ;; upsert the script itself
        temp-id (d/tempid :db.part/user)
        tx (d/transact conn [(merge {:db/id       temp-id
                                     :script/id   id
                                     :script/name (default-on-nil name "")}
                                    (when doc {:script/doc doc}))])]
    ;; update the steps
    (update-steps conn id steps :script/steps :script/id)

    ;; return the created/updated entity db id
    (d/resolve-tempid (d/db conn) (:tempids @tx) temp-id)))

(defn update-fixture [{:keys [id name doc setup teardown]}]
  (let [conn (d/connect uri)
        ;; upsert the fixture itself
        temp-id (d/tempid :db.part/user)
        tx (d/transact conn [(merge {:db/id        temp-id
                                     :fixture/id   id
                                     :fixture/name (default-on-nil name "")}
                                    (when doc {:fixture/doc doc}))])]
    ;; update the steps
    (update-steps conn id setup :fixture/beforeEach :fixture/id)
    (update-steps conn id teardown :fixture/afterEach :fixture/id)

    ;; return the created/updated entity db id
    (d/resolve-tempid (d/db conn) (:tempids @tx) temp-id)))

(defn update-test [{:keys [id name doc steps fixtures]}]
  (let [conn (d/connect uri)
        ;; upsert the test itself
        temp-id (d/tempid :db.part/user)
        tx (d/transact conn [(merge {:db/id     temp-id
                                     :test/id   id
                                     :test/name (default-on-nil name "")}
                                    (when doc {:test/doc doc}))])]
    ;; steps
    (update-steps conn id steps :test/steps :test/id)
    ;; fixtures
    (update-fixture-refs conn id fixtures :test/fixtures :test/id)

    ;; return the created/updated entity db id
    (d/resolve-tempid (d/db conn) (:tempids @tx) temp-id)))

(defn update-suite [{:keys [id name doc tests once-fixtures each-fixtures]}]
  (let [conn (d/connect uri)
        ;; upsert the test itself
        temp-id (d/tempid :db.part/user)
        tx (d/transact conn [(merge {:db/id      temp-id
                                     :suite/id   id
                                     :suite/name (default-on-nil name "")}
                                    (when doc {:suite/doc doc}))])]
    ;; fixtures
    (update-fixture-refs conn id once-fixtures :suite/onceFixtures :suite/id)
    (update-fixture-refs conn id each-fixtures :suite/eachFixtures :suite/id)
    ;; tests
    (update-test-refs conn id tests :suite/tests :suite/id)

    ;; return the created/updated entity db id
    (d/resolve-tempid (d/db conn) (:tempids @tx) temp-id)))

(defmulti import-entity :type)
(defmethod import-entity :testscript [e] (update-script e))
(defmethod import-entity :testfixture [e] (update-fixture e))
(defmethod import-entity :test [e] (update-test e))
(defmethod import-entity :testsuite [e] (update-suite e))
(defmethod import-entity :default [e])

(defn import-entities [entities]
  (doseq [entity entities]
    (import-entity entity)))

(comment

  ;; do this (at home)
  (setup-environment {:app-url "https://sg-mbp-2013.local"})

  ;; or this (at work)
  (setup-environment)

  ;; or do this
  (do
    (def entities [{:id    :script1
                    :type  :testscript
                    :steps ["(println \"hey from script1\")"]}

                   {:id    :script2
                    :type  :testscript
                    :steps [:script1]}

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
                               "(expect-equals 1 1)"]}

                   {:id       :s1
                    :name     "suite1"
                    :type     :testsuite
                    :tests    [:t1 :t2]
                    :teardown []}

                   {:id           :s2
                    :name         "suite2"
                    :type         :testsuite
                    :tests        [:t1]
                    :onceFixtures [:fixture1]
                    :eachFixtures [:fixture2]
                    :teardown     []}])
    (init-test-db)
    (add-tests entities))

  ;; then this to create the db and load schema

  (d/delete-database uri)
  (init-database)

  ;; and this to load the data based on whatever is in @test-db

  (map import-entity (vals @test-db))

  ;; some manual loading

  (update-script {:id :script88 :doc "hello" :steps [:script1 :script9 "help"]})

  (update-fixture {:id :fixture1 :name "chrome" :data {:a 1 :b "bye"} :setup ["login"]})

  (update-test {:id    :test10 :name "no-name" :doc "abcn"
                :steps ["help" "me" :script88] :fixtures [:fixture1]})

  (update-suite {:id :suite1 :name "daily" :fixtures [:fixture1] :tests [:test10]})

  ;; now do some queries over the data

  (pprint (get-entities :test))
  (pprint (get-entities :suite))
  (pprint (get-entities :script))
  (pprint (get-entities :fixture))

  (pprint (get-entity :t1))
  (pprint (get-entity :steve))

  (d/touch (d/entity (d/db (d/connect uri)) [:test/id :t1]))

  (pprint (get-entity :t2))
  (pprint (get-entity :s1))
  (pprint (get-entity :s2))
  (pprint (get-entity :fixture1))
  (pprint (get-entity :script1))

  (pprint (d/touch (get-raw-entity :script88)))

  (count (get-entities :test))

  ;; load only certain types of entities

  (map update-script (filter #(= :testscript (:type %)) (vals @test-db)))
  (map update-test (filter #(= :test (:type %)) (vals @test-db)))
  (map update-fixture (filter #(= :testfixture (:type %)) (vals @test-db)))
  (map update-suite (filter #(= :testsuite (:type %)) (vals @test-db)))

  ;; delete an entity

  (d/transact (d/connect uri) [[:db.fn/retractEntity [:test/id :test10]]])

  ;; count orphaned steps

  (count (d/q '[:find ?e
                :where
                [?e :step/expr]
                (not [_ :script/steps ?e])
                (not [_ :test/steps ?e])
                (not [_ :fixture/beforeEach ?e])
                (not [_ :fixture/afterEach ?e])]
              (d/db (d/connect uri)))
         #_(map (comp d/touch (partial d/entity (d/db (d/connect uri))) first)))

  ;; get tests or suites with the given ids

  (->> (d/q '[:find ?e ?id :in $ [?id ...] :where (or [?e :test/id ?id] [?e :suite/id ?id])]
            (d/db (d/connect uri)) [:t1 :s1])
       #_(map (comp d/touch (partial d/entity (d/db (d/connect uri))) first)))
  )

(defn create-testrun
  "Create a new test run and add it to the test run database.
  The output folder for the test run artifacts is created."
  []
  (let [date (Date.)
        id-str (str (rt.lib.util/datetime-str date) "-" (System/getenv "COMPUTERNAME"))
        id (keyword id-str)
        out-dir (-> (get-test-runs-dir) (or ".") (str "/" id-str))
        conn (d/connect uri)
        tx (d/transact conn [{:db/id           (d/tempid :db.part/user)
                              :testrun/id      id
                              :testrun/created date
                              :testrun/dirName out-dir}])]
    ;; todo - defer creating the directory only when we start running
    #_(println "Creating output folder:" out-dir)
    #_(.mkdir (File. out-dir))
    id))

(defn add-tests-to-testrun [id & tests]
  (let [conn (d/connect uri)
        db (d/db conn)
        tests-tx (map #(first (d/q '[:find ?aname ?id :in $ ?id
                                     :where
                                     (or [?e :test/id ?id] [?e :suite/id ?id])
                                     [?e ?a ?id]
                                     [?a :db/ident ?aname]]
                                   db %)) tests)
        tx (d/transact conn [{:db/id            [:testrun/id id]
                              :testrun/entities tests-tx}])]
    id))

(defn get-testrun [id]
  (let [conn (d/connect uri)]
    (->> (d/q '[:find ?e :in $ ?id :where [?e :testrun/id ?id]]
              (d/db conn) id)
         (map (comp d/touch (partial d/entity (d/db conn)) first))
         first)))

(comment
  ;; thoughts - the get-entity and get-testrun functions are not returning the datomic entity
  ;; and so cannot be used to walk further in the model without doing a query... although
  ;; maybe not true.. can query or ask for the entity. anyhoo, maybe to stick with the datomic entity
  ;; ... I can't recall why I moved it to a plain hashmap

  (def testrun-id (create-testrun))
  (add-tests-to-testrun testrun-id :t1 :s1)
  (def r (get-testrun testrun-id))

  ;; get the tests and suites given a testrun
  (->> (:testrun/entities r)
       (filter :test/id)
       first
       :test/id
       get-entity)

  (->> (:testrun/entities r)
       (filter :suite/id)
       first
       :suite/id
       get-entity)

  ;; get the list of suites, tests, fixtures or scripts available
  ;; - may filter on id, name and doc, or other attributes such as tags

  ;; for a given suite id, get the data required to render it, including related
  ;; entity (fixtures, tests and suites) ids, names and docs.

  ;; ditto for tests, fixtures and scripts.

  ;; get a list of driver functions, filtered by namespace, name or doc, or other meta we may add

  ;; get a listing of the most recent testruns, including their id and date and test result summary

  ;; create a new test run

  ;; add a suite or test to the test run

  ;; get the full hierarical view of a test run, including suites, tests and all fixtures, scripts, steps and events
  ;; - hmmm, do we really want it all, it may get large. It might be best to always work with something
  ;; a little more consumable.... so maybe only one "level" at a time. For example we get the immediate suites and tests,
  ;; and then we can ask for a suite and we'll get its fixtures and tests, or we can ask for a test and we'll
  ;; get its fixtures and script with all steps and execution events included.

  ;; get all suites a test belongs to (so we can run the test in the context of that suite)

  ;; get a test run's execution events after a given event

  ;; get the next step to run
  ;; - track this with a say a test run "cursor" that is the current test and the next step
  ;; - given the next step can we determine the current test? We can walk the reverse relationship
  ;; if it is a test step, but not if a fixture related step

  ;; run a step in a context and return the results
  ;; - the step is the expression, the context may define a wd driver to bind to

  ;; create a go block that reads a channel and runs each step received
  ;; and calls the callback after each step is performed
  ;; - the messages received on the channel include the step to execute,
  ;; the context to pass to the execute function, and a callback to call
  ;; with the results [return value, stdout] when done



  ;; get all the test runs
  (->> (d/q '[:find ?e :where [?e :testrun/id]]
            (d/db (d/connect uri)))
       (map (comp d/touch (partial d/entity (d/db (d/connect uri))) first)))

  (clojure.pprint/pp)

  )
