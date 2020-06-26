using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MyCNBlog.Core;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Repositories.Abstractions;
using MyCNBlog.Services.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using MyCNBlog.Services.ResourceShaping;
using System.Dynamic;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Api.Extensions;
using System.Collections.Generic;
using MyCNBlog.Services.Sort;

namespace MyCNBlog.Api.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : AppBaseController
    {
        private readonly IPostRepository _postRepo;
        private readonly IPostFileService _postFileServ;
        private readonly ITagRepository _tagRepo;
        private readonly IBlogUserRepository _userRepo;
        private readonly IAuthorizationService _authServ;
        private readonly IPropertyMappingContainer _propMappingContainer;

        public PostController(IUnitOfWork unitOfWork,
            IPostRepository postRepo,
            IMapper mapper,
            UserManager<BlogUser> userManager,
            IPostFileService postFileServ,
            ITagRepository tagRepo,
            IBlogUserRepository userRepo,
            IAuthorizationService authServ,
            IOptions<IdentityOptions> identityOptions,
            ITypeService typeService, 
            IPropertyMappingContainer propMappingContainer)
            : base(unitOfWork, mapper, userManager, identityOptions, typeService)
        {
            _postRepo = postRepo ?? throw new ArgumentNullException(nameof(postRepo));
            _postFileServ = postFileServ ?? throw new ArgumentNullException(nameof(postFileServ));
            _tagRepo = tagRepo ?? throw new ArgumentNullException(nameof(tagRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _authServ = authServ ?? throw new ArgumentNullException(nameof(authServ));
            _propMappingContainer = propMappingContainer;
        }

        /// <summary>
        /// 博文查询接口; 普通用户登录情况下, 允许查询自己已删除的博文和私有博文
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous()]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
        [Produces(ContentTypes.JsonContentType)]
        public async Task<IActionResult> GetPosts(
            [FromQuery] PostQueryParameters queryParams)
        {
            if(!TypeService.HasProperties<PostDto>(queryParams.Fields))
                return await FieldsNotExist();

            // 对查询参数鉴权
            bool accessable = (await _authServ.AuthorizeAsync(
                User,
                queryParams,
                AuthorizationPolicies.PostsQueryPolicy)).Succeeded;

            if(!accessable)
                return Forbid();

            PaginationList<Post> posts = await _postRepo.QueryAsync(queryParams);
            IQueryable<Post> sortedPosts = await posts.AsQueryable().ApplySortAsync(
                queryParams.OrderBy,
                _propMappingContainer.Resolve<PostDto, Post>());

            IEnumerable<PostDto> postsDto = Mapper.Map<IEnumerable<PostDto>>(sortedPosts);

            SetPostDtoContentUrl(postsDto);

            IEnumerable<ExpandoObject> shapedPosts = postsDto.ToDynamicObject(queryParams.Fields);

            this.AddPaginationHeader(posts);

            return Ok(shapedPosts);
        }

        private void SetPostDtoContentUrl(PostDto postDto)
        {
            postDto.PostContentUrl = Url.Link(
                    nameof(GetPostContent),
                    new { id = postDto.Id });
        }

        private void SetPostDtoContentUrl(IEnumerable<PostDto> postsDto)
        {
            foreach(PostDto postDto in postsDto)
                postDto.PostContentUrl = Url.Link(
                        nameof(GetPostContent),
                        new { id = postDto.Id });
        }

        /// <summary>
        /// 根据指定的Id, 查询博文
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetPostById")]
        public async Task<IActionResult> GetPost([FromRoute] int id,
            [FromQuery] string fields)
        {
            if(!TypeService.HasProperties<PostDto>(fields))
                return await FieldsNotExist();

            Post post = await _postRepo.QueryByIdAsync(id);

            if(post is null)
                return NotFound();

            PostDto postDto = Mapper.Map<PostDto>(post);

            SetPostDtoContentUrl(postDto);

            ExpandoObject shapedPost = postDto.ToDynamicObject(fields);

            return Ok(shapedPost);
        }

        /// <summary>
        /// 获取博文内容
        /// </summary>
        /// <param name="id">博文id</param>
        /// <returns></returns>
        [HttpGet("{id}/content", Name = nameof(GetPostContent))]
        public async Task<IActionResult> GetPostContent(int id)
        {
            Post post = await _postRepo.QueryByIdAsync(id);

            if(post == null)
                return NotFound();

            // TODO: 封装授权 
            // _authServ.AuthorizeAsync(User, post, new PrivatePostAuthorizationRequirement())
            // _authServ.AuthorizeAsync(User, post, new DeletePostAuthorizationRequirement())
            if(!post.IsPublic || post.IsDeleted)
            {
                bool isAuthorOrAdmin = (await _authServ.AuthorizeAsync(
                User,
                post,
                AuthorizationPolicies.PostAuthorPolicy)).Succeeded;
                if(!isAuthorOrAdmin)
                    return Forbid();
            }

            return File(
                _postFileServ.GetPostContentStream(post.Path),
                ContentTypes.Text);
        }

        /// <summary>
        /// 创建博文; Permission: <see cref="AuthorizationPolicies.BlogerPolicy"/>;
        /// 用户想要开通博客, 需要管理员授权
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)]
        [Consumes(ContentTypes.JsonContentType)]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        [Produces(ContentTypes.JsonContentType)]
        public async Task<IActionResult> AddPost([FromBody] PostAddDto dto)
        {
            if(!ModelState.IsValid)
                return await Task.FromResult(BadRequest(ModelState));

            Post post = Mapper.Map<Post>(dto);
            BlogUser user = await _userRepo.QueryByIdAsync(int.Parse(UserManager.GetUserId(User)));

            post.BlogId = user.Blog.Id;
            post.AuthorId = int.Parse(UserManager.GetUserId(User));

            // 处理文件和处理标签可以并发
            Task<string> fileTask = _postFileServ.SavePostToFileAsync(
                    dto.Content, dto.Title, post.AuthorId);
            Task tagTask = HandleTagsAsync(dto.Tags, post);

            string postFilePath = await fileTask;
            await tagTask;
            post.Path = postFilePath;

            _postRepo.Add(post);
            bool result = await UnitOfWork.SaveChangesAsync();

            if(!result)
                throw new Exception($"Create post: {post} failed while write data to database");

            PostDto postDto = Mapper.Map<PostDto>(post);
            postDto.Blog = null;
            return CreatedAtRoute("GetPostById", new { postDto.Id }, postDto);
        }

        /// <summary>
        /// 因为Post和Tag是多对多的关系, 所以这里单独处理下 Tag 的添加
        /// 主要是防止重复的Tag
        /// </summary>
        /// <param name="tagsDto">传进来的Tags</param>
        /// <param name="post">将要被添加到数据库的Post</param>
        /// <returns></returns>
        private async Task HandleTagsAsync(IEnumerable<TagDto> tagsDto, Post post)
        {
            tagsDto = tagsDto.Distinct(new TagDtoComparer());
            Tag[] tags = Mapper.Map<Tag[]>(tagsDto);
            if(tags == null)
                return ;
            Tag existTag;
            // 查询数据库中是否有重名的Tag, 如果有, 则用数据库中的Tag替换之
            for(int i = 0; i < tags.Length; i++)
            {
                Tag tag = tags[i];
                existTag = (await _tagRepo.QueryAsync(
                    t => t.Name.ToLower() == tag.Name.ToLower())).FirstOrDefault();
                if(existTag != null)
                    tags[i] = existTag;
            }
            post.PostTags.Clear();
            post.AddTags(tags);
        }

        /// <summary>
        /// 删除博文
        /// </summary>
        /// <param name="id"></param>
        /// <param name="softDelete">是否软删除, 这个选项只对管理员有效, 普通用户只能软删除</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)] // 这里加这个Authorize先过滤一部分不符和要求的角色, 这样不用每次都去查询数据库
        public async Task<IActionResult> DeletePost(
            [FromRoute] int id, [FromQuery] bool softDelete = true)
        {
            Post post = await _postRepo.QueryByIdAsync(id);

            if(post == null)
                return NotFound();
            // 这里手动鉴权
            AuthorizationResult authRes = await _authServ.AuthorizeAsync(
                User, post, AuthorizationPolicies.PostAuthorPolicy);

            if(!authRes.Succeeded)
                return Forbid(JwtBearerDefaults.AuthenticationScheme);
            // 超级管理员可以有权限硬删除, 普通用户只能软删除
            if(User.IsInRole(RoleConstants.SuperAdmin))
            {
                if(!softDelete)
                    await _postFileServ.DeletePostFileAsync(post.Path);
            }
            else
                softDelete = true;

            _postRepo.Delete(post, softDelete);

            await SaveChangesAndThrowIfFailed();

            return NoContent();
        }


        /// <summary>
        /// 更新博文 (Auth: <see cref="AuthorizationPolicies.BlogerPolicy   "/>, <see cref="AuthorizationPolicies.PostAuthorPolicy"/>)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [Authorize(Policy = AuthorizationPolicies.BlogerPolicy)]    // 这里加这个Authorize先过滤一部分不符和要求的角色, 这样不用每次都去查询数据库
        public async Task<IActionResult> PatchPost(
            [FromRoute] int id, [FromBody] JsonPatchDocument<PostAddDto> patchDoc)
        {
            Post post = await _postRepo.QueryByIdAsync(id);
            if(post == null)
                return BadRequest();

            await _authServ.AuthorizeAsync(
                User, post, AuthorizationPolicies.PostAuthorPolicy);

            PostAddDto dto = Mapper.Map<PostAddDto>(post);
            patchDoc.ApplyTo(dto);
            Mapper.Map(dto, post);

            await _postFileServ.SavePostToFileAsync(post.Path, dto.Content);

            await HandleTagsAsync(dto.Tags, post);

            _postRepo.Update(post);

            await SaveChangesAndThrowIfFailed();

            return NoContent();
        }
    }
}
