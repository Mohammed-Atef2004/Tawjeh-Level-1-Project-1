using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

/// <summary>
/// CRUD for candidates. Enforces email uniqueness.
/// </summary>
public class CandidateService
{
    private readonly DataStore<Candidate> _store;

    public CandidateService(DataStore<Candidate> store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public (bool Success, string Message) Add(Candidate candidate)
    {
        if (candidate == null)
            return (false, "Candidate cannot be null.");

        // Email must be unique (case-insensitive).
        foreach (var existing in _store.GetAll())
        {
            if (string.Equals(existing.Email, candidate.Email, StringComparison.OrdinalIgnoreCase))
                return (false, $"A candidate with email '{candidate.Email}' already exists.");
        }

        try
        {
            _store.Add(candidate);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
        return (true, $"Candidate '{candidate.FullName}' added with Id {candidate.Id}.");
    }

    public (bool Success, string Message) Update(Candidate candidate)
    {
        if (candidate == null)
            return (false, "Candidate cannot be null.");

        // Email uniqueness on update must ignore the candidate being updated itself.
        foreach (var existing in _store.GetAll())
        {
            if (existing.Id != candidate.Id &&
                string.Equals(existing.Email, candidate.Email, StringComparison.OrdinalIgnoreCase))
                return (false, $"A different candidate already uses email '{candidate.Email}'.");
        }

        if (!_store.Update(candidate))
            return (false, $"Candidate with Id {candidate.Id} not found.");
        return (true, $"Candidate {candidate.Id} updated.");
    }

    public (bool Success, string Message) Remove(int id)
    {
        if (id <= 0)
            return (false, "Candidate Id must be a positive integer.");
        if (!_store.Remove(id))
            return (false, $"Candidate with Id {id} not found.");
        return (true, $"Candidate {id} removed.");
    }

    public Candidate? GetById(int id) => _store.GetById(id);

    public List<Candidate> GetAll() => _store.GetAll();
}
