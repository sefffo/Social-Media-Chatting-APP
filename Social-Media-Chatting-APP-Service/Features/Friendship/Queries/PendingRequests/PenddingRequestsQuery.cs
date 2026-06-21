using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Queries.PenddingRequests;

public record PendingRequestsQuery(Guid CurrentUserId) : IRequest<Result<IEnumerable<FriendRequestItemDto>>>;