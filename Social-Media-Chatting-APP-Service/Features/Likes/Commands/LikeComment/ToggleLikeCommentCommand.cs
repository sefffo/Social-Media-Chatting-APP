using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Likes.Commands.LikeComment;

public record ToggleLikeCommentCommand(
    string UserId,
    Guid CommentId
) : IRequest<Result<bool>>;
