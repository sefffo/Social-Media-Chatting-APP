using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Commands.FriendRequest.CancelFriendRequest;

public record CancelFriendRequestCommand(Guid CurrentUserId, Guid FriendshipId)
    : IRequest<Result<FriendshipActionResultDto>>;