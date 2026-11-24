using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

/// <summary>
/// CRUD for recruiters. Validates that a recruiter's CompanyId references an
/// existing company.
/// </summary>
public class RecruiterService
{
    private readonly DataStore<Recruiter> _store;
    private readonly CompanyService _companyService;

    public RecruiterService(DataStore<Recruiter> store, CompanyService companyService)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
    }

    public (bool Success, string Message) Add(Recruiter recruiter)
    {
        if (recruiter == null)
            return (false, "Recruiter cannot be null.");

        if (_companyService.GetById(recruiter.CompanyId) == null)
            return (false, $"Cannot add recruiter: Company with Id {recruiter.CompanyId} does not exist.");

        try
        {
            _store.Add(recruiter);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
        return (true, $"Recruiter '{recruiter.Name}' added with Id {recruiter.Id}.");
    }

    public (bool Success, string Message) Update(Recruiter recruiter)
    {
        if (recruiter == null)
            return (false, "Recruiter cannot be null.");
        if (_companyService.GetById(recruiter.CompanyId) == null)
            return (false, $"Cannot update recruiter: Company with Id {recruiter.CompanyId} does not exist.");
        if (!_store.Update(recruiter))
            return (false, $"Recruiter with Id {recruiter.Id} not found.");
        return (true, $"Recruiter {recruiter.Id} updated.");
    }

    public (bool Success, string Message) Remove(int id)
    {
        if (id <= 0)
            return (false, "Recruiter Id must be a positive integer.");
        if (!_store.Remove(id))
            return (false, $"Recruiter with Id {id} not found.");
        return (true, $"Recruiter {id} removed.");
    }

    public Recruiter? GetById(int id) => _store.GetById(id);

    public List<Recruiter> GetAll() => _store.GetAll();
}
