(ns rt.test.runner-test
  (:require [midje.sweet :refer :all]
            [rt.test.runner :refer :all]))

(comment
  (defn get-test-db []
    {})

  (facts "about test runner"
         (get-test-db) => {})

  (fact "can save an entity"
        (upsert-test-entity {}) => (throws AssertionError)
        (upsert-test-entity {:id 123}) => (throws AssertionError)
        (upsert-test-entity {:id :abc}) => {:id :abc})




  ;; Trying on some test driven development ....
  ;; What functions do I want?
  ;; To present a UI to allow the development CRUD of the various test entities
  ;; I would need the following
  ;;

  (unfinished get-driver-functions get-scripts)


  (background
    (get-driver-functions) => (seq [{:name "function-1"}])
    (get-scripts) => [{:id :script-1}])

  (fact (get-driver-functions) =not=> empty?)

  (fact (get-scripts) =not=> empty?))
