namespace Social_Media_Chatting_APP_SharedLibrary.Dto_s;

public record OTPDataDto(
        string Code,
        string UserId,
        int Attempts 
    );