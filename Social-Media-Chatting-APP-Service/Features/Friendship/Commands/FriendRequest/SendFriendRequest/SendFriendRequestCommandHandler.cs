using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.SendFriendRequest;

public class SendFriendRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<SendFriendRequestCommand, Result<FriendshipActionResultDto>>
{
    public async Task<Result<FriendshipActionResultDto>> Handle(SendFriendRequestCommand request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();
        var result = await FriendshipQueryHelper.GetAsync(repo, request.AddresseeId, request.CurrentUserId);
        if (result is not null)
        {
            if (result.Status == FriendshipStatus.Blocked)
                return Error.Forbidden("Friendship.Blocked", "This action is not allowed.");

            if (result.Status == FriendshipStatus.Accepted)
                return Error.BadRequest("Friendship.AlreadyFriends", "You are already friends with this user.");

            if (result.Status == FriendshipStatus.Pending)
                return Error.BadRequest("Friendship.AlreadyPending", "A friend request already exists.");
            
            if(result.Status == FriendshipStatus.Blocked)
            {
                if (result.BlockedById == request.CurrentUserId)
                    return Error.Forbidden("Friendship.YouBlocked", 
                        "You have blocked this user. Unblock them to interact.");

                return Error.Forbidden("Friendship.BlockedByUser", 
                    "This user has blocked you.");
            }

            if (result.Status == FriendshipStatus.Rejected)
            {
                var cooldownEnds = result.UpdatedAt.AddDays(14);
                if (DateTime.UtcNow < cooldownEnds)
                {
                    var daysLeft = (int)(cooldownEnds - DateTime.UtcNow).TotalDays + 1;
                    return Error.BadRequest("Friendship.Cooldown",
                        $"You cannot send a request to this user for another {daysLeft} day(s).");
                }

                repo.Remove(result);
                await unitOfWork.SaveChangesAsync();
            }
        }

        var reverseRequest = await repo.FindAsync(f =>
            f.RequestId == request.AddresseeId &&
            f.AddresseeId == request.CurrentUserId &&
            f.Status == FriendshipStatus.Pending);


        if (reverseRequest is not null)
        {
            reverseRequest.Status = FriendshipStatus.Accepted;
            reverseRequest.UpdatedAt = DateTime.UtcNow;
            reverseRequest.CreatedAt = DateTime.UtcNow;
            repo.Update(reverseRequest);
            await unitOfWork.SaveChangesAsync();
            var mappedReverseResult = mapper.Map<FriendshipActionResultDto>(reverseRequest);
            return Result<FriendshipActionResultDto>.Ok(mappedReverseResult);
        }
        var friendship = new Social_Media_Chatting_APP_Domain.Entities.Friendship
        {
            Id = Guid.NewGuid(),
            RequestId = request.CurrentUserId,
            AddresseeId = request.AddresseeId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await repo.AddAsync(friendship);
        await unitOfWork.SaveChangesAsync();
        var mappedResult = mapper.Map<FriendshipActionResultDto>(friendship);
        return Result<FriendshipActionResultDto>.Ok(mappedResult);

    }
}