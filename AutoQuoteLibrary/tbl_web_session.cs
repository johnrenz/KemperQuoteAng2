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
    
    public partial class tbl_web_session
    {
        public System.Guid guid { get; set; }
        public string drc_xml { get; set; }
        public string current_page { get; set; }
        public Nullable<byte> err_count { get; set; }
        public string err_details { get; set; }
        public Nullable<System.DateTime> first_save { get; set; }
        public Nullable<System.DateTime> last_save { get; set; }
        public Nullable<int> queue_inuse { get; set; }
        public Nullable<byte> queue_complete { get; set; }
        public Nullable<System.DateTime> queue_start { get; set; }
        public Nullable<System.DateTime> queue_end { get; set; }
        public Nullable<System.DateTime> drc_end { get; set; }
        public Nullable<int> amf_account_no { get; set; }
        public string keycode { get; set; }
        public string quote_number { get; set; }
        public string consumer_id { get; set; }
        public string comments { get; set; }
        public string csr_queue { get; set; }
        public string quote_errmsg { get; set; }
        public Nullable<int> bind_status { get; set; }
        public Nullable<int> uw_comp { get; set; }
        public string knockout { get; set; }
        public Nullable<byte> dnq_template { get; set; }
        public Nullable<byte> dnq_reason { get; set; }
        public Nullable<byte> dnq_email_sent { get; set; }
        public Nullable<double> oqpremium { get; set; }
        public Nullable<int> oqpremiumfulltort { get; set; }
        public Nullable<int> drc_premium { get; set; }
        public Nullable<int> drc_premiumfulltort { get; set; }
        public Nullable<int> email_success { get; set; }
        public Nullable<System.DateTime> email_date_sent { get; set; }
        public Nullable<byte> email_attempt_no { get; set; }
        public string orig_quote_no { get; set; }
        public string leadprofileid { get; set; }
        public string quote_id { get; set; }
        public string orig_app { get; set; }
        public string email { get; set; }
        public string dnq_description { get; set; }
        public Nullable<byte> form_complete { get; set; }
        public Nullable<byte> email_suppress { get; set; }
        public Nullable<int> clickthru_partner_id { get; set; }
        public string clickthru_custom { get; set; }
        public Nullable<int> amend_status { get; set; }
        public Nullable<System.DateTime> followup_email { get; set; }
        public string state { get; set; }
        public string keywords { get; set; }
        public Nullable<decimal> referer_id { get; set; }
    }
}
