using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Entities.Enums;

namespace Social_Media_Chatting_APP_Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.Property(m => m.SentAt).IsRequired();
        builder.Property(m => m.IsDeleted).HasDefaultValue(false);
        builder.Property(m => m.FileName).HasMaxLength(255);
        builder.Property(m => m.Content).HasConversion<string>();
        builder.Property(m => m.MediaUrl)
            .HasMaxLength(500);
        builder.Property(m => m.MediaPublicId)
            .HasMaxLength(256);
        builder.Property(m => m.FileName)
            .HasMaxLength(255);
        //index will increase the performance of the Read with very samll cost on the write  
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.SentAt);

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade); // on deleting the conversation all the messages are deleted


        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict); //dont delete messages if user is deleted 
        
        builder.HasOne(m=>m.ReplyToMessage)
            .WithMany()
            .HasForeignKey(m=>m.ReplyToMessageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}