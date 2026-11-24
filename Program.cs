using JobApplicationTracker.Models;

using JobApplicationTracker.Services;
using JobApplicationTracker.Practice;
using JobApplicationTracker.Events;
namespace JobApplicationTracker;

/// <summary>
/// Entry point and console menu system for the Job Application Tracker (Sprint 1).
/// </summary>
internal static class Program
{
    private static AppServices _app = null!;
    private static EventHandler<ApplicationSubmittedEventArgs>? _lambdaSubmittedHandler;

    private static void Main()
    {
        _app = AppServices.Build();

        // Part 2 - M2: subscribe named method handlers + a lambda handler.
        _app.Companies.CompanyApproved += OnCompanyApproved;
        EventHandler<ApplicationSubmittedEventArgs> lambdaSubmitted = (s, e) =>
            Console.WriteLine($"[Lambda] Application submitted: AppId={e.Application.Id}, Job={e.Job.Title}, Candidate={e.Candidate.FullName}");
        _app.Applications.ApplicationSubmitted += lambdaSubmitted;
        _app.Applications.ApplicationStatusChanged += OnApplicationStatusChanged;
        // Keep a reference so we can demonstrate unsubscribing later.
        _lambdaSubmittedHandler = lambdaSubmitted;

        Console.WriteLine("==============================================");
        Console.WriteLine("  Tawjeeh Job Application Tracker - Sprint 1  ");
        Console.WriteLine("==============================================");

        bool running = true;
        while (running)
        {
            ShowMainMenu();
            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": CompaniesMenu();     break;
                case "2": RecruitersMenu();    break;
                case "3": JobsMenu();          break;
                case "4": CandidatesMenu();    break;
                case "5": ApplicationsMenu();  break;
                case "0":
                    running = false;
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please choose a number between 0 and 6.");
                    break;
            }
        }
    }

    // ---------------------------------------------------------------
    // Main menu
    // ---------------------------------------------------------------
    private static void ShowMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("---------- MAIN MENU ----------");
        Console.WriteLine(" 1. Companies");
        Console.WriteLine(" 2. Recruiters");
        Console.WriteLine(" 3. Jobs");
        Console.WriteLine(" 4. Candidates");
        Console.WriteLine(" 5. Applications");
        Console.WriteLine(" 6. Practice demonstrations");
        Console.WriteLine(" 0. Exit");
        Console.WriteLine("------------------------------");
        Console.Write("Choice: ");
    }

    // ---------------------------------------------------------------
    // 1. Companies
    // ---------------------------------------------------------------
    private static void CompaniesMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("------- COMPANIES -------");
            Console.WriteLine(" 1. List all companies");
            Console.WriteLine(" 2. List companies by status");
            Console.WriteLine(" 3. Add company");
            Console.WriteLine(" 4. Update company");
            Console.WriteLine(" 5. Remove company");
            Console.WriteLine(" 6. Approve company (E3)");
            Console.WriteLine(" 7. Reject company (E3)");
            Console.WriteLine(" 8. Undo last status change (H2)");
            Console.WriteLine(" 9. Show undo stack size");
            Console.WriteLine(" 0. Back");
            Console.WriteLine("-------------------------");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListCompanies(_app.Companies.GetAll()); break;
                case "2": ListCompaniesByStatus();               break;
                case "3": AddCompany();                          break;
                case "4": UpdateCompany();                        break;
                case "5": RemoveCompany();                        break;
                case "6": ChangeCompanyStatus(CompanyStatus.Approved); break;
                case "7": ChangeCompanyStatus(CompanyStatus.Rejected); break;
                case "8": UndoCompanyStatus();                    break;
                case "9":
                    Console.WriteLine($"Undo stack contains {_app.Companies.UndoCount} change(s).");
                    break;
                case "0": back = true; break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void ListCompanies(List<Company> companies)
    {
        Console.WriteLine();
        Console.WriteLine($"Companies ({companies.Count}):");
        if (companies.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }
        PrintTable(
            header: new[] { "Id", "Name", "Industry", "City", "Status" },
            rows: companies,
            cellSelector: c => new[] { c.Id.ToString(), c.Name, c.Industry, c.City, c.Status.ToString() });
    }

    private static void ListCompaniesByStatus()
    {
        Console.Write("Status to filter by (Pending / Approved / Rejected): ");
        string? input = Console.ReadLine();
        if (!Enum.TryParse<CompanyStatus>(input, ignoreCase: true, out var status))
        {
            Console.WriteLine("Invalid status. Please enter Pending, Approved, or Rejected.");
            return;
        }
        ListCompanies(_app.Companies.GetByStatus(status));
    }

    private static void AddCompany()
    {
        string name      = ConsoleHelper.ReadRequiredString("Name: ");
        string industry  = ConsoleHelper.ReadString("Industry: ");
        string city       = ConsoleHelper.ReadString("City: ");
        // New companies always start as Pending per the spec; status changes
        // are made through the Approve/Reject menu items.
        var company = new Company(0, name, industry, city, CompanyStatus.Pending);

        var (ok, msg) = _app.Companies.Add(company);
        Console.WriteLine(msg);
    }

    private static void UpdateCompany()
    {
        int id = ConsoleHelper.ReadInt("Company Id to update: ");
        var existing = _app.Companies.GetById(id);
        if (existing == null)
        {
            Console.WriteLine($"Company with Id {id} not found.");
            return;
        }

        Console.WriteLine($"Leave a field blank to keep the current value.");
        string name = ConsoleHelper.ReadStringOrDefault("Name", existing.Name);
        string industry = ConsoleHelper.ReadStringOrDefault("Industry", existing.Industry);
        string city = ConsoleHelper.ReadStringOrDefault("City", existing.City);

        // We must preserve the existing Id and Status.
        var updated = new Company(existing.Id,
            string.IsNullOrWhiteSpace(name) ? existing.Name : name,
            string.IsNullOrWhiteSpace(industry) ? existing.Industry : industry,
            string.IsNullOrWhiteSpace(city) ? existing.City : city,
            existing.Status);

        var (ok, msg) = _app.Companies.Update(updated);
        Console.WriteLine(msg);
    }

    private static void RemoveCompany()
    {
        int id = ConsoleHelper.ReadInt("Company Id to remove: ");
        var (ok, msg) = _app.Companies.Remove(id);
        Console.WriteLine(msg);
    }

    private static void ChangeCompanyStatus(CompanyStatus newStatus)
    {
        int id = ConsoleHelper.ReadInt($"Company Id to mark as {newStatus}: ");
        var (ok, msg) = _app.Companies.ChangeStatus(id, newStatus);
        Console.WriteLine(msg);
    }

    private static void UndoCompanyStatus()
    {
        var (ok, msg) = _app.Companies.Undo();
        Console.WriteLine(msg);
    }

    // ---------------------------------------------------------------
    // 2. Recruiters
    // ---------------------------------------------------------------
    private static void RecruitersMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("------- RECRUITERS -------");
            Console.WriteLine(" 1. List recruiters");
            Console.WriteLine(" 2. Add recruiter");
            Console.WriteLine(" 3. Update recruiter");
            Console.WriteLine(" 4. Remove recruiter");
            Console.WriteLine(" 0. Back");
            Console.WriteLine("-------------------------");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListRecruiters(); break;
                case "2": AddRecruiter();   break;
                case "3": UpdateRecruiter();break;
                case "4": RemoveRecruiter();break;
                case "0": back = true;     break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void ListRecruiters()
    {
        var recruiters = _app.Recruiters.GetAll();
        Console.WriteLine();
        Console.WriteLine($"Recruiters ({recruiters.Count}):");
        if (recruiters.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }
        PrintTable(
            header: new[] { "Id", "Name", "Email", "CompanyId" },
            rows: recruiters,
            cellSelector: r => new[] { r.Id.ToString(), r.Name, r.Email, r.CompanyId.ToString() });
    }

    private static void AddRecruiter()
    {
        string name  = ConsoleHelper.ReadRequiredString("Name: ");
        string email = ConsoleHelper.ReadRequiredEmail("Email: ");
        int companyId = ConsoleHelper.ReadInt("CompanyId: ");

        try
        {
            var recruiter = new Recruiter(0, name, email, companyId);
            var (ok, msg) = _app.Recruiters.Add(recruiter);
            Console.WriteLine(msg);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void UpdateRecruiter()
    {
        int id = ConsoleHelper.ReadInt("Recruiter Id to update: ");
        var existing = _app.Recruiters.GetById(id);
        if (existing == null)
        {
            Console.WriteLine($"Recruiter with Id {id} not found.");
            return;
        }

        Console.WriteLine("Leave a field blank to keep the current value.");
        string name  = ConsoleHelper.ReadStringOrDefault("Name", existing.Name);
        string email = ConsoleHelper.ReadEmailOrDefault("Email", existing.Email);
        int companyId = ConsoleHelper.ReadIntOrDefault("CompanyId", existing.CompanyId);

        try
        {
            var updated = new Recruiter(existing.Id,
                string.IsNullOrWhiteSpace(name) ? existing.Name : name,
                string.IsNullOrWhiteSpace(email) ? existing.Email : email,
                companyId);

            var (ok, msg) = _app.Recruiters.Update(updated);
            Console.WriteLine(msg);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void RemoveRecruiter()
    {
        int id = ConsoleHelper.ReadInt("Recruiter Id to remove: ");
        var (ok, msg) = _app.Recruiters.Remove(id);
        Console.WriteLine(msg);
    }

    // ---------------------------------------------------------------
    // 3. Jobs
    // ---------------------------------------------------------------
    private static void JobsMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--------- JOBS ---------");
            Console.WriteLine(" 1. List jobs");
            Console.WriteLine(" 2. Look up job by Id (uses Dictionary - E5)");
            Console.WriteLine(" 3. Add job");
            Console.WriteLine(" 4. Update job");
            Console.WriteLine(" 5. Remove job");
            Console.WriteLine(" 6. Show dictionary consistency check");
            Console.WriteLine(" 0. Back");
            Console.WriteLine(" 7. Search jobs by title keyword (M3)");
            Console.WriteLine(" 8. Search jobs by minimum salary (M3)");
            Console.WriteLine(" 9. Search jobs (keyword + min salary) (M3)");
            Console.WriteLine("10. Sort jobs by salary ascending (M3)");
            Console.WriteLine("11. Sort jobs by salary descending (M3)");
            Console.WriteLine("12. Sort jobs by title (M3)");
            Console.WriteLine("------------------------");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListJobs();             break;
                case "2": LookupJobById();        break;
                case "3": AddJob();               break;
                case "4": UpdateJob();            break;
                case "5": RemoveJob();            break;
                case "6": ShowJobDictionaryCheck();break;
                case "7": SearchJobsByTitle(); break;
                case "8": SearchJobsByMinSalary(); break;
                case "9": SearchJobsCombined(); break;
                case "10": SortJobsBySalary(true); break;
                case "11": SortJobsBySalary(false); break;
                case "12": SortJobsByTitle(); break;
                case "0": back = true;            break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void ListJobs()
    {
        var jobs = _app.Jobs.GetAll();
        Console.WriteLine();
        Console.WriteLine($"Jobs ({jobs.Count}):");
        if (jobs.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }
        PrintTable(
            header: new[] { "Id", "Title", "Salary", "Location", "Open", "CompanyId", "RecruiterId" },
            rows: jobs,
            cellSelector: j => new[]
            {
                j.Id.ToString(),
                j.Title,
                j.Salary.ToString("0.00"),
                j.Location,
                j.IsOpen ? "Yes" : "No",
                j.CompanyId.ToString(),
                j.RecruiterId.ToString()
            });
    }

    private static void LookupJobById()
    {
        int id = ConsoleHelper.ReadInt("Job Id to look up: ");
        var job = _app.Jobs.GetById(id);
        if (job == null)
        {
            Console.WriteLine($"Job with Id {id} not found in the dictionary.");
            return;
        }
        Console.WriteLine($"Dictionary lookup result: {job}");
    }

    private static void AddJob()
    {
        string title       = ConsoleHelper.ReadRequiredString("Title: ");
        string description = ConsoleHelper.ReadString("Description: ");
        decimal salary     = ConsoleHelper.ReadPositiveDecimal("Salary (> 0): ");
        string location    = ConsoleHelper.ReadString("Location: ");
        int companyId      = ConsoleHelper.ReadInt("CompanyId: ");
        int recruiterId    = ConsoleHelper.ReadInt("RecruiterId: ");

        try
        {
            var job = new Job(0, title, description, salary, location, companyId, recruiterId, true);
            var (ok, msg) = _app.Jobs.Add(job);
            Console.WriteLine(msg);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void UpdateJob()
    {
        int id = ConsoleHelper.ReadInt("Job Id to update: ");
        var existing = _app.Jobs.GetById(id);
        if (existing == null)
        {
            Console.WriteLine($"Job with Id {id} not found.");
            return;
        }

        Console.WriteLine("Leave a field blank to keep the current value.");
        string title = ConsoleHelper.ReadStringOrDefault("Title", existing.Title);
        string description = ConsoleHelper.ReadStringOrDefault("Description", existing.Description);
        decimal salary = ConsoleHelper.ReadDecimalOrDefault("Salary", existing.Salary);
        string location = ConsoleHelper.ReadStringOrDefault("Location", existing.Location);
        int companyId = ConsoleHelper.ReadIntOrDefault("CompanyId", existing.CompanyId);
        int recruiterId = ConsoleHelper.ReadIntOrDefault("RecruiterId", existing.RecruiterId);
        Console.Write($"Is open (Y/N, current={existing.IsOpen}): ");
        string? openInput = Console.ReadLine();
        bool isOpen = existing.IsOpen;
        if (!string.IsNullOrWhiteSpace(openInput))
            isOpen = openInput.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);

        try
        {
            var updated = new Job(existing.Id,
                string.IsNullOrWhiteSpace(title) ? existing.Title : title,
                string.IsNullOrWhiteSpace(description) ? existing.Description : description,
                salary,
                string.IsNullOrWhiteSpace(location) ? existing.Location : location,
                companyId,
                recruiterId,
                isOpen,
                existing.PostedDate);

            var (ok, msg) = _app.Jobs.Update(updated);
            Console.WriteLine(msg);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void RemoveJob()
    {
        int id = ConsoleHelper.ReadInt("Job Id to remove: ");
        var (ok, msg) = _app.Jobs.Remove(id);
        Console.WriteLine(msg);
    }

    private static void ShowJobDictionaryCheck()
    {
        bool consistent = _app.Jobs.IsDictionaryConsistent();
        Console.WriteLine($"JobService dictionary is {(consistent ? "CONSISTENT" : "INCONSISTENT")} " +
                          $"(store={_app.Jobs.GetAll().Count}, dictionary={_app.Jobs.DictionaryCount}).");
    }

    // ---------------------------------------------------------------
    // 4. Candidates
    // ---------------------------------------------------------------
    private static void CandidatesMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("------- CANDIDATES -------");
            Console.WriteLine(" 1. List candidates");
            Console.WriteLine(" 2. Add candidate");
            Console.WriteLine(" 3. Update candidate");
            Console.WriteLine(" 4. Remove candidate");
            Console.WriteLine(" 0. Back");
            Console.WriteLine("-------------------------");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListCandidates(); break;
                case "2": AddCandidate();   break;
                case "3": UpdateCandidate();break;
                case "4": RemoveCandidate();break;
                case "0": back = true;      break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void ListCandidates()
    {
        var candidates = _app.Candidates.GetAll();
        Console.WriteLine();
        Console.WriteLine($"Candidates ({candidates.Count}):");
        if (candidates.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }
        PrintTable(
            header: new[] { "Id", "FullName", "Email", "Phone", "YrsExp" },
            rows: candidates,
            cellSelector: c => new[]
            {
                c.Id.ToString(),
                c.FullName,
                c.Email,
                c.Phone,
                c.YearsOfExperience.ToString()
            });
    }

    private static void AddCandidate()
    {
        string fullName = ConsoleHelper.ReadRequiredString("Full name: ");
        string email    = ConsoleHelper.ReadRequiredEmail("Email: ");
        string phone    = ConsoleHelper.ReadString("Phone: ");
        int years       = ConsoleHelper.ReadNonNegativeInt("Years of experience (>= 0): ");

        try
        {
            var candidate = new Candidate(0, fullName, email, phone, years);
            var (ok, msg) = _app.Candidates.Add(candidate);
            Console.WriteLine(msg);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void UpdateCandidate()
    {
        int id = ConsoleHelper.ReadInt("Candidate Id to update: ");
        var existing = _app.Candidates.GetById(id);
        if (existing == null)
        {
            Console.WriteLine($"Candidate with Id {id} not found.");
            return;
        }

        Console.WriteLine("Leave a field blank to keep the current value.");
        string fullName = ConsoleHelper.ReadStringOrDefault("Full name", existing.FullName);
        string email    = ConsoleHelper.ReadEmailOrDefault("Email", existing.Email);
        string phone    = ConsoleHelper.ReadStringOrDefault("Phone", existing.Phone);
        int years       = ConsoleHelper.ReadIntOrDefault("Years of experience", existing.YearsOfExperience);

        try
        {
            var updated = new Candidate(existing.Id,
                string.IsNullOrWhiteSpace(fullName) ? existing.FullName : fullName,
                string.IsNullOrWhiteSpace(email) ? existing.Email : email,
                string.IsNullOrWhiteSpace(phone) ? existing.Phone : phone,
                years);

            var (ok, msg) = _app.Candidates.Update(updated);
            Console.WriteLine(msg);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void RemoveCandidate()
    {
        int id = ConsoleHelper.ReadInt("Candidate Id to remove: ");
        var (ok, msg) = _app.Candidates.Remove(id);
        Console.WriteLine(msg);
    }

    // ---------------------------------------------------------------
    // 5. Applications (H1)
    // ---------------------------------------------------------------
    private static void ApplicationsMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("------- APPLICATIONS -------");
            Console.WriteLine(" 1. List applications");
            Console.WriteLine(" 2. Apply to a job (H1 workflow)");
            Console.WriteLine(" 3. Remove application");
            Console.WriteLine(" 4. Update application status (M2)");
            Console.WriteLine(" 4. Show HashSet consistency check");
            Console.WriteLine(" 5. Update application status (M2)");
            Console.WriteLine(" 0. Back");
            Console.WriteLine("----------------------------");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ListApplications(); break;
                case "2": ApplyToJob();       break;
                case "3": RemoveApplication();break;
                case "5": UpdateApplicationStatus(); break;
                case "4": ShowApplicationCheck(); break;
                case "0": back = true;        break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }

    private static void ListApplications()
    {
        var apps = _app.Applications.GetAll();
        Console.WriteLine();
        Console.WriteLine($"Applications ({apps.Count}):");
        if (apps.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }
        PrintTable(
            header: new[] { "Id", "JobId", "CandidateId", "AppliedDate", "Status" },
            rows: apps,
            cellSelector: a => new[]
            {
                a.Id.ToString(),
                a.JobId.ToString(),
                a.CandidateId.ToString(),
                a.AppliedDate.ToString("yyyy-MM-dd HH:mm"),
                a.Status.ToString()
            });
    }

    private static void ApplyToJob()
    {
        int jobId      = ConsoleHelper.ReadInt("Job Id to apply to: ");
        int candidateId = ConsoleHelper.ReadInt("Candidate Id applying: ");

        var (ok, msg, _) = _app.Applications.Apply(jobId, candidateId);
        Console.WriteLine(msg);
    }

    private static void UpdateApplicationStatus()
    {
        int id = ConsoleHelper.ReadPositiveInt("Application Id: ");
        Console.WriteLine("New status (Submitted / UnderReview / Interview / Accepted / Rejected): ");
        string? input = Console.ReadLine();
        if (!Enum.TryParse<ApplicationStatus>(input, ignoreCase: true, out var status))
        {
            Console.WriteLine("Invalid status.");
            return;
        }
        var (ok, msg) = _app.Applications.UpdateStatus(id, status);
        Console.WriteLine(msg);
    }

    private static void RemoveApplication()
    {
        int id = ConsoleHelper.ReadInt("Application Id to remove: ");
        var (ok, msg) = _app.Applications.Remove(id);
        Console.WriteLine(msg);
    }

    private static void ShowApplicationCheck()
    {
        bool consistent = _app.Applications.IsHashSetConsistent();
        Console.WriteLine($"ApplicationService HashSet is {(consistent ? "CONSISTENT" : "INCONSISTENT")} " +
                          $"(applications={_app.Applications.ApplicationCount}).");
    }

    // ---------------------------------------------------------------
    // 6. Practice demonstrations
    // ---------------------------------------------------------------


    private static void PracticeMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("------- PRACTICE -------");
            Console.WriteLine(" 1. Collections practice (List, Dictionary, HashSet, Queue, Stack)");
            Console.WriteLine(" 2. Generics practice (method, class, constraints)");
            Console.WriteLine(" 3. Events demonstration (M2, C1)");
            Console.WriteLine(" 0. Back");
            Console.WriteLine("-----------------------");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": CollectionsPractice.RunAll(); break;
                case "2": GenericsPractice.RunAll();    break;
                case "3": EventsDemo();                 break;
                case "0": back = true;                  break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }
    // Part 2 - M2: named method event handlers.
    private static void OnCompanyApproved(object? sender, CompanyApprovedEventArgs e)
    {
        Console.WriteLine($"[Named handler] Company approved: {e.Company.Name} (Id={e.Company.Id}).");
    }

    private static void OnApplicationStatusChanged(object? sender, ApplicationStatusChangedEventArgs e)
    {
        Console.WriteLine($"[Named handler] Application {e.Application.Id} status changed: {e.OldStatus} -> {e.NewStatus}.");
    }

    // Part 2 - M2: practice subscribing and unsubscribing (C1-safe via GetInvocationList).
    private static void EventsDemo()
    {
        Console.WriteLine();
        Console.WriteLine("===== EVENTS DEMONSTRATION =====");

        // Subscribe an extra lambda handler, then unsubscribe it to prove the
        // subscription can be removed.
        EventHandler<CompanyApprovedEventArgs> extra = (s, e) =>
            Console.WriteLine($"[Extra lambda] {e.Company.Name} was approved.");
        _app.Companies.CompanyApproved += extra;

        Console.WriteLine("Subscribed an extra lambda handler. Approving company 2 (FutureSoft)...");
        var (ok, msg) = _app.Companies.ChangeStatus(2, CompanyStatus.Approved);
        Console.WriteLine(msg);

        Console.WriteLine("Unsubscribing the extra lambda handler.");
        _app.Companies.CompanyApproved -= extra;

        Console.WriteLine("Approving company 3 (OldCorp) - extra handler should NOT fire now.");
        var (ok2, msg2) = _app.Companies.ChangeStatus(3, CompanyStatus.Approved);
        Console.WriteLine(msg2);

        Console.WriteLine("===== END OF EVENTS DEMONSTRATION =====");
    }
    private static void SearchJobsByTitle()
    {
        string keyword = ConsoleHelper.ReadRequiredString("Title keyword: ");
        var results = _app.Jobs.SearchByTitle(keyword);
        PrintJobs(results, $"Jobs matching title '{keyword}'");
    }

    private static void SearchJobsByMinSalary()
    {
        decimal min = ConsoleHelper.ReadDecimal("Minimum salary: ");
        var results = _app.Jobs.SearchByMinSalary(min);
        PrintJobs(results, $"Jobs with salary >= {min}");
    }

    private static void SearchJobsCombined()
    {
        string keyword = ConsoleHelper.ReadString("Title keyword (blank = any): ");
        decimal min = ConsoleHelper.ReadDecimalOrDefault("Minimum salary", 0);
        var results = _app.Jobs.Search(string.IsNullOrWhiteSpace(keyword) ? null : keyword, min <= 0 ? (decimal?)null : min);
        PrintJobs(results, "Combined search results");
    }

    private static void SortJobsBySalary(bool ascending)
    {
        var original = _app.Jobs.GetAll();
        int before = original.Count;
        var sorted = _app.Jobs.SortBySalary(ascending);
        Console.WriteLine($"Sorted by salary {(ascending ? "ascending" : "descending")} (original count={before}, sorted count={sorted.Count}).");
        PrintJobs(sorted, "Sorted jobs");
    }

    private static void SortJobsByTitle()
    {
        var sorted = _app.Jobs.SortByTitle();
        PrintJobs(sorted, "Jobs sorted by title");
    }

    private static void PrintJobs(List<Job> jobs, string title)
    {
        Console.WriteLine();
        Console.WriteLine(title + $" ({jobs.Count}):");
        if (jobs.Count == 0)
        {
            Console.WriteLine("  (none)");
            return;
        }
        PrintTable(
            header: new[] { "Id", "Title", "Salary", "Location", "Open", "CompanyId" },
            rows: jobs,
            cellSelector: j => new[] { j.Id.ToString(), j.Title, j.Salary.ToString("0.00"), j.Location, j.IsOpen ? "Yes" : "No", j.CompanyId.ToString() });
    }

    // Helpers: aligned console table + safe input
    // ---------------------------------------------------------------
    private static void PrintTable<T>(string[] header, List<T> rows, Func<T, string[]> cellSelector)
    {
        int columnCount = header.Length;
        int[] widths = new int[columnCount];
        for (int i = 0; i < columnCount; i++)
            widths[i] = header[i].Length;

        foreach (var row in rows)
        {
            var cells = cellSelector(row);
            for (int i = 0; i < columnCount && i < cells.Length; i++)
            {
                if (cells[i].Length > widths[i])
                    widths[i] = cells[i].Length;
            }
        }

        // Build the table divider without using LINQ (Sprint 1 forbids LINQ).
        var parts = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
            parts[i] = new string('-', widths[i] + 2);
        string divider = "+" + string.Join("+", parts) + "+";

        Console.WriteLine(divider);
        // header row
        var headerCells = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
            headerCells[i] = " " + header[i].PadRight(widths[i]) + " ";
        Console.WriteLine("|" + string.Join("|", headerCells) + "|");
        Console.WriteLine(divider);

        foreach (var row in rows)
        {
            var cells = cellSelector(row);
            var rowCells = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                string text = i < cells.Length ? cells[i] : "";
                rowCells[i] = " " + text.PadRight(widths[i]) + " ";
            }
            Console.WriteLine("|" + string.Join("|", rowCells) + "|");
        }
        Console.WriteLine(divider);
    }
}


