(ns rt.test.db-test
  (:require [midje.sweet :refer :all]
            [rt.test.db :refer :all]))

(comment
(facts "about get-test-list"
       (count (get-test-list)) => 0
       (provided
         (get-test-db) => {})

       (get-test-list) => (contains {:id ...test-id...})
       (provided
         (get-test-db) => {...test-id... {:id ...test-id...}}))


(fact "get-test-run-scripts doesn't crash"
      (get-test-run-scripts :blah) => anything)
)