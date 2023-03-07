using YellowDuck.Api.Extensions;
using YellowDuck.Api.Plugins.GraphQL;
using YellowDuck.Api.Plugins.MediatR;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using YellowDuck.Api.DbModel;
using System.Linq;
using GraphQL.Conventions;
using YellowDuck.Api.DbModel.Entities.Alerts;
using Microsoft.EntityFrameworkCore;
using YellowDuck.Api.Gql.Schema.GraphTypes;

namespace YellowDuck.Api.Requests.Commands.Mutations.Alerts
{
    public class ConfirmAlert : IRequestHandler<ConfirmAlert.Input, ConfirmAlert.Payload>
    {
        private readonly AppDbContext db;
        private readonly ILogger<ConfirmAlert> logger;

        public ConfirmAlert(AppDbContext db, ILogger<ConfirmAlert> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task<Payload> Handle(Input request, CancellationToken cancellationToken)
        {
            var alertId = request.AlertId.LongIdentifierForType<Alert>();
            var alert = await db.Alerts.Where(x => x.Id == alertId && x.Email == request.Email).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (alert == null || alert.EmailConfirmed) throw new NoNeedToConfirmException();

            alert.EmailConfirmed = true;
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Alert Id {alert.Id} email address confirmed {alert.Email}");

            return new Payload
            {
                Alert = new AlertGraphType(alert)
            };
        }

        [MutationInput]
        public class Input : IRequest<Payload>
        {
            public Id AlertId { get; set; }
            public string Email { get; set; }

        }

        [MutationPayload]
        public class Payload
        {
            public AlertGraphType Alert { get; set; }
        }

        public abstract class ConfirmEmailException : RequestValidationException { }
        public class NoNeedToConfirmException : ConfirmEmailException { }
    }
}
