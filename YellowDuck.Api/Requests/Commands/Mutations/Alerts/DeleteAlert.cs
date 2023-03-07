using GraphQL.Conventions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YellowDuck.Api.DbModel;
using YellowDuck.Api.DbModel.Entities.Alerts;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Gql.Interfaces;
using YellowDuck.Api.Plugins.GraphQL;
using YellowDuck.Api.Plugins.MediatR;
using YellowDuck.Api.Services.System;

namespace YellowDuck.Api.Requests.Commands.Mutations.Alerts
{
    public class DeleteAlert : IRequestHandler<DeleteAlert.Input, DeleteAlert.Payload>
    {
        private readonly AppDbContext db;

        public DeleteAlert(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<Payload> Handle(Input request, CancellationToken cancellationToken)
        {
            if (request.Email == null) throw new EmailNotSetException();
            var alertId = request.AlertId.LongIdentifierForType<Alert>();

            var alert = await db.Alerts.Include(x => x.User).Where(x => x.Id == alertId).Where(x => (x.User != null && x.User.Email == request.Email) || x.Email == request.Email).FirstOrDefaultAsync(cancellationToken);

            if (alert == null) throw new AlertNotFoundException();

            db.Alerts.Remove(alert);
            await db.SaveChangesAsync(cancellationToken);

            return new Payload()
            {
                Success = true
            };
        }

        [MutationInput]
        public class Input : IHaveAlertId, IRequest<Payload>
        {
            public Id AlertId { get; set; }
            public string Email { get; set; }
        }

        [MutationPayload]
        public class Payload
        {
            public bool Success { get; set; }
        }

        public abstract class DeleteAlertException : RequestValidationException { }
        public class EmailNotSetException : DeleteAlertException { }
        public class AlertNotFoundException : DeleteAlertException { }
    }
}
