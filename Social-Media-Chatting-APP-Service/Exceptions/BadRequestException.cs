namespace Social_Media_Chatting_APP_Service.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message) { }
}
