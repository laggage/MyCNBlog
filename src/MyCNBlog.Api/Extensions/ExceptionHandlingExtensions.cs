using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyCNBlog.Core;

namespace MyCNBlog.Api.Extensions
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        public static void UseAppExceptionHandler(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = ContentTypes.Text;
                    Exception ex = context.Features.Get<IExceptionHandlerFeature>().Error;
                    if (!(ex is null))
                    {   
                        string exMsgs = await FlattenExceptionMsgAsync(ex);
                        ILogger logger = loggerFactory.CreateLogger("Global exception handler");
                        logger.LogError(
                            $"Encounter error while handling request. ExceptionMsgs: {exMsgs}");
                    }

                    await context.Response.WriteAsync(
                        "Sorroy we encounter some errors that cause us unable to handle you request.");
                });
            });
        }

        private static Task<string> FlattenExceptionMsgAsync(Exception ex)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();
                Exception t = ex;
                while(!(t is null))
                {
                    sb.AppendLine($"ExceptoinType-{ex.GetType().Name}: {ex.Message}");
                    t = ex.InnerException;
                };
                return sb.ToString();
            });
        }
    }
}
