namespace JobApplicationTracker.Models;

/// <summary>
/// Lifecycle status of a single job application.
/// Sprint 1 only ever creates applications in the Submitted state;
/// later sprints will implement status transitions.
/// </summary>
public enum ApplicationStatus
{
    Submitted,
    UnderReview,
    Interview,
    Accepted,
    Rejected
}

/// <summary>
/// A candidate's application to a specific job.
/// </summary>
public class Application : IEntity
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int CandidateId { get; set; }
    public DateTime AppliedDate { get; set; }
    public ApplicationStatus Status { get; set; }

    public Application(int id, int jobId, int candidateId, DateTime appliedDate, ApplicationStatus status = ApplicationStatus.Submitted)
    {
        // id validation is intentionally NOT enforced here so that the
        // ApplicationService can pass 0 and let DataStore<T> auto-assign.
        if (jobId <= 0)
            throw new ArgumentException("Application must reference a valid Job Id.", nameof(jobId));
        if (candidateId <= 0)
            throw new ArgumentException("Application must reference a valid Candidate Id.", nameof(candidateId));

        Id = id;
        JobId = jobId;
        CandidateId = candidateId;
        AppliedDate = appliedDate;
        Status = status;
    }

    public override string ToString() =>
        $"[{Id}] JobId={JobId}, CandidateId={CandidateId}, {AppliedDate:yyyy-MM-dd HH:mm}, {Status}";
}
