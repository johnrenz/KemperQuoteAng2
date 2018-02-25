using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using AutoQuoteLibrary.AutoQuoteHelper;

namespace AutoQuoteLibrary.Concrete
{
    public class EFDbContext : DbContext
    {
        public DbSet<VehicleMake> VehicleMakes {get;set;}
        public DbSet<VehicleModel> VehicleModels {get;set;}
        public DbSet<VehicleInfo> Vehicles {get;set;}
    }
}