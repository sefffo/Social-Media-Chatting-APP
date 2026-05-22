using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Quries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<Result<PublicUserProfileDto>>;
