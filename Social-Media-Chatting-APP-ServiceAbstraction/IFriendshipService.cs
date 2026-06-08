using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_ServiceAbstraction;

public interface IFriendshipService
{
    Task<Result<FriendshipActionResultDto>> SendRequestAsync(Guid currentUserId, SendFriendRequestDto dto);
    Task<Result<FriendshipActionResultDto>> RespondAsync(Guid currentUserId, Guid requesterId, RespondToFriendRequestDto dto);
    Task<Result> BlockAsync(Guid currentUserId, BlockUserDto dto);
    Task<Result> UnfriendAsync(Guid currentUserId, Guid friendId);
    Task<Result<IReadOnlyList<FriendListItemDto>>> GetFriendsAsync(Guid currentUserId);
    Task<Result<IReadOnlyList<FriendRequestItemDto>>> GetPendingRequestsAsync(Guid currentUserId);
    Task<Result<IReadOnlyList<FriendRequestItemDto>>> GetSentRequestsAsync(Guid currentUserId);
    Task<bool> AreFriendsAsync(Guid userA, Guid userB);
}