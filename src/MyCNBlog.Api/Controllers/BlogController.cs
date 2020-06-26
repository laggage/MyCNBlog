using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyCNBlog.Api.Extensions;
using MyCNBlog.Core;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Repositories.Abstractions;
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
        protected IValidator<BlogAddDto> Validator { get; }

        public BlogController(IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<BlogUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            ITypeService typeService,
            IBlogRepository blogRepo,
            IAuthorizationService authServ,
            IBlogUserRepository userRepo,
            IValidator<BlogAddDto> validator)
            : base(unitOfWork, mapper, userManager, identityOptions, typeService)
        {
            BlogRepo = blogRepo;
            AuthServ = authServ;
            UserRepo = userRepo;
            Validator = validator;
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
        [Consumes(ContentTypes.JsonContentType)]
        [Authorize]
        public async Task<IActionResult> PartiallyUpdate(
             [FromRoute] int blogId,
             [FromBody] JsonPatchDocument<BlogAddDto> patchDoc)
        {
            Blog blog = await BlogRepo.QueryByIdAsync(blogId);
            if(blog == null)
                return NotFound();

            BlogAddDto blogDto = Mapper.Map<BlogAddDto>(blog);
            patchDoc.ApplyTo(blogDto);

            await this.ValidateModelAsync(Validator, blogDto);
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!(await AuthServ.AuthorizeAsync(
                    User, blogDto, AuthorizationPolicies.OpenBlogPolicy))
                    .Succeeded)
                return Forbid();

            Mapper.Map(blogDto, blog);

            BlogRepo.Update(blog);

            return await SaveChangesAndThrowIfFailed();
        }
    }
}
