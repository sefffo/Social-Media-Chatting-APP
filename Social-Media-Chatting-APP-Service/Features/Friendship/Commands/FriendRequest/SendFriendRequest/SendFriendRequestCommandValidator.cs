using FluentValidation;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.SendFriendRequest;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.SendFriendRequest;

public class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
{
    public SendFriendRequestCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.");

        RuleFor(x => x.AddresseeId)
            .NotEmpty().WithMessage("Target user ID is required.")
            .NotEqual(x => x.CurrentUserId).WithMessage("You cannot send a friend request to yourself.");
    }
}
