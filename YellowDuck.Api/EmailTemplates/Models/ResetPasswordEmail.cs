﻿using YellowDuck.Api.Services.Mailer;

namespace YellowDuck.Api.EmailTemplates.Models
{
    public class ResetPasswordEmail : EmailModel
    {
        public override string Subject => "Demande de réinitialisation de votre mot de passe / Request to reset your password";

        public string UserName { get; set; }
        public string Token { get; set; }

        public ResetPasswordEmail(string to) : base(to)
        {
        }
    }
}
