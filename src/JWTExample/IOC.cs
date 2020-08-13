using JWTExample.Domain.Helpers;
using JWTExample.Domain.Helpers.Interfaces;
using JWTExample.Domain.Mappers;
using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Domain.Services;
using JWTExample.Domain.Services.Interfaces;
using JWTExample.Logging;
using JWTExample.Logging.Interfaces;
using JWTExample.Models;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;
using JWTExample.Models.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JWTExample
{
    public static class IOC
    {
        public static void RegisterDependencies(IServiceCollection services, IConfiguration configuration)
        {
            RegisterSettings(services, configuration);
            RegisterLogging(services);
            RegisterServices(services);
            RegisterMappers(services);
        }

        private static void RegisterMappers(IServiceCollection services)
        {
            services.AddTransient<IMapper<Account, AccountResponse>, AccountResponseMapper>();
            services.AddTransient<IMapper<CreateRequest, Account>, CreateRequestMapper>();
            services.AddTransient<IMapper<Account, AuthenticateResponse>, AuthenticateResponseMapper>();
            services.AddTransient<IMapper<RegisterRequest, Account>, RegisterRequestMapper>();

            services.AddTransient<IAccountUpdater, AccountUpdater>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IAccountControllerHelpers, AccountControllerHelpers>();
            services.AddTransient<IAccountService, AccountService>();
        }

        private static void RegisterLogging(IServiceCollection services)
        {
            services.AddScoped<ILoggerContext, LoggerContext>();
        }

        private static void RegisterSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAppSettings>((services) => new AppSettings(configuration));
        }
    }
}