namespace JobApplicationTracker.Models;

/// <summary>
/// A candidate who can apply to jobs.
/// </summary>
public class Candidate : IEntity
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int YearsOfExperience { get; set; }

    public Candidate(int id, string fullName, string email, string phone, int yearsOfExperience)
    {
        // id validation is intentionally NOT enforced here so that the menu
        // can pass 0 and let DataStore<T> auto-assign the next free Id.
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Candidate FullName is required.", nameof(fullName));
        if (!Recruiter.IsValidEmail(email))
            throw new ArgumentException("Candidate Email is not in a valid format.", nameof(email));
        if (yearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative.", nameof(yearsOfExperience));

        Id = id;
        FullName = fullName;
        Email = email;
        Phone = phone ?? string.Empty;
        YearsOfExperience = yearsOfExperience;
    }

    public override string ToString() =>
        $"[{Id}] {FullName} <{Email}> - {YearsOfExperience} yrs - {Phone}";
}
