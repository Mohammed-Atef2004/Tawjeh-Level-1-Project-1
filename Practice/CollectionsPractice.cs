namespace JobApplicationTracker.Practice;

/// <summary>
/// Small, focused demonstrations of the five .NET collection types used in the
/// Job Application Tracker. Each method is self-contained and prints its
/// example output to the console, so students can call them from the menu and
/// see the result.
/// </summary>
public static class CollectionsPractice
{
    public static void RunAll()
    {
        Console.WriteLine();
        Console.WriteLine("====== COLLECTIONS PRACTICE ======");
        DemonstrateList();
        DemonstrateDictionary();
        DemonstrateHashSet();
        DemonstrateQueue();
        DemonstrateStack();
        Console.WriteLine("====== END OF COLLECTIONS PRACTICE ======");
        Console.WriteLine();
    }

    // ---------------------------------------------------------------
    // 1. List<T>
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: we need an ordered, indexable sequence of jobs that grows
    /// dynamically as recruiters post more jobs.
    /// Why List<T>: it is the simplest ordered collection; supports add,
    /// remove, index access, and iteration by index.
    /// </summary>
    public static void DemonstrateList()
    {
        Console.WriteLine();
        Console.WriteLine("--- 1. List<T> ---");
        Console.WriteLine("Problem: store an ordered, growable list of job titles.");

        var jobTitles = new List<string>
        {
            "Senior Backend Engineer",
            "Frontend Engineer (React)",
            "DevOps Engineer"
        };

        jobTitles.Add("QA Automation Engineer");

        Console.WriteLine($"After Add: {jobTitles.Count} jobs in the list.");
        for (int i = 0; i < jobTitles.Count; i++)
        {
            Console.WriteLine($"  [{i}] {jobTitles[i]}");
        }

        jobTitles.RemoveAt(0);
        Console.WriteLine($"After RemoveAt(0): {jobTitles.Count} jobs remain.");
        Console.WriteLine("Why List<T>: ordered, indexable, grows automatically as new items are added.");
    }

    // ---------------------------------------------------------------
    // 2. Dictionary<TKey, TValue>
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: given a Job Id we want to find the Job object quickly without
    /// scanning the whole list.
    /// Why Dictionary: average O(1) lookup by key. Used by JobService for
    /// exactly this purpose (E5).
    /// </summary>
    public static void DemonstrateDictionary()
    {
        Console.WriteLine();
        Console.WriteLine("--- 2. Dictionary<TKey, TValue> ---");
        Console.WriteLine("Problem: look up a job by its Id without scanning every job.");

        var jobsById = new Dictionary<int, string>
        {
            { 1, "Senior Backend Engineer" },
            { 2, "Frontend Engineer (React)" },
            { 3, "DevOps Engineer" }
        };

        Console.WriteLine("Looking up Job Id 2 ...");
        if (jobsById.TryGetValue(2, out string? title))
            Console.WriteLine($"  Found: {title}");
        else
            Console.WriteLine("  Not found.");

        Console.WriteLine("Looking up Job Id 999 ...");
        if (jobsById.TryGetValue(999, out string? missing))
            Console.WriteLine($"  Found: {missing}");
        else
            Console.WriteLine("  Not found.");

        Console.WriteLine("Why Dictionary: average O(1) lookup by key. Perfect for Id-based lookups.");
    }

    // ---------------------------------------------------------------
    // 3. HashSet<T>
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: ensure a candidate has not already applied to the same job
    /// without scanning every application.
    /// Why HashSet: Add() returns false on duplicates, giving us a fast
    /// uniqueness check. Used by ApplicationService for duplicate protection.
    /// </summary>
    public static void DemonstrateHashSet()
    {
        Console.WriteLine();
        Console.WriteLine("--- 3. HashSet<T> ---");
        Console.WriteLine("Problem: detect whether a (JobId, CandidateId) pair was already added.");

        var applications = new HashSet<(int JobId, int CandidateId)>();

        bool firstAttempt  = applications.Add((1, 5));
        bool secondAttempt = applications.Add((1, 5)); // duplicate

        Console.WriteLine($"First Add((1, 5)) returned : {firstAttempt}");
        Console.WriteLine($"Second Add((1, 5)) returned: {secondAttempt} (duplicate detected)");
        Console.WriteLine($"Set now contains {applications.Count} pair(s).");
        Console.WriteLine("Why HashSet: Add() returns false on duplicate, an O(1) uniqueness check.");
    }

    // ---------------------------------------------------------------
    // 4. Queue<T>
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: process incoming job applications in the order they arrived
    /// (FIFO), e.g. for a recruiter's review queue.
    /// Why Queue: first-in-first-out ordering; Dequeue() always returns the
    /// oldest enqueued item.
    /// </summary>
    public static void DemonstrateQueue()
    {
        Console.WriteLine();
        Console.WriteLine("--- 4. Queue<T> ---");
        Console.WriteLine("Problem: review applications in the order they arrived (FIFO).");

        var reviewQueue = new Queue<string>();
        reviewQueue.Enqueue("Application #1 (Omar -> Job #1)");
        reviewQueue.Enqueue("Application #2 (Fatima -> Job #2)");
        reviewQueue.Enqueue("Application #3 (Yusuf -> Job #3)");

        Console.WriteLine($"Queue size: {reviewQueue.Count}");
        while (reviewQueue.Count > 0)
        {
            string next = reviewQueue.Dequeue();
            Console.WriteLine($"  Reviewing: {next}");
        }
        Console.WriteLine("Why Queue: Enqueue/Dequeue give first-in-first-out ordering.");
    }

    // ---------------------------------------------------------------
    // 5. Stack<T>
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: implement an "Undo last company status change" feature where
    /// the most recent change must be reverted first (LIFO).
    /// Why Stack: last-in-first-out ordering. Used by CompanyService for H2.
    /// </summary>
    public static void DemonstrateStack()
    {
        Console.WriteLine();
        Console.WriteLine("--- 5. Stack<T> ---");
        Console.WriteLine("Problem: undo the most recent company status change (LIFO).");

        var undoStack = new Stack<(int CompanyId, string PreviousStatus)>();
        undoStack.Push((1, "Pending"));
        undoStack.Push((2, "Approved"));
        undoStack.Push((3, "Pending"));

        Console.WriteLine($"Stack size: {undoStack.Count}");
        while (undoStack.Count > 0)
        {
            var entry = undoStack.Pop();
            Console.WriteLine($"  Undo: Company {entry.CompanyId} -> restore status '{entry.PreviousStatus}'");
        }
        Console.WriteLine("Why Stack: Push/Pop give last-in-first-out ordering, perfect for Undo.");
    }
}
