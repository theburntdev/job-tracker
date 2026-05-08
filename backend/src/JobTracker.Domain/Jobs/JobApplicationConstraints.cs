namespace JobTracker.Domain.Jobs;

public static class JobApplicationConstraints
{
    public const int TitleMaxLength       = 200;
    public const int CompanyMaxLength     = 200;
    public const int LocationMaxLength    = 200;
    public const int UrlMaxLength         = 2048;
    public const int DescriptionMaxLength = 9000;
}
