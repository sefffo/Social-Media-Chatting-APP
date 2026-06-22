using FluentValidation;
using Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.RespondToFriendRequest;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.RespondToFriendRequest;

public class RespondToFriendRequestCommandValidator : AbstractValidator<RespondToFriendRequestCommand>
{
    private static readonly string[] ValidDecisions = { "Accepted", "Rejected" };

    public RespondToFriendRequestCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("Current user ID is required.");

        RuleFor(x => x.FriendshipId)
            .NotEmpty().WithMessage("Friendship ID is required.");

        RuleFor(x => x.Decision)
            .NotEmpty().WithMessage("Decision is required.")
            .Must(d => ValidDecisions.Contains(d))
            .WithMessage("Decision must be either 'Accepted' or 'Rejected'.");
    }
}
