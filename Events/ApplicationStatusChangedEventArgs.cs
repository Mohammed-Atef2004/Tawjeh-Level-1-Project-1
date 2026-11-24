namespace JobApplicationTracker.Events;

/// <summary>
/// Carries information about an application status change.
/// </summary>
public class ApplicationStatusChangedEventArgs : EventArgs
{
    public Models.Application Application { get; }
    public Models.ApplicationStatus OldStatus { get; }
    public Models.ApplicationStatus NewStatus { get; }

    public ApplicationStatusChangedEventArgs(Models.Application application, Models.ApplicationStatus oldStatus, Models.ApplicationStatus newStatus)
    {
        Application = application;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
