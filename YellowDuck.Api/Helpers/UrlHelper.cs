using GraphQL.Conventions;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using YellowDuck.Api.DbModel.Enums;

namespace YellowDuck.Api.Helpers
{
    public static class UrlHelper
    {
        public static string ConfirmEmail(string to, string token, string lang = null)
        {
            var langParameter = (lang != null) ? $"&lang={lang}" : "";
            return $"confirmez-courriel?email={System.Net.WebUtility.UrlEncode(to)}&token={System.Net.WebUtility.UrlEncode(token)}{langParameter}";
        }

        public static string RegistrationAdmin(string to, string token)
        {
            return $"registration/admin?email={System.Net.WebUtility.UrlEncode(to)}&token={System.Net.WebUtility.UrlEncode(token)}";
        }

        public static string ResetPassword(string to, string token, string lang = null)
        {
            var langParameter = (lang != null) ? $"&lang={lang}" : "";
            return $"reinitialiser-mot-de-passe?email={System.Net.WebUtility.UrlEncode(to)}&token={System.Net.WebUtility.UrlEncode(token)}{langParameter}";
        }

        public static string ManageAlert(string lang = null)
        {
            var langParameter = (lang != null) ? $"?lang={lang}" : "";
            return $"alertes{langParameter}";
        }

        public static string ConfirmAlert(string id, string email, string lang = null)
        {
            var langParameter = (lang != null) ? $"&lang={lang}" : "";
            return $"alertes/confirmer/{id}?email={email}{langParameter}";
        }

        public static string UnsubscribeAlert(Id id, string lang = null)
        {
            var langParameter = (lang != null) ? $"?lang={lang}" : "";
            return $"alertes/supprimer/{id}{langParameter}";
        }

        public static string Ad(Id id, string lang = null)
        {
            var langParameter = (lang != null) ? $"?lang={lang}" : "";
            return $"annonces/{id}{langParameter}";
        }

        public static string AdsFiltered(AdsFilteredParams filters, string lang = null)
        {
            var snakeCaseStrategy = new SnakeCaseNamingStrategy();
            var langParameter = (lang != null) ? $"&lang={lang}" : "";
            var queryParameters = new List<string>
            {
                $"address={System.Net.WebUtility.UrlEncode(filters.Address)}",
                $"lat={System.Net.WebUtility.UrlEncode(filters.Latitude.ToString())}",
                $"lon={System.Net.WebUtility.UrlEncode(filters.Longitude.ToString())}",
                $"category={snakeCaseStrategy.GetPropertyName(filters.Category.ToString(), false).ToUpper()}"
            };
            if (filters.DeliveryTruckType != null) queryParameters.Add($"deliveryTruckType={snakeCaseStrategy.GetPropertyName(filters.DeliveryTruckType.ToString(), false).ToUpper()}");
            if (filters.DayAvailability) queryParameters.Add("dayAvailability=ANY");
            if (filters.EveningAvailability) queryParameters.Add("eveningAvailability=ANY");
            if (filters.Refrigerated) queryParameters.Add("refrigerated=true");
            if (filters.CanHaveDriver) queryParameters.Add("canHaveDriver=true");
            if (filters.CanSharedRoad) queryParameters.Add("canSharedRoad=true");
            if (filters.View != null) queryParameters.Add($"view={filters.View}");
            if(filters.ProfessionalKitchenEquipment?.Any() ?? false)
            {
                foreach(var professionalKitchenEquipment in filters.ProfessionalKitchenEquipment)
                {
                    queryParameters.Add($"professionalKitchenEquipment={snakeCaseStrategy.GetPropertyName(professionalKitchenEquipment.ToString(), false).ToUpper()}");
                }
            }

            return $"annonces?{string.Join("&", queryParameters)}{langParameter}";
        }

        public class AdsFilteredParams
        {
            public AdCategory? Category { get; set; }
            public DeliveryTruckType? DeliveryTruckType { get; set; }
            public IList<ProfessionalKitchenEquipment> ProfessionalKitchenEquipment { get; set; }
            public bool DayAvailability { get; set; }
            public bool EveningAvailability { get; set; }
            public bool Refrigerated { get; set; }
            public bool CanHaveDriver { get; set; }
            public bool CanSharedRoad { get; set; }
            public string Address { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public ViewMode? View { get; set; }
            public SortType? Sort { get; set; }
            public DirectionValue? Direction { get; set; }

            public enum ViewMode
            {
                LIST,
                MAP
            }

            public enum SortType
            {
                DATE,
                DISTANCE
            }

            public enum DirectionValue
            {
                ASC,
                DESC
            }
        }
    }
}
