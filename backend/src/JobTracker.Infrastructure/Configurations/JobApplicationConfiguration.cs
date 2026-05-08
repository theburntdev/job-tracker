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
            .HasMaxLength(JobApplicationConstraints.TitleMaxLength);

        builder.Property(j => j.Company)
            .IsRequired()
            .HasMaxLength(JobApplicationConstraints.CompanyMaxLength);

        builder.Property(j => j.Location)
            .HasMaxLength(JobApplicationConstraints.LocationMaxLength);

        builder.Property(j => j.Url)
            .HasMaxLength(JobApplicationConstraints.UrlMaxLength);

        builder.Property(j => j.Description)
            .HasMaxLength(JobApplicationConstraints.DescriptionMaxLength);

        builder.Property(j => j.Stage)
            .IsRequired();

        builder.Property(j => j.AppliedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : null);

        builder.Property(j => j.PostedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : null);

        builder.Property(j => j.CreatedAt)
            .HasConversion(
                v => v.ToUnixTimeMilliseconds(),
                v => DateTimeOffset.FromUnixTimeMilliseconds(v));

        builder.Property(j => j.UpdatedAt)
            .HasConversion(
                v => v.ToUnixTimeMilliseconds(),
                v => DateTimeOffset.FromUnixTimeMilliseconds(v));
    }
}
