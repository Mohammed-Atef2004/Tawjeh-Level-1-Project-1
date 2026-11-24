namespace JobApplicationTracker.Events;

/// <summary>
/// Carries information about a newly submitted application.
/// </summary>
public class ApplicationSubmittedEventArgs : EventArgs
{
    public Models.Application Application { get; }
    public Models.Job Job { get; }
    public Models.Candidate Candidate { get; }

    public ApplicationSubmittedEventArgs(Models.Application application, Models.Job job, Models.Candidate candidate)
    {
        Application = application;
        Job = job;
        Candidate = candidate;
    }
}
