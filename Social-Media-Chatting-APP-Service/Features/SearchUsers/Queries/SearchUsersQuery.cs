using MediatR;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.SearchUsers.Queries;

public record SearchUsersQuery(string SearchTerm , Guid CurrentUserId):IRequest<Result<IEnumerable<UserSearchResultDto>>>;