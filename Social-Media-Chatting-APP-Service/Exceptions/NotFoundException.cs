namespace Social_Media_Chatting_APP_Service.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(string name, object key)
        : base($"{name} with id ({key}) was not found.") { }
}
