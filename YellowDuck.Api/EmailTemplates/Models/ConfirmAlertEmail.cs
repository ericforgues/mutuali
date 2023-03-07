using YellowDuck.Api.DbModel.Entities.Alerts;
using YellowDuck.Api.Services.Mailer;

namespace YellowDuck.Api.EmailTemplates.Models
{
    public class ConfirmAlertEmail : EmailModel
    {
        public override string Subject => "Confirmer l'alerte Mutuali";

        public Alert Alert { get; set; }

        public ConfirmAlertEmail(string to, Alert alert) : base(to)
        {
            Alert = alert;
        }
    }
}
