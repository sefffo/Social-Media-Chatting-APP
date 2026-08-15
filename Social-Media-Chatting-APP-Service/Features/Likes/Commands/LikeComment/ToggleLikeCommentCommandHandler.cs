using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Likes.Commands.LikeComment;

public class ToggleLikeCommentCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<ToggleLikeCommentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleLikeCommentCommand request, CancellationToken cancellationToken)
    {
        var commentRepo = unitOfWork.GetRepository<Comment, Guid>();
        var comment = await commentRepo.FindAsync(c => c.Id == request.CommentId && !c.IsDeleted);
        if (comment is null)
            return Error.NotFound("Comment.NotFound", "Comment not found");

        var existingLike = await unitOfWork.FindAsync<CommentLike>(
            cl => cl.CommentId == request.CommentId && cl.UserId == request.UserId);

        if (existingLike is not null)
        {
            unitOfWork.Remove(existingLike);
            await unitOfWork.SaveChangesAsync();
            return Result<bool>.Ok(false);
        }

        await unitOfWork.AddAsync(new CommentLike
        {
            CommentId = request.CommentId,
            UserId = request.UserId,
            LikedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
