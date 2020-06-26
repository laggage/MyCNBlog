using FluentValidation;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Core.Validators
{
    public class PostValidator:AbstractValidator<PostAddDto>
    {
        public PostValidator()
        {
            RuleFor(p => p.Title)
                .NotEmpty()
                .NotNull()
                .MaximumLength(128);

            RuleFor(p => p.Description)
                .NotNull()
                .NotEmpty()
                .MaximumLength(512);
            RuleFor(p => p.Content)
                .NotNull()
                .NotEmpty();
        }
    }
}
