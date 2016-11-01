(ns rn.tablet.form
  (require rn.mobile.form
           rt.po.edit-form
           rt.po.view-form))

;; Redirect to the mobile and common libraries where possible
(def form-nav-back rt.po.view-form/form-nav-back)
(def get-field-value rt.po.view-form/get-field-value)
(def get-number-field-value rn.mobile.form/get-number-field-value)
(def set-choice-value rn.mobile.form/set-choice-value)
(def set-date-field-value rn.mobile.form/set-date-field-value)
(def set-lookup rn.mobile.form/set-lookup)
(def set-multi-select-choice-value rn.mobile.form/set-multi-select-choice-value)
(def set-number-field-value rn.mobile.form/set-number-field-value)
(def set-text-field-value rn.mobile.form/set-text-field-value)
