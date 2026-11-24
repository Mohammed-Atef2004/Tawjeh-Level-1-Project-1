using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

/// <summary>
/// Seeds the application with the E1 sample data set:
///   - 3 Companies (with a mix of statuses)
///   - 3 Recruiters
///   - 6 Jobs   (one of them references a Pending company as legacy data)
///   - 5 Candidates
///
/// All sample entities are added directly to the DataStore instances so that
/// seeding can bypass business rules where needed (the legacy Pending-company
/// job would otherwise be rejected by JobService.Add).
/// </summary>
public static class SeedData
{
    public static void Seed(
        DataStore<Company> companyStore,
        DataStore<Recruiter> recruiterStore,
        DataStore<Job> jobStore,
        DataStore<Candidate> candidateStore)
    {
        if (companyStore == null) throw new ArgumentNullException(nameof(companyStore));
        if (recruiterStore == null) throw new ArgumentNullException(nameof(recruiterStore));
        if (jobStore == null) throw new ArgumentNullException(nameof(jobStore));
        if (candidateStore == null) throw new ArgumentNullException(nameof(candidateStore));

        // -------- Companies: mix of statuses --------
        // 1 Approved, 1 Pending, 1 Rejected so the approval workflow can be
        // exercised and so the JobService can demonstrate that only Approved
        // companies can publish new jobs.
        companyStore.Add(new Company(1, "Microsoft", "Software", "Redmond", CompanyStatus.Approved));
        companyStore.Add(new Company(2, "FutureSoft", "Software", "Dubai", CompanyStatus.Pending));
        companyStore.Add(new Company(3, "OldCorp", "Logistics", "Cairo", CompanyStatus.Rejected));

        // -------- Recruiters: all reference an existing company --------
        recruiterStore.Add(new Recruiter(1, "Sarah Connor", "sarah.connor@microsoft.com", 1));
        recruiterStore.Add(new Recruiter(2, "Khalid Ahmed", "khalid.ahmed@futuresoft.com", 2));
        recruiterStore.Add(new Recruiter(3, "Lina Haddad", "lina.haddad@oldcorp.com", 3));

        // -------- Jobs: 6 total --------
        // 5 belong to the Approved company (Microsoft) and one to the Pending
        // company (FutureSoft) as legacy data â€” required by E1 so that the
        // application rejection rule for non-Approved companies can be tested
        // without breaking the rule that NEW jobs cannot be posted for
        // Pending/Rejected companies.
        jobStore.Add(new Job(1, "Senior Backend Engineer", ".NET 8, microservices, Azure",
            18000m, "Dubai", 1, 1, true, new DateTime(2026, 10, 1)));
        jobStore.Add(new Job(2, "Frontend Engineer (React)", "React 18, TypeScript, Tailwind",
            14000m, "Remote", 1, 1, true, new DateTime(2026, 10, 5)));
        jobStore.Add(new Job(3, "DevOps Engineer", "Kubernetes, CI/CD, Azure DevOps",
            20000m, "Abu Dhabi", 1, 1, true, new DateTime(2026, 10, 8)));
        jobStore.Add(new Job(4, "Data Engineer", "SQL Server, Power BI, ETL",
            16000m, "Riyadh", 1, 1, false, new DateTime(2026, 9, 20)));   // closed
        jobStore.Add(new Job(5, "QA Automation Engineer", "Playwright, NUnit, CI integration",
            12000m, "Cairo", 1, 1, true, new DateTime(2026, 10, 10)));
        // Legacy job at the Pending company:
        jobStore.Add(new Job(6, "Junior .NET Developer (Legacy)", "Maintain legacy WinForms app",
            8000m, "Dubai", 2, 2, true, new DateTime(2026, 9, 1)));

        // -------- Candidates: 5, all unique emails --------
        candidateStore.Add(new Candidate(1, "Omar Khalil", "omar.khalil@example.com", "+971501234567", 5));
        candidateStore.Add(new Candidate(2, "Fatima Zahra", "fatima.zahra@example.com", "+971559876543", 3));
        candidateStore.Add(new Candidate(3, "Yusuf Rahman", "yusuf.rahman@example.com", "+966512345678", 7));
        candidateStore.Add(new Candidate(4, "Aisha Nasser", "aisha.nasser@example.com", "+201001234567", 1));
        candidateStore.Add(new Candidate(5, "John Smith", "john.smith@example.com", "+12025550177", 10));
    }
}
