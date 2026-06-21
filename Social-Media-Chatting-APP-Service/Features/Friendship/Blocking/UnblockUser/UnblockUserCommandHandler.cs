using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.UnblockUser;

public class UnblockUserCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<UnblockUserCommand, Result<FriendshipActionResultDto>>
{
    public async Task<Result<FriendshipActionResultDto>> Handle(UnblockUserCommand request,
        CancellationToken cancellationToken)

    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();

        var existing = await FriendshipQueryHelper.GetAsync(repo, request.CurrentUser, request.TargetedUserId);


        if (existing is null)
            return Error.NotFound("Friendship.NotFound", "No relationship found with this user.");

        if (existing.Status != FriendshipStatus.Blocked)
        {
            return Error.BadRequest("Unblock.NotBlocked", "You cant unblock this user as you didn't block him/her");
        }

        if (existing.BlockedById != request.CurrentUser)
        {
            return Error.Forbidden("Friendship.NotBlocker", "You did not block this user");
        }


        repo.Remove(existing);
        await unitOfWork.SaveChangesAsync();
        return Result<FriendshipActionResultDto>.Ok(mapper.Map<FriendshipActionResultDto>(existing));
    }
}