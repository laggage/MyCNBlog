using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyCNBlog.Core;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Repositories.Abstractions;
using MyCNBlog.Services;
using MyCNBlog.Services.ResourceShaping;

namespace MyCNBlog.Api.Controllers
{
    [ApiController]
    [Route("api/blogs")]
    public class BlogController : AppBaseController
    {
        protected IBlogRepository BlogRepo { get; }
        protected IAuthorizationService AuthServ { get; }
        protected IBlogUserRepository UserRepo { get; }

        public BlogController(IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<BlogUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            ITypeService typeService,
            IBlogRepository blogRepo,
            IAuthorizationService authServ,
            IBlogUserRepository userRepo)
            : base(unitOfWork, mapper, userManager, identityOptions, typeService)
        {
            BlogRepo = blogRepo;
            AuthServ = authServ;
            UserRepo = userRepo;
        }

        /// <summary>
        /// 修改指定博客id的博客信息, 只有管理员有这个权限
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="patchDoc">
        /// <see cref="BlogAddDto"/>
        /// </param>
        /// <returns></returns>
        [HttpPatch("{blogId}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnlyPolicy)]
        [Consumes(ContentTypes.JsonContentType)]
        public async Task<IActionResult> PartiallyUpdate(
            [FromRoute] int blogId,
             [FromBody] JsonPatchDocument<BlogAddDto> patchDoc)
        {
            Blog blog = await BlogRepo.QueryByIdAsync(blogId);
            if(blog == null)
                return NotFound();

            BlogAddDto blogDto = Mapper.Map<BlogAddDto>(blog);
            patchDoc.ApplyTo(blogDto);
            // 为下面一个重载服务的鉴权
            if(!User.IsInRole(RoleConstants.SuperAdmin))
            {   // 只允许管理员设置 IsOpened属性
                if(!(await AuthServ.AuthorizeAsync(
                    User, blogDto, AuthorizationPolicies.OpenBlogPolicy))
                    .Succeeded)
                    return Forbid();
            }

            Mapper.Map(blogDto, blog);

            BlogRepo.Update(blog);

            return await SaveChangesAndThrowIfFailed();
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> PartiallyUpdate(
            JsonPatchDocument<BlogAddDto> patchDoc)
        {
            BlogUser user = await UserRepo.QueryByIdAsync(
                AuthHelpers.GetUserId(
                    User, IdentityOptions.ClaimsIdentity.UserIdClaimType).Value);

            int blogId = user.Blog.Id;

            return await PartiallyUpdate(blogId, patchDoc);
        }
    }
}
