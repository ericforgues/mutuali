using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.DbModel.Entities.Alerts;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Requests.Commands.Mutations.Alerts;

namespace YellowDuck.ApiTests.Requests.Commands.Mutations.Alerts
{
    public class DeleteAlertTests : TestBase
    {
        private readonly DeleteAlert handler;
        private readonly AppUser user;
        private readonly Alert alert;

        public DeleteAlertTests()
        {
            handler = new DeleteAlert(DbContext);
            user = AddUser("test@example.com", UserType.User);
            SetLoggedInUser(user);

            alert = new Alert()
            {
                Category = AdCategory.DeliveryTruck,
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
                }.NonNull(),
                User = user,
                Radius = 10
            };

            DbContext.Alerts.Add(alert);
            DbContext.SaveChanges();
        }

        private DeleteAlert.Input GetValidInput()
        {
            return new DeleteAlert.Input
            {
                AlertId = alert.GetIdentifier(),
                Email = "test@example.com"
            };
        }

        [Fact]
        public async Task ShouldDeleteAlert()
        {
            var input = GetValidInput();
            await handler.Handle(input, CancellationToken.None);

            using var db = CreateDbContext();
            var alert = await db.Alerts.FirstOrDefaultAsync();

            alert.Should().BeNull();
        }

        [Fact]
        public async Task ShouldThrowErrorOnInvalidId()
        {
            var input = new DeleteAlert.Input
            {
                AlertId = "QWxlcnQ6MjAwMDc=",
                Email = "test@example.com"
            };

            await F(() => handler.Handle(input, CancellationToken.None))
                .Should().ThrowAsync<DeleteAlert.AlertNotFoundException>();
        }

        [Fact]
        public async Task ShouldThrowErrorOnUnSpecifiedEmail()
        {
            var input = new DeleteAlert.Input
            {
                AlertId = "QWxlcnQ6MjAwMDc=",
            };

            await F(() => handler.Handle(input, CancellationToken.None))
                .Should().ThrowAsync<DeleteAlert.EmailNotSetException>();
        }
    }
}
