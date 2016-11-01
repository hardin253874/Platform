(ns rt.scratch.conc
  (:require [clj-webdriver.taxi :refer :all]
            [clj-webdriver.core :refer [->actions move-to-element]]
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
            [rt.test.expects :refer [expect expect-equals]]
            [rt.test.core :refer [*tc* *test-context*]]
            [clj-webdriver.taxi :as taxi]))


(comment

  (* 2 2)

  )

(comment

  (do
    ;; NORMAL
    (alter-var-root (var *tc*)
                    (constantly (merge {:tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                       {:target :chrome})))

    ;; MOBILE
    ;(start-browser :chrome {:device "Apple iPhone 5" :width 400 :height 800})
    (alter-var-root (var *tc*)
                    (constantly (merge {:tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                       {:target :chrome}
                                       {:target-device "Apple iPhone 5"
                                        :target-width  400
                                        :target-height 800})))

    ;; TABLET
    ;(start-browser :chrome {:device "Apple iPad 3 / 4" :width 1024 :height 768})
    ;(app/start-app)
    ;(app/login)
    (alter-var-root (var *tc*)
                    (constantly (merge {:tenant   "EDC"
                                        :username "Administrator"
                                        :password "tacoT0wn"}
                                       {:target :chrome}
                                       {:target-device "Apple iPad"
                                        :target-width  1024
                                        :target-height 768})))


    (clojure.pprint/pprint *tc*)

    (rn.common/start-app-and-login)

    (rt.app/setup-environment)

    ; Steps
    (do)    

    )
  )
