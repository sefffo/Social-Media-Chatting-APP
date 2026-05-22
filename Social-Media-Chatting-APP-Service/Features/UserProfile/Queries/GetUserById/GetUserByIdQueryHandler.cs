using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Quries.GetUserById;

public class GetUserByIdQueryHandler(
    UserManager<AppUser> userManager,
    IMapper mapper
    ) : IRequestHandler<GetUserByIdQuery , Result<PublicUserProfileDto>>
{
    public async Task<Result<PublicUserProfileDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    { 
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return Result<PublicUserProfileDto>.Fail(Error.NotFound("User.NotFound", "User not found."));

        var dto = mapper.Map<PublicUserProfileDto>(user);
        return Result<PublicUserProfileDto>.Ok(dto);
    }
}