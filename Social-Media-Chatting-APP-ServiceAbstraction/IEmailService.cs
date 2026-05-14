namespace Social_Media_Chatting_APP_ServiceAbstraction;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true);

}