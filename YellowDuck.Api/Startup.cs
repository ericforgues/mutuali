using YellowDuck.Api.Authorization;
using YellowDuck.Api.Authorization.Requirements;
using YellowDuck.Api.Configuration;
using YellowDuck.Api.Constants;
using YellowDuck.Api.DbModel;
using YellowDuck.Api.DbModel.Entities;
using YellowDuck.Api.DbModel.Enums;
using YellowDuck.Api.Extensions;
using YellowDuck.Api.Gql;
using YellowDuck.Api.Gql.Interfaces;
using YellowDuck.Api.Plugins.Hangfire;
using YellowDuck.Api.Plugins.Identity;
using YellowDuck.Api.Plugins.MediatR;
using YellowDuck.Api.Services.Mailer;
using YellowDuck.Api.Services.Razor;
using YellowDuck.Api.Services.System;
using GraphQL.Conventions;
using Hangfire;
using Hangfire.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using StackifyLib;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YellowDuck.Api.Plugins.ImageSharp;
using YellowDuck.Api.Services.Crypto;
using YellowDuck.Api.Services.Files;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp.Web.DependencyInjection;
using YellowDuck.Api.Services.Twilio;
using YellowDuck.Api.Services.Twilio.Conversations;
using YellowDuck.Api.BackgroundJobs;
using Sentry.AspNetCore;
using Sentry;
using YellowDuck.Api.DbModel.Entities.Profiles;
using YellowDuck.Api.Services.Stripe;
using YellowDuck.Api.DbModel.Entities.Ads;

namespace YellowDuck.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Telemetry
            services.AddApplicationInsightsTelemetry();

            // Entity Framework
            services.AddDbContext<AppDbContext>(ConfigureDbContext);

            // MVC
            services.AddMvc()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // SPA Services

            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "ClientApp";
            });

            // Identity, Authentication, Authorization

            ConfigureClaimTypeMaps();

            services.Configure<JwtOptions>(Configuration.GetSection("jwt"));
            services.Configure<IdentityOptions>(Configuration.GetSection("identity"));

            services.AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

            services
                .AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<LongLivedTokenProvider>(TokenProviders.EmailInvites);

            services
                .AddAuthentication(ConfigureAuthentication)
                .AddJwtBearer(ConfigureJwtBearer);

            services.AddAuthorization(ConfigureAuthorization);

            AddAuthorizationHandlers(services);

            // GraphQL

            // Ça cause un warning lors du build, mais on doit quand même créer l'instance du authorization policy
            // provider à ce stade pour que la création de l'engin GraphQL fonctionne 
            using (var serviceProvider = services.BuildServiceProvider())
            {
                AnnotatePolicyAttribute.AuthorizationPolicyProvider =
                    serviceProvider.GetService<IAuthorizationPolicyProvider>();
            }

            services.AddScoped<IAppUserContext, AppUserContext>();
            services.AddScoped<IUserContext>(s => s.GetRequiredService<IAppUserContext>());
            services.AddScoped<IDependencyInjector, DependencyInjector>();
            services.AddScoped<DataLoader>();
            services.AddSingleton(GraphQLEngineFactory.Create());

            // MediatR

            services.AddMediatR(typeof(Startup).Assembly);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ConcurrencyHandlingBehavior<,>));

            // FluentEmail

            services
                .AddFluentEmail(Configuration["Mailer:FromEmail"], Configuration["Mailer:FromName"])
                .AddSmtpSender(CreateSmtpClient);

            // ImageSharp

            services.AddImageSharp()
                .ClearProviders().AddProvider<AppImageProvider>()
                .SetCache<AppImageCache>();

            // Crypto

            services.Configure<HmacSignatureOptions>(Configuration.GetSection("hmac"));
            services.AddSingleton<ISignatureService, HmacSignatureService>();

            // File system

            services.AddFileManager(Configuration.GetSection("files"));
            services.AddScoped<ImageUrlProvider>();
            services.AddScoped<FileUrlProvider>();

            // Hangfire

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("AppDbContext")));
            services.AddHangfireServer();

            // App services

            services.AddHttpClient();
            services.AddLazyCache();

            services.AddSingleton<IClock>(NodaTime.SystemClock.Instance);

            services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

            services.AddScoped<IRazorRenderer, RazorRenderer>();
            AddDecoratedMailer(services);

            // Twilio
            AddTwilioServices(services);

            // Stripe
            AddStripeServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext db, UserManager<AppUser> userManager, ILoggerFactory loggerFactory)
        {
            if (Configuration.GetValue<bool>("Stackify:Enabled"))
            {
                app.ConfigureStackifyLogging((IConfigurationRoot)Configuration);
            }

            db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
            db.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseGraphiQL();
                SeedDevUsers(userManager, db).GetAwaiter().GetResult();
            }
            else
            {
                app.UseHsts();
                SeedProdUsers(Configuration, userManager, db).GetAwaiter().GetResult();
            }

            if (env.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseImageSharp();
            app.UseHangfireDashboard(options: new DashboardOptions {
                Authorization = new[] {
                    new AnyAuthorizationFilter(
                        new LocalRequestsOnlyAuthorizationFilter(),
                        new IpWhitelistAuthorizationFilter(Configuration["hangfire:ipWhitelist"], loggerFactory.CreateLogger<IpWhitelistAuthorizationFilter>())
                    )
                }
            });

            app.UseRouting();

            AddSentry();
            app.UseSentryTracing();

            app.UseAuthorization();
            app.UseEndpoints(builder =>
            {
                builder.MapControllers();
            });

            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer(Configuration["FrontendDevServer"]);
                }
            });

            SendDailyConversationNotificationEmail.RegisterJob(Configuration);
            CloseCompletedContract.RegisterJob(Configuration);
            SendPayoutsForContractsStarted.RegisterJob(Configuration);
            SendKPIsEmail.RegisterJob(Configuration);
            SendAlert.RegisterJob(Configuration);
        }

        private void ConfigureDbContext(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(
                Configuration.GetConnectionString("AppDbContext"),
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); });
        }

        private static void ConfigureClaimTypeMaps()
        {
            Map("stamp", "AspNet.Identity.SecurityStamp");
            Map("utype", AppClaimTypes.UserType);

            void Map(string jwtClaim, string identityClaim)
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Add(jwtClaim, identityClaim);
                JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Add(identityClaim, jwtClaim);
            }
        }

        private void ConfigureAuthentication(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        private void ConfigureJwtBearer(JwtBearerOptions jwtBearerOptions)
        {
            var jwtOptions = Configuration.GetSection("jwt").Get<JwtOptions>();
            jwtBearerOptions.TokenValidationParameters = jwtOptions.GetTokenValidationParameters();

            jwtBearerOptions.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    if (ctx.Exception is SecurityTokenExpiredException)
                    {
                        ctx.Response.Headers.Add("Token-Expired", "true");
                    }

                    return Task.CompletedTask;
                }
            };
        }

        private void ConfigureAuthorization(AuthorizationOptions options)
        {
            options.AddPolicy(AuthorizationPolicies.LoggedIn, policy => { policy.RequireAuthenticatedUser(); });
            options.AddPolicy(AuthorizationPolicies.IsAdmin, policy => { policy.RequireClaim(AppClaimTypes.UserType, UserType.Admin.ToString()); });
            options.AddPolicy(AuthorizationPolicies.IsUser, policy => { policy.RequireClaim(AppClaimTypes.UserType, UserType.User.ToString()); });

            options.AddPolicy(AuthorizationPolicies.ManageAd, policy => { policy.AddRequirements(new CanManageAdRequirement()); });
            options.AddPolicy(AuthorizationPolicies.ManageAlert, policy => { policy.AddRequirements(new CanManageAlertRequirement()); });
            options.AddPolicy(AuthorizationPolicies.ManageUser, policy => { policy.AddRequirements(new CanManageUserRequirement()); });

            options.AddPolicy(AuthorizationPolicies.InConversation, policy => { policy.AddRequirements(new InConversationRequirement()); });
        }

        private void AddAuthorizationHandlers(IServiceCollection services)
        {
            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes => classes.AssignableTo<IAuthorizationHandler>())
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime();
            });
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(
                Configuration.GetValue<string>("Mailer:SmtpHost"),
                Configuration.GetValue<int>("Mailer:SmtpPort"))
            {
                Credentials = new NetworkCredential(
                    Configuration.GetValue<string>("Mailer:SmtpUsername"),
                    Configuration.GetValue<string>("Mailer:SmtpPassword")),
                EnableSsl = Configuration.GetValue<bool>("Mailer:SmtpSsl")
            };
        }

        private static void AddDecoratedMailer(IServiceCollection services)
        {
            services.AddScoped<FluentMailer>();

            services.AddScoped<IMailer>(x => new RetryingMailer(
                x.GetService<FluentMailer>(),
                x.GetService<ILogger<RetryingMailer>>())
            );
        }

        private static void AddTwilioServices(IServiceCollection services)
        {
            services.AddScoped<ITwilioService, TwilioService>();
            services.AddScoped<IConversationsService, ConversationsService>();
        }

        private static void AddStripeServices(IServiceCollection services)
        {
            services.AddScoped<IPaymentService, PaymentService>();
        }

        private void AddSentry()
        {
            var options = new SentryOptions();
            options.Dsn = "https://13a428379b104aff859022ff52144e3e@o793438.ingest.sentry.io/5876091";
            options.TracesSampleRate = 0.5;

#if NCRUNCH
            options.Release = "ncrunch";
#else
            options.Release = $"{GitVersionInformation.CommitDate}/{GitVersionInformation.Sha}";
#endif
            options.Environment = Configuration["Sentry:Environment"];
            SentrySdk.Init(options);
        }

        private static async Task SeedDevUsers(UserManager<AppUser> userManager, AppDbContext db)
        {
            const string defaultPassword = "Abcd1234!!";

            await userManager.CreateOrUpdateAsync(db, new CreateOrUpdateIdentity()
            {
                Email = "ubert@example.com",
                Type = UserType.User,
                Password = defaultPassword,
                FirstName = "Ubert",
                LastName = "Example",
                OrganizationName = "Example Inc.",
                OrganizationType = OrganizationType.NonProfitOrganizations,
                Industry = Industry.HealthAndSocialServices,
                PostalCode = "G1K 3G5",
                PhoneNumber = "514 555-1234",
                ShowEmail = true,
                ShowPhoneNumber = false,
            });
            
            await userManager.CreateOrUpdateAsync(db, new CreateOrUpdateIdentity()
            {
                Email = "uguette@example.com",
                Type = UserType.User,
                Password = defaultPassword,
                FirstName = "Uguette",
                LastName = "Example",
                OrganizationName = "Example Inc.",
                OrganizationType = OrganizationType.PrivateCompany,
                Industry = Industry.Catering,
                PostalCode = "G1K 3G5",
                PhoneNumber = "514 555-1234",
                ShowEmail = false,
                ShowPhoneNumber = false,
            });

            await userManager.CreateOrUpdateAsync(db, new CreateOrUpdateIdentity()
            {
                Email = "alain@example.com",
                Type = UserType.Admin,
                Password = defaultPassword,
                FirstName = "Alain",
                LastName = "Example",
                OrganizationName = "Example Inc.",
                OrganizationType = OrganizationType.PublicSector,
                Industry = Industry.EducationAndTeaching,
                PostalCode = "G1K 3G5",
                PhoneNumber = "514 555-1234",
                ShowEmail = true,
                ShowPhoneNumber = true,
            });

            await userManager.CreateOrUpdateAsync(db, new CreateOrUpdateIdentity()
            {
                Email = "aline@example.com",
                Type = UserType.Admin,
                Password = defaultPassword,
                FirstName = "Aline",
                LastName = "Example",
                OrganizationName = "Example Inc.",
                OrganizationType = OrganizationType.NonProfitOrganizations,
                Industry = Industry.FoodProcessingAndDistribution,
                PostalCode = "G1K 3G5",
                PhoneNumber = "514 555-1234",
                ShowEmail = false,
                ShowPhoneNumber = true,
            });
        }

        private static async Task SeedProdUsers(IConfiguration configuration, UserManager<AppUser> userManager, AppDbContext db)
        {
            var email = configuration.GetValue<string>("defaultAdmin:email");
            var password = configuration.GetValue<string>("defaultAdmin:password");
            var firstName = configuration.GetValue<string>("defaultAdmin:firstName");
            var lastName = configuration.GetValue<string>("defaultAdmin:lastName");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

            await userManager.CreateOrUpdateAsync(db, new CreateOrUpdateIdentity()
            {
                Email = email,
                Type = UserType.Admin,
                Password = password,
                FirstName = firstName,
                LastName = lastName
            });
        }
    }
}
