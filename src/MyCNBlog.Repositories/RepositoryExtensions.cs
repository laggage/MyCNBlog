using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories
{
    public static class RepositoryExtensions
    {
        public static void AddRepositories<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
        {
            // 要保证Repository和Unitofwork得到相同scope下的 DbContext
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            AddRepositoriesFromCurAssembly(services);
        }

        /// <summary>
        /// 采用反射的方式注册 Reposigory
        /// </summary>
        /// <param name="services"></param>
        private static void AddRepositoriesFromCurAssembly(
            IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            Type baseInterface = typeof(IRepository<>);
            Type baseType = typeof(AppRepository<>);
            var repoImpls = assembly.GetTypes()
                .Where(x => !x.IsAbstract && !x.IsInterface &&
                x.BaseType.Name == baseType.Name).ToList();

            foreach(Type type in repoImpls)
            {
                Type @interface = type.GetInterfaces()
                    .FirstOrDefault(
                    x => x.Name.Substring(0, x.Name.Length - 1) !=
                    baseInterface.Name.Substring(
                        0, baseInterface.Name.Length - 1));
                services.Add(
                    ServiceDescriptor.Scoped(@interface, type));
            }
        }
    }
}
