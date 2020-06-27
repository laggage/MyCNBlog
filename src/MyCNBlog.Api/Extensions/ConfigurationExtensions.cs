using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MyCNBlog.Api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static int GetUploadImageFileMaxSize(this IConfiguration config)
        {
            return config.GetValue<int>("StaticFile:ImageMaxSize")*1024*1024;
        }

        public static string GetStaticFileRootPath(this IConfiguration config)
        {
            string path = config.GetValue<string>("StaticFile:StaticFileRootPath");
            path = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
            return string.IsNullOrEmpty(path) ? AppContext.BaseDirectory + "wwwroot" : path;
        }

        public static string GetStaticFileRequestPath(this IConfiguration config)
        {
            string path = config.GetValue<string>("StaticFile:StaticFileRequestPath");
            return string.IsNullOrEmpty(path) ? "/static/files" : path;
        }

        /// <summary>
        /// 获取默认的用户头像的静态文件路径
        /// </summary>
        /// <param name="config"></param>
        /// <returns>完整的物理路径</returns>
        public static string GetDefaultUserAvatarFilePath(this IConfiguration config)
        {
            return Path.Combine(
                config.GetStaticFileRootPath(),
                config.GetValue<string>("StaticFile:DefaultUserAvatarFilePath"));
        }
    }
}
