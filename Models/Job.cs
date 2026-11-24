namespace JobApplicationTracker.Models;

/// <summary>
/// A job posting belonging to a company and posted by a recruiter.
/// </summary>
public class Job : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Salary { get; set; }
    public string Location { get; set; }
    public bool IsOpen { get; set; }
    public DateTime PostedDate { get; set; }
    public int CompanyId { get; set; }
    public int RecruiterId { get; set; }

    public Job(
        int id,
        string title,
        string description,
        decimal salary,
        string location,
        int companyId,
        int recruiterId,
        bool isOpen = true,
        DateTime? postedDate = null)
    {
        // id validation is intentionally NOT enforced here so that the menu
        // can pass 0 and let DataStore<T> auto-assign the next free Id.
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Job Title is required.", nameof(title));
        if (salary <= 0)
            throw new ArgumentException("Job Salary must be greater than zero.", nameof(salary));
        if (companyId <= 0)
            throw new ArgumentException("Job must reference a valid Company Id.", nameof(companyId));
        if (recruiterId <= 0)
            throw new ArgumentException("Job must reference a valid Recruiter Id.", nameof(recruiterId));

        Id = id;
        Title = title;
        Description = description ?? string.Empty;
        Salary = salary;
        Location = location ?? string.Empty;
        IsOpen = isOpen;
        PostedDate = postedDate ?? DateTime.Now;
        CompanyId = companyId;
        RecruiterId = recruiterId;
    }

    public override string ToString() =>
        $"[{Id}] {Title} - {Salary:C} - {Location} - {(IsOpen ? "Open" : "Closed")} (CompanyId={CompanyId}, RecruiterId={RecruiterId})";
}
