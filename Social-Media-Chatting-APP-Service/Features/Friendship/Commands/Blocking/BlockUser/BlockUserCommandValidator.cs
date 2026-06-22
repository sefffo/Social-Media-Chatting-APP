using FluentValidation;
using Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.BlockUser;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.BlockUser;

public class BlockUserCommandValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.");

        RuleFor(x => x.TargetedUserId)
            .NotEmpty().WithMessage("Target user ID is required.")
            .NotEqual(x => x.CurrentUserId).WithMessage("You cannot block yourself.");
    }
}
