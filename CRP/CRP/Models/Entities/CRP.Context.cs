﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRP.Models.Entities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CRPEntities : DbContext
    {
        public CRPEntities()
            : base("name=CRPEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<BookingReceipt> BookingReceipts { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Garage> Garages { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Model> Models { get; set; }
        public virtual DbSet<ModelCategoryMapping> ModelCategoryMappings { get; set; }
        public virtual DbSet<Nation> Nations { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<PriceGroup> PriceGroups { get; set; }
        public virtual DbSet<PriceGroupItem> PriceGroupItems { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<VehicleGroup> VehicleGroups { get; set; }
        public virtual DbSet<VehicleImage> VehicleImages { get; set; }
    }
}
