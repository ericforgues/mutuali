﻿using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Plugins.GraphQL;
using YellowDuck.Api.Plugins.MediatR;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace YellowDuck.Api.Requests.Commands.Mutations.Accounts
{
    public class ConfirmEmail : AsyncRequestHandler<ConfirmEmail.Input>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<ConfirmEmail> logger;

        public ConfirmEmail(UserManager<AppUser> userManager, ILogger<ConfirmEmail> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        protected override async Task Handle(Input request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null || user.EmailConfirmed) throw new NoNeedToConfirmException();

            var result = await userManager.ConfirmEmailAsync(user, request.Token);
            result.AssertSuccess();

            logger.LogInformation($"Email address confirmed {user.Email}");
        }

        [MutationInput]
        public class Input : IRequest
        {
            public string Email { get; set; }
            public string Token { get; set; }
        }

        public abstract class ConfirmEmailException : RequestValidationException { }
        public class NoNeedToConfirmException : ConfirmEmailException { }
    }
}
