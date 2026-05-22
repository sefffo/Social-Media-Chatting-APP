using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Quries.GetMyProfile;

public class GetMyProfileQueryHandler(
    UserManager<AppUser> userManager,
    IMapper mapper
) : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result<UserProfileDto>.Fail(Error.NotFound("User.NotFound", "User not found."));

        var userProfileDto = mapper.Map<UserProfileDto>(user);
        return Result<UserProfileDto>.Ok(userProfileDto);
    }
}