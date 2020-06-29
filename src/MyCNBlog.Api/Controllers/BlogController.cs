using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        protected IPostRepository PostRepo { get; }
        protected ICommentRepository CommentRepo { get; }

        public BlogController(IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<BlogUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            ITypeService typeService,
            IBlogRepository blogRepo,
            IAuthorizationService authServ,
            IBlogUserRepository userRepo,
            IValidator<BlogAddDto> validator,
            IPostRepository postRepo,
            ICommentRepository commentRepo)
            : base(unitOfWork, mapper, userManager, identityOptions, typeService)
        {
            BlogRepo = blogRepo;
            AuthServ = authServ;
            UserRepo = userRepo;
            Validator = validator;
            PostRepo = postRepo;
            CommentRepo = commentRepo;
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
            bool isOpen = blog.IsOpened;
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
            
            if(!isOpen && blogDto.IsOpened == true)
                blog.OpenDate = DateTime.Now;

            Mapper.Map(blogDto, blog);

            BlogRepo.Update(blog);

            return await SaveChangesOrThrowIfFailed();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BlogDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBlog([FromRoute]int id, [FromQuery]string fields)
        {
            //UserManager.FindByIdAsync()
            Blog blog = await BlogRepo.Query().FirstOrDefaultAsync(x => x.Id == id);
            blog.User = await UserManager.FindByIdAsync(blog.UserId.ToString());
            if(blog == null)
                return BadRequest();
            
            BlogDto dto = Mapper.Map<BlogDto>(blog);
            dto.Blogger.AvatarUrl = GetUserAvatorUrl(blog.User);
            ExpandoObject blogger = dto.Blogger.ToDynamicObject("id, username, avatarUrl, sex");
            dto.TotalPostsCount = await PostRepo.Query().CountAsync(x => x.BlogId == id);
            dto.TotalCommentsCount = await CommentRepo.Query().CountAsync(x => x.RepliedUserId == blog.User.Id);

            ExpandoObject shapedUser = dto.ToDynamicObject(fields);
            shapedUser.Remove(nameof(dto.Blogger), out _);
            shapedUser.TryAdd(nameof(dto.Blogger), blogger);

            return Ok(shapedUser);
        }
    }
}
