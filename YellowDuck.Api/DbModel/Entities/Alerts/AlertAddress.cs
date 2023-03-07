using System.Collections.Generic;
using System.Linq;

namespace YellowDuck.Api.DbModel.Entities.Alerts
{
    public class AlertAddress : IHaveLongIdentifier
    {
        public long Id { get; set; }

        public Alert Alert { get; set; }

        public string StreetNumber { get; set; }
        public string Route { get; set; }
        public string Locality { get; set; }
        public string PostalCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Neighborhood { get; set; }
        public string Sublocality { get; set; }

        public string Raw { get; set; }

        public string FormatedAddress
        {
            get {
                if (this == null) return "";
                var firstSection = new List<string>();
                var lastSection = new List<string>();
                if (!string.IsNullOrEmpty(StreetNumber)) firstSection.Add(StreetNumber);
                if (!string.IsNullOrEmpty(Route)) firstSection.Add(Route);
                if(!string.IsNullOrEmpty(Locality)) lastSection.Add(Locality);

                var address = string.Join(" ", firstSection);
                if (address.Trim() != "" && lastSection.Any()) address += ", ";
                address += string.Join(", ", lastSection);
                if (string.IsNullOrEmpty(address)) address = $"{Latitude}, {Longitude}";

                return address.Trim();
            }
        }
    }
}
