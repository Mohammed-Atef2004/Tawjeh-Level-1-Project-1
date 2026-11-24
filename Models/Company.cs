namespace JobApplicationTracker.Models;

/// <summary>
/// Lifecycle status of a Company.
/// New companies start as Pending and must be Approved before
/// they are allowed to publish jobs.
/// </summary>
public enum CompanyStatus
{
    Pending,
    Approved,
    Rejected
}

/// <summary>
/// A company that publishes jobs through the tracker.
/// </summary>
public class Company : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Industry { get; set; }
    public string City { get; set; }
    public CompanyStatus Status { get; set; }

    public Company(int id, string name, string industry, string city, CompanyStatus status = CompanyStatus.Pending)
    {
        // id validation is intentionally NOT enforced here so that the menu
        // can pass 0 and let DataStore<T> auto-assign the next free Id.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company Name is required.", nameof(name));

        Id = id;
        Name = name;
        Industry = industry ?? string.Empty;
        City = city ?? string.Empty;
        Status = status;
    }

    public override string ToString() => $"[{Id}] {Name} ({Industry}, {City}) - {Status}";
}
