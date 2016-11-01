(ns rn.services.security
  (:require [rt.lib.wd-rn :refer [get-entities-of-type put-entity]]
            [rt.lib.util :refer [make-test-name]]
            [rt.lib.wd :refer [execute-async-script]]
            [rn.services.report :refer [run-report]]
            [clojure.data.json :as json]
            [rn.services.entity :refer [get-entity-id-for-name]]
            [taoensso.timbre :refer [trace debug info warn error tracef debugf infof warnf errorf]]))

(def ^:private test-account-pwd "Read1N0w")

(defn get-accounts-in-roles
  "Returns sequence maps with :person and :account pairs, each with name and id"
  [roles]
  (let [fmt-str "([Person has user account].[Security roles].Name = '%s' or [Person has user account].[Security roles].[Included by roles].Name = '%s')"
        filters (map #(format fmt-str % %)
                     roles)
        filters (interpose " and " filters)
        filter (apply str "[Person has user account].[Account status].Name = 'Active' and " filters)
        results (get-entities-of-type "person" "name,personHasUserAccount.name" {:filter filter})]
    (if (:error results)
      results
      (->> results
           (remove (comp not :name))
           (map #(hash-map :person {:name (:name %) :id (-> % :_id :_id)}
                           :account {:name (-> % :personHasUserAccount first :name)
                                     :id   (-> % :personHasUserAccount first :_id :_id)}))))))

(defn get-roles-for-account [account-name]
  (let [results (get-entities-of-type "userAccount" "name,userHasRole.name"
                                      {:filter (format "Name = '%s'" account-name)})]
    (if (:error results)
      results
      (->> results first :userHasRole
           (map #(hash-map :name (-> % :name) :id (-> % :_id :_id)))))))

(defn get-accounts-in-role-old [role] (get-accounts-in-roles [role]))

(defn get-accounts-in-role [role]
  (->> (run-report "User Accounts" {:conds [{:title "User roles" :oper "Contains" :value role}]})
       (map #(hash-map :account {:name (-> % (get "Username")) :id (:id %)}
                       :person (-> % (get "Account holder") first)))))

(defn get-account-holder-name [account-name]
  (->> (get-entities-of-type "userAccount" "name,accountHolder.name"
                             {:filter (format "Name = '%s'" account-name)})
       first :accountHolder :name))

(defn create-account
  ([account-name] (create-account account-name ["Administrators"] "Person"))
  ([account-name role-names holder-type-name]
   (let [role-ids (->> role-names
                       (map #(get-entity-id-for-name % "role"))
                       (filter identity))
         _ (when (empty? role-ids) (throw (Exception. (str "Cannot find roles: " role-names))))
         holder-type-id (get-entity-id-for-name holder-type-name "definition")
         result (put-entity {"typeId"        "core:userAccount"
                             "name"          account-name
                             "password"      test-account-pwd
                             "accountStatus" {"id" "active"}
                             "accountHolder" {"typeId" holder-type-id
                                              "name"   account-name}
                             "userHasRole"   (mapv #(hash-map "id" %) role-ids)})]
     (if (:error result)
       {:error (-> result :error :data :Message)}
       result))))

(defn ensure-account-in-roles
  [account-name role-names]
  (let [role-ids (into #{} (concat (map :id (get-roles-for-account account-name))
                                   (map #(get-entity-id-for-name % "role") role-names)))
        account-id (get-entity-id-for-name account-name "userAccount")
        result (put-entity {"id"            account-id
                            "accountStatus" {"id" "active"}
                            "userHasRole"   (mapv #(hash-map "id" %) role-ids)})]
    (if (:error result)
      {:error (-> result :error :data :Message)}
      result)))

(defn ensure-account-only-in-roles
  [account-name role-names]
  (let [role-ids (into #{} (map #(get-entity-id-for-name % "role") role-names))
        account-id (get-entity-id-for-name account-name "userAccount")
        result (put-entity {"id"            account-id
                            "accountStatus" {"id" "active"}
                            "userHasRole"   (mapv #(hash-map "id" %) role-ids)})]
    (if (:error result)
      {:error (-> result :error :data :Message)}
      result)))

(defn ensure-test-accounts
  "This only creates and doesn't update. So use a prefix that matches the desired roles...
  And it only looks for accounts in the first role, but create, if needed, in all roles..."
  [role-names prefix desired-count]
  (let [get-test-accounts (fn [] (->> (get-accounts-in-role (first role-names))
                                      (filter #(some-> % :account :name (.startsWith prefix)))))
        test-accounts (get-test-accounts)]
    (when (< (count test-accounts) desired-count)
      (info "Creating" (- desired-count (count test-accounts)) "accounts with roles" role-names)
      (doall (repeatedly (- desired-count (count test-accounts))
                         #(create-account (make-test-name prefix) role-names "Employee"))))
    (->> (get-test-accounts)
         (map #(hash-map :username (-> % :account :name) :password test-account-pwd)))))

(comment

  (run-report "User Accounts" {:conds [{:title "Username" :oper "StartsWith" :value "BIAUSER"}]})

  (->> (run-report "User Roles" {:conds [{:title "User Role" :oper "Contains" :value "BIA"}]}))

  (->> (run-report "User Accounts" {:conds [{:title "User roles" :oper "Contains" :value "BIA"}]})
       count)

  (get-entity-id-for-name "Employee")

  (get-roles-for-account "BIAUser")

  (->> (get-accounts-in-role-old "BIA User") first)
  (->> (get-accounts-in-role "BIA User") first)

  (get-accounts-in-role "Risk Manager")

  (create-account (make-test-name "RT-User") "Risk Manager")

  (get-entity-id-for-name "BCP Manager" "role")
  (get-entity-id-for-name "Employee" "definition")

  (get-accounts-in-role "BCP Manager")

  (ensure-account-in-roles "BIAUser" ["BCP Manager" "BIA User" "Staff"])
  (ensure-account-only-in-roles "BIAUser" ["BCP Manager" "BIA User"])
  (get-accounts-in-roles ["Administrators"])

  (rt.lib.util/timeit "" (->> (get-accounts-in-role "BIA User")
                              (filter #(some-> % :account :name (.startsWith "BIAUSER")))
                              count))
  (rt.lib.util/timeit "" (->> (get-accounts-in-role "BCP Manager")
                              (filter #(some-> % :account :name (.startsWith "BCP")))
                              ))

  (rt.lib.util/timeit "" (ensure-test-accounts ["BIA User"] "BIAUSER" 50))
  (rt.lib.util/timeit "" (ensure-test-accounts ["BCP Manager"] "BCP" 50))

  (get-fixed-list "BCP Manager")
  )

