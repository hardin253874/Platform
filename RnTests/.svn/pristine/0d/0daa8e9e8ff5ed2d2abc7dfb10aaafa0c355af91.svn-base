(ns rn.scripts.bcm)

(defn edit-bu "Assumes showing BU report"
  [bu]
  (rt.po.report-view/set-search-text bu)
  (rt.po.report-view/right-click-row-by-text bu)
  (rt.po.app/choose-context-menu "Edit"))

(defn fill-bf-form
  "Assume showing BF form"
  [{:keys [name owner op-impacts]}]
  (when name (rt.po.edit-form/set-text-field-value "Business Function" name))
  (when owner (rt.po.edit-form/set-lookup "Owner" owner))
  (when (not-empty op-impacts)
    (rt.po.edit-form/set-choice-value-v2 "Op Impact 1 day" (rand-nth op-impacts))
    (rt.po.edit-form/set-choice-value-v2 "Op Impact 2-5 days" (rand-nth op-impacts))
    (rt.po.edit-form/set-choice-value-v2 "Op Impact 1 week+" (rand-nth op-impacts)))
  (rt.po.edit-form/set-number-value-v2 "Fin Impact 1 day" (rand-int 10000))
  (rt.po.edit-form/set-number-value-v2 "Fin Impact 2-5 days" (rand-int 100000))
  (rt.po.edit-form/set-number-value-v2 "Fin Impact 1 week" (rand-int 1000000)))

(defn fill-recovery-requirement-form
  "Assume showing recovery requirement form"
  [{:keys [buildings sites]}]
  (when (not-empty buildings) (rt.po.edit-form/set-lookup "Building" (rand-nth buildings)))
  (when (not-empty sites) (rt.po.edit-form/set-lookup "Recovery Site" (rand-nth sites)))
  (rt.po.edit-form/set-number-value-v2 "People Day 1" (rand-int 20))
  (rt.po.edit-form/set-number-value-v2 "People Day 2-5" (rand-int 200))
  (rt.po.edit-form/set-number-value-v2 "People Week 1" (rand-int 1000))
  (rt.po.edit-form/set-number-value-v2 "Positions Day 1" (rand-int 10))
  (rt.po.edit-form/set-number-value-v2 "Positions Day 2-5" (rand-int 100))
  (rt.po.edit-form/set-number-value-v2 "Positions Week 1" (rand-int 500)))

(defn fill-plan-form [n stage]
  (rt.po.edit-form/set-number-value-v2 "Sequence number" n)
  (rt.po.edit-form/set-choice-value-v2 "Stage" stage)
  (rt.po.edit-form/set-text-field-value "Action" (str "Do something useful for step " n))
  (rt.po.edit-form/set-multiline "Detail" "Seriously, make it so!")
  (rt.po.edit-form/set-number-value-v2 "Duration" (rand-int 5)))

(defn fill-bu-risk-form
  [{:keys [name bu-name levels drivers impacts likelihoods owner mitigating-controls]}]
  (when name (rt.po.edit-form/set-text-field-value "Name" name))
  (when (not-empty levels) (rt.po.edit-form/set-choice-value-v2 "Level of Risk" (rand-nth levels)))
  (when (not-empty drivers) (rt.po.edit-form/set-choice-value-v2 "Risk Driver" (rand-nth drivers)))
  (when (not-empty likelihoods)
    (rt.po.edit-form/set-lookup "Inherent Likelihood" (rand-nth likelihoods))
    (rt.po.edit-form/set-lookup "Residual Likelihood" (rand-nth likelihoods))
    (rt.po.edit-form/set-lookup "Target Likelihood" (rand-nth likelihoods)))
  (when (not-empty impacts)
    (rt.po.edit-form/set-lookup "Inherent Impact" (rand-nth impacts))
    (rt.po.edit-form/set-lookup "Residual Impact" (rand-nth impacts))
    (rt.po.edit-form/set-lookup "Target Impact" (rand-nth impacts)))
  (when owner (rt.po.edit-form/set-lookup "Owner" owner))
  (when bu-name (rt.po.edit-form/set-lookup "Business Unit" bu-name))
  (when (not-empty mitigating-controls)
    (rt.po.view-form/open-tab-action-menu "Mitigating Control" "Link")
    (rt.po.edit-form/choose-in-entity-picker-dialog (rand-nth mitigating-controls))))