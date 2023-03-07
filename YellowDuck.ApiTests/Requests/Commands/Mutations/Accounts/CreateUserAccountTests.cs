using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.EmailTemplates.Models;
using YellowDuck.Api.Requests.Commands.Mutations.Accounts;
using YellowDuck.Api.Services.Mailer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static YellowDuck.Api.Requests.Commands.Mutations.Accounts.CreateUserAccount;

namespace YellowDuck.ApiTests.Requests.Commands.Mutations.Accounts
{
    public class CreateUserAccountTest : TestBase
    {
        private readonly CreateUserAccount handler;
        private readonly Mock<IMailer> mailer;

        private const string Email = "test@example.com";
        private const string Password = "1234aAuuuu";
        private const string FirstName = "Example";
        private const string LastName = "Bliblou";
        private const string OrganizationName = "Example Inc.";
        private const string PostalCode = "G1K 0H1";
        private const string PhoneNumber = "514 555-1234";
        private const bool ShowPhoneNumber = true;
        private const bool ShowEmail = false;


        public CreateUserAccountTest()
        {
            mailer = new Mock<IMailer>();
            handler = new CreateUserAccount(UserManager, DbContext, mailer.Object, NullLogger<CreateUserAccount>.Instance, HttpContextAccessor);
        }

        [Fact]
        public async Task CreatesTheAccount()
        {
            var input = new Input
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                OrganizationName = OrganizationName,
                OrganizationType = OrganizationType.NonProfitOrganizations,
                Industry = Industry.HealthAndSocialServices,
                PostalCode = PostalCode,
                PhoneNumber = PhoneNumber,
                ShowPhoneNumber = ShowPhoneNumber,
                ShowEmail = ShowEmail
            };

            await handler.Handle(input, CancellationToken.None);

            var user = await DbContext.Users.Include(x => x.Profile).FirstAsync();

            user.Email.Should().Be(Email);
            user.Type.Should().Be(UserType.User);
            user.EmailConfirmed.Should().BeFalse();
            user.AcceptedTos.Should().Be(TosVersion.Latest);
            user.Profile.FirstName.Should().Be(FirstName);
            user.Profile.LastName.Should().Be(LastName);
            user.Profile.OrganizationName.Should().Be(OrganizationName);
            user.Profile.OrganizationType.Should().Be(OrganizationType.NonProfitOrganizations);
            user.Profile.Industry.Should().Be(Industry.HealthAndSocialServices);
            user.Profile.PostalCode.Should().Be(PostalCode);
            user.Profile.PhoneNumber.Should().Be(PhoneNumber);
            user.Profile.ShowPhoneNumber.Should().Be(ShowPhoneNumber);
            user.Profile.ShowEmail.Should().Be(ShowEmail);
        }

        [Fact]
        public async Task SendsConfirmationEmail()
        {
            var input = new Input
            {
                Email = Email,
                Password = Password,
                FirstName = FirstName,
                LastName = LastName,
                OrganizationName = OrganizationName,
                OrganizationType = OrganizationType.NonProfitOrganizations,
                Industry = Industry.HealthAndSocialServices,
                PostalCode = PostalCode,
                PhoneNumber = PhoneNumber,
                ShowPhoneNumber = ShowPhoneNumber,
                ShowEmail = ShowEmail
            };

            await handler.Handle(input, CancellationToken.None);

            mailer.Verify(x => x.Send(It.IsAny<ConfirmEmailEmail>()));
        }
    }
}
