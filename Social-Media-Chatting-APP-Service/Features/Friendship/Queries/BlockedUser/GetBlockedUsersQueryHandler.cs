using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Queries.BlockedUser;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

public class GetBlockedUsersQueryHandler(
    IUnitOfWork unitOfWork,
    UserManager<AppUser> userManager,
    IMapper mapper)
    : IRequestHandler<GetBlockedUsersQuery, Result<IEnumerable<BlockedUserItemDto>>>
{
    public async Task<Result<IEnumerable<BlockedUserItemDto>>> Handle(
        GetBlockedUsersQuery request, CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Friendship, Guid>();

        // Step 1 — only rows where YOU are the blocker
        var blockedRows = await repo.FindAllAsync(f =>
            f.Status == FriendshipStatus.Blocked &&
            f.BlockedById == request.CurrentUserId);

        // Step 2 — collect the blocked person's ID from each row
        var blockedIds = blockedRows
            .Select(f => f.RequestId == request.CurrentUserId
                ? f.AddresseeId
                : f.RequestId)
            .ToList();
        
        var blockedIdsStrings = blockedIds.Select(id => id.ToString()).ToList();

        // Step 3 — one DB call to load all profiles
        var userDict = await userManager.Users
            .Where(u => blockedIdsStrings.Contains(u.Id))
            .ToDictionaryAsync(u => Guid.Parse(u.Id), cancellationToken);

        // Step 4 — build the result
        var result = blockedRows.Select(f =>
        {
            var blockedId = f.RequestId == request.CurrentUserId
                ? f.AddresseeId
                : f.RequestId;

            return new BlockedUserItemDto
            {
                User      = mapper.Map<PublicUserProfileDto>(userDict[blockedId]),
                BlockedAt = f.UpdatedAt
            };
        }).ToList();

        return Result<IEnumerable<BlockedUserItemDto>>.Ok(result);
    }
}