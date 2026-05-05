using JobTracker.Domain.Jobs;
using Riok.Mapperly.Abstractions;

namespace JobTracker.Application.Jobs;

[Mapper]
public partial class JobMapper
{
    [MapProperty("Id.Value", nameof(JobResponse.Id))]
    public partial JobResponse ToResponse(Job job);
}
