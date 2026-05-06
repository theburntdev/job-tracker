using JobTracker.Domain.Jobs;
using Riok.Mapperly.Abstractions;

namespace JobTracker.Application.JobApplications;

[Mapper]
public partial class JobApplicationMapper
{
    [MapProperty("Id.Value", nameof(JobApplicationResponse.Id))]
    public partial JobApplicationResponse ToResponse(JobApplication jobApplication);
}
