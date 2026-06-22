using FluentValidation;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.MangeFriendship;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.MangeFriendship;

public class UnfriendCommandValidator : AbstractValidator<UnfriendCommand>
{
    public UnfriendCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user ID is required.")
            .NotEqual(x => x.CurrentUserId).WithMessage("You cannot unfriend yourself.");
    }
}
