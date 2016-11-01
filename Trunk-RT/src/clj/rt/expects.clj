(ns rt.expects
  "The ReadiTest expections library.

  TODO - add more!"
  (:require [rt.session :refer [raise-event!]]))

(defn assert-predicate
  [msg form]
  (let [args (rest form)
        pred (first form)]
    `(let [values# (list ~@args)
           result# (apply ~pred values#)]
       (if result#
         (raise-event! {:type     :pass
                       :message  ~msg
                       :expected '~form})
         (raise-event! {:type     :fail
                       :message  ~msg
                       :expected '~form
                       :actual   (list '~'not (cons '~pred values#))}))
       result#)))

(defmacro try-expr
  [msg form]
  `(try ~(assert-predicate msg form)
        (catch Throwable t#
          (raise-event! {:type     :error,
                        :message  ~msg,
                        :expected '~form, :actual t#}))))

(defmacro expect
  ([form] `(expect ~form nil))
  ([form msg] `(try-expr ~msg ~form)))

(defmacro expect-equals
  ([a b] `(expect (= ~a ~b)))
  ([a b msg] `(expect (= ~a ~b) ~msg)))


