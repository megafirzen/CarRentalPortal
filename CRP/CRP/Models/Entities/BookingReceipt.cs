//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class BookingReceipt
    {
        public int ID { get; set; }
        public string CustomerID { get; set; }
        public double RentalPrice { get; set; }
        public double Deposit { get; set; }
        public double BookingFee { get; set; }
        public Nullable<decimal> Star { get; set; }
        public string Comment { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public Nullable<int> VehicleID { get; set; }
        public string LicenseNumber { get; set; }
        public string VehicleName { get; set; }
        public string GarageName { get; set; }
        public string GarageAddress { get; set; }
        public int ModelID { get; set; }
        public int Year { get; set; }
        public int TransmissionType { get; set; }
        public string TransmissionDetail { get; set; }
        public Nullable<int> FuelType { get; set; }
        public string Engine { get; set; }
        public int Color { get; set; }
        public bool IsCanceled { get; set; }
        public bool IsPending { get; set; }
        public bool IsSelfBooking { get; set; }
        public int GarageID { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Model Model { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual Garage Garage { get; set; }
    }
}
