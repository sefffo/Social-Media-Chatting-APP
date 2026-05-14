using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Social_Media_Chatting_APP_ServiceAbstraction;

namespace Social_Media_Chatting_APP_Service.Common.Email;

public class EmailService(
        IOptions<EmailSettings> emailSettings
    ) : IEmailService   
{
    /// <summary>
    /// Build the email object → connect to Gmail → authenticate → hand off the email → disconnect.
    /// </summary>
    public async Task SendAsync(string to, string subject, string body, bool isHtml = true)
    {
        //implementing the Send Email Service 
        
        //get an object of the MimiKit message 
        var message = new MimeMessage();
        
        message.From.Add(new MailboxAddress(emailSettings.Value.FromName, emailSettings.Value.FromEmail));

        message.To.Add(new MailboxAddress(to, to));
            
        message.Subject = subject;

        
        message.Body = new TextPart(isHtml ? "html" : "plain")
        {
            Text = body
        };
        
        //opening a Connection With the SMTP service 
        using var client = new SmtpClient();
        //Opens a TCP connection to smtp.gmail.com:587 and upgrades it to encrypted using STARTTLS. This is the "knocking on the post office door" step.
        await client.ConnectAsync(emailSettings.Value.Host, emailSettings.Value.Port, emailSettings.Value.UseSsl);
        //Logs into the Gmail SMTP server with your app's credentials.
        //This is where Google validates your App Password. If this fails, you get an AuthenticationException.
        await client.AuthenticateAsync(emailSettings.Value.Username, emailSettings.Value.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);





    }
}