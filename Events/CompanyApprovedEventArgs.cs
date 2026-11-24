namespace JobApplicationTracker.Events;

/// <summary>
/// Carries information about a company that has just been approved.
/// </summary>
public class CompanyApprovedEventArgs : EventArgs
{
    public Models.Company Company { get; }

    public CompanyApprovedEventArgs(Models.Company company)
    {
        Company = company;
    }
}
