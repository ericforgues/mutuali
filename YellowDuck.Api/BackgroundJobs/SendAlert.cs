using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using YellowDuck.Api.DbModel;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.EmailTemplates.Models;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Helpers;
using YellowDuck.Api.Services.Mailer;

namespace YellowDuck.Api.BackgroundJobs
{
    public class SendAlert
    {
        private readonly AppDbContext db;
        private readonly IMailer mailer;
        private readonly ILogger<SendAlert> logger;

        public SendAlert(AppDbContext db, IMailer mailer, ILogger<SendAlert> logger)
        {
            this.db = db;
            this.mailer = mailer;
            this.logger = logger;
        }

        public static void RegisterJob(IConfiguration config)
        {
            RecurringJob.AddOrUpdate<SendAlert>(
                x => x.Run(),
                Cron.Daily(),
                TimeZoneInfo.FindSystemTimeZoneById(config["systemLocalTimezone"]));
        }

        public async Task Run()
        {
            var lastWeekUTC = DateTime.UtcNow.AddDays(-7);
            var alerts = await db.Alerts
                .Include(x => x.User)
                .Include(x => x.Address)
                .Where(x => x.LastSendDateUTC == null || x.LastSendDateUTC < lastWeekUTC.Date)
                .Where(x => x.EmailConfirmed || x.UserId != null)
                .ToArrayAsync();
            var recentAds = await db.Ads
                .Include(x => x.Address)
                .Where(x => x.CreatedAtUTC >= lastWeekUTC.Date)
                .ToArrayAsync();
            await db.SaveChangesAsync();

            foreach (var alert in alerts)
            {
                var filteredAds = recentAds
                    .Where(x => x.Category == alert.Category)
                    .Where(x => alert.DayAvailability == false || x.DayAvailability.Any())
                    .Where(x => alert.EveningAvailability == false || x.EveningAvailability.Any())
                    .Where(x => CoordinateHelper.GetDistanceInMeters(x.Address.Latitude, x.Address.Longitude, alert.Address.Latitude, alert.Address.Longitude)/1000 <= alert.Radius);

                switch (alert.Category)
                {
                    case AdCategory.ProfessionalKitchen:
                        filteredAds = filteredAds.Where(x => !alert.ProfessionalKitchenEquipments?.Any() ?? true || x.ProfessionalKitchenEquipments.Select(x => x.ProfessionalKitchenEquipment).Intersect(alert.ProfessionalKitchenEquipments.Select(x => x.ProfessionalKitchenEquipment)).Count() == alert.ProfessionalKitchenEquipments.Count);
                        break;
                    case AdCategory.DeliveryTruck:
                        filteredAds = filteredAds.Where(x => alert.DeliveryTruckType == null || x.DeliveryTruckType == alert.DeliveryTruckType)
                                               .Where(x => !alert.Refrigerated || alert.Refrigerated)
                                               .Where(x => !alert.CanHaveDriver || alert.CanHaveDriver)
                                               .Where(x => !alert.CanSharedRoad || alert.CanSharedRoad);
                        break;
                }

                if (filteredAds.Any())
                {
                    var filtersParams = new UrlHelper.AdsFilteredParams {
                        Category = alert.Category,
                        DeliveryTruckType = alert.DeliveryTruckType,
                        ProfessionalKitchenEquipment = alert.ProfessionalKitchenEquipments?.Select(x => x.ProfessionalKitchenEquipment).ToList(),
                        Refrigerated = alert.Refrigerated,
                        CanHaveDriver = alert.CanHaveDriver,
                        CanSharedRoad = alert.CanSharedRoad,
                        DayAvailability = alert.DayAvailability,
                        EveningAvailability = alert.EveningAvailability,
                        Address = alert.Address.FormatedAddress,
                        Latitude = alert.Address.Latitude,
                        Longitude = alert.Address.Longitude,
                        View = UrlHelper.AdsFilteredParams.ViewMode.LIST,
                        Sort = UrlHelper.AdsFilteredParams.SortType.DATE,
                        Direction = UrlHelper.AdsFilteredParams.DirectionValue.DESC
                    };

                    
                    var emailModel = new AlertEmail(alert.User?.Email ?? alert.Email)
                    {
                        Category = AdCategoryHelper.AdCategoryToFrenchString(alert.Category),
                        AdsCount = filteredAds.Count(),
                        CtaUrl = filteredAds.Count() == 1 ? UrlHelper.Ad(filteredAds.First().GetIdentifier()) : UrlHelper.AdsFiltered(filtersParams),
                        UnsubscribeUrl = UrlHelper.UnsubscribeAlert(alert.GetIdentifier()),
                        ManageAlertUrl = (alert.User != null) ? UrlHelper.ManageAlert() : null
                    };
                    await mailer.Send(emailModel);
                    logger.LogInformation($"Alert id {alert.Id} sent with {filteredAds.Count()} ad(s)");
                    alert.LastSendDateUTC = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            } 
        }
    }
}
