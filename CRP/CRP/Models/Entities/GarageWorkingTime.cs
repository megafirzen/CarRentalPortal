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
    
    public partial class GarageWorkingTime
    {
        public int GarageID { get; set; }
        public int DayOfWeek { get; set; }
        public int StartTimeInMinute { get; set; }
        public int EndTimeInMinute { get; set; }
    
        public virtual Garage Garage { get; set; }
    }
}
