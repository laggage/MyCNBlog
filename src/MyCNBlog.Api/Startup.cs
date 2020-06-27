using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyCNBlog.Api.Extensions;
using MyCNBlog.Core;
using MyCNBlog.Core.Validators;
using MyCNBlog.Database;
using MyCNBlog.Repositories;
using MyCNBlog.Services.Extensions;
using Newtonsoft.Json;

namespace MyCNBlog.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _enviroment;
        private const string _corsSectionKey = "AppCors";
        private const string _policyNameKey = "AppCors:Policy";

        public Startup(IConfiguration configuration,
            IWebHostEnvironment enviroment)
        {
            _configuration = configuration;
            _enviroment = enviroment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabase(_configuration, _enviroment);
            services.AddAuthServices(_configuration);
            services.AddAutoMapper(typeof(Map));
            services.AddTextCryptoService(_configuration);
            services.AddValidators();
            services.AddTypeSerivce();
            services.AddSwaggerServices();
            services.AddRepositories<MyCNBlogDbContext>();
            services.AddPostFileServices();
            services.AddControllers(o =>
            {
                o.ReturnHttpNotAcceptable = true;
                o.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
                o.InputFormatters.Insert(0, new StringInputTextFormatter());
                o.RespectBrowserAcceptHeader = true;
            })
                .AddFluentValidation()
                .AddNewtonsoftJson(c =>
                {
                    c.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    c.SerializerSettings.ContractResolver = JsonConvertSettings.CamelCasePropertyName.ContractResolver;
                });
            services.AddSortServices();


            services.AddCors(o =>
            {
                IConfigurationSection sec = _configuration.GetSection(_corsSectionKey);
                var p = new CorsPolicy();
                sec.Bind(p);
                o.AddPolicy(_configuration.GetValue<string>(_policyNameKey), p);
            });
        }



        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            //app.UseExceptionHandler("/api/error");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{AppConstants.ApiName} {AppConstants.ApiVersion}");
                c.RoutePrefix = string.Empty;
            });

            app.UseStaticFiles(
                new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(_configuration.GetStaticFileRootPath()),
                    RequestPath = _configuration.GetStaticFileRequestPath(),
                });

            app.UseRouting();
            app.UseCors(_configuration.GetValue<string>(_policyNameKey));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            ServiceProvider builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }
    }
}
