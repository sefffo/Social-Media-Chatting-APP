using MediatR;
using Microsoft.AspNetCore.Http;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.UserProfile.Commands.UploadProfilePicture;

public record UploadProfilePictureCommand(Guid UserId, IFormFile File)
    : IRequest<Result<string>>;