using System.Collections.Generic;
using YellowDuck.Api.DbModel.Enums;
using System;

namespace YellowDuck.Api.DbModel.Entities.Alerts
{
    public class Alert : IHaveLongIdentifier
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public AdCategory Category { get; set; }
        public IList<AlertProfessionalKitchenEquipment> ProfessionalKitchenEquipments { get; set; }
        public DeliveryTruckType? DeliveryTruckType { get; set; }
        public bool Refrigerated { get; set; }
        public bool CanSharedRoad { get; set; }
        public bool CanHaveDriver { get; set; }
        public bool DayAvailability { get; set; }
        public bool EveningAvailability { get; set; }
        public double? Radius { get; set; }
        public long AddressId { get; set; }
        public AlertAddress Address { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastSendDateUTC { get; set; }
    }
}
