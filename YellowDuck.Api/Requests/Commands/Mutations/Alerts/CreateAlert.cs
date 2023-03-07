using GraphQL.Conventions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using YellowDuck.Api.Constants;
using YellowDuck.Api.DbModel;
using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.DbModel.Entities.Ads;
using YellowDuck.Api.DbModel.Entities.Alerts;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.EmailTemplates.Models;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Gql.Schema.GraphTypes;
using YellowDuck.Api.Gql.Schema.Types;
using YellowDuck.Api.Plugins.GraphQL;
using YellowDuck.Api.Plugins.MediatR;
using YellowDuck.Api.Services.Mailer;
using YellowDuck.Api.Services.System;

namespace YellowDuck.Api.Requests.Commands.Mutations.Alerts
{
    public class CreateAlert : IRequestHandler<CreateAlert.Input, CreateAlert.Payload>
    {
        private readonly AppDbContext db;
        private readonly ILogger<CreateAlert> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly ICurrentUserAccessor currentUserAccessor;
        private readonly IMailer mailer;

        public CreateAlert(AppDbContext db, UserManager<AppUser> userManager, ILogger<CreateAlert> logger, ICurrentUserAccessor currentUserAccessor, IMailer mailer)
        {
            this.db = db;
            this.logger = logger;
            this.userManager = userManager;
            this.currentUserAccessor = currentUserAccessor;
            this.mailer = mailer;
        }

        public async Task<Payload> Handle(Input request, CancellationToken cancellationToken)
        {
            var owner = await currentUserAccessor.GetCurrentUser();
            ValidateRequest(request, owner);
            var address = request.Address.Value;

            var alert = new Alert
            {
                Category = request.Category,
                Address = new AlertAddress()
                {
                    Raw = address.Raw,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude
                },
                Radius = request.Radius
            };

            request.Address.Value.Locality.IfSet(v => alert.Address.Locality = v);
            request.Address.Value.PostalCode.IfSet(v => alert.Address.PostalCode = v);
            request.Address.Value.Route.IfSet(v => alert.Address.Route = v);
            request.Address.Value.StreetNumber.IfSet(v => alert.Address.StreetNumber = v);
            request.Address.Value.Neighborhood.IfSet(v => alert.Address.Neighborhood = v);
            request.Address.Value.Sublocality.IfSet(v => alert.Address.Sublocality = v);
            request.DeliveryTruckType.IfSet(v => alert.DeliveryTruckType = v);
            request.Refrigerated.IfSet(v => alert.Refrigerated = v);
            request.CanSharedRoad.IfSet(v => alert.CanSharedRoad = v);
            request.CanHaveDriver.IfSet(v => alert.CanHaveDriver = v);
            request.DayAvailability.IfSet(v => alert.DayAvailability = v);
            request.EveningAvailability.IfSet(v => alert.EveningAvailability = v);
            request.Email.IfSet(v => alert.Email = v.Trim());

            request.ProfessionalKitchenEquipment.IfSet(v =>
            {
                alert.ProfessionalKitchenEquipments = new List<AlertProfessionalKitchenEquipment>();
                v.ForEach(x => alert.ProfessionalKitchenEquipments.Add(new AlertProfessionalKitchenEquipment() { ProfessionalKitchenEquipment = x }));
            });

            if (owner != null) alert.UserId = owner.Id;

            db.Alerts.Add(alert);

            await db.SaveChangesAsync(cancellationToken);
            if (owner != null)
            {
                await userManager.AddClaimAsync(owner, new Claim(AppClaimTypes.AlertOwner, Id.New<Alert>(alert.Id.ToString()).ToString()));
            }
            else
            {
                await mailer.Send(new ConfirmAlertEmail(alert.Email, alert));
            }

            logger.LogInformation($"New alert created {request.Category} ({alert.Id})");

            return new Payload
            {
                Alert = new AlertGraphType(alert)
            };
        }

        private static void ValidateRequest(Input request, AppUser owner)
        {
            if (request.Address.Value.Latitude == 0.0d || request.Address.Value.Longitude == 0.0d)
            {
                throw new AddressInvalidException();
            }

            if (owner == null && !request.Email.IsSet())
            {
                throw new EmailRequiredException();
            }
        }

        [MutationInput]
        public class Input : IRequest<Payload>
        {
            public AdCategory Category { get; set; }
            public Maybe<bool> DayAvailability { get; set; }
            public Maybe<bool> EveningAvailability { get; set; }
            public Maybe<List<ProfessionalKitchenEquipment>> ProfessionalKitchenEquipment { get; set; }
            public Maybe<DeliveryTruckType> DeliveryTruckType { get; set; }
            public Maybe<bool> Refrigerated { get; set; }
            public Maybe<bool> CanSharedRoad { get; set; }
            public Maybe<bool> CanHaveDriver { get; set; }
            public double? Radius { get; set; }
            public NonNull<AddressInput> Address { get; set; }
            public Maybe<string> Email { get; set; }
        }

        [MutationPayload]
        public class Payload
        {
            public AlertGraphType Alert { get; set; }
        }

        [InputType]
        public class AddressInput
        {
            public Maybe<NonNull<string>> StreetNumber { get; set; }
            public Maybe<NonNull<string>> Route { get; set; }
            public Maybe<NonNull<string>> Locality { get; set; }
            public NonNull<string> Raw { get; set; }
            public Maybe<NonNull<string>> PostalCode { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public Maybe<NonNull<string>> Neighborhood { get; set; }
            public Maybe<NonNull<string>> Sublocality { get; set; }
        }

        public abstract class CreateAlertException : RequestValidationException { }

        public class AddressInvalidException : CreateAlertException { }
        public class EmailRequiredException : CreateAlertException { }

    }
}
