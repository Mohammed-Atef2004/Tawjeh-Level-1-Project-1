using JobApplicationTracker.Models;
using JobApplicationTracker.Services;

namespace JobApplicationTracker.Practice;

/// <summary>
/// Small, focused demonstrations of generics used in the Job Application Tracker.
/// </summary>
public static class GenericsPractice
{
    public static void RunAll()
    {
        Console.WriteLine();
        Console.WriteLine("====== GENERICS PRACTICE ======");
        DemonstrateGenericMethod();
        DemonstrateGenericClass();
        DemonstrateGenericConstraints();
        Console.WriteLine("====== END OF GENERICS PRACTICE ======");
        Console.WriteLine();
    }

    // ---------------------------------------------------------------
    // 1. A generic method
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: we want one helper that can print the first item of any
    /// List&lt;T&gt; without writing an overload per type.
    /// What T represents: the element type of the list (Company, Job, etc.).
    /// </summary>
    public static void DemonstrateGenericMethod()
    {
        Console.WriteLine();
        Console.WriteLine("--- 1. Generic method: FirstOrDefault<T> ---");
        Console.WriteLine("Problem: print the first item of any List<T> without writing overloads.");

        var companies = new List<string> { "Microsoft", "FutureSoft", "OldCorp" };
        string? firstCompany = FirstOrDefault(companies);
        Console.WriteLine($"  First company: {firstCompany ?? "(empty list)"}");

        var numbers = new List<int> { 10, 20, 30 };
        int? firstNumber = FirstOrDefault(numbers);
        Console.WriteLine($"  First number : {firstNumber}");

        var empty = new List<DateTime>();
        DateTime? nothing = FirstOrDefault(empty);
        Console.WriteLine($"  First date   : {(nothing.HasValue ? nothing.ToString() : "(empty list)")}");

        Console.WriteLine("Why a generic method: one implementation serves every element type.");
    }

    /// <summary>
    /// A single generic method that works on any List&lt;T&gt;. T is unbounded
    /// here, so the method can be used with any type.
    /// </summary>
    public static T? FirstOrDefault<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
            return default;   // default(T) is null for reference types, 0/false for value types
        return list[0];
    }

    // ---------------------------------------------------------------
    // 2. A generic class
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: we want a tiny reusable container that holds "the last N items
    /// added" (a bounded history). We do not want one class per item type.
    /// Why a generic class: a single History&lt;T&gt; works for any item type.
    /// </summary>
    public static void DemonstrateGenericClass()
    {
        Console.WriteLine();
        Console.WriteLine("--- 2. Generic class: History<T> ---");
        Console.WriteLine("Problem: a small reusable 'last N items' container that works for any type.");

        var statusHistory = new History<string>(capacity: 3);
        statusHistory.Add("Pending");
        statusHistory.Add("Approved");
        statusHistory.Add("Rejected");
        statusHistory.Add("Approved");  // capacity 3 -> drops oldest

        Console.WriteLine("  statusHistory (capacity=3) after 4 adds:");
        foreach (var s in statusHistory.Items)
            Console.WriteLine($"    {s}");

        var idHistory = new History<int>(capacity: 2);
        idHistory.Add(1);
        idHistory.Add(2);
        idHistory.Add(3);
        Console.WriteLine("  idHistory (capacity=2) after 3 adds:");
        foreach (var i in idHistory.Items)
            Console.WriteLine($"    {i}");

        Console.WriteLine("Why a generic class: one reusable container for any element type.");
    }

    /// <summary>
    /// A tiny generic class that keeps at most <paramref name="capacity"/> items,
    /// dropping the oldest when full. Demonstrates that a single class can serve
    /// many element types.
    /// </summary>
    public sealed class History<T>
    {
        private readonly List<T> _items = new();
        private readonly int _capacity;

        public History(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            _capacity = capacity;
        }

        public void Add(T item)
        {
            _items.Add(item);
            if (_items.Count > _capacity)
                _items.RemoveAt(0);     // drop oldest
        }

        public IReadOnlyList<T> Items => _items;
    }

    // ---------------------------------------------------------------
    // 3. Generic constraints: where T : class, IEntity
    // ---------------------------------------------------------------
    /// <summary>
    /// Problem: DataStore&lt;T&gt; must be able to read entity.Id for every entity,
    /// but it should refuse to store types that have no Id (e.g. string, int).
    ///
    /// <para>What the constraint does:</para>
    /// <list type="bullet">
    /// <item><c>where T : class</c>        - T must be a reference type.</item>
    /// <item><c>where T : IEntity</c>      - T must implement IEntity, so we can read T.Id.</item>
    /// </list>
    /// <para>Together they let the compiler enforce, at compile-time, that only
    /// entity reference types can be stored in DataStore&lt;T&gt;.</para>
    /// </summary>
    public static void DemonstrateGenericConstraints()
    {
        Console.WriteLine();
        Console.WriteLine("--- 3. Generic constraints: where T : class, IEntity ---");
        Console.WriteLine("Problem: DataStore<T> must read T.Id but reject non-entity types.");

        var store = new DataStore<Company>();
        store.Add(new Company(1, "Microsoft", "Software", "Redmond", CompanyStatus.Approved));
        store.Add(new Company(2, "FutureSoft", "Software", "Dubai", CompanyStatus.Pending));

        Console.WriteLine($"  Stored {store.Count} companies in DataStore<Company>.");

        Company? found = store.GetById(2);
        Console.WriteLine($"  GetById(2) => {found?.Name ?? "(null)"}");

        // The following lines would NOT compile, proving the constraint works:
        //
        //   var badIntStore = new DataStore<int>();        // int is not a class
        //   var badStrStore = new DataStore<string>();      // string does not implement IEntity

        Console.WriteLine("  DataStore<int> and DataStore<string> would fail to compile");
        Console.WriteLine("    because int is not 'class' and string does not implement IEntity.");
        Console.WriteLine("Why constraints: they let the compiler enforce, at compile time, that");
        Console.WriteLine("  DataStore<T> can only be used with reference types that have an Id.");
    }
}
