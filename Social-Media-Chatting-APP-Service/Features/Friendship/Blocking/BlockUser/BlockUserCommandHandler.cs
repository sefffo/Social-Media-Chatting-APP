using AutoMapper;
using MediatR;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.Friendship.Blocking.BlockUser;

public class BlockUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<BlockUserCommand, Result<FriendshipActionResultDto>>
{
    public async Task<Result<FriendshipActionResultDto>> Handle(BlockUserCommand request,
        CancellationToken cancellationToken)
    {
        var repo = unitOfWork.GetRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid>();

        var existing = await FriendshipQueryHelper.GetAsync(repo, request.CurrentUserId, request.TargetedUserId);

        if (existing is null)
        {
            var newFriendship = new Social_Media_Chatting_APP_Domain.Entities.Friendship
            {
                Id = Guid.NewGuid(),
                RequestId = request.CurrentUserId,
                AddresseeId = request.TargetedUserId,
                Status = FriendshipStatus.Blocked,
                BlockedById = request.CurrentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await repo.AddAsync(newFriendship);
            await unitOfWork.SaveChangesAsync();
            return Result<FriendshipActionResultDto>.Ok(mapper.Map<FriendshipActionResultDto>(newFriendship));
        }

        if (existing.Status == FriendshipStatus.Blocked)
        {
            return Error.BadRequest("Friendship.AlreadyBlocked", "This user is already blocked");
        }

        existing.Status = FriendshipStatus.Blocked;
        existing.BlockedById = request.CurrentUserId;
        existing.UpdatedAt = DateTime.UtcNow;
        repo.Update(existing);
        await unitOfWork.SaveChangesAsync();
        var mappedResult = mapper.Map<FriendshipActionResultDto>(existing);
        return Result<FriendshipActionResultDto>.Ok(mappedResult);
    }
}