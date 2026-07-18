using Microsoft.AspNetCore.SignalR;

namespace Social_Media_Chatting_APP_Presentation.Infrastructure.SignalR;

public class UserIdProvider : IUserIdProvider
{
    public Task<string> GetUserIdAsync(HubConnectionContext connection)
    {   
        throw new NotImplementedException();
    }
}