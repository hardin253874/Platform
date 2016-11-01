(ns rt.scratch.template
  (:require [clj-webdriver.taxi :refer :all]
            [rt.scripts.common :refer [start-app-and-login]]
            [rt.app :refer [setup-environment]]
            [clojure.repl :refer :all]
            [clojure.pprint :refer [pprint]]
            [clojure.string :as string]
            [clojure.walk :as walk]
            [clojure.data.json :as json]))

(comment

  (* 2 2)

  )


(comment

  (do

    (setup-environment)

    (alter-var-root (var *test-context*)
                    (constantly {:tenant   "EDC"
                                :username "Administrator"
                                :password "tacoT0wn"
                                :target :chrome}))

    (clojure.pprint/pprint *test-context*)
    (start-app-and-login)
  )
  )
