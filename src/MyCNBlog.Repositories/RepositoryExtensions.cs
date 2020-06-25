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
            //using(IServiceScope scope = services.BuildServiceProvider().CreateScope())
            //{
            //    DbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            //    services.AddScoped<IUnitOfWork>(sp =>
            //    {
            //        IServiceScope s = scope;
            //        context = s.ServiceProvider.GetRequiredService<TDbContext>();

            //        return new UnitOfWork(context);
            //    });
            //    AddRepositoriesFromCurAssembly<TDbContext>(services, scope);
            //}
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            AddRepositoriesFromCurAssembly(services);
        }

        private static void AddRepositoriesFromCurAssembly(
            IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Type repoBaseInterface = typeof(IRepository<>);
            var repoImpls = assembly.GetTypes()
                .Where(x => !x.IsAbstract && !x.IsInterface &&
                x.GetInterfaces().Any(i => i.Name == repoBaseInterface.Name)).ToList();

            foreach(Type type in repoImpls)
            {
                Type @interface = type.GetInterfaces()
                    .FirstOrDefault(
                    x => x.Name.Substring(0, x.Name.Length - 1) !=
                    repoBaseInterface.Name.Substring(0, repoBaseInterface.Name.Length - 1));
                services.Add(
                    ServiceDescriptor.Scoped(@interface, type));
            }
        }
    }
}
