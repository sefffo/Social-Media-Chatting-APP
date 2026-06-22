using FluentValidation;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.CancelFriendRequest;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.CancelFriendRequest;

public class CancelFriendRequestCommandValidator : AbstractValidator<CancelFriendRequestCommand>
{
    public CancelFriendRequestCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.");

        RuleFor(x => x.FriendshipId)
            .NotEmpty().WithMessage("Friendship ID is required.");
    }
}
