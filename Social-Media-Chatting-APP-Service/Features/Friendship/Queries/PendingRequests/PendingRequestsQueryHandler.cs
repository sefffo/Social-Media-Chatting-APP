using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Queries.PenddingRequests;

public class PendingRequestsQueryHandler(
    UserManager<AppUser> userManager,
    IMapper mapper,
    IUnitOfWork unitOfWork
) : IRequestHandler<PendingRequestsQuery, Result<IEnumerable<FriendRequestItemDto>>>
{
    public async Task<Result<IEnumerable<FriendRequestItemDto>>> Handle(PendingRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();

        // Only fetch requests where current user is the addressee (incoming) with Pending status
        var pendingRequests = await repo.FindAllAsync(f =>
            f.AddresseeId == request.CurrentUserId &&
            f.Status == FriendshipStatus.Pending
        );

        // Collect requester IDs as strings for EF-translatable query
        var requesterIdStrings = pendingRequests
            .Select(f => f.RequestId.ToString())
            .ToList();

        // Single DB call — load all requester profiles into a dictionary for O(1) lookup
        var userDict = await userManager.Users
            .Where(u => requesterIdStrings.Contains(u.Id))
            .ToDictionaryAsync(u => Guid.Parse(u.Id), cancellationToken);

        var result = pendingRequests.Select(f =>
        {
            return new FriendRequestItemDto
            {
                FriendshipId = f.Id,
                User = mapper.Map<PublicUserProfileDto>(userDict[f.RequestId]),
                SentAt = f.CreatedAt,
                Direction = "Incoming"
            };
        }).ToList();

        return Result<IEnumerable<FriendRequestItemDto>>.Ok(result);
    }
}
