using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    UserManager<AppUser> userManager,
    IMapper mapper
) : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        mapper.Map(request.Dto, user);
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<UserProfileDto>.Fail(Error.Failure("User.UpdateFailed", result.Errors.First().Description));

        var userProfileDto = mapper.Map<UserProfileDto>(user);
        return Result<UserProfileDto>.Ok(userProfileDto);
    }
}