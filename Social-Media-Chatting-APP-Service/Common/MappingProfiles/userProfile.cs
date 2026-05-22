using AutoMapper;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

namespace Social_Media_Chatting_APP_Service.Common.MappingProfiles;

public class userProfile : Profile
{
    public userProfile()
    {
        CreateMap<AppUser, UserProfileDto>().ForMember(dest => dest.ProfilePictureUrl,
            opt =>
                opt.MapFrom(src => src.ProfilePicture));
        CreateMap<AppUser, PublicUserProfileDto>()
            .ForMember(dest => dest.ProfilePictureUrl, opt =>
                opt.MapFrom(src => src.ProfilePicture));

        // UpdateProfileDto → AppUser (for the update, null fields are ignored)
        CreateMap<UpdateProfileDto, AppUser>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        // AppUser → UserProfileDto (for the response)
        CreateMap<AppUser, UserProfileDto>()
            .ForMember(dest => dest.ProfilePictureUrl, opt =>
                opt.MapFrom(src => src.ProfilePicture));
    }
}