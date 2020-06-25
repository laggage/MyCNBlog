using Microsoft.Extensions.DependencyInjection;
using MyCNBlog.Core.Models.Dtos;
using FluentValidation;

namespace MyCNBlog.Core.Validators
{
    public static class ValidatorExtensions
    {
        public static void AddValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<UserLoginDto>, UserLoginValidator>();
            services.AddTransient<IValidator<UserRegisterDto>, UserRegisterValidator>();
        }
    }
}
