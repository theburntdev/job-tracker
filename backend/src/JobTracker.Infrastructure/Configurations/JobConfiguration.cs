using JobTracker.Domain.Common;
using JobTracker.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTracker.Infrastructure.Configurations;

internal sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasConversion(
                id => id.Value,
                value => new JobId(value));

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
    }
}
