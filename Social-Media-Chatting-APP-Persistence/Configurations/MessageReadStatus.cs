using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Social_Media_Chatting_APP_Persistence.Configurations;

public class MessageReadStatus : IEntityTypeConfiguration<Social_Media_Chatting_APP_Domain.Entities.MessageReadStatus>
{
    public void Configure(EntityTypeBuilder<Social_Media_Chatting_APP_Domain.Entities.MessageReadStatus> builder)
    {
        builder.ToTable("MessageReadStatuses");

        // Composite PK
        builder.HasKey(mrs => new { mrs.MessageId, mrs.UserId });
        builder.Property(mrs => mrs.ReadAt).IsRequired();

        builder.HasOne(mrs => mrs.Message)
            .WithMany(mrs => mrs.ReadStatuses)
            .HasForeignKey(mrs => mrs.MessageId)
            .OnDelete(DeleteBehavior
                .Cascade); //if a user was deleted, the message read status of that user is deleted too
        builder.HasOne(mrs => mrs.User)
            .WithMany(mrs => mrs.MessageReadStatuses)
            .HasForeignKey(mrs => mrs.UserId)
            .OnDelete(DeleteBehavior
                .Restrict);


        //Wait — Cascade would actually work here too. So why Restrict?
        //Consistency with your deletion strategy.
        //You're using Restrict on all user FKs to force your service layer to handle
        //user deletion properly in one controlled place
            //— not let the DB silently cascade-delete things in different directions.
        //If you used Cascade on some user FKs and Restrict on others,
        //your user deletion service would be unpredictable — some things auto-delete,
        //some things throw errors. Restrict everywhere means your service always knows:
        //"I must clean everything up manually before deleting the user." One clear rule, no surprises.
    }
}