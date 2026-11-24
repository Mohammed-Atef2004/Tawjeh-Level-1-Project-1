using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

/// <summary>
/// CRUD for jobs.
///
/// <para>
/// E5 requirement: a <see cref="Dictionary{TKey,TValue}"/> of <c>(int Id, Job)</c>
/// is maintained in parallel with the underlying <see cref="DataStore{T}"/> and
/// is used for actual Id-based lookups. The dictionary is re-synchronised with
/// the store after every Add/Update/Remove and is also seeded from the store in
/// the constructor, so the two collections can never drift apart.
/// </para>
/// <para>
/// E3 requirement: Add() refuses to post jobs for companies whose status is
/// Pending or Rejected. Only Approved companies can publish new jobs.
/// </para>
/// </summary>
public class JobService
{
    private readonly DataStore<Job> _store;
    private readonly CompanyService _companyService;    public ApplicationService? ApplicationService { get; set; }
    /// <summary>
    /// The dictionary required by E5. Used for fast Id-based lookup (O(1) average).
    /// Kept private so external code cannot mutate it without going through Add/Update/Remove.
    /// </summary>
    private readonly Dictionary<int, Job> _jobDictionary = new();

    public JobService(DataStore<Job> store, CompanyService companyService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));

        // Sync the dictionary from the store at construction time so any data
        // that was seeded directly into the DataStore before this service
        // existed (e.g. legacy Pending-company job) is also indexed.
        foreach (var job in _store.GetAll())
        {
            _jobDictionary[job.Id] = job;
        }
    }

    public (bool Success, string Message) Add(Job job)
    {
        if (job == null)
            return (false, "Job cannot be null.");

        var company = _companyService.GetById(job.CompanyId);
        if (company == null)
            return (false, $"Cannot post job: Company with Id {job.CompanyId} does not exist.");

        // E3: only Approved companies can publish new jobs.
        if (company.Status != CompanyStatus.Approved)
            return (false,
                $"Cannot post job for company '{company.Name}': its status is {company.Status}. " +
                "Only Approved companies can publish new jobs.");

        try
        {
            _store.Add(job);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }

        // Keep the dictionary in sync with the store.
        _jobDictionary[job.Id] = job;

        return (true, $"Job '{job.Title}' added with Id {job.Id}.");
    }

    public (bool Success, string Message) Update(Job job)
    {
        if (job == null)
            return (false, "Job cannot be null.");

        if (!_store.Update(job))
            return (false, $"Job with Id {job.Id} not found.");

        _jobDictionary[job.Id] = job;
        return (true, $"Job {job.Id} updated.");
    }

    public (bool Success, string Message) Remove(int id)
    {
        if (id <= 0)
            return (false, "Job Id must be a positive integer.");

        if (ApplicationService != null && ApplicationService.HasApplications(id))
            return (false, $"Cannot remove job {id}: it still has applications.");

        if (!_store.Remove(id))
            return (false, $"Job with Id {id} not found.");

        _jobDictionary.Remove(id);
        return (true, $"Job {id} removed.");
    }

    /// <summary>
    /// Id-based lookup. Uses the dictionary directly (O(1) average), per E5.
    /// </summary>
    public Job? GetById(int id)
    {
        if (id <= 0)
            return null;
        return _jobDictionary.TryGetValue(id, out var job) ? job : null;
    }

    public List<Job> GetAll() => _store.GetAll();

    /// <summary>
    /// Returns true if the dictionary and the DataStore agree on the count.
    /// Used by the Practice demonstration to prove that the two collections
    /// are kept consistent.
    /// </summary>
    public bool IsDictionaryConsistent()
    {
        if (_store.Count != _jobDictionary.Count)
            return false;

        foreach (var job in _store.GetAll())
        {
            if (!_jobDictionary.TryGetValue(job.Id, out var dictJob) || !ReferenceEquals(dictJob, job))
                return false;
        }
        return true;
    }

    public int DictionaryCount => _jobDictionary.Count;
    // ---- Part 2 - M3: search & sort (no LINQ) ----
    public List<Job> SearchByTitle(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<Job>(_store.GetAll());
        return _store.Find(j => j.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public List<Job> SearchByMinSalary(decimal minSalary)
    {
        return _store.Find(j => j.Salary >= minSalary);
    }

    public List<Job> Search(string? keyword, decimal? minSalary)
    {
        return _store.Find(j =>
            (string.IsNullOrWhiteSpace(keyword) || j.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)) &&
            (!minSalary.HasValue || j.Salary >= minSalary.Value));
    }

    public List<Job> SortBySalary(bool ascending)
    {
        return _store.Sort(ascending
            ? (a, b) => a.Salary.CompareTo(b.Salary)
            : (a, b) => b.Salary.CompareTo(a.Salary));
    }

    public List<Job> SortByTitle()
    {
        return _store.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasApplications(int jobId)
    {
        if (ApplicationService == null) return false;
        foreach (var app in ApplicationService.GetAll())
        {
            if (app.JobId == jobId)
                return true;
        }
        return false;
    }
}
