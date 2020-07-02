using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCNBlog.Api.Controllers;
using MyCNBlog.Core;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;
using MyCNBlog.Services.Authorization;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;

namespace MyCNBlog.Api.Extensions
{
    public static class ServiceConfigExtensions
    {
        public static void AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddDbContext<MyCNBlogDbContext>(options =>
            {
                options.UseMySql(GetConnectionString("mysql", configuration));
                if(env.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
                else
                {
                    // 生产环境不记录ef执行的sql
                    options.UseLoggerFactory(LoggerFactory.Create(builder =>
                    {
                        builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name
                            && level == LogLevel.Information);
                    }));
                }
            });
        }

        public static void AddAuthServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // See github issue https://github.com/GestionSystemesTelecom/fake-authentication-jwtbearer/issues/4
            // 改变HttpContext的 NameClaimType 和 RoleClaimType
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddIdentity<BlogUser, IdentityRole<int>>(option =>
            {
                IConfigurationSection section = configuration.GetSection("Auth:PasswordOptions");
                section.Bind(option.Password);
                option.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
                option.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
                option.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
                option.User.AllowedUserNameCharacters = string.Empty;  // 允许中文字符用户名
            })
                .AddEntityFrameworkStores<MyCNBlogDbContext>()
                .AddDefaultTokenProviders();
            
            services.AddAuthentication(options =>
            {
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = JwtHelpers.GetIssuer(configuration),
                        ValidAudience = JwtHelpers.GetAudience(configuration),
                        IssuerSigningKey = JwtHelpers.GetSigningKey(configuration),
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireAudience = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                    };
                });

            // 基于角色的授权策略, 管理员有最高权限, 其次只有 Blogger 可以发表博文
            services.AddAuthorization(c =>
            {
                c.AddPolicy(AuthorizationPolicies.AdminOnlyPolicy, b => b.RequireRole(RoleConstants.SuperAdmin));
                c.AddPolicy(AuthorizationPolicies.BlogerPolicy, b => b.RequireRole(RoleConstants.Bloger, RoleConstants.SuperAdmin));
                c.AddPolicy(
                    AuthorizationPolicies.PostAuthorPolicy,
                    c => {
                        c.Requirements.Add(new SameAuthorRequirement(RoleConstants.SuperAdmin));
                    });
                c.AddPolicy(
                    AuthorizationPolicies.PostsQueryPolicy,
                    c => c.Requirements.Add(new PostsQueryAuthorizationRequirment()));
                c.AddPolicy(
                    AuthorizationPolicies.OpenBlogPolicy,
                    c => c.RequireRole(RoleConstants.SuperAdmin));
            });
            services.AddSingleton<IAuthorizationHandler, PostAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, PostsQueryAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogPartiallyUpdateAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ResourceOwnerAuthorizationHandler<int>>();
        }

        public static string GetConnectionString(string database, IConfiguration configuration)
        {
            string path = configuration.GetValue<string>($"Database:ConnectionStringFilesPath:{database.ToLower()}");
            return File.ReadAllText(path);
        }

        public static void AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = AppConstants.ApiName,
                        Version = AppConstants.ApiVersion,
                        Description = "基于AspNetCore和Angular8+前后的分离的博客系统",
                        Contact = new OpenApiContact
                        {
                            Email = AppConstants.Email,
                            Name = AppConstants.Author,
                            Url = new Uri(AppConstants.AuthorBlogsUrl),
                        }
                    });

                // TODO: 3.0 swagger 配置jwt的写法和2.x不同, 后面博文记录下
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.AddSecurityDefinition(
                    "oauth2",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = "Jwt授权, 令牌将通过请求头传输",
                        Type = SecuritySchemeType.ApiKey,
                    });
                var assemblies = new List<Assembly>
                {
                    typeof(AuthController).Assembly,
                    typeof(Entity).Assembly
                };
                foreach(Assembly assembly in assemblies)
                {
                    string xmlFile = $"{assembly.GetName().Name}.xml";
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath, true);
                }
            });
        }
    }
}
