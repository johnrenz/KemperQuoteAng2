//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoQuoteLibrary
{
    using System;
    using System.Collections.Generic;
    
    public partial class QuoteVehicle
    {
        public int PRIME_KEY { get; set; }
        public int PRIME_KEY_V { get; set; }
        public string QM_VEHICLE_NO_V { get; set; }
        public string QM_VEH_MAKE { get; set; }
        public string QM_VEH_MODEL { get; set; }
        public string QM_VEH_BODY_STYLE { get; set; }
        public string QM_VEH_VIN_NUMBER { get; set; }
        public string QM_ALT_GARAGE_CITY { get; set; }
        public Nullable<short> QM_VEH_ALT_TERR { get; set; }
        public Nullable<short> QM_VEH_YEAR { get; set; }
        public Nullable<short> QM_VEH_SYMBOL { get; set; }
        public Nullable<short> QM_RETIRED_DRIV_AGE_CAT { get; set; }
        public Nullable<short> QM_DISTANCE { get; set; }
        public Nullable<short> QM_DAYS_PER_WEEK { get; set; }
        public Nullable<short> QM_WEEKS_PER_MONTH { get; set; }
        public Nullable<short> QM_VEH_USE { get; set; }
        public Nullable<short> QM_PERCENT_BUSINESS { get; set; }
        public Nullable<short> QM_VEH_TYPE { get; set; }
        public Nullable<short> QM_PERFORMANCE { get; set; }
        public Nullable<short> QM_VEH_EXPOSURE { get; set; }
        public Nullable<short> QM_TERR_ADJ_TYPE { get; set; }
        public Nullable<short> QM_TERR_ADQ_TYPE { get; set; }
        public Nullable<short> QM_SAFE_VEH { get; set; }
        public Nullable<short> QM_EX_LIAB_TEST { get; set; }
        public Nullable<short> QM_STORED_VEH_TEST { get; set; }
        public Nullable<short> QM_DISABLING_DEVICE { get; set; }
        public Nullable<short> QM_PASSIVE_RESTRAINT { get; set; }
        public Nullable<short> QM_AIR_BAG_TEST { get; set; }
        public Nullable<short> QM_ANTI_LOCK_BRAKE_TEST { get; set; }
        public Nullable<short> QM_REG_TO_NON_RELATIVES_TEST { get; set; }
        public Nullable<short> QM_PARKING_TYPE { get; set; }
        public Nullable<short> QM_FACTORY_CUST_TEST { get; set; }
        public Nullable<short> QM_DRIVE_TYPE_TEST { get; set; }
        public Nullable<short> QM_TONAGE_TEST { get; set; }
        public Nullable<short> QM_MANUAL_OVERRIDE_TEST { get; set; }
        public Nullable<short> QM_MAKE_NUMBER { get; set; }
        public Nullable<short> QM_CAMPER_TOP_TEST { get; set; }
        public Nullable<short> QM_ALT_ZIP_LOCK_TEST { get; set; }
        public Nullable<short> QM_WINDOW_ETCHING_TEST { get; set; }
        public Nullable<short> QM_VEHICLE_RECOVERY_TEST { get; set; }
        public Nullable<short> QM_DAYTIME_LIGHTS_TEST { get; set; }
        public Nullable<int> QM_ALT_GAR_ZIP_CODE_1 { get; set; }
        public Nullable<int> QM_ALT_GAR_ZIP_CODE_2 { get; set; }
        public Nullable<int> QM_ANNUAL_MILEAGE { get; set; }
        public Nullable<int> QM_COST_NEW { get; set; }
        public Nullable<int> QM_COST_OF_CAMPER_TOP { get; set; }
        public string QM_ENTER_VEH_DATE { get; set; }
        public Nullable<int> QM_COVERED_PROP_AMT { get; set; }
        public Nullable<int> QM_CUSTOM_EQUIP_COST { get; set; }
        public Nullable<int> QM_STORED_VEH_DATE { get; set; }
        public Nullable<int> QM_MODEL_NUMBER { get; set; }
        public Nullable<int> QM_ADDL_INSURED_NO { get; set; }
        public Nullable<int> QM_LOSS_PAYEE_NO { get; set; }
        public Nullable<int> QM_TAX_CODE { get; set; }
        public Nullable<int> QM_CURRENT_ODOM { get; set; }
        public Nullable<int> QM_PURCHASE_ODOM { get; set; }
        public Nullable<int> QM_PURCH_VEH_DATE { get; set; }
        public Nullable<short> QM_MULTI_CAR_DIS { get; set; }
        public Nullable<short> QM_SAFE_VEH_DIS { get; set; }
        public Nullable<short> QM_RETIRED_DRIV_DIS { get; set; }
        public Nullable<short> QM_PREMIER_DISCOUNT_DATA { get; set; }
        public Nullable<short> QM_RENEWAL_CREDIT_DATA { get; set; }
        public Nullable<short> QM_MULTI_POLICY_DIS { get; set; }
        public Nullable<short> QM_MUNI_RATE { get; set; }
        public Nullable<short> QM_MUNI_SURCHARGE { get; set; }
        public Nullable<short> QM_AUTO_SURCHARGE { get; set; }
        public Nullable<short> QM_WEB_DATA { get; set; }
        public Nullable<short> QM_LEASE_AI_TEST { get; set; }
        public Nullable<short> QM_ORIG_VEHICLE_NO { get; set; }
        public string QM_RO_ADDL_FIRST_NAME { get; set; }
        public string QM_RO_ADDL_LAST_NAME { get; set; }
        public string QM_RO_ADDL_MIDDLE_INIT { get; set; }
        public string QM_RO_ADDRESS_CITY { get; set; }
        public string QM_RO_ADDRESS_LINE_1 { get; set; }
        public string QM_RO_FIRST_NAME { get; set; }
        public string QM_RO_LAST_NAME { get; set; }
        public string QM_RO_MIDDLE_INIT { get; set; }
        public string QM_RO_STATE_CODE { get; set; }
        public string QM_RO_ZIP_CODE_1 { get; set; }
        public string QM_RO_ZIP_CODE_2 { get; set; }
        public Nullable<int> QM_VEHICLE_ADDED_DATE { get; set; }
        public Nullable<int> QM_ODOM_2 { get; set; }
        public Nullable<int> QM_ODOM_DATE_2 { get; set; }
        public string QM_RO_ADDRESS_LINE_2 { get; set; }
        public Nullable<int> QM_CUST_EST_PLEASURE_MILES { get; set; }
        public Nullable<short> QM_VEH_SYMBOL_LIAB { get; set; }
        public Nullable<short> QM_VEH_SYMBOL_COMP { get; set; }
        public Nullable<short> QM_VEH_SYMBOL_COLL { get; set; }
        public Nullable<short> QM_VEH_SYMBOL_PIP { get; set; }
        public Nullable<short> QM_VEH_SYMBOL_YEAR_ADJ { get; set; }
        public Nullable<short> QM_VEH_TERR_BI { get; set; }
        public Nullable<short> QM_VEH_TERR_PD { get; set; }
        public Nullable<short> QM_VEH_TERR_PIP { get; set; }
        public Nullable<short> QM_VEH_HI_PERF_IND { get; set; }
        public Nullable<short> QM_HOMEOWNER_DIS { get; set; }
        public Nullable<short> QM_PD_IN_FULL_DIS { get; set; }
        public Nullable<short> QM_INSPECTION_REQ_TEST { get; set; }
        public Nullable<short> QM_EFT_DIS { get; set; }
        public Nullable<short> QM_VEH_TERR_COMP { get; set; }
        public Nullable<short> QM_VEH_TERR_COLL { get; set; }
        public Nullable<short> QM_VEH_TERR_UM { get; set; }
        public string QM_ADJUST_TO_MAKE_MODEL { get; set; }
        public Nullable<short> QM_FL_COUNTY_TEST { get; set; }
        public Nullable<short> QM_REPLACEMENT_VEHICLE_FLAG { get; set; }
        public string QM_YEAR_MAKE_MODEL { get; set; }
        public Nullable<int> QM_CITY_CODE { get; set; }
        public Nullable<int> QM_COUNTY_CODE { get; set; }
        public Nullable<int> Qm_prior_annual_mileage { get; set; }
        public Nullable<int> QM_CURR_ODOM_DATE { get; set; }
        public Nullable<int> QM_PRIOR_ODOM { get; set; }
        public Nullable<int> QM_PRIOR_ODOM_DATE { get; set; }
        public Nullable<int> QM_UPD_ANNUAL_MILEAGE { get; set; }
        public Nullable<int> QM_UPD_CURRENT_ODOM { get; set; }
        public Nullable<int> QM_UPD_ODOM_DATE { get; set; }
        public Nullable<int> QM_ODOM_NEEDS_UPD_FLAG { get; set; }
        public Nullable<short> QM_VEH_ISO_COMP { get; set; }
        public Nullable<short> QM_VEH_ISO_COLL { get; set; }
        public string QM_VEH_WEB_MODEL { get; set; }
    }
}
