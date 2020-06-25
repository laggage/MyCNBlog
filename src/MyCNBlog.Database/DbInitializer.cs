using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MyCNBlog.Core;
using MyCNBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MyCNBlog.Database
{
    public class DbInitializer
    {
        private readonly MyCNBlogDbContext _dbContext;
        private readonly UserManager<BlogUser> _userManager;
        private readonly ILogger _logger;

        public DbInitializer(MyCNBlogDbContext dbContext, 
            ILoggerFactory loggerFacotry,
            UserManager<BlogUser> userManager)
        {
            _dbContext=dbContext;
            _userManager=userManager;
            _logger = loggerFacotry.CreateLogger<DbInitializer>();
        }

        public void InitDatabase()
        {
            using (IDbContextTransaction transaction = _dbContext.Database.BeginTransaction())
            {
                _logger.LogInformation("Begin a transaction to seed database");
                EnsureRoleInitialized();
                //transaction.Commit();
                EnsureUserInitialized();
                transaction.Commit();
                _logger.LogInformation("Finished a transaction to seed database");
            }
        }

        private void EnsureRoleInitialized()
        {
            FieldInfo[] fields = typeof(RoleConstants).GetFields(BindingFlags.Static|BindingFlags.Public|BindingFlags.GetField);
            foreach(FieldInfo propertyInfo in fields)
            {
                string roleName = propertyInfo.GetRawConstantValue() as string;

                _logger.LogInformation($"Seeding Role: {roleName}");

                IdentityRole<int> existRole = _dbContext.Roles.FirstOrDefault(x => x.Name == roleName);
                var role = new IdentityRole<int>(roleName) { NormalizedName = roleName.ToUpper() } ;
                if (existRole != null)
                    _dbContext.Roles.Remove(existRole);
                _dbContext.Roles.Add(role);
            }
        }

        private void EnsureUserInitialized()
        {
            var users = new Dictionary<BlogUser, Tuple<string, string>>
            {
                { new BlogUser("root"), new Tuple<string, string>("123456789", RoleConstants.SuperAdmin) },
                { new BlogUser("老王"), new Tuple<string, string>("123456789", RoleConstants.Bloger) },
                { new BlogUser("小张"), new Tuple<string, string>("123456789", RoleConstants.Visitor) },
            };

            foreach(KeyValuePair<BlogUser, Tuple<string, string>> user in users)
            {
                BlogUser u = _userManager.FindByNameAsync(
                    user.Key.NormalizedUserName).Result;
                _logger.LogInformation($"Seeding user: {u}");
                IdentityResult result;
                if (u != null)
                {
                    UpdateUser(u, user.Key);
                    result = _userManager.UpdateAsync(u).Result;
                    if(!result.Succeeded)
                        _logger.LogError($"Failed to update user {u}");
                }
                else
                {
                    u = user.Key;
                    result = _userManager.CreateAsync(u, user.Value.Item1).Result;
                    if (!result.Succeeded)
                        _logger.LogError($"Failed to create user {u}");
                }

                if(result.Succeeded)
                {
                    result = _userManager.AddToRoleAsync(u, user.Value.Item2).Result;
                    if (!result.Succeeded)
                        _logger.LogError($"Failed to creat add user {u} to role ${user.Value.Item2}");
                }
            }
        }

        private void UpdateUser(BlogUser existUser, BlogUser newUser)
        {
            existUser.UserName = newUser.UserName;
            existUser.Sex = newUser.Sex;
            existUser.Birth = newUser.Birth;
            existUser.Email = newUser.Email;
            existUser.PhoneNumber = newUser.PhoneNumber;
        }
        
    }
}
