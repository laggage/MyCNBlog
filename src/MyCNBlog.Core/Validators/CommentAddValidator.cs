using FluentValidation;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Core.Validators
{
    public class CommentAddValidator:AbstractValidator<CommentAddDto>
    {
        public CommentAddValidator()
        {
            RuleFor(x => x.Comment)
                .NotEmpty()
                .NotNull()
                .WithName("评论内容")
                .MaximumLength(2048);  
            RuleFor(x => x.RepliedPostId)
                .Must(x => x.HasValue)
                .WithName("被回复博文Id");
            RuleFor(x => x.RepliedCommentId)
                .Custom((id, ctx) =>
                {
                    var dto = ctx.InstanceToValidate as CommentAddDto;
                    if(dto.RepliededUserId.HasValue && !dto.RepliedCommentId.HasValue)
                        ctx.AddFailure(
                            $"{ctx.PropertyName} must set a non-null value due to the property {nameof(CommentAddDto.RepliededUserId)} has been seted");
                });
        }
    }
}
