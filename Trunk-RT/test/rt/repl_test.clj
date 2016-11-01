(ns rt.repl-test
  (:require [midje.sweet :refer :all]
            [rt.repl :refer :all]
            rt.test.db))

(comment
(def mock-test-db {:t1 {:id :t1 :type :test }
                   :s1 {:id :s1 :type :testsuite }})

(background
  (rt.test.db/get-test-db) => mock-test-db)

(facts "some assumptions (still playing with midje)"
       (rt.test.db/get-test-db) => mock-test-db
       (count (rt.test.db/get-test-list)) => (count (vals mock-test-db)))

(fact "find all tests returns all tests duh"
      (count (find-tests "")) => #(= % (count (rt.test.db/get-test-list))))

(fact "find resulting in no matches returns 0"
       (count (find-tests "NOTESTHASTHISNAME^&&*^%")) => 0)

(facts "about listing tests"
       (get-tests) => #(= :t1 (-> % first :id))
       (get-tests) => #(= 0 (-> % first :index))
       (list-tests) => anything)

(fact "get-test-id-by-index works"
       (get-test-id-by-index 0) => :t1)

)