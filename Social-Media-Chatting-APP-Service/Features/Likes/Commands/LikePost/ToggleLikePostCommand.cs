using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Likes.Commands.LikePost;

public record ToggleLikePostCommand(
    string UserId,
    Guid PostId
) : IRequest<Result<bool>>;
