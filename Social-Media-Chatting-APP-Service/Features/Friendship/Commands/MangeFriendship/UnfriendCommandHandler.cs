using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.MangeFriendship;

public class UnfriendCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UnfriendCommand, Result<FriendshipActionResultDto>>
{
    public async Task<Result<FriendshipActionResultDto>> Handle(UnfriendCommand request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();

        var existing = await FriendshipQueryHelper.GetAsync(repo, request.CurrentUserId, request.TargetedUserId);

        if (existing is null)
        {
            return Error.NotFound("Friendship.NotFound", "No friendship found with this user.");
        }

        if (existing.Status != FriendshipStatus.Accepted)
        {
            return Error.BadRequest("Friendship.NotFriends", "You are not friends with this user");
        }

        repo.Remove(existing);
        await unitOfWork.SaveChangesAsync();
        return Result<FriendshipActionResultDto>.Ok(mapper.Map<FriendshipActionResultDto>(existing));
    }
}