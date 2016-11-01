(ns rt.test.expects
  "The ReadiTest expections library.

  TODO - add more!"
  (:require [rt.test.core :refer [do-report]]
            [rt.lib.util :refer [contains-match match get-time]]))

(defn assert-predicate
  [msg form]
  (let [args (rest form)
        pred (first form)]
    `(let [values# (list ~@args)
           result# (apply ~pred values#)]
       (if result#
         (do-report {:type     :pass
                     :message  ~msg
                     :expected '~form})
         (do-report {:type     :fail
                     :message  ~msg
                     :expected '~form
                     :actual   (list '~'not (cons '~pred values#))}))
       result#)))

(defmacro try-expr
  [msg form]
  `(try ~(assert-predicate msg form)
        (catch Throwable t#
          (do-report {:type     :error,
                      :message  ~msg,
                      :expected '~form, :actual t#}))))

(defmacro expect
  ([form] `(expect ~form nil))
  ([form msg] `(try-expr ~msg ~form)))

(defmacro expect-equals
  ([a b] `(expect (= ~a ~b)))
  ([a b msg] `(expect (= ~a ~b) ~msg)))

(defmacro expect-not
  ([a] `(expect (not ~a)))
  ([a msg] `(expect (not ~a) ~msg)))

(defmacro expect-contains
  "Passes if the first argument is a member of the second argument.

  Examples:
  (expect-contains \"hello\" (some-list-returning-function))
  "
  ([a b] `(expect-contains ~a ~b nil))
  ([a b msg] `(expect (clojure.set/subset? #{~a} (set ~b)) ~msg)))

(defmacro expect-match
  "Passes if the first argument regex pattern is found in the second list argument."
  ([a b] `(expect-match ~a ~b nil))
  ([a b msg] `(expect (match ~a ~b) ~msg)))

(defmacro expect-contains-match
  "Passes if the first argument regex pattern is matched to any member in the second list argument."
  ([a b] `(expect-contains-match ~a ~b nil))
  ([a b msg] `(expect (contains-match ~a ~b) ~msg)))

(defmacro expect-max-time
  "Given the number of msecs limit and an expression to eval, returns true if
  eval time is less than the limit."
  ([a b] `(expect-max-time ~a ~b '~b))
  ([a b msg] `(let [t0# (.getTime (java.util.Date.))
                    _# ~b
                    t1# (.getTime (java.util.Date.))]
                (expect (> ~a (- t1# t0#)) ~msg))))

