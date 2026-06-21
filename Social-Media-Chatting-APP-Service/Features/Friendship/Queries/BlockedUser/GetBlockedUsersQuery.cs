using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Queries.BlockedUser;

public record GetBlockedUsersQuery(Guid CurrentUserId) : IRequest<Result<IEnumerable<BlockedUserItemDto>>>;