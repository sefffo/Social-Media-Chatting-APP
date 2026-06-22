using FluentValidation;
using Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.UnblockUser;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.UnblockUser;

public class UnblockUserCommandValidator : AbstractValidator<UnblockUserCommand>
{
    public UnblockUserCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user ID is required.")
            .NotEqual(x => x.CurrentUserId).WithMessage("You cannot unblock yourself.");
    }
}
