(ns rt.session-test
  (:require [midje.sweet :refer :all]
            [rt.session :refer :all]))

(comment
  (facts "about test session"
         (create-sample-session) =not=> nil?))