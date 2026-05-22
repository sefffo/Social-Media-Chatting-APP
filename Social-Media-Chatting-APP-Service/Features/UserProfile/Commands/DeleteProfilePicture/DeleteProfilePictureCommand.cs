using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.DeleteProfilePicture;

public record DeleteProfilePictureCommand(Guid UserId  ) : IRequest<Result<string>>;