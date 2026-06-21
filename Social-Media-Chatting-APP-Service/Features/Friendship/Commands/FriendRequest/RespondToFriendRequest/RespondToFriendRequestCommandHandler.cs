using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.RespondToFriendRequest;

public class RespondToFriendRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<RespondToFriendRequestCommand, Result<FriendshipActionResultDto>>
{
    public async Task<Result<FriendshipActionResultDto>> Handle(RespondToFriendRequestCommand request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        var friendship = await repo.GetByIdAsync(request.FriendshipId);
        if (friendship is null)
        {
            return Error.NotFound("Friendship.NotFound", "No Friend Request was found.");
        }

        // check if the user is the one who sent the request and can't accept the ones he sent
        if (friendship.AddresseeId != request.CurrentUserId)
        {
            return Error.Forbidden("Friendship.Forbidden", "You can't accept requests you sent.");
        }

        if (friendship.Status != FriendshipStatus.Pending)
        {
            return Error.BadRequest("Friendship.BadRequest", "Friendship status is not pending.");
        }

        friendship.Status = Enum.Parse<FriendshipStatus>(request.Decision);
        friendship.UpdatedAt = DateTime.UtcNow;
        friendship.CreatedAt = DateTime.UtcNow;
        repo.Update(friendship);
        await unitOfWork.SaveChangesAsync();
        var mappedResult = mapper.Map<FriendshipActionResultDto>(friendship);
        return Result<FriendshipActionResultDto>.Ok(mappedResult);
    }
}