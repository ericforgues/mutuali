using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YellowDuck.Api.Constants;
using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.DbModel.Entities.Alerts;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.EmailTemplates.Models;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Gql.Schema.Types;
using YellowDuck.Api.Requests.Commands.Mutations.Alerts;
using YellowDuck.Api.Services.Mailer;

namespace YellowDuck.ApiTests.Requests.Commands.Mutations.Alerts
{
    public class CreateAlertTests : TestBase
    {
        private readonly CreateAlert handler;
        private readonly AppUser user;
        private readonly Mock<IMailer> mailer;

        public CreateAlertTests()
        {
            mailer = new Mock<IMailer>();
            handler = new CreateAlert(DbContext, UserManager, NullLogger<CreateAlert>.Instance, UserAccessor, mailer.Object);
            user = AddUser("test@example.com", UserType.User);
    }

        [Fact]
        public async Task ShouldAddTheAlertToDb()
        {
            SetLoggedInUser(user);
            var input = new CreateAlert.Input
            {
                Category = AdCategory.ProfessionalKitchen,
                DayAvailability = true,
                ProfessionalKitchenEquipment = new Maybe<List<ProfessionalKitchenEquipment>>()
                {
                    Value = new List<ProfessionalKitchenEquipment>()
                    {
                        ProfessionalKitchenEquipment.WORKPLAN,
                        ProfessionalKitchenEquipment.OTHER
                    }
                },
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Latitude = 46.8214225354811,
                    Longitude = -71.23779832297491,
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull(),
                Radius = 25
            };

            await handler.Handle(input, CancellationToken.None);

            // Creating a new Db Context to verify that SaveChanges was called
            using var db = CreateDbContext();
            var alert = await db.Alerts
                .Include(x => x.ProfessionalKitchenEquipments)
                .FirstAsync();

            alert.Category.Should().Be(AdCategory.ProfessionalKitchen);
            alert.ProfessionalKitchenEquipments.Should().HaveCount(2);
            alert.DayAvailability.Should().Be(true);
        }

        [Fact]
        public async Task ShouldAddTheAnonymeAlertToDb()
        {
            var email = "test@example.com";
            var input = new CreateAlert.Input
            {
                Email = email,
                Category = AdCategory.ProfessionalKitchen,
                DayAvailability = true,
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Latitude = 46.8214225354811,
                    Longitude = -71.23779832297491,
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull(),
                Radius = 25
            };

            await handler.Handle(input, CancellationToken.None);

            // Creating a new Db Context to verify that SaveChanges was called
            using var db = CreateDbContext();
            var alert = await db.Alerts.FirstAsync();

            mailer.Verify(x => x.Send(It.Is<ConfirmAlertEmail>(x => x.To.Contains(email))));

            alert.Category.Should().Be(AdCategory.ProfessionalKitchen);
            alert.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task ShouldReturnTheCreatedAlert()
        {
            SetLoggedInUser(user);
            var input = new CreateAlert.Input
            {
                Category = AdCategory.DeliveryTruck,
                DeliveryTruckType = DeliveryTruckType.Van,
                Radius = 25,
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Latitude = 46.8214225354811,
                    Longitude = -71.23779832297491,
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull(),
            };

            var result = await handler.Handle(input, CancellationToken.None);

            var category = await result.Alert.Category;
            category.Should().Be(AdCategory.DeliveryTruck);
        }

        [Fact]
        public async Task ShouldReturnIdOfCreatedAlert()
        {
            SetLoggedInUser(user);
            var input = new CreateAlert.Input
            {
                Category = AdCategory.DeliveryTruck,
                DeliveryTruckType = DeliveryTruckType.Van,
                Radius= 25,
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Latitude = 46.8214225354811,
                    Longitude = -71.23779832297491,
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull()
            };

            var result = await handler.Handle(input, CancellationToken.None);

            var alertId = result.Alert.Id.LongIdentifierForType<Alert>();
            var alert = await DbContext.Alerts.FindAsync(alertId);

            alert.Category.Should().Be(AdCategory.DeliveryTruck);
        }

        [Fact]
        public async Task ValidateClaims()
        {
            SetLoggedInUser(user);
            var input = new CreateAlert.Input
            {
                Category = AdCategory.DeliveryTruck,
                DeliveryTruckType = DeliveryTruckType.Van,
                Radius = 25,
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Latitude = 46.8214225354811,
                    Longitude = -71.23779832297491,
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull(),
             };

            var result = await handler.Handle(input, CancellationToken.None);

            var claim = new Claim(AppClaimTypes.AlertOwner, result.Alert.Id.ToString());
            var assignedUsers = await UserManager.GetUsersForClaimAsync(claim);
            assignedUsers.Should().HaveCount(1)
                .And.Contain(x => x.Email == "test@example.com");
        }

        [Fact]
        public async Task ValidateMissingLattitudeLongitude()
        {
            SetLoggedInUser(user);
            var input = new CreateAlert.Input
            {
                Category = AdCategory.DeliveryTruck,
                Radius = 25,
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull()
            };

            await F(() => handler.Handle(input, CancellationToken.None))
                .Should().ThrowAsync<CreateAlert.AddressInvalidException>();
        }

        [Fact]
        public async Task ValidateMissingEmail()
        {
            var input = new CreateAlert.Input
            {
                Category = AdCategory.DeliveryTruck,
                Radius = 25,
                Address = new CreateAlert.AddressInput()
                {
                    StreetNumber = "123".NonNull(),
                    Route = "Example route".NonNull(),
                    Latitude = 46.8214225354811,
                    Longitude = -71.23779832297491,
                    Locality = "Québec".NonNull(),
                    PostalCode = "G1K-0H1".NonNull(),
                    Raw = "{raw object}"
                }.NonNull(),
            };

            await F(() => handler.Handle(input, CancellationToken.None))
                .Should().ThrowAsync<CreateAlert.EmailRequiredException>();
        }
    }
}
