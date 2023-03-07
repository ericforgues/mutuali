using GraphQL.Conventions;
using System.Collections.Generic;
using System.Threading.Tasks;
using YellowDuck.Api.Gql.Interfaces;
using System.Linq;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.DbModel.Entities.Alerts;

namespace YellowDuck.Api.Gql.Schema.GraphTypes
{
    public class AlertGraphType : LazyGraphType<Alert>
    {
        private readonly long id;

        public AlertGraphType(IAppUserContext ctx, long alertId) : base(() => ctx.LoadAlert(alertId))
        {
            id = alertId;
        }

        public AlertGraphType(Alert alert) : base(alert)
        {
            id = alert.Id;
        }

        public Id Id => Id.New<Alert>(id);

        public async Task<UserGraphType> User(IAppUserContext ctx)
        {
            var data = await Data;

            return data.User != null  
                ? new UserGraphType(data.User)
                : new UserGraphType(ctx, data.UserId);
        }

        public Task<AdCategory> Category => WithData(x => x.Category);

        public Task<bool> DayAvailability => WithData(x => x.DayAvailability);

        public Task<bool> EveningAvailability => WithData(x => x.EveningAvailability);

        public async Task<IEnumerable<ProfessionalKitchenEquipment>> ProfessionalKitchenEquipment(IAppUserContext ctx)
        {
            var professionalKitchenEquipments = await ctx.LoadProfessionalKitchenEquipmentsByAlertId(id);
            return professionalKitchenEquipments.Select(x => x.ProfessionalKitchenEquipment).ToList();
        }
        public Task<DeliveryTruckType?> DeliveryTruckType => WithData(x => x.DeliveryTruckType);

        public Task<bool> Refrigerated => WithData(x => x.Refrigerated);

        public Task<bool> CanSharedRoad => WithData(x => x.CanSharedRoad);

        public Task<bool> CanHaveDriver => WithData(x => x.CanHaveDriver);

        public Task<double?> Radius => WithData(x => x.Radius);

        public async Task<AlertAddressGraphType> Address(IAppUserContext ctx)
        {
            var data = await Data;

            return data.Address != null
                ? new AlertAddressGraphType(data.Address)
                : new AlertAddressGraphType(ctx, data.AddressId);
        }

        public Task<string> Email => WithData(x => x.Email);
    }
}
