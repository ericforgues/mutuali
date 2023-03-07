using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.DbModel.Entities.Profiles;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.EmailTemplates.Models;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Gql.Schema.GraphTypes;
using YellowDuck.Api.Plugins.GraphQL;
using YellowDuck.Api.Services.Mailer;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using GraphQL.Conventions;
using YellowDuck.Api.Gql.Schema.Types;
using System.Collections.Generic;
using System.Linq;
using YellowDuck.Api.DbModel;
using Microsoft.EntityFrameworkCore;

namespace YellowDuck.Api.Requests.Commands.Mutations.Accounts
{
    public class CreateUserAccount : IRequestHandler<CreateUserAccount.Input, CreateUserAccount.Payload>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly AppDbContext db;
        private readonly IMailer mailer;
        private readonly ILogger<CreateUserAccount> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CreateUserAccount(UserManager<AppUser> userManager, AppDbContext db, IMailer mailer, ILogger<CreateUserAccount> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.db = db;
            this.mailer = mailer;
            this.logger = logger;
        }

        public async Task<Payload> Handle(Input request, CancellationToken cancellationToken)
        {
            var ipAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            var user = new AppUser(request.Email?.Trim())
            {
                Type = UserType.User,
                Profile = new UserProfile()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    OrganizationName = request.OrganizationName,
                    OrganizationType = request.OrganizationType,
                    Industry = request.Industry,
                    PostalCode = request.PostalCode,
                    PhoneNumber = request.PhoneNumber,
                    ShowPhoneNumber = request.ShowPhoneNumber,
                    ShowEmail = request.ShowEmail,
                    ContactAuthorizationNews = request.ContactAuthorizationNews,
                    ContactAuthorizationSurveys = request.ContactAuthorizationSurveys
                },
                AcceptedTos = TosVersion.Latest,
                TosAcceptationDate = DateTime.Now,
                TosAcceptationIpAddress = ipAddress,
                CreatedAtUtc = DateTime.UtcNow
            };

            request.OrganizationTypeOtherSpecification.IfSet(v => user.Profile.OrganizationTypeOtherSpecification = v);
            request.IndustryOtherSpecification.IfSet(v => user.Profile.IndustryOtherSpecification = v);

            request.RegisteringInterests.IfSet(v =>
            {
                user.Profile.RegisteringInterests = new List<UserProfileRegisteringInterest>();
                v.ForEach(x => user.Profile.RegisteringInterests.Add(new UserProfileRegisteringInterest() { RegisteringInterest = x }));
            });

            var result = await userManager.CreateAsync(user, request.Password);
            result.AssertSuccess();
            var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var alerts = await db.Alerts.Where(x => x.Email == request.Email && x.EmailConfirmed).ToListAsync(cancellationToken: cancellationToken);
            alerts.ForEach(x => x.UserId = user.Id);
            await db.SaveChangesAsync(cancellationToken);

            await mailer.Send(new ConfirmEmailEmail(request.Email, confirmToken, request.ReturnPath));

            logger.LogInformation($"User account created ({user.Email}).");

            return new Payload
            {
                User = new UserGraphType(user)
            };
        }

        [MutationInput]
        public class Input : IRequest<Payload>
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string OrganizationName { get; set; }
            public OrganizationType OrganizationType { get; set; }
            public Maybe<NonNull<string>> OrganizationTypeOtherSpecification { get; set; }
            public Industry Industry { get; set; }
            public Maybe<NonNull<string>> IndustryOtherSpecification { get; set; }
            public string PostalCode { get; set; }
            public string PhoneNumber { get; set; }
            public bool ShowPhoneNumber { get; set; }
            public bool ShowEmail { get; set; }
            public bool ContactAuthorizationNews { get; set; }
            public bool ContactAuthorizationSurveys { get; set; }
            public Maybe<List<RegisteringInterest>> RegisteringInterests { get; set; }

            public string ReturnPath { get; set; }
        }

        [MutationPayload]
        public class Payload
        {
            public UserGraphType User { get; set; }
        }
    }
}
