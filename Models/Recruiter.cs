namespace JobApplicationTracker.Models;

/// <summary>
/// A recruiter who belongs to a company and posts jobs on its behalf.
/// </summary>
public class Recruiter : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int CompanyId { get; set; }

    public Recruiter(int id, string name, string email, int companyId)
    {
        // id validation is intentionally NOT enforced here so that the menu
        // can pass 0 and let DataStore<T> auto-assign the next free Id.
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recruiter Name is required.", nameof(name));
        if (!IsValidEmail(email))
            throw new ArgumentException("Recruiter Email is not in a valid format.", nameof(email));
        if (companyId <= 0)
            throw new ArgumentException("Recruiter must reference a valid Company Id.", nameof(companyId));

        Id = id;
        Name = name;
        Email = email;
        CompanyId = companyId;
    }

    /// <summary>
    /// Lightweight email format check used for both Recruiter and Candidate.
    /// A full regex is intentionally avoided to keep the reference solution simple.
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        // Must contain exactly one '@' and at least one '.' after it.
        int at = email.IndexOf('@');
        if (at <= 0 || at == email.Length - 1)
            return false;
        string domain = email.Substring(at + 1);
        return domain.Contains('.') && domain.IndexOf('.') != domain.Length - 1;
    }

    public override string ToString() => $"[{Id}] {Name} <{Email}> (CompanyId={CompanyId})";
}
