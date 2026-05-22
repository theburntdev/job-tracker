using JobTracker.Domain.Jobs;
using Riok.Mapperly.Abstractions;

namespace JobTracker.Application.Activities;

[Mapper]
public partial class ActivityMapper
{
    [MapProperty("Id.Value", nameof(ActivityResponse.Id))]
    [MapProperty("JobApplicationId.Value", nameof(ActivityResponse.JobApplicationId))]
    [MapProperty("JobApplication.Title", nameof(ActivityResponse.JobTitle))]
    [MapProperty("JobApplication.Company", nameof(ActivityResponse.Company))]
    public partial ActivityResponse ToResponse(Activity activity);
}
