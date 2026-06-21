using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
namespace Social_Media_Chatting_APP_Service.Features.Friendship.Helpers;

public static class FriendshipQueryHelper
{
    public async static Task<Social_Media_Chatting_APP_Domain.Entities.Friendship?> GetAsync(
        IGenericRepository<Social_Media_Chatting_APP_Domain.Entities.Friendship, Guid> repository,
        Guid userAId, Guid userBId)
    {
        return await repository.FindAsync(f =>
            (f.RequestId == userAId && f.AddresseeId == userBId) ||
            (f.RequestId == userBId && f.AddresseeId == userAId));
    }
}