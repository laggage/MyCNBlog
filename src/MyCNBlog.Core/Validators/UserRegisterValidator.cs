using FluentValidation;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Core.Validators
{
    public class UserRegisterValidator : AbstractValidator<UserRegisterDto>
    {
        public UserRegisterValidator()
        {
            RuleFor(x => x.Avatar)
                .Must(x => x == null || x.Length < 1024*1024*2);
            RuleFor(x => x.UserName)
                .MaximumLength(32)
                .WithName("用户名")
                .NotEmpty().NotNull();
            RuleFor(x => x.SecurePassword)
                .NotEmpty()
                .WithName("密码")
                .NotNull();
        }
    }

    public class UserLoginValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginValidator()
        {
            RuleFor(x => x.UserName)
                .MaximumLength(32)
                .WithName("用户名")
                .NotEmpty().NotNull();
            RuleFor(x => x.SecurePassword)
                .NotEmpty()
                .WithName("密码")
                .NotNull();
        }
    }
}
