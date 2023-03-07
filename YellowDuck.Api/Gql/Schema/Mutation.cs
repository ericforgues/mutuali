using YellowDuck.Api.Authorization;
using YellowDuck.Api.Constants;
using YellowDuck.Api.Plugins.GraphQL;
using YellowDuck.Api.Requests.Commands.Mutations.Accounts;
using YellowDuck.Api.Requests.Commands.Mutations.Profiles;
using GraphQL.Conventions;
using MediatR;
using System.Threading.Tasks;
using YellowDuck.Api.Requests.Commands.Mutations.Ads;
using YellowDuck.Api.Requests.Commands.Mutations.Conversations;
using YellowDuck.Api.Requests.Commands.Mutations.Notifications;
using YellowDuck.Api.Requests.Commands.Mutations.Contracts;
using YellowDuck.Api.Requests.Commands.Mutations.Payment;
using YellowDuck.Api.Requests.Commands.Mutations.Alerts;

namespace YellowDuck.Api.Gql.Schema
{
    public class Mutation
    {
        [ApplyPolicy(AuthorizationPolicies.IsAdmin)]
        [AnnotateErrorCodes(typeof(CreateAdminAccount))]
        public Task<CreateAdminAccount.Payload> CreateAdminAccount(
            [Inject] IMediator mediator,
            NonNull<CreateAdminAccount.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [AnnotateErrorCodes(typeof(CompleteAdminRegistration))]
        public Task<CompleteAdminRegistration.Payload> CompleteAdminRegistration(
            [Inject] IMediator mediator,
            NonNull<CompleteAdminRegistration.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [AnnotateErrorCodes(typeof(CreateUserAccount))]
        public Task<CreateUserAccount.Payload> CreateUserAccount(
            [Inject] IMediator mediator,
            NonNull<CreateUserAccount.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.ManageUser)]
        [AnnotateErrorCodes(typeof(UpdateUserProfile))]
        public Task<UpdateUserProfile.Payload> UpdateUserProfile(
            [Inject] IMediator mediator,
            NonNull<UpdateUserProfile.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [AnnotateErrorCodes(typeof(ResendConfirmationEmail))]
        public async Task<bool> ResendConfirmationEmail(
            [Inject] IMediator mediator,
            NonNull<ResendConfirmationEmail.Input> input)
        {
            await mediator.Send(input.Value);
            return true;
        }

        [AnnotateErrorCodes(typeof(ConfirmEmail))]
        public async Task<bool> ConfirmEmail(
            [Inject] IMediator mediator,
            NonNull<ConfirmEmail.Input> input)
        {
            await mediator.Send(input.Value);
            return true;
        }

        [ApplyPolicy(AuthorizationPolicies.IsAdmin)]
        [AnnotateErrorCodes(typeof(VerifyEmail))]
        public async Task<bool> VerifyEmail(
            [Inject] IMediator mediator,
            NonNull<VerifyEmail.Input> input)
        {
            await mediator.Send(input.Value);
            return true;
        }

        [Description("Sends a password-reset email to the specified user, if it exists. Always returns `true`, even if the email was unknown.")]
        [AnnotateErrorCodes(typeof(SendPasswordReset))]
        public async Task<bool> SendPasswordReset(
            [Inject] IMediator mediator,
            NonNull<SendPasswordReset.Input> input)
        {
            await mediator.Send(input.Value);

            return true;
        }

        [AnnotateErrorCodes(typeof(ResetPassword))]
        public Task<ResetPassword.Payload> ResetPassword(
            [Inject] IMediator mediator,
            NonNull<ResetPassword.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.LoggedIn)]
        [AnnotateErrorCodes(typeof(ChangePassword))]
        public Task<ChangePassword.Payload> ChangePassword(
            [Inject] IMediator mediator,
            NonNull<ChangePassword.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.LoggedIn)]
        [AnnotateErrorCodes(typeof(UpdateFirstLoginModalClosed))]
        public Task<UpdateFirstLoginModalClosed.Payload> UpdateFirstLoginModalClosed(
            [Inject] IMediator mediator,
            NonNull<UpdateFirstLoginModalClosed.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(CreateAd))]
        public Task<CreateAd.Payload> CreateAd(
            [Inject] IMediator mediator,
            NonNull<CreateAd.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.ManageAd)]
        [AnnotateErrorCodes(typeof(UpdateAd))]
        public Task<UpdateAd.Payload> UpdateAd(
            [Inject] IMediator mediator,
            NonNull<UpdateAd.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.ManageAd)]
        [AnnotateErrorCodes(typeof(UpdateAdTranslation))]
        public Task<UpdateAdTranslation.Payload> UpdateAdTranslation(
            [Inject] IMediator mediator,
            NonNull<UpdateAdTranslation.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [AnnotateErrorCodes(typeof(CreateAlert))]
        public Task<CreateAlert.Payload> CreateAlert(
            [Inject] IMediator mediator,
            NonNull<CreateAlert.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [AnnotateErrorCodes(typeof(ConfirmAlert))]
        public Task<ConfirmAlert.Payload> ConfirmAlert(
            [Inject] IMediator mediator,
            NonNull<ConfirmAlert.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.ManageAlert)]
        [AnnotateErrorCodes(typeof(UpdateAlert))]
        public Task<UpdateAlert.Payload> UpdateAlert(
            [Inject] IMediator mediator,
            NonNull<UpdateAlert.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [AnnotateErrorCodes(typeof(DeleteAlert))]
        public Task<DeleteAlert.Payload> DeleteAlert(
            [Inject] IMediator mediator,
            NonNull<DeleteAlert.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(CreateConversation))]
        public Task<CreateConversation.Payload> CreateConversation(
            [Inject] IMediator mediator,
            NonNull<CreateConversation.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(RemoveConversationNotification))]
        public Task<RemoveConversationNotification.Payload> RemoveConversationNotification(
            [Inject] IMediator mediator,
            NonNull<RemoveConversationNotification.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(CreateContract))]
        public Task<CreateContract.Payload> CreateContract(
            [Inject] IMediator mediator,
            NonNull<CreateContract.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(UpdateContract))]
        public Task<UpdateContract.Payload> UpdateContract(
            [Inject] IMediator mediator,
            NonNull<UpdateContract.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(RateContract))]
        public Task<RateContract.Payload> RateContract(
            [Inject] IMediator mediator,
            NonNull<RateContract.Input> input)
        {
            return mediator.Send(input.Value);
        }
        
        [AnnotateErrorCodes(typeof(CreateStripeAccount))]
        public Task<CreateStripeAccount.Payload> CreateStripeAccount(
            [Inject] IMediator mediator,
            NonNull<CreateStripeAccount.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(CreateCheckoutSession))]
        public Task<CreateCheckoutSession.Payload> CreateCheckoutSession(
            [Inject] IMediator mediator,
            NonNull<CreateCheckoutSession.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.IsUser)]
        [AnnotateErrorCodes(typeof(CancelCheckoutSession))]
        public Task<CancelCheckoutSession.Payload> CancelCheckoutSession(
            [Inject] IMediator mediator,
            NonNull<CancelCheckoutSession.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.ManageAd)]
        [AnnotateErrorCodes(typeof(PublishAd))]
        public Task<PublishAd.Payload> PublishAd(
            [Inject] IMediator mediator,
            NonNull<PublishAd.Input> input)
        {
            return mediator.Send(input.Value);
        }

        [ApplyPolicy(AuthorizationPolicies.ManageAd)]
        [AnnotateErrorCodes(typeof(UnpublishAd))]
        public Task<UnpublishAd.Payload> UnpublishAd(
            [Inject] IMediator mediator,
            NonNull<UnpublishAd.Input> input)
        {
            return mediator.Send(input.Value);
        }
    }
}