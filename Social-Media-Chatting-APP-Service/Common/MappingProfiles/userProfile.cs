using AutoMapper;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;

namespace Social_Media_Chatting_APP_Service.Common.MappingProfiles;

public class userProfile : Profile
{
    public userProfile()
    {
        // AppUser → UserProfileDto
        CreateMap<AppUser, UserProfileDto>()
            .ForMember(dest => dest.ProfilePictureUrl, opt =>
                opt.MapFrom(src => src.ProfilePicture));

        // AppUser → PublicUserProfileDto
        CreateMap<AppUser, PublicUserProfileDto>()
            .ForMember(dest => dest.ProfilePictureUrl, opt =>
                opt.MapFrom(src => src.ProfilePicture));

        // AppUser → UserSearchResultDto
        CreateMap<AppUser, UserSearchResultDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.ProfilePictureUrl, opt =>
                opt.MapFrom(src => src.ProfilePicture));

        // UpdateProfileDto → AppUser (partial update, null fields are ignored)
        CreateMap<UpdateProfileDto, AppUser>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}