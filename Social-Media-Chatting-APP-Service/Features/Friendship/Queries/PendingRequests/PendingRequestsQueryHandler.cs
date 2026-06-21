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

        var pendingRequests = await repo.FindAllAsync(f =>
            (
                f.AddresseeId == request.CurrentUserId ||
                f.RequestId == request.CurrentUserId && f.Status == FriendshipStatus.Pending
            )
        );


        var otherIds = pendingRequests.Select(f => f.AddresseeId == request.CurrentUserId ? f.RequestId : f.AddresseeId
        ).ToList();

        var userDict = await userManager.Users
            .Where(u => otherIds.Contains(Guid.Parse(u.Id)))
            .ToDictionaryAsync(u => Guid.Parse(u.Id), cancellationToken);


        var result = pendingRequests.Select(f =>
        {
            var otherId = f.RequestId == request.CurrentUserId
                ? f.AddresseeId
                : f.RequestId;

            return new FriendRequestItemDto
            {
                FriendshipId = f.Id,
                User = mapper.Map<PublicUserProfileDto>(userDict[otherId]),
                SentAt = f.CreatedAt,
                Direction = f.AddresseeId == request.CurrentUserId
                    ? "Incoming"
                    : "Outgoing"
            };
        }).ToList();

        return Result<IEnumerable<FriendRequestItemDto>>.Ok(result);
    }
}