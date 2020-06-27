using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCNBlog.Api.Extensions;
using MyCNBlog.Core;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Services;
using MyCNBlog.Services.Abstractions;
using MyCNBlog.Services.Authorization;
using MyCNBlog.Services.ResourceShaping;

namespace MyCNBlog.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class BlogUserController : AppBaseController
    {
        private readonly IBlogUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly IValidator<UserRegisterDto> _userRegisterDtoValidator;
        private readonly ILogger _logger;
        private readonly ITextCryptoService _cryptoService;
        private readonly IAuthorizationService _authServ;

        public BlogUserController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            UserManager<BlogUser> userManager, 
            IOptions<IdentityOptions> identityOptions, 
            ITypeService typeService,
            IBlogUserRepository userRepo,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IValidator<UserRegisterDto> userRegisterDtoValidator,
            ITextCryptoService cryptoService,
            ILoggerFactory loggerFactory,
            IAuthorizationService authServ) :
            base(unitOfWork, mapper, userManager, identityOptions, typeService)
        {
            _logger = loggerFactory?.CreateLogger(GetType().FullName) ??
                throw new ArgumentNullException(nameof(loggerFactory));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userRegisterDtoValidator = userRegisterDtoValidator ?? throw new ArgumentNullException(nameof(userRegisterDtoValidator));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _authServ = authServ ?? throw new ArgumentNullException(nameof(authServ));
        }
        #region Avatar

        /// <summary>
        /// TODO: 图片裁剪后保存, 配合前端传进来尺寸区域参数
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<string> SaveUserAvatorAsync(IFormFile file)
        {
            if(file == null)
                throw new ArgumentNullException(nameof(file));
            string basedir = Path.Combine(AppConstants.BaseFileDirectory);
            string subdir = "user-avatars";
            Directory.CreateDirectory(Path.Combine(basedir, subdir));
            string fileName = $"User-Avatar-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            string savePath = Path.Combine(basedir, subdir, fileName);
            using(var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
                await fs.FlushAsync();
                fs.Close();
            }

            return savePath;
        }

        private string GetUserAvatorUrl(BlogUser user)
        {
            return GetUserAvatorUrl(user.Id);
        }

        private string GetUserAvatorUrl(int userId)
        {
            return Url.Link(nameof(BlogUserController.GetUserAvatar), new { userId });
        }

        /// <summary>
        /// 用户头像获取
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/avatar", Name = nameof(GetUserAvatar))]
        public async Task<IActionResult> GetUserAvatar(int userId)
        {
            BlogUser user = await _userRepo.QueryByIdAsync(userId);

            if(user == null)
                return BadRequest();

            string path = user.AvatarPath;
            if(string.IsNullOrEmpty(path))
                path = _configuration.GetDefaultUserAvatarFilePath();

            var fp = new FileExtensionContentTypeProvider();
            fp.TryGetContentType(Path.GetFileName(path), out string contentType);
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            return File(fs, contentType);
        }

        /// <summary>
        /// 修改用户头像
        /// </summary>
        /// <returns></returns>
        [HttpPut("avatar")]
        [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)]
        public async Task<IActionResult> UpdateAvatar()
        {
            IFormFile formFile = HttpContext.Request.Form.Files.FirstOrDefault();
            if(formFile == null)
                return BadRequest();

            int userId = AuthHelpers.GetUserId(User, UserIdClaimType).Value;
            BlogUser user = await _userRepo.QueryByIdAsync(userId);
            user.AvatarPath = await SaveUserAvatorAsync(formFile);

            _userRepo.Update(user);

            await SaveChangesOrThrowIfFailed();

            return NoContent();
        }

        #endregion

        /// <summary>
        /// 注册用户, 生产环境需要使用公钥加密用户密码
        /// </summary>
        /// <param name="dto">用户信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserRegisterDto dto)
        {
            if(!_env.IsDevelopment())
                dto.SecurePassword = _cryptoService.Decrypt(dto.SecurePassword);

            if(!ModelState.IsValid)
                return BadRequest(dto);

            BlogUser user = Mapper.Map<BlogUser>(dto);

            user.AvatarPath = dto.Avatar == null ?
                _configuration.GetDefaultUserAvatarFilePath() :
                await SaveUserAvatorAsync(dto.Avatar);

            IdentityResult result = await UserManager.CreateAsync(user, dto.SecurePassword);

            if(!result.Succeeded)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, $"Failed to create user, errors: {result.Errors?.Select(x => x.Description + ";")}");

            BlogUserDto userDto = Mapper.Map<BlogUserDto>(user);
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
        [HttpGet("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnlyPolicy)]
        [Produces(ContentTypes.JsonContentType)]
        [ProducesResponseType(typeof(BlogUserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser([FromRoute] int id, [FromQuery] string fields)
        {
            if(!TypeService.HasProperties<BlogUserDto>(fields))
                this.FieldsNotExist(fields);

            BlogUser user = await UserManager.FindByIdAsync(id.ToString());

            if(user == null)
                return BadRequest($"Failed to find user with id: ${id}");

            if(user.IsDeleted)
            {   // 已经软删除的用户只有管理员可以查询, 即使用户本人也禁止查询
                bool accessable = (await _authServ.AuthorizeAsync(
                    User, null, 
                    new RolesAuthorizationRequirement(new string[] { RoleConstants.SuperAdmin })))
                    .Succeeded;
                if(!accessable)
                    return Forbid();
            }

            BlogUserDto userDto = Mapper.Map<BlogUserDto>(user);
            userDto.AvatarUrl = GetUserAvatorUrl(user);

            ExpandoObject shapedUserDto = userDto.ToDynamicObject(fields);

            return Ok(shapedUserDto);
        }

        /// <summary>
        /// 获取当前登录的用户信息
        /// </summary>
        /// <param name="fields">需要的用户信息, 按需获取, 节省带宽</param>
        /// <returns></returns>
        [HttpGet("active")]
        [Authorize]
        [Produces(ContentTypes.JsonContentType)]
        [ProducesResponseType(typeof(BlogUserDto), StatusCodes.Status200OK)]
        public Task<IActionResult> GetUser([FromQuery] string fields)
        {
            int.TryParse(UserManager.GetUserId(User), out int id);
            return GetUser(id, fields);
        }

        /// <summary>
        /// 分页获取所有用户数据; 
        /// Permission: <see cref="AuthorizationPolicies.AdminOnlyPolicy"/>
        /// </summary>
        /// <param name="parameters">查询参数</param>
        /// <returns></returns>
        [HttpGet(Name = "GetUsers")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnlyPolicy)]
        [ProducesResponseType(typeof(IEnumerable<BlogUserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] BlogUserQueryParameters parameters)
        {
            PaginationList<BlogUser> users = await _userRepo.QueryAsync(parameters);

            IList<BlogUserDto> usersDto = Mapper.Map<IList<BlogUserDto>>(users)
                .Select(x =>
                {
                    x.AvatarUrl = GetUserAvatorUrl(x.Id);
                    return x;
                }).ToList();


            IEnumerable<ExpandoObject> shapedUsers = usersDto.ToDynamicObject(parameters.Fields);

            this.AddPaginationHeader(users);

            return Ok(shapedUsers);
        }

        /// <summary>
        /// 局部更新用户数据; Permission: <see cref="RoleConstants.SuperAdmin"/>
        /// </summary>
        /// <param name="id">用户id</param>
        /// <param name="pathDoc"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)]
        [Consumes(ContentTypes.JsonContentType)]
        public async Task<IActionResult> PatchUpdateUser([FromRoute] int id, [FromBody] JsonPatchDocument<UserRegisterDto> pathDoc)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            BlogUser user = await UserManager.FindByIdAsync(id.ToString());

            // 管理员权力至高无上, 可以修改任何用户的数据
            bool accessable = (await _authServ.AuthorizeAsync(
                User, null,
                new RolesAuthorizationRequirement(
                    new string[] { RoleConstants.SuperAdmin }))).Succeeded;
            // 普通用户只能修改自己的数据, 如果传进来其他用户, 则403
            if(!accessable)
                accessable = (await _authServ.AuthorizeAsync(
                User, null,
                new ResourceOwnerAuthorizationRequirement<int>(id))).Succeeded;

            if(!accessable)
                return Forbid();

            if(user == null)
                return BadRequest($"Can not find user with id {id}");

            UserRegisterDto userDto = Mapper.Map<UserRegisterDto>(user);
            pathDoc.ApplyTo(userDto);

            // TODO: 写博客记录, Patch方法结合FluentValidation做模型验证的方案
            // 手动模型验证
            await this.ValidateModelAsync(_userRegisterDtoValidator, userDto);
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            Mapper.Map(userDto, user);
            IdentityResult result = await UserManager.UpdateAsync(user);
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
        [Authorize(AuthorizationPolicies.AdminOnlyPolicy)]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId, [FromQuery] bool hardDelete)
        {
            _userRepo.DeleteById(userId, !hardDelete);

            bool result = await UnitOfWork.SaveChangesAsync();

            if(!result)
                throw new Exception($"Delete user {userId} failed while write database");

            return NoContent();
        }

        /// <summary>
        /// 用户注销API, 软删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete()]
        [Authorize()]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = int.Parse(UserManager.GetUserId(User));
            _userRepo.DeleteById(userId, true);

            await SaveChangesOrThrowIfFailed();

            return NoContent();
        }
    }
}
