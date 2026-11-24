using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

using JobApplicationTracker.Events;
/// <summary>
/// CRUD + approval workflow + Undo (H2) for companies.
/// </summary>
public class CompanyService
{
    private readonly DataStore<Company> _store;

    /// <summary>
    /// Stack of pending undo operations. Each entry snapshots the
    /// Company Id and the status that was active BEFORE the change
    /// was applied, so Undo can restore it correctly (LIFO).
    /// </summary>
    private readonly Stack<(int CompanyId, CompanyStatus PreviousStatus)> _undoStack = new();
    public event EventHandler<CompanyApprovedEventArgs>? CompanyApproved;
    public CompanyService(DataStore<Company> store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // ---------- CRUD ----------

    public (bool Success, string Message) Add(Company company)
    {
        if (company == null)
            return (false, "Company cannot be null.");
        try
        {
            _store.Add(company);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
        return (true, $"Company '{company.Name}' added with Id {company.Id}.");
    }

    public (bool Success, string Message) Update(Company company)
    {
        if (company == null)
            return (false, "Company cannot be null.");
        if (!_store.Update(company))
            return (false, $"Company with Id {company.Id} not found.");
        return (true, $"Company {company.Id} updated.");
    }

    public (bool Success, string Message) Remove(int id)
    {
        if (id <= 0)
            return (false, "Company Id must be a positive integer.");
        if (!_store.Remove(id))
            return (false, $"Company with Id {id} not found.");
        return (true, $"Company {id} removed.");
    }

    public Company? GetById(int id) => _store.GetById(id);

    public List<Company> GetAll() => _store.GetAll();

    // ---------- E3: Approval workflow ----------

    /// <summary>
    /// Returns only companies whose Status equals <paramref name="status"/>.
    /// </summary>
    public List<Company> GetByStatus(CompanyStatus status)
    {
        var result = new List<Company>();
        foreach (var c in _store.GetAll())
        {
            if (c.Status == status)
                result.Add(c);
        }
        return result;
    }

    /// <summary>
    /// Changes a company's status and records the previous status on the undo stack.
    /// </summary>
    public (bool Success, string Message) ChangeStatus(int companyId, CompanyStatus newStatus)
    {
        var company = _store.GetById(companyId);
        if (company == null)
            return (false, $"Company with Id {companyId} not found.");

        CompanyStatus oldStatus = company.Status;

        // Push the OLD state BEFORE applying the new one (H2 requirement).
        _undoStack.Push((companyId, oldStatus));

        company.Status = newStatus;
        if (newStatus == CompanyStatus.Approved)
            RaiseCompanyApproved(company);

        return (true, $"Company '{company.Name}' status changed from {oldStatus} to {newStatus}.");
    }

    // ---------- H2: Undo ----------

    /// <summary>
    /// Restores the most recent company status change. Returns false (with a
    /// friendly message) if there is nothing to undo.
    /// </summary>
    public (bool Success, string Message) Undo()
    {
        if (_undoStack.Count == 0)
            return (false, "Nothing to undo: no company status changes have been recorded yet.");

        var entry = _undoStack.Pop();
        var company = _store.GetById(entry.CompanyId);
        if (company == null)
            return (false, $"Cannot undo: company with Id {entry.CompanyId} no longer exists.");

        CompanyStatus current = company.Status;
        company.Status = entry.PreviousStatus;
        return (true, $"Undo applied: Company '{company.Name}' status restored from {current} to {entry.PreviousStatus}.");
    }

    public int UndoCount => _undoStack.Count;
    /// <summary>
    /// Raises CompanyApproved safely (C1): each subscriber is invoked inside
    /// its own try/catch so one failing handler cannot break the others.
    /// </summary>
    private void RaiseCompanyApproved(Company company)
    {
        var handlers = CompanyApproved?.GetInvocationList();
        if (handlers == null) return;
        foreach (var h in handlers)
        {
            try { ((EventHandler<CompanyApprovedEventArgs>)h)(this, new CompanyApprovedEventArgs(company)); }
            catch (Exception ex) { Console.WriteLine($"[Event handler error] {ex.Message}"); }
        }
    }
}
