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
    
    public partial class states_master
    {
        public string state_master { get; set; }
        public Nullable<double> state_number { get; set; }
        public string state_name { get; set; }
        public Nullable<int> time_dif { get; set; }
        public string quote_redirect { get; set; }
        public string pageset { get; set; }
        public Nullable<bool> allow_amend { get; set; }
        public byte esig { get; set; }
        public byte vehicle_endorsement { get; set; }
        public Nullable<byte> esig_discount { get; set; }
        public bool allow_ho_instant_renter { get; set; }
        public string allow_trac_rating { get; set; }
        public string allow_trac_bind { get; set; }
        public bool allow_affinity_auto { get; set; }
        public bool allow_affinity_condo { get; set; }
        public bool allow_affinity_home { get; set; }
        public bool allow_affinity_renters { get; set; }
        public bool allow_affinity_embedded_renter { get; set; }
        public bool is_vibe_state { get; set; }
        public bool is_homeownerDisc { get; set; }
        public bool is_webDisc { get; set; }
        public bool isDnqBySdip_state { get; set; }
        public bool is_ivanka_state { get; set; }
        public bool is_new_bind_state { get; set; }
        public bool is_vibe_6mo_state { get; set; }
        public bool is_EDoc_state { get; set; }
        public bool is_kdquoteflow_state { get; set; }
    }
}
