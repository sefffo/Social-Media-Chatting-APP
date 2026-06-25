using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social_Media_Chatting_APP_Domain.Entities;

namespace Social_Media_Chatting_APP_Persistence.Configurations;

public class ConversationParticipantsConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("ConversationParticipants");
        builder.Property(cp => cp.JoinedAt).IsRequired();
        builder.Property(cp => cp.IsAdmin).HasDefaultValue(false);


        builder.HasKey(c => new { c.ConversationId, c.UserId });
        builder.HasOne(c => c.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(c => c.ConversationId)
            .OnDelete(DeleteBehavior
                .Cascade); // "If the Conversation is deleted → automatically delete all its ConversationParticipant rows" 

        //User "Saif" is deleted
        //→ All ConversationParticipant rows for Saif are deleted
        //→ If Saif was in a group of 3 people, the group still exists
        //   but Saif's messages are now orphaned with no sender
        //→ If it was a DM, the conversation now has only 1 participant — broken

        builder.HasOne(c => c.User)
            .WithMany(u => u.ConversationParticipants)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict); // don't delete the conversation if user is deleted 
    }
}