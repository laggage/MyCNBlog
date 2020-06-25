using AutoMapper;
using FluentValidation;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCNBlog.Api.Extensions;
using MyCNBlog.Core;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Services.Abstractions;
using MyCNBlog.Services.Extensions;
using MyCNBlog.Services.ResourceShaping;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserManager<BlogUser> _userManager;
        private readonly ILogger _logger;
        private readonly ITextCryptoService _cryptoService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IValidator<UserRegisterDto> _userRegisterDtoValidator;
        private readonly ITypeService _typeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlogUserRepository _userRepo;

        public AuthController(IMapper mapper,
            UserManager<BlogUser> userManager,
            ILoggerFactory loggerFactory,
            ITextCryptoService cryptoService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IValidator<UserRegisterDto> userRegisterDtoValidator,
            ITypeService typeService,
            IUnitOfWork unitOfWork,
            IBlogUserRepository userRepo)
        {
            _logger = loggerFactory?.CreateLogger(GetType().FullName) ??
                throw new ArgumentNullException(nameof(loggerFactory));

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _userRegisterDtoValidator = userRegisterDtoValidator ?? throw new ArgumentNullException(nameof(userRegisterDtoValidator));
            _typeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        /// <summary>
        /// 注册用户, 生产环境需要使用公钥加密用户密码
        /// </summary>
        /// <param name="dto">用户信息</param>
        /// <returns></returns>
        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromForm] UserRegisterDto dto)
        {
            if(!_env.IsDevelopment())
                dto.SecurePassword = _cryptoService.Decrypt(dto.SecurePassword);

            if(!ModelState.IsValid)
                return BadRequest(dto);

            BlogUser user = _mapper.Map<BlogUser>(dto);
            IdentityResult result = await _userManager.CreateAsync(user, dto.SecurePassword);

            if(!result.Succeeded)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"Failed to create user, errors: {result.Errors?.Select(x => x.Description + ";")}");

            BlogUserDto userDto = _mapper.Map<BlogUserDto>(user);
            _logger.LogInformation($"User: {user.Id},{user} created");

            return Ok(userDto);
        }


        /// <summary>
        /// 根据ID获取用户信息
        /// Permission: <see cref="RoleConstants.SuperAdmin"/>
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="fields">需要的用户信息, 按需获取, 节省带宽</param>
        /// <returns></returns>
        [HttpGet("user/{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnlyPolicy)]
        [Produces(ContentTypes.JsonContentType)]
        [ProducesResponseType(typeof(BlogUserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser([FromRoute] int id, [FromQuery] string fields)
        {
            if(!_typeService.HasProperties<BlogUserDto>(fields))
                this.FieldsNotExist(fields);

            BlogUser user = await _userManager.FindByIdAsync(id.ToString());
            if(user == null)
                return BadRequest($"Failed to find user with id: ${id}");
            BlogUserDto userDto = _mapper.Map<BlogUserDto>(user);

            ExpandoObject shapedUserDto = userDto.ToDynamicObject(fields);

            return Ok(shapedUserDto);
        }

        /// <summary>
        /// 获取当前登录的用户信息
        /// </summary>
        /// <param name="fields">需要的用户信息, 按需获取, 节省带宽</param>
        /// <returns></returns>
        [HttpGet("user")]
        [Authorize]
        [Produces(ContentTypes.JsonContentType)]
        [ProducesResponseType(typeof(BlogUserDto), StatusCodes.Status200OK)]
        public Task<IActionResult> GetUser([FromQuery] string fields)
        {
            int.TryParse(_userManager.GetUserId(User), out int id);
            return GetUser(id, fields);
        }

        /// <summary>
        /// 分页获取所有用户数据; 
        /// Permission: <see cref="AuthorizationPolicies.AdminOnlyPolicy"/>
        /// </summary>
        /// <param name="parameters">查询参数</param>
        /// <returns></returns>
        [HttpGet("users", Name = "GetUsers")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<BlogUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers(
            [FromQuery]BlogUserQueryParameters parameters)
        {
            PaginationList<BlogUser> users = await _userRepo.QueryAsync(parameters);

            IList<BlogUserDto> usersDto = _mapper.Map<IList<BlogUserDto>>(users);

            IEnumerable<ExpandoObject> shapedUsers = usersDto.ToDynamicObject(parameters.Fields);
            
            this.AddPaginationHeader(users);

            return Ok(shapedUsers);
        }

        /// <summary>
        /// 局部更新用户数据; Permission: <see cref="RoleConstants.SuperAdmin"/>
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [HttpPatch("user/{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnlyPolicy)]
        [Consumes(ContentTypes.JsonContentType)]
        public async Task<IActionResult> PatchUpdateUser([FromRoute] int id, [FromBody] JsonPatchDocument<UserRegisterDto> pathDoc)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            BlogUser user = await _userManager.FindByIdAsync(id.ToString());
            if(user == null)
                return BadRequest($"Can not find user with id {id}");

            UserRegisterDto userDto = _mapper.Map<UserRegisterDto>(user);
            pathDoc.ApplyTo(userDto);

            // TODO: 写博客记录, Patch方法结合FluentValidation做模型验证的方案
            // 手动模型验证
            await this.ValidateModelAsync(_userRegisterDtoValidator, userDto);
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            _mapper.Map(userDto, user);
            IdentityResult result = await _userManager.UpdateAsync(user);
            if(!result.Succeeded)
                throw new Exception($"Faild to update user: {user} while write database");

            return NoContent();
        }

        /// <summary>
        /// 根据Id删除某个用户; 
        /// Permission: <see cref="AuthorizationPolicies.AdminOnlyPolicy"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="hardDelete">是否从物理上删除数据</param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(AuthorizationPolicies.AdminOnlyPolicy)]
        public async Task<IActionResult> DeleteUser([FromRoute]int userId, [FromQuery]bool hardDelete)
        {
            _userRepo.DeleteById(userId, !hardDelete);

            bool result = await _unitOfWork.SaveChangesAsync();

            if(!result)
                throw new Exception($"Delete user {userId} failed while write database");
            
            return NoContent();
        }

        /// <summary>
        /// 用户注销API, 软删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize()]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = int.Parse(_userManager.GetUserId(User));
            _userRepo.DeleteById(userId, true);

            bool result = await _unitOfWork.SaveChangesAsync();

            if(!result)
                throw new Exception($"Delete user {userId} failed while write database");

            return NoContent();
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
        public async Task<IActionResult> GetJwtToken([FromBody] UserLoginDto dto)
        {
            if(!ModelState.IsValid)
                return UnprocessableEntity(dto);

            BlogUser user = await _userManager.FindByNameAsync(dto.UserName);
            if(!_env.IsDevelopment())
            {   // 非生产环境, 前端将密码通过公钥加密发送过来, 这里使用私钥解密
                // TODO: 每次登录使用不同的RSA密钥
                dto.SecurePassword = _cryptoService.Decrypt(dto.SecurePassword);
            }

            if(user == null || !await _userManager.CheckPasswordAsync(user, dto.SecurePassword))
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

            string[] roles = (await _userManager.GetRolesAsync(user)).ToArray();
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
