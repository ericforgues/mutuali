using System.Collections.Generic;
using System.Linq;
using YellowDuck.Api.DbModel.Enums;

namespace YellowDuck.Api.Helpers
{
    public static class AdCategoryHelper
    {
        public static string AdCategoryToFrenchString(AdCategory adCategory)
        {
            var dict = new Dictionary<AdCategory, string>() {
                { AdCategory.ProfessionalKitchen, "Cuisines professionnelles" },
                { AdCategory.DeliveryTruck, "Camions de livraison" },
                { AdCategory.StorageSpace, "Espaces d'entreposage" },
                { AdCategory.Other, "Autres" },
            };
            return dict[adCategory];
        }
    }
}

