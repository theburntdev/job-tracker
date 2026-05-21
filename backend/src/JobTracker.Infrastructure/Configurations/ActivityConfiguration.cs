using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Configurations;

internal sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => new ActivityId(value));

        builder.Property(a => a.JobApplicationId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new JobApplicationId(value));

        builder.Property(a => a.ActivityType)
            .IsRequired();

        builder.Property(a => a.OccurredAt)
            .IsRequired()
            .HasConversion(
                v => v.ToUnixTimeMilliseconds(),
                v => DateTimeOffset.FromUnixTimeMilliseconds(v));

        builder.Property(a => a.ContactName)
            .HasMaxLength(200);

        builder.Property(a => a.ContactEmail)
            .HasMaxLength(320);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasConversion(
                v => v.ToUnixTimeMilliseconds(),
                v => DateTimeOffset.FromUnixTimeMilliseconds(v));

        builder.HasOne<JobApplication>("JobApplication")
            .WithMany()
            .HasForeignKey(a => a.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
