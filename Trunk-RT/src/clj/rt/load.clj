(ns rt.load
  (:require [rt.lib.util :refer [->int]]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]])
  (:import (java.util Date)))

(def rt-agents (atom {}))
(def rt-sessions (atom {}))
(def rt-quitting (atom false))

(defn update-agents [agents host max-sessions]
  {:pre [host]}
  (assoc agents host (merge (get agents host)
                            (when max-sessions {:max-sessions max-sessions})
                            {:last-modified    (Date.)
                             :desired-sessions 1})))

(defn update-sessions [sessions host id]
  {:pre [host id]}
  (-> sessions
      (assoc-in [host id :last-modified] (Date.))
      (update-in [host id :register-count] (fnil inc 0))))

(defn register-agent [host max-sessions]
  (let [max-sessions (->int max-sessions)]
    (info "RT Server - register-agent" host max-sessions)
    (swap! rt-agents update-agents host max-sessions)
    ;; return a command
    (cond
      @rt-quitting {:action :quit}
      :default {:action :start-sessions :count max-sessions})))

(defn register-session [host id]
  (info "RT Server- register-session" host id)
  (swap! rt-sessions update-sessions host id)
  ;; return a command
  (cond
    @rt-quitting {:action :quit}
    (= 10 (rand-int 20)) {:action :run-test :test :samples/suite :app-url "https://localhost"}
    :default {:action :snooze}))

(defn report-event [value]
  (info "RT Server - report-event" value))

(defn get-sessions []
  (let [t (- (.getTime (Date.)) 30000)]
    (->> @rt-sessions
         (remove #(< (-> % second :last-modified .getTime) t))
         (into {}))))

(comment

  @rt-agents
  @rt-sessions
  (get-sessions)

  )

