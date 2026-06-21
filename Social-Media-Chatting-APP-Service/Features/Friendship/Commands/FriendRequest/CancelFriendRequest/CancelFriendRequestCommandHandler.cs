using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.CancelFriendRequest;

public class CancelFriendRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<CancelFriendRequestCommand, Result<FriendshipActionResultDto>>
{
    private IRequestHandler<CancelFriendRequestCommand, Result<FriendshipActionResultDto>>
        _requestHandlerImplementation;

    public async Task<Result<FriendshipActionResultDto>> Handle(CancelFriendRequestCommand request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        var friendship = await repo.GetByIdAsync(request.FriendshipId);
        if (friendship is null) return Error.NotFound("Friendship.NotFound", "No friend request was sent");
        if (friendship.RequestId != request.CurrentUserId)
            return Error.Forbidden("Friendship.Forbidden", "You can't Cancel this Request");
        if (friendship.Status != FriendshipStatus.Pending)
        {
            return Error.BadRequest("Friendship.BadRequest", "The Addressee Already Took action in Your request");
        }
        repo.Remove(friendship);
        await unitOfWork.SaveChangesAsync();
        var mappedResult = mapper.Map<FriendshipActionResultDto>(friendship);
        return Result<FriendshipActionResultDto>.Ok(mappedResult);
    }
}