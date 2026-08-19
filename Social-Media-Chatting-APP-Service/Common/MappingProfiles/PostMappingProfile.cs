using AutoMapper;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.commonDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.PostsDTO_s;

namespace Social_Media_Chatting_APP_Service.Common.MappingProfiles;

public class PostMappingProfile : Profile
{
    public PostMappingProfile()
    {
        // MediaAsset → MediaAssetDto
        CreateMap<MediaAsset, MediaAssetDto>()
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.ResourceType, opt => opt.MapFrom(src => src.ResourceType));

        // AppUser → AuthorDto
        CreateMap<AppUser, AuthorDto>()
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePicture));

        // Post → PostDto
        // Note: LikeCount, CommentCount, RepostCount, IsLikedByMe are computed manually in handlers
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
            .ForMember(dest => dest.MediaAssets, opt => opt.MapFrom(src => src.MediaAssets))
            //.ForMember(dest => dest.OriginalPost, opt => opt.MapFrom(src => src.OriginalPost))
            .ForMember(dest => dest.OriginalPost, opt => opt.MapFrom(src => src.OriginalPost != null ? src.OriginalPost : null))
            .ForMember(dest => dest.LikeCount, opt => opt.Ignore())
            .ForMember(dest => dest.CommentCount, opt => opt.Ignore())
            .ForMember(dest => dest.RepostCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsLikedByMe, opt => opt.Ignore());
    }
}
