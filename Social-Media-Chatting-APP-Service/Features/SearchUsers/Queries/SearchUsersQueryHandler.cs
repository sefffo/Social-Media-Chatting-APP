using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s.UserDTO_s;
using Social_Media_Chatting_APP_SharedLibrary.SharedResponse;

namespace Social_Media_Chatting_APP_Service.Features.SearchUsers.Queries;

public class SearchUsersQueryHandler(
    UserManager<AppUser> userManager,
    IMapper mapper
) : IRequestHandler<SearchUsersQuery, Result<IEnumerable<UserSearchResultDto>>>
{
    public async Task<Result<IEnumerable<UserSearchResultDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return Error.BadRequest("Search.EmptyTerm", "Search term cannot be empty.");

        var searchTerm = request.SearchTerm.Trim().ToLower();

        var users = await userManager.Users
            .Where(u =>
                u.Id != request.CurrentUserId.ToString() &&
                (u.UserName.ToLower().Contains(searchTerm) ||
                 u.DisplayName.ToLower().Contains(searchTerm)))
            .Take(20)
            .ToListAsync(cancellationToken);

        var result = mapper.Map<List<UserSearchResultDto>>(users);
        return Result<IEnumerable<UserSearchResultDto>>.Ok(result);
    }
}