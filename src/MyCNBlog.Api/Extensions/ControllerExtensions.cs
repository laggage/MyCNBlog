using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using MyCNBlog.Core;
using MyCNBlog.Core.Abstractions;
using Newtonsoft.Json;

namespace MyCNBlog.Api.Extensions
{
    public static class ControllerExtensions
    {
        public static async Task ValidateModelAsync<T>(
            this ControllerBase controller, IValidator<T> validator, T model)
        {
            ValidationResult result = await validator.ValidateAsync(model);
            result.AddToModelState(controller.ModelState, typeof(T).Name);
        }

        public static IActionResult FieldsNotExist(this ControllerBase controller, string fields)
        {
            return controller.BadRequest($"Some of fields {{{$"{fields} "}}}not exist.");
        }

        /// <summary>
        /// 将分页信息添加到Http头部
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="controller"></param>
        /// <param name="source">分页数据源</param>
        public static void AddPaginationHeader<TEntity>(
            this ControllerBase controller, PaginationList<TEntity> source)
        {
            var meta = new
            {
                source.PageIndex,
                source.PageSize,
                source.PageCount,
                source.TotalItemsCount
            };

            controller.HttpContext.Response.Headers.Add(
                HttpConstants.PaginationHeaderKey,
                JsonConvert.SerializeObject(
                    meta, 
                    JsonConvertSettings.CamelCasePropertyName));
        }
    }
}
