using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using MyCNBlog.Services.Abstractions;

namespace MyCNBlog.Services
{
    public class PostFileService : IPostFileService
    {
        private readonly PhysicalFileProvider _fileProvider;

        private static readonly string _baseDirectory;

        static PostFileService()
        {
            _baseDirectory = Path.Combine(AppContext.BaseDirectory);
        }

        public PostFileService() 
        {
            _fileProvider = new PhysicalFileProvider(_baseDirectory);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return _fileProvider.GetFileInfo(subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _fileProvider.GetDirectoryContents(subpath);
        }

        /// <summary>
        /// 没有实现的需求
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IChangeToken Watch(string filter)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> SavePostToFileAsync(string postContents, string postTitle, int userId)
        {
            string fileName = GenerateFileName(postTitle, userId);
            string dir = GetOrGenerateDirectory(userId);
            string subpath = Path.Combine(dir, fileName);
            string fullPath = Path.Combine(_fileProvider.Root, subpath);
            await File.WriteAllTextAsync(fullPath, postContents);

            return subpath;
        }

        public async Task SavePostToFileAsync(string subPath, string postContents)
        {
            IFileInfo fileInfo = _fileProvider.GetFileInfo(subPath);
            await File.WriteAllTextAsync(fileInfo.PhysicalPath, postContents);
        }

        public Task DeletePostFileAsync(string subpath)
        {
            return Task.Run(() =>
            {
                IFileInfo file = _fileProvider.GetFileInfo(subpath);
                if(!file.IsDirectory)
                {
                    File.Delete(file.PhysicalPath);
                }
            });
        }
        
        
        private static string GenerateFileName(string postTitle, int userId)
        {
            // 加GUID防止重复文件
            return $"{userId}-{postTitle}-{Guid.NewGuid()}.post";
        }

        private string GetOrGenerateDirectory(int userId)
        {
            string dir = Path.Combine("MyCNBlog", userId.ToString(), "posts");
            IFileInfo directoryInfo = _fileProvider.GetFileInfo(dir);
            if(!directoryInfo.Exists)
                Directory.CreateDirectory(directoryInfo.PhysicalPath);

            return dir;
        }

        public string LoadPostContent(string subpath)
        {
            IFileInfo fileInfo = _fileProvider.GetFileInfo(subpath);
            return File.ReadAllText(fileInfo.PhysicalPath);
        }

        public Stream GetPostContentStream(string path)
        {
            return File.OpenRead(_fileProvider.GetFileInfo(path).PhysicalPath);
        }
    }
}
