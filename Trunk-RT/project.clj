(defproject rt "0.1.0"
            :description "ReadiNow Test Automation"
            :url "http://www.enterprisedata.com.au"
            :dependencies [[org.clojure/clojure "1.8.0"]
                           [org.clojure/core.async "0.2.374"]
                           [org.clojure/tools.reader "1.0.0-alpha2"]
                           [org.clojure/tools.cli "0.3.5"]
                           [org.clojure/tools.nrepl "0.2.11"]
                           [org.clojure/tools.namespace "0.2.11"]
                           [org.clojure/data.codec "0.1.0"]
                           [org.clojure/data.csv "0.1.3"]
                           [org.clojure/data.json "0.2.6"]
                           [org.clojure/data.xml "0.0.8"]
                           [ring "1.4.0"]
                           [ring/ring-json "0.4.0"]
                           [ring-middleware-format "0.7.0"]
                           [compojure "1.4.0"]
                           [clj-http "2.0.1"]
                           [com.taoensso/timbre "4.2.1"]
                           [org.seleniumhq.selenium/selenium-java "2.53.1"]
                           [org.seleniumhq.selenium/selenium-server "2.53.1"]
                           [org.seleniumhq.selenium/selenium-remote-driver "2.53.1"]
                           [org.seleniumhq.selenium/selenium-support "2.53.1"]
                           [org.seleniumhq.selenium/htmlunit-driver "2.21"]
                           [com.codeborne/phantomjsdriver "1.2.1"]
                           [clj-webdriver "0.6.1" :exclusions [org.clojure/core.cache]]
                           [clj-time "0.11.0"]
                           [com.datomic/datomic-free "0.9.5130" :exclusions [joda-time]]
                           [cljfmt "0.3.0"]

                           [org.clojure/clojurescript "1.7.228"]
                           [cljs-http "0.1.39"]
                           [cljsjs/react "0.14.3-0"]
                           [reagent "0.6.0-alpha"]
                           [reagent-forms "0.5.15"]
                           [reagent-utils "0.1.7"]
                           [re-frame "0.7.0-alpha-2"]
                           [secretary "1.2.3"]
                           [spellhouse/clairvoyant "0.0-72-g15e1e44"]
                           [markdown-clj "0.9.85"]]

            :profiles {:dev {:dependencies [[midje "1.8.3"]]
                             :plugins      [[lein-midje "3.2"]]}}

            :plugins [[lein-cljsbuild "1.1.2"]
                      [lein-ring "0.9.7"]
                      [lein-pdo "0.1.1"]
                      [lein-marginalia "0.8.0"]
                      [lein-codox "0.9.1"]
                      ]

            :aliases {"up" ["pdo" "cljsbuild" "auto" "dev," "ring" "server-headless"]}

            :min-lein-version "2.0.0"

            :ring {:handler      rt.server/app
                   :init         rt.server/init
                   :auto-reload? false}

            :codox {:metadata   {:doc/format :markdown}
                    :output-path "resources/public/doc"}

            :main rt.main
            :aot [rt.main]
            :jvm-opts ["-Xmx1G" "-XX:-OmitStackTraceInFastThrow"]
            :jar-name "rt-nodeps.jar"
            :uberjar-name "rt-standalone.jar"

            :source-paths ["src/clj"]

            :cljsbuild {:builds [{:id           "dev"
                                  :source-paths ["src/cljs"]
                                  :compiler     {:output-to     "resources/public/js/app.js"
                                                 :output-dir    "resources/public/js/out"
                                                 :optimizations :none
                                                 :source-map    true}}
                                 {:id           "release"
                                  :source-paths ["src/cljs"]
                                  :compiler     {:output-to     "resources/public/js/app.js"
                                                 :optimizations :advanced
                                                 :pretty-print  false
                                                 :preamble      ["react/react.min.js"]
                                                 :externs       ["react/externs/react.js"]}}]})
