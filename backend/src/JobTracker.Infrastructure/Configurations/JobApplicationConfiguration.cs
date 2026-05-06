using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Configurations;

internal sealed class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasConversion(
                id => id.Value,
                value => new JobApplicationId(value));

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Company)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Location)
            .HasMaxLength(200);

        builder.Property(j => j.Url)
            .HasMaxLength(2048);

        builder.Property(j => j.Description)
            .HasMaxLength(2048);

        builder.Property(j => j.Stage)
            .IsRequired();
    }
}
