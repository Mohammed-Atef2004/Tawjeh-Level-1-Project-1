using JobApplicationTracker.Models;using JobApplicationTracker.Events;
namespace JobApplicationTracker.Services;

/// <summary>
/// H1: Application workflow with full validation and duplicate protection.
///
/// <para>
/// A <see cref="HashSet{T}"/> of <c>(int JobId, int CandidateId)</c> tuples tracks
/// every accepted (job, candidate) pair. <see cref="HashSet{T}"/>.Add returns
/// false if the pair already exists, which we use as the duplicate check
/// (no manual loop needed).
/// </para>
/// <para>
/// The HashSet is kept strictly in sync with the underlying
/// <see cref="DataStore{T}"/> of applications: if saving the application fails
/// after reserving the pair, the pair is rolled back; when an application is
/// removed, its pair is also removed.
/// </para>
/// </summary>
public class ApplicationService
{
    private readonly DataStore<Application> _store;
    private readonly JobService _jobService;
    private readonly CandidateService _candidateService;
    private readonly CompanyService _companyService;

    /// <summary>
    /// Duplicate protection: every successful application reserves a
    /// (JobId, CandidateId) pair here. HashSet.Add returns false on duplicate.
    /// </summary>
    private readonly HashSet<(int JobId, int CandidateId)> _applicationPairs = new();
    public event EventHandler<ApplicationSubmittedEventArgs>? ApplicationSubmitted;
    public event EventHandler<ApplicationStatusChangedEventArgs>? ApplicationStatusChanged;
    public ApplicationService(
        DataStore<Application> store,
        JobService jobService,
        CandidateService candidateService,
        CompanyService companyService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _candidateService = candidateService ?? throw new ArgumentNullException(nameof(candidateService));
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
    }

    /// <summary>
    /// The full application workflow (H1 + E4 rules).
    /// All six validations happen before any state mutation.
    /// </summary>
    public (bool Success, string Message, Application? Application) Apply(int jobId, int candidateId)
    {
        // 1. Candidate exists
        var candidate = _candidateService.GetById(candidateId);
        if (candidate == null)
            return (false, $"Cannot apply: Candidate with Id {candidateId} does not exist.", null);

        // 2. Job exists
        var job = _jobService.GetById(jobId);
        if (job == null)
            return (false, $"Cannot apply: Job with Id {jobId} does not exist.", null);

        // 3. Job is open
        if (!job.IsOpen)
            return (false, $"Cannot apply: Job '{job.Title}' is currently closed.", null);

        // 4. Job's company exists
        var company = _companyService.GetById(job.CompanyId);
        if (company == null)
            return (false, $"Cannot apply: Company with Id {job.CompanyId} (referenced by the job) does not exist.", null);

        // 5. Company is Approved
        if (company.Status != CompanyStatus.Approved)
            return (false,
                $"Cannot apply: Company '{company.Name}' has status {company.Status}. " +
                "Applications are only accepted for jobs at Approved companies.", null);

        // 6. Candidate has not already applied (HashSet.Add returns false on duplicate).
        var pair = (jobId, candidateId);
        if (!_applicationPairs.Add(pair))
            return (false,
                $"Cannot apply: Candidate '{candidate.FullName}' has already applied to job '{job.Title}'.", null);

        // All validations passed. Create and save the application.
        Application application;
        try
        {
            application = new Application(
                id: _store.PeekNextId(),
                jobId: jobId,
                candidateId: candidateId,
                appliedDate: DateTime.Now,
                status: ApplicationStatus.Submitted);

            _store.Add(application);
            RaiseApplicationSubmitted(application, job, candidate);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException || ex is ArgumentException)
        {
            // Rollback the HashSet reservation so storage and the set stay consistent.
            _applicationPairs.Remove(pair);
            return (false, $"Failed to save application: {ex.Message}", null);
        }

        return (true,
            $"Application submitted successfully. Application Id: {application.Id}.",
            application);
    }

    public List<Application> GetAll() => _store.GetAll();

    public Application? GetById(int id) => _store.GetById(id);

    /// <summary>
    /// Removes an application and its corresponding (JobId, CandidateId) pair.
    /// Both must succeed or neither must change.
    /// </summary>
    public (bool Success, string Message) Remove(int id)
    {
        if (id <= 0)
            return (false, "Application Id must be a positive integer.");

        var application = _store.GetById(id);
        if (application == null)
            return (false, $"Application with Id {id} not found.");

        // Remove from the DataStore first; if that fails, do not touch the HashSet.
        if (!_store.Remove(id))
            return (false, $"Failed to remove application with Id {id}.");

        // Then remove the pair to keep the two collections in sync.
        _applicationPairs.Remove((application.JobId, application.CandidateId));

        return (true, $"Application {id} removed.");
    }

    /// <summary>
    /// Returns true if the HashSet and the DataStore agree on the number of
    /// tracked pairs. Used by the Practice demonstration to prove consistency.
    /// </summary>
    /// <summary>
    /// Changes the status of an application and raises ApplicationStatusChanged.
    /// </summary>
    public (bool Success, string Message) UpdateStatus(int applicationId, ApplicationStatus newStatus)
    {
        var application = _store.GetById(applicationId);
        if (application == null)
            return (false, $"Application with Id {applicationId} not found.");

        ApplicationStatus oldStatus = application.Status;
        if (oldStatus == newStatus)
            return (true, $"Application {applicationId} is already {newStatus}.");

        application.Status = newStatus;
        if (!_store.Update(application))
            return (false, $"Failed to update application {applicationId}.");

        RaiseApplicationStatusChanged(application, oldStatus, newStatus);
        return (true, $"Application {applicationId} status changed from {oldStatus} to {newStatus}.");
    }

    private void RaiseApplicationSubmitted(Application application, Job job, Candidate candidate)
    {
        var handlers = ApplicationSubmitted?.GetInvocationList();
        if (handlers == null) return;
        foreach (var h in handlers)
        {
            try { ((EventHandler<ApplicationSubmittedEventArgs>)h)(this, new ApplicationSubmittedEventArgs(application, job, candidate)); }
            catch (Exception ex) { Console.WriteLine($"[Event handler error] {ex.Message}"); }
        }
    }

    private void RaiseApplicationStatusChanged(Application application, ApplicationStatus oldStatus, ApplicationStatus newStatus)
    {
        var handlers = ApplicationStatusChanged?.GetInvocationList();
        if (handlers == null) return;
        foreach (var h in handlers)
        {
            try { ((EventHandler<ApplicationStatusChangedEventArgs>)h)(this, new ApplicationStatusChangedEventArgs(application, oldStatus, newStatus)); }
            catch (Exception ex) { Console.WriteLine($"[Event handler error] {ex.Message}"); }
        }
    }
    public bool IsHashSetConsistent()
    {
        if (_store.Count != _applicationPairs.Count)
            return false;

        foreach (var app in _store.GetAll())
        {
            if (!_applicationPairs.Contains((app.JobId, app.CandidateId)))
                return false;
        }
        return true;
    }

    public int ApplicationCount => _store.Count;
    public bool HasApplications(int jobId)
    {
        foreach (var app in _store.GetAll())
        {
            if (app.JobId == jobId)
                return true;
        }
        return false;
    }
}
