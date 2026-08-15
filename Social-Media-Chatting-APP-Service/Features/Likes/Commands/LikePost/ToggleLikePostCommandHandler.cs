using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Likes.Commands.LikePost;

public class ToggleLikePostCommandHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<ToggleLikePostCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleLikePostCommand request, CancellationToken cancellationToken)
    {
        var postRepo = unitOfWork.GetRepository<Post, Guid>();
        var post = await postRepo.FindAsync(p => p.Id == request.PostId && !p.IsDeleted);
        if (post is null)
            return Error.NotFound("Post.NotFound", "Post not found");

        var existingLike = await unitOfWork.FindAsync<PostLike>(
            pl => pl.PostId == request.PostId && pl.UserId == request.UserId);

        if (existingLike is not null)
        {
            unitOfWork.Remove(existingLike);
            await unitOfWork.SaveChangesAsync();
            return Result<bool>.Ok(false);
        }

        await unitOfWork.AddAsync(new PostLike
        {
            PostId = request.PostId,
            UserId = request.UserId,
            LikedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
