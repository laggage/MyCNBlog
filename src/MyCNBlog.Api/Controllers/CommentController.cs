using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyCNBlog.Api.Extensions;
using MyCNBlog.Core;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Repositories.Abstractions;
using MyCNBlog.Services;
using MyCNBlog.Services.Authorization;
using MyCNBlog.Services.ResourceShaping;

namespace MyCNBlog.Api.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)]
    [Route("api/comments")]
    public class CommentController : AppBaseController
    {
        protected ICommentRepository CommentRepo { get; }
        protected IAuthorizationService AuthServ { get; }

        public CommentController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<BlogUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            ITypeService typeService,
            ICommentRepository commentRepo,
            IAuthorizationService authServ)
            : base(
                  unitOfWork,
                  mapper, userManager,
                  identityOptions, typeService)
        {
            CommentRepo = commentRepo;
            AuthServ = authServ;
        }

        [HttpPost]
        [Consumes(ContentTypes.JsonContentType)]
        [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [Produces(ContentTypes.JsonContentType)]
        public async Task<IActionResult> AddComment([FromBody] CommentAddDto addDto)
        {
            if(!ModelState.IsValid)
                return await Task.FromResult(BadRequest(ModelState));

            int userId = AuthHelpers.GetUserId(User, UserIdClaimType) ??
                throw new Exception("User authenticated, but cannot resolve user id from user claims");

            PostComment comment = Mapper.Map<PostComment>(addDto);
            comment.UserId = userId;
            comment.RepliedPostId = addDto.RepliedPostId.Value;
            comment.RepliedUserId = addDto.RepliededUserId ?? null;
            comment.PostedTime = DateTime.Now;

            CommentRepo.Add(comment);
            await SaveChangesOrThrowIfFailed();
            CommentDto commentDto = Mapper.Map<CommentDto>(comment);
            return Created(string.Empty, commentDto);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            PostComment comment = await CommentRepo.QueryByIdAsync(commentId);

            if(comment == null)
                return NotFound();

            bool accessable = (await AuthServ.AuthorizeAsync(
                User, null,
                new ResourceOwnerAuthorizationRequirement<int>(
                    comment.UserId))).Succeeded;

            if(!accessable)
                accessable = (await AuthServ.AuthorizeAsync(
                    User, null,
                    new RolesAuthorizationRequirement(
                        new string[] { RoleConstants.SuperAdmin }))).Succeeded;

            if(!accessable)
                return Forbid();

            CommentRepo.Delete(comment);

            return await SaveChangesOrThrowIfFailed();
        }

        [HttpPut("{commentId}")]
        [Consumes(ContentTypes.Text)]
        public async Task<IActionResult> UpdateComment([FromRoute]int commentId, [FromBody] string content)
        {
            PostComment comment = await CommentRepo.QueryByIdAsync(commentId);
            if(comment == null)
                return NotFound();

            bool accessable = (await AuthServ.AuthorizeAsync(
                User, null,
                new RolesAuthorizationRequirement(new string[] { RoleConstants.SuperAdmin })))
                .Succeeded;
            if(!accessable)
                accessable = (await AuthServ.AuthorizeAsync(
                    User, null, new ResourceOwnerAuthorizationRequirement<int>(comment.UserId))).Succeeded;
            if(!accessable)
                return Forbid();

            if(string.IsNullOrEmpty(content))
                return BadRequest();

            comment.Comment = content;
            await SaveChangesOrThrowIfFailed();
            return NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> GetComments(
            [FromQuery] CommentQueryParameters queryParams)
        {
            bool accessable = true;
            // 普通用户查询, 至少指定除UserId一个参数
            if(queryParams.UserId.HasValue || !queryParams.UserId.HasValue ||
                !queryParams.RepliedPostId.HasValue ||
                !queryParams.RepliedCommentId.HasValue)
            {
                accessable = false;
                if(queryParams.UserId.HasValue)
                    // 普通用户可以查询自己的所有评论
                    accessable = (await AuthServ.AuthorizeAsync(
                        User, null,
                        new ResourceOwnerAuthorizationRequirement<int>(
                            queryParams.UserId.Value))).Succeeded;
                if(!accessable)
                    accessable = (await AuthServ.AuthorizeAsync(
                        User, null,
                        new RolesAuthorizationRequirement(new string[] { RoleConstants.SuperAdmin })))
                        .Succeeded;
                if(!accessable)
                    return Forbid();
            }

            IQueryable<PostComment> query = CommentRepo.Query()
                .Include(x => x.User)
                .Include(x => x.RepliedPost)
                .Include(x => x.RepliedUser);
            if(queryParams.RepliedCommentId.HasValue)
                query = query.Where(
                    x => x.RepliedCommentId != null && x.RepliedCommentId == queryParams.RepliedCommentId);
            if(queryParams.UserId.HasValue)
                query = query.Where(x => x.UserId == queryParams.UserId);
            if(queryParams.RepliedPostId.HasValue)
                query = query.Where(x => x.RepliedPostId == queryParams.RepliedPostId);
            if(queryParams.RepliedUserId.HasValue)
                query = query.Where(x => x.RepliedUserId == queryParams.RepliedUserId);
            query = query.OrderByDescending(x => x.PostedTime);

            PaginationList<PostComment> comments = await query.PagingAsync(queryParams);

            this.AddPaginationHeader(comments);
            IEnumerable<CommentDto> commentsDto = Mapper.Map<IEnumerable<CommentDto>>(comments.AsEnumerable());
            await Task.Run(() =>
            {   // 计算回复数量
                commentsDto.Select(x =>
                {
                    x.RepliedCount = CommentRepo.Query()
                    .Count(c =>
                    c.RepliedCommentId == x.Id);
                    return x;
                }).ToList();
            });
            IEnumerable<ExpandoObject> shapedComments = commentsDto.ToDynamicObject(queryParams.Fields);

            return Ok(shapedComments);
        }
    }
}
