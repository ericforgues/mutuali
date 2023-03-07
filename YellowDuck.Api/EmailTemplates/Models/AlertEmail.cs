using YellowDuck.Api.Services.Mailer;

namespace YellowDuck.Api.EmailTemplates.Models
{
    public class AlertEmail : EmailModel
    {
        public override string Subject => $"Alerte Mutuali: {AdsCount} offre(s) pourrai(en)t vous intéresser.";
        public string Category { get; set; }
        public int AdsCount { get; set; }
        public string CtaUrl { get; set; }
        public string UnsubscribeUrl { get; set; }
        public string ManageAlertUrl { get; set; }

        public AlertEmail(string to) : base(to) { }
    }
}
