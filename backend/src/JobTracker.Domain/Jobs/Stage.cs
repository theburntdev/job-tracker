namespace JobTracker.Domain.Jobs;

public enum Stage
{
    Applied = 0,
    Screening = 1,
    Interviewing = 2,
    Offer = 3,
    Rejected = 4,
    Withdrawn = 5
}
