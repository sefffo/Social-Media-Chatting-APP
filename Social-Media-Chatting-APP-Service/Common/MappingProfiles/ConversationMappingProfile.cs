using AutoMapper;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.ConversationDTO_s;

namespace Social_Media_Chatting_APP_Service.Common.MappingProfiles;

public class ConversationMappingProfile : Profile
{
    public ConversationMappingProfile()
    {
        // ConversationParticipant → ParticipantDto
        CreateMap<ConversationParticipant, ParticipantDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.User.DisplayName ?? src.User.UserName))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.ProfilePicture))
            .ForMember(dest => dest.IsOnline, opt => opt.Ignore()); // set by PresenceTracker

        // Conversation → ConversationDto (shallow — computed fields done in handler)
        CreateMap<Conversation, ConversationDto>()
            .ForMember(dest => dest.UnreadCount, opt => opt.Ignore())
            .ForMember(dest => dest.LastMessagePreview, opt => opt.MapFrom(src => src.LastMessage != null
                ? src.LastMessage.TextContent ?? "📎 Attachment"
                : null))
            .ForMember(dest => dest.OtherParticipant, opt => opt.Ignore()); // set in handler
    }
}