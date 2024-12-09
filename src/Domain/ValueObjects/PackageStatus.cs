namespace Domain.ValueObjects;

public enum PackageStatus
{
     /// <summary>
     /// on its way to the warehouse at the origin
     /// </summary>
     Awaiting = 1,

     /// <summary>
     /// arrived at the warehouse at the origin
     /// </summary>
     AtOrigin,

     /// <summary>
     /// on its way to the destination, either by flight, land, or sea
     /// </summary>
     InTransit,

     /// <summary>
     /// arrived at the destination
     /// </summary>
     AtDestination,

     /// <summary>
     /// collected by the receiver
     /// </summary>
     Delivered,
}