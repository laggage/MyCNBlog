﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyCNBlog.Core.Models;
using MyCNBlog.Repositories.Abstraction;
using MyCNBlog.Services;
using MyCNBlog.Services.ResourceShaping;

namespace MyCNBlog.Api.Controllers
{
    public class AppBaseController : ControllerBase
    {
        public AppBaseController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<BlogUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            ITypeService typeService)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            TypeService = typeService ?? throw new ArgumentNullException(nameof(typeService));
            IdentityOptions = identityOptions.Value ?? throw new ArgumentNullException(nameof(identityOptions));
        }

        protected IUnitOfWork UnitOfWork { get; }
        protected IMapper Mapper { get; }
        protected UserManager<BlogUser> UserManager { get; }
        protected ITypeService TypeService { get; }
        protected IdentityOptions IdentityOptions { get; }

        protected string RoleClaimType => IdentityOptions.ClaimsIdentity.RoleClaimType;
        protected string UserNameClaimType => IdentityOptions.ClaimsIdentity.UserNameClaimType; 
        protected string UserIdClaimType => IdentityOptions.ClaimsIdentity.UserIdClaimType;

        protected async Task<NoContentResult> SaveChangesOrThrowIfFailed(string failedMessage = null)
        {
            if(!(await UnitOfWork.SaveChangesAsync()))
                throw new Exception(failedMessage ?? "Operation failed while write database.");

            return NoContent();
        }

        protected Task<BadRequestObjectResult> FieldsNotExist()
        {
            return Task.FromResult(BadRequest("Fields not exist."));
        }

        protected virtual string GetUserAvatorUrl(BlogUser user)
        {
            return GetUserAvatorUrl(user.Id);
        }

        protected virtual string GetUserAvatorUrl(int userId)
        {
            return Url.Link(nameof(BlogUserController.GetUserAvatar), new { userId });
        }
    }
}
