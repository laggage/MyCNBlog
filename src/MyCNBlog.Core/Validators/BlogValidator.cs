using FluentValidation;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Core.Validators
{
    public class BlogValidator:AbstractValidator<BlogAddDto>
    {
        public BlogValidator()
        {
        }
    }
}
