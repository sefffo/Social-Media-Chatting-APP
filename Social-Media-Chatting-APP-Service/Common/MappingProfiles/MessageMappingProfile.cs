using AutoMapper;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.MessagesDTO_s;

namespace Social_Media_Chatting_APP_Service.Common.MappingProfiles;

public class MessageMappingProfile : Profile
{
    public MessageMappingProfile()
    {
        CreateMap<AppUser, SenderDto>()
            .ForMember(dest => dest.Id, opt
                => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src
                => src.DisplayName ?? src.UserName))
            .ForMember(dest => dest.AvatarUrl, opt
                => opt.MapFrom(src => src.ProfilePicture));

        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender));
        
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.ReplyTo,
                opt => opt.MapFrom(src => src.ReplyToMessageId))
            .ForMember(dest => dest.IsReply,
                opt => opt.MapFrom(src => src.ReplyToMessageId != null))
            .ForMember(dest => dest.ReadByCount,
                opt => opt.MapFrom(src => src.ReadStatuses.Count))
            .ForMember(dest => dest.IsRead,
                opt => opt.Ignore()); // set in handler based on current user
    }
}