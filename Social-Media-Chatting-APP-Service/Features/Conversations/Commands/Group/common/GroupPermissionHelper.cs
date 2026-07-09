using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_Service.Features.Conversations.Group.Helpers;

public static class GroupPermissionHelper
{
    public static bool IsGroupAdmin(Conversation conversation, string userId)
    {
        return conversation.Participants.Any(p =>
            p.UserId == userId && p.Role == GroupRole.GroupAdmin);
    }

    public static bool IsParticipant(Conversation conversation, string userId)
    {
        return conversation.Participants.Any(p => p.UserId == userId);
    }
}