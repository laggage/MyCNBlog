using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Services.Abstractions;
using MyCNBlog.Services.ResourceShaping;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MyCNBlog.Services.Extensions
{
    public static class ServicesExtensions
    {
        public static OptionsBuilder<RSATextCryptoServiceOptions> AddTextCryptoService(
            this IServiceCollection services)
        {
            OptionsBuilder<RSATextCryptoServiceOptions> builder = services.AddOptions<RSATextCryptoServiceOptions>();
           
            services.AddSingleton<ITextCryptoService, RSATextCryptoService>();
            return builder;
        }

        public static void AddTextCryptoService(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTextCryptoService()
                .Configure(config =>
                {
                    IConfigurationSection sec = configuration.GetSection("Auth:Security");
                    sec.Bind(config);
                });
        }

        public static Task<RSAPublicKeyParametersDto> GetRSAPublicKeyParametersDtoAsync(this ITextCryptoService service)
        {
            return Task.Run(() =>
            {
                RSAParameters rsaParams = service.GetRSAParameters();
                var parameters = new RSAPublicKeyParametersDto()
                {
                    Modulus = Convert.ToBase64String(rsaParams.Modulus),
                    Exponents = Convert.ToBase64String(rsaParams.Exponent)
                };

                return parameters;
            });
        }

        public static void AddTypeSerivce(this IServiceCollection services)
        {
            services.AddTransient<ITypeService, TypeService>();
        }
    }
}
