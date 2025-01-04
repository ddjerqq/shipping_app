using System.ComponentModel.DataAnnotations;

namespace Domain.ValueObjects;

public enum PackageStatus
{
     /// <summary>
     /// on its way to the warehouse at the origin
     /// </summary>
     [Display(Name = "Awaiting")]
     Awaiting = 1,

     /// <summary>
     /// arrived at the warehouse at the origin
     /// </summary>
     [Display(Name = "Warehouse")]
     InWarehouse,

     /// <summary>
     /// on its way to the destination, either by flight, land, or sea
     /// </summary>
     [Display(Name = "Pending")]
     InTransit,

     /// <summary>
     /// arrived at the destination
     /// </summary>
     [Display(Name = "Arrived")]
     Arrived,

     /// <summary>
     /// collected by the receiver
     /// </summary>
     [Display(Name = "Delivered")]
     Delivered,
}