using AutoMapper;
using IdentityModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyCNBlog.Api.Extensions;
using MyCNBlog.Core;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Services.Abstractions;
using MyCNBlog.Services.Extensions;
using MyCNBlog.Services.ResourceShaping;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyCNBlog.Api.Controllers
{
    /// <summary>
    /// 鉴权/身份/用户数据控制器
    /// </summary>
    [ApiController]
    [Route("auth")]
    public class AuthController : AppBaseController
    {
        private readonly ILogger _logger;
        private readonly ITextCryptoService _cryptoService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AuthController(IMapper mapper,
            UserManager<BlogUser> userManager,
            ILoggerFactory loggerFactory,
            ITextCryptoService cryptoService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            ITypeService typeService,
            IUnitOfWork unitOfWork,
            IOptions<IdentityOptions> identityOptions)
            :base(unitOfWork, mapper, userManager, identityOptions, typeService)
        {
            _logger = loggerFactory?.CreateLogger(GetType().FullName) ??
                throw new ArgumentNullException(nameof(loggerFactory));

            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        


        /// <summary>
        /// 获取经过Base64编码的RSA公钥参数(modulus, exponent)
        /// </summary>
        /// <returns></returns>
        [HttpGet("publicKey")]
        public async Task<IActionResult> GetPublicKey()
        {
            RSAPublicKeyParametersDto dto = await _cryptoService.GetRSAPublicKeyParametersDtoAsync();
            return Ok(dto);
        }

        /// <summary>
        /// 颁发 Jwt Token, 生产环境下, 前端必须先请求publickey对用户密码进行加密
        /// 过期时间: dto.RememberMe = true ? 30 day : 1day;
        /// </summary>
        /// <param name="dto">用户登录信息对象</param>
        /// <returns></returns>
        [HttpPost("login/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes(ContentTypes.JsonContentType)]
        public async Task<IActionResult> GetJwtToken([FromBody] UserLoginDto dto)
        {
            if(!ModelState.IsValid)
                return UnprocessableEntity(dto);

            BlogUser user = await UserManager.FindByNameAsync(dto.UserName);
            if(user == null)
                user = await UserManager.FindByEmailAsync(dto.UserName);
            if(user.IsDeleted)
                return BadRequest("用户已注销");

            if(!_env.IsDevelopment())
            {   // 非生产环境, 前端将密码通过公钥加密发送过来, 这里使用私钥解密
                // TODO: 每次登录使用不同的RSA密钥
                dto.SecurePassword = _cryptoService.Decrypt(dto.SecurePassword);
            }

            if(user == null || !await UserManager.CheckPasswordAsync(user, dto.SecurePassword))
                return Unauthorized("Invalid username or password");

            DateTime issuedAt = DateTime.Now;
            DateTime expired = dto.RememberMe ?
                issuedAt.AddDays(30) :
                issuedAt.AddDays(1);

            var claims = new List<Claim> {
                new Claim(JwtClaimTypes.Name, user.UserName),
                new Claim(JwtClaimTypes.Issuer, JwtHelpers.GetIssuer(_configuration)),
                new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
            };

            string[] roles = (await UserManager.GetRolesAsync(user)).ToArray();
            foreach(string role in roles)
                claims.Add(new Claim(JwtClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Claims = new Dictionary<string, object>(claims.Select(x => new KeyValuePair<string, object>(x.Type, x.Value))),
                Expires = expired,
                Audience = JwtHelpers.GetAudience(_configuration),
                IssuedAt = issuedAt,
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = JwtHelpers.GetSigningCredentials(_configuration),
                Issuer = JwtHelpers.GetIssuer(_configuration),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            string access_token = tokenHandler.WriteToken(token);

            return Ok(new
            {
                access_token,
                expires_at = new DateTimeOffset(expired).ToUnixTimeSeconds(),
                auth_time = new DateTimeOffset(issuedAt).ToUnixTimeSeconds(),
                sid = user.Id
            });
        }
    }
}
