using JobApplicationTracker.Models;
using JobApplicationTracker.Services;

namespace JobApplicationTracker;

internal sealed class AppServices
{
    public CompanyService     Companies     { get; }
    public RecruiterService   Recruiters     { get; }
    public JobService         Jobs           { get; }
    public CandidateService   Candidates     { get; }
    public ApplicationService Applications   { get; }

    private AppServices(
        CompanyService companies,
        RecruiterService recruiters,
        JobService jobs,
        CandidateService candidates,
        ApplicationService applications)
    {
        Companies     = companies;
        Recruiters     = recruiters;
        Jobs           = jobs;
        Candidates     = candidates;
        Applications   = applications;
    }

    public static AppServices Build()
    {
        var companyStore     = new DataStore<Company>();
        var recruiterStore   = new DataStore<Recruiter>();
        var jobStore         = new DataStore<Job>();
        var candidateStore   = new DataStore<Candidate>();
        var applicationStore = new DataStore<Application>();

        SeedData.Seed(companyStore, recruiterStore, jobStore, candidateStore);

        var companyService   = new CompanyService(companyStore);
        var recruiterService = new RecruiterService(recruiterStore, companyService);
        var jobService       = new JobService(jobStore, companyService);
        var candidateService = new CandidateService(candidateStore);
        var applicationService = new ApplicationService(applicationStore, jobService, candidateService, companyService);

        jobService.ApplicationService = applicationService;

        return new AppServices(companyService, recruiterService, jobService, candidateService, applicationService);
    }
}
