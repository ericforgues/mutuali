using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YellowDuck.Api.Constants;
using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.DbModel.Entities.Alerts;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Requests.Commands.Mutations.Alerts;
using YellowDuck.Api.Services.Files;

namespace YellowDuck.ApiTests.Requests.Commands.Mutations.Alerts
{
    public class UpdateAlertTests : TestBase
    {
        private readonly UpdateAlert handler;
        private readonly AppUser user;
        private readonly Alert alert;

        public UpdateAlertTests()
        {
            handler = new UpdateAlert(DbContext, NullLogger<UpdateAlert>.Instance);
            user = AddUser("test@example.com", UserType.User);
            SetLoggedInUser(user);

            alert = new Alert()
            {
                Category = AdCategory.StorageSpace,
                DayAvailability = true,
                Address = new AlertAddress()
                {
                    Latitude = 46.8214098,
                    Longitude = -71.237595,
                    Locality = "Québec",
                    Neighborhood = "Saint-Roch",
                    Sublocality = "La Cité-Limoilou",
                    PostalCode = "G1K0H1",
                    Raw = "{\"street_number\":\"395\",\"route\":\"Rue Bickell\",\"locality\":\"Québec\",\"administrative_area_level_2\":\"Communauté - Urbaine - de - Québec\",\"administrative_area_level_1\":\"QC\",\"country\":\"Canada\",\"latitude\":46.8214098,\"longitude\":-71.237595,\"neighborhood\":\"Saint - Roch\",\"sublocality\":\"La Cité - Limoilou\"}",
                    Route = "Rue Bickell",
                    StreetNumber = "395"
                },
                Radius = 10
            };

            DbContext.Alerts.Add(alert);
            DbContext.SaveChanges();
        }

        private UpdateAlert.Input GetValidInput()
        {
            var input = new UpdateAlert.Input
            {
                AlertId = alert.GetIdentifier(),
                Category = AdCategory.ProfessionalKitchen,
                DayAvailability = false,
                Address = new UpdateAlert.AddressInput()
                {
                    Latitude = 45.5576387,
                    Longitude = -73.5515693,
                    Locality = "Montréal".NonNull(),
                    Neighborhood = "".NonNull(),
                    PostalCode = "H2G 2B3".NonNull(),
                    Raw = "{\"street_number\":\"5650\",\"route\":\"Rue D'Iberville\",\"locality\":\"Montréal\",\"administrative_area_level_2\":\"Communauté-Urbaine-de-Montréal\",\"administrative_area_level_1\":\"QC\",\"country\":\"Canada\",\"postal_code\":\"H2G 2B3\",\"latitude\":45.544332,\"longitude\":-73.58167399999999}".NonNull(),
                    Route = "Rue D'Iberville".NonNull(),
                    StreetNumber = "5650".NonNull(),
                    Sublocality = "Rosemont-La Petite-Patrie".NonNull()
                }.NonNull(),
                ProfessionalKitchenEquipment = new List<ProfessionalKitchenEquipment>()
                {
                    ProfessionalKitchenEquipment.WORKPLAN,
                    ProfessionalKitchenEquipment.OVEN
                },
                Refrigerated = true,
                CanHaveDriver = true,
                CanSharedRoad = false
            };
            return input;
        }

        [Fact]
        public async Task ShouldUpdateFields()
        {
            var input = GetValidInput();
            await handler.Handle(input, CancellationToken.None);

            using var db = CreateDbContext();
            var alert = await db.Alerts
                .Include(x => x.Address)
                .Include(x => x.ProfessionalKitchenEquipments)
                .FirstAsync();

            alert.Category.Should().Be(AdCategory.ProfessionalKitchen);
            alert.Address.Latitude.Should().Be(45.5576387);
            alert.Address.Longitude.Should().Be(-73.5515693);
            alert.Address.Locality.Should().Be("Montréal");
            alert.Address.Neighborhood.Should().Be("");
            alert.Address.PostalCode.Should().Be("H2G 2B3");
            alert.Address.Raw.Should().Be("{\"street_number\":\"5650\",\"route\":\"Rue D'Iberville\",\"locality\":\"Montréal\",\"administrative_area_level_2\":\"Communauté-Urbaine-de-Montréal\",\"administrative_area_level_1\":\"QC\",\"country\":\"Canada\",\"postal_code\":\"H2G 2B3\",\"latitude\":45.544332,\"longitude\":-73.58167399999999}");
            alert.Address.Route.Should().Be("Rue D'Iberville");
            alert.Address.StreetNumber.Should().Be("5650");
            alert.Address.Sublocality.Should().Be("Rosemont-La Petite-Patrie");
            alert.ProfessionalKitchenEquipments.Should().Contain(x => x.ProfessionalKitchenEquipment == ProfessionalKitchenEquipment.WORKPLAN);
            alert.ProfessionalKitchenEquipments.Should().Contain(x => x.ProfessionalKitchenEquipment == ProfessionalKitchenEquipment.OVEN);
            alert.DayAvailability.Should().Be(false);
            alert.Refrigerated.Should().Be(true);
            alert.CanHaveDriver.Should().Be(true);
            alert.CanSharedRoad.Should().Be(false);
        }

        [Fact]
        public async Task ShouldReturnTheUpdatedAlert()
        {
            var input = GetValidInput();
            var result = await handler.Handle(input, CancellationToken.None);

            result.Alert.Should().NotBeNull();

            var category = await result.Alert.Category;
            category.Should().Be(AdCategory.ProfessionalKitchen);

            var id = result.Alert.Id.LongIdentifierForType<Alert>();
            id.Should().Be(alert.Id);
        }

        [Fact]
        public async Task OnlyUpdatesSpecifiedFields()
        {
            var input = new UpdateAlert.Input
            {
                AlertId = this.alert.GetIdentifier(),
                Category = AdCategory.StorageSpace
            };
            
            await handler.Handle(input, CancellationToken.None);

            using var db = CreateDbContext();
            var alert = await db.Alerts.Include(x => x.Address).FirstAsync();
            alert.Category.Should().Be(AdCategory.StorageSpace);
            alert.Address.Latitude.Should().Be(46.8214098);
            alert.Address.Longitude.Should().Be(-71.237595);
            alert.Address.Locality.Should().Be("Québec");
            alert.Address.Neighborhood.Should().Be("Saint-Roch");
            alert.Address.Sublocality.Should().Be("La Cité-Limoilou");
            alert.Address.PostalCode.Should().Be("G1K0H1");
            alert.Address.Raw.Should().Be("{\"street_number\":\"395\",\"route\":\"Rue Bickell\",\"locality\":\"Québec\",\"administrative_area_level_2\":\"Communauté - Urbaine - de - Québec\",\"administrative_area_level_1\":\"QC\",\"country\":\"Canada\",\"latitude\":46.8214098,\"longitude\":-71.237595,\"neighborhood\":\"Saint - Roch\",\"sublocality\":\"La Cité - Limoilou\"}");
            alert.Address.Route.Should().Be("Rue Bickell");
            alert.Address.StreetNumber.Should().Be("395");
        }

        [Fact]
        public async Task ShouldResetAddressOnAddressChange()
        {
            var input = new UpdateAlert.Input
            {
                AlertId = this.alert.GetIdentifier(),
                Category = AdCategory.StorageSpace,
                Address = new UpdateAlert.AddressInput()
                {
                    Latitude = 45.5576387,
                    Longitude = -73.5515693,
                    Raw = "{\"street_number\":\"5650\",\"route\":\"Rue D'Iberville\",\"locality\":\"Montréal\",\"administrative_area_level_2\":\"Communauté-Urbaine-de-Montréal\",\"administrative_area_level_1\":\"QC\",\"country\":\"Canada\",\"postal_code\":\"H2G 2B3\",\"latitude\":45.544332,\"longitude\":-73.58167399999999}".NonNull(),
                }.NonNull()
            };

            await handler.Handle(input, CancellationToken.None);

            using var db = CreateDbContext();
            var alert = await db.Alerts.Include(x => x.Address).FirstAsync();
            alert.Address.Latitude.Should().Be(45.5576387);
            alert.Address.Longitude.Should().Be(-73.5515693);
            alert.Address.Raw.Should().Be("{\"street_number\":\"5650\",\"route\":\"Rue D'Iberville\",\"locality\":\"Montréal\",\"administrative_area_level_2\":\"Communauté-Urbaine-de-Montréal\",\"administrative_area_level_1\":\"QC\",\"country\":\"Canada\",\"postal_code\":\"H2G 2B3\",\"latitude\":45.544332,\"longitude\":-73.58167399999999}");

            alert.Address.Locality.Should().Be("");
            alert.Address.Neighborhood.Should().Be("");
            alert.Address.Sublocality.Should().Be("");
            alert.Address.PostalCode.Should().Be("");
            alert.Address.Route.Should().Be("");
            alert.Address.StreetNumber.Should().Be("");
        }

        [Fact]
        public async Task ShouldThrowErrorOnInvalidId()
        {
            var input = new UpdateAlert.Input
            {
                AlertId = "QWxlcnQ6MjAwMDc="
            };

            await F(() => handler.Handle(input, CancellationToken.None))
                .Should().ThrowAsync<UpdateAlert.AlertNotFoundException>();
        }
    }
}
