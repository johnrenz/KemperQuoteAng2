﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AutoQuoteEntitie7 : DbContext
    {
        public AutoQuoteEntitie7()
            : base("name=AutoQuoteEntitie7")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ModelYearMake> ModelYearMakes { get; set; }
        public virtual DbSet<Quote> Quotes { get; set; }
        public virtual DbSet<QuoteAccident> QuoteAccidents { get; set; }
        public virtual DbSet<QuoteDriver> QuoteDrivers { get; set; }
        public virtual DbSet<QuoteVehicle> QuoteVehicles { get; set; }
        public virtual DbSet<QuoteViolation> QuoteViolations { get; set; }
        public virtual DbSet<states_master> states_master { get; set; }
        public virtual DbSet<tbl_web_session> tbl_web_session { get; set; }
        public virtual DbSet<tbl_web_session_flex> tbl_web_session_flex { get; set; }
        public virtual DbSet<tbl_web_split_zips> tbl_web_split_zips { get; set; }
        public virtual DbSet<tbl_web_state_zip_ranges> tbl_web_state_zip_ranges { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<YearMake> YearMakes { get; set; }
    }
}
