using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace MyCNBlog.Services.Abstractions
{
    /// <summary>
    /// 使用装饰器模式包装 IFileProvider
    /// </summary>
    public interface IPostFileService : IFileProvider
    {
        Task DeletePostFileAsync(string subpath);
        Task<string> SavePostToFileAsync(string postContents, string postTitle, int userId);
        Task SavePostToFileAsync(string subPath, string postContents);
        string LoadPostContent(string subpath);
        Stream GetPostContentStream(string subpath);
    }
}
