using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Quries.GetMyProfile;

public record GetMyProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
