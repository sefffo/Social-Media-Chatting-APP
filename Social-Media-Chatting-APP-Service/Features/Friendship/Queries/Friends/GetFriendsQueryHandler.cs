using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Queries.Friends;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;


/// <summary>
/// DB: Friendship table          DB: AspNetUsers table
///
/// DB: Friendship table          DB: AspNetUsers table
///  ──────────────────            ──────────────────────
///[YOU ↔ Sara]   ──┐            
///[Omar ↔ YOU]   ──┼── Step 3 ──► Dictionary { Sara.Id → Sara, Omar.Id → Omar }
///[YOU ↔ Ahmed]  ──┘                                     │
   // │
//Step 4: loop each row ──► grab user from dict ──► map to DTO
  //  │
//FriendListItemDto
  //  ├── User: { "Sara", avatar... }
//    └── FriendsSince: 2026-01-15
/// </summary>
/// <param name="unitOfWork"></param>
/// <param name="userManager"></param>
/// <param name="mapper"></param>


public class GetFriendsQueryHandler(
    IUnitOfWork unitOfWork,
    UserManager<AppUser> userManager,
    IMapper mapper)
    : IRequestHandler<GetFriendsQuery, Result<IEnumerable<FriendListItemDto>>>
{
    public async Task<Result<IEnumerable<FriendListItemDto>>> Handle(
        GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Friendship, Guid>();

        // Step 1 — get all accepted rows where I appear on either side
        var friendships = await repo.FindAllAsync(f =>
            (f.RequestId == request.CurrentUserId || f.AddresseeId == request.CurrentUserId) &&
            f.Status == FriendshipStatus.Accepted);

        // Step 2 — extract the OTHER person's ID from each row
        var friendIds = friendships
            .Select(f => f.RequestId == request.CurrentUserId
                ? f.AddresseeId
                : f.RequestId)
            .ToList();

        // Step 3 — one DB call to load all friend profiles into a dictionary
        var userDict = await userManager.Users
            .Where(u => friendIds.Contains(Guid.Parse(u.Id)))
            .ToDictionaryAsync(u => Guid.Parse(u.Id), cancellationToken);
        
        var result = friendships.Select(f =>
        {
            var friendId = f.RequestId == request.CurrentUserId
                ? f.AddresseeId
                : f.RequestId;

            return new FriendListItemDto
            {
                User = mapper.Map<PublicUserProfileDto>(userDict[friendId]),
                FriendsSince = f.UpdatedAt
            };
        }).ToList();

        return Result<IEnumerable<FriendListItemDto>>.Ok(result);
    }
}

     //AppUser                         PublicUserProfileDto
//──────────────────────          ────────────────────────────
//Id            → "abc-123"  →    Id:             Guid("abc-123")
//UserName      → "sara99"   →    UserName:       "sara99"
//DisplayName   → "Sara"     →    DisplayName:    "Sara"
//ProfilePicture→ "sara.jpg" →    ProfilePictureUrl: "sara.jpg"
