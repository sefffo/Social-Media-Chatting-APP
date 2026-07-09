using Social_Media_Chatting_APP_Domain.Entities;

namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

public class ChangeGroupRoleRequestDto
{
    public Guid TargetUserId { get; set; }
    public GroupRole NewRole { get; set; }
}