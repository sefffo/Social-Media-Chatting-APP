using Microsoft.AspNetCore.Identity;

namespace Social_Media_Chatting_APP_Domain.Entities
{
    public class AppUser : IdentityUser
    {
        //profile attributes 
        public string? DisplayName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? ProfilePicturePublicId { get; set; }
        public DateOnly DateOfBirth { get; set; }

        /// <summary>
        /// adding a bio about that Profile ==>appear to friends  
        /// </summary>
        public string? Bio { get; set; }

        public string Gender { get; set; }
        public bool IsDeactivated { get; set; }
        public bool ShowOnlineStatus { get; set; } = true;
        public bool ShowLastSeen { get; set; } = true;
        public bool AllowMessageFromStrangers { get; set; } = false;
        public string? Website { get; set; }
        public string? Location { get; set; }


        public bool IsTwoFactorSetup { get; set; }
        public bool IsGoogleAccount { get; set; }
        public bool IsOnline { get; set; }


        public DateTime CreatedAt { get; set; }

        public DateTime? LastSeen { get; set; }


        //nav prop 
        /// <summary>
        /// for creating a Conversation and Group chats 
        /// so we have 1-M as one user can have many convos or group chats
        /// also one user can send or receive many messages in a conversation so it becomes
        /// one-to-many relationship 1-M
        /// ConversationParticipants is Many-to-Many relationship
        /// </summary>

        public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = [];

        public ICollection<Message> SentMessages { get; set; } = [];
        public ICollection<MessageReadStatus> MessageReadStatuses { get; set; } = [];

        /// <summary>
        /// As we are implementing multi session so a use can have multuple refreshtokens
        /// so we need to have one-to-many Realtionship   
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}