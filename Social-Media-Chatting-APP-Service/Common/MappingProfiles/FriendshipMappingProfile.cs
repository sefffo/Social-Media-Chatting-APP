using AutoMapper;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.FriendshipDTO_s;

namespace Social_Media_Chatting_APP_Service.Common.MappingProfiles;

public class FriendshipMappingProfile : Profile
{
    public FriendshipMappingProfile()
    {
        // Friendship → FriendshipActionResultDto
        CreateMap<Friendship, FriendshipActionResultDto>()
            .ForMember(dest => dest.FriendshipId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RequesterId,  opt => opt.MapFrom(src => src.RequesterId))
            .ForMember(dest => dest.AddresseeId,  opt => opt.MapFrom(src => src.AddresseeId))
            .ForMember(dest => dest.Status,       opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CreatedAt,    opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt,    opt => opt.MapFrom(src => src.UpdatedAt));
    }
}
