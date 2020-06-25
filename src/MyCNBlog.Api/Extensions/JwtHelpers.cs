using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyCNBlog.Services;
using System.Security.Cryptography;

namespace MyCNBlog.Api.Extensions
{
    public static class JwtHelpers
    {
        private static RSA _rsa = null;
        private static SecurityKey _key = null;
        private static string _issuer = null;
        private static string _audience = null;

        private const string _issuerKey = "Auth:Jwt:Issuer";
        private const string _audienceKey = "Auth:Jwt:Audience";
        public static SigningCredentials GetSigningCredentials(IConfiguration configuration)
        {
            return new SigningCredentials(
                GetSigningKey(configuration), 
                SecurityAlgorithms.RsaSha256Signature); ;
        }

        public static string GetIssuer(IConfiguration configuration)
            => _issuer ??= configuration.GetValue<string>(_issuerKey);

        public static string GetAudience(IConfiguration configuration)
            => _audience ??= configuration.GetValue<string>(_audienceKey);

        public static SecurityKey GetSigningKey(IConfiguration configuration)
        {
            if(_key == null)
            {
                var rsaOptions = new RSATextCryptoServiceOptions();
                configuration.GetSection("Auth:Security").Bind(rsaOptions);

                _rsa ??= RSATextCryptoService.CreateRSA(
                    rsaOptions.CertificatePath, rsaOptions.KeyFilePath,
                    rsaOptions.Password);

                _key = new RsaSecurityKey(_rsa);
            }

            return _key;
        }
    }
}
