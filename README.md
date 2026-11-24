# Tawjeeh Job Application Tracker â€” Sprint 1

A C# console application that manages companies, recruiters, jobs, candidates,
and job applications. This repository is the **reference (model) solution** for
Tawjeeh Sprint 1. It is intentionally written in a clear, straightforward style
so students can study it after completing their own implementation.

- **Target framework:** .NET 8
- **Project type:** Console application
- **No LINQ is used anywhere.**

---

## 1. Project overview

The Job Application Tracker models a small piece of a recruiting platform:

- Companies apply for approval before they can publish jobs.
- Recruiters belong to companies and post jobs on their behalf.
- Candidates create a profile.
- Candidates apply to open jobs at Approved companies.
- The system prevents the same candidate from applying to the same job twice.

The Sprint 1 scope focuses on **collections, generics, basic OOP, dictionaries,
HashSets, stacks, console menus, and exception handling** â€” not on real
persistence, events, or sorting.

---

## 2. Project scope and Sprint 1 objectives

Implemented in Sprint 1:

| ID  | Area                          | Status |
|-----|-------------------------------|--------|
| E1  | Collections + sample data      | Done   |
| E2  | Console menus + CRUD          | Done   |
| E3  | Company approval               | Done   |
| E5  | Dictionary-based job lookup   | Done   |
| M1  | Generic `DataStore<T>`        | Done   |
| H1  | Application workflow + dup guard | Done |
| H2  | Undo with Stack               | Done   |

Explicitly **deferred to later sprints** (do **not** expect them in this code):

- Events and `EventHandler`.
- Delegates and advanced lambda usage.
- Job searching and sorting.
- Recruiter-specific job ownership workflows.
- File persistence.
- Custom application exceptions (M7).
- Advanced file I/O handling.

---

## 3. Folder structure

```
JobApplicationTracker/
â”œâ”€â”€ Models/
â”‚   â”œâ”€â”€ IEntity.cs
â”‚   â”œâ”€â”€ Company.cs
â”‚   â”œâ”€â”€ Recruiter.cs
â”‚   â”œâ”€â”€ Job.cs
â”‚   â”œâ”€â”€ Candidate.cs
â”‚   â””â”€â”€ Application.cs
â”œâ”€â”€ Services/
â”‚   â”œâ”€â”€ DataStore.cs
â”‚   â”œâ”€â”€ CompanyService.cs
â”‚   â”œâ”€â”€ JobService.cs
â”‚   â”œâ”€â”€ CandidateService.cs
â”‚   â”œâ”€â”€ ApplicationService.cs
â”‚   â”œâ”€â”€ RecruiterService.cs
â”‚   â””â”€â”€ SeedData.cs
â”œâ”€â”€ Practice/
â”‚   â”œâ”€â”€ CollectionsPractice.cs
â”‚   â””â”€â”€ GenericsPractice.cs
â”œâ”€â”€ Program.cs
â”œâ”€â”€ AppServices.cs
â”œâ”€â”€ ConsoleHelper.cs
â”œâ”€â”€ JobApplicationTracker.csproj
â””â”€â”€ README.md
```

Two helper files outside the requested structure (`AppServices.cs` and
`ConsoleHelper.cs`) keep `Program.cs` short and readable; both are described in
the design-decisions section below.

---

## 4. Main features

- **Companies:** add, update, remove, list, list-by-status, approve, reject,
  undo last status change.
- **Recruiters:** add, update, remove, list (each recruiter must reference an
  existing company).
- **Jobs:** add, update, remove, list, dictionary-based lookup by Id.
- **Candidates:** add, update, remove, list (unique email enforced).
- **Applications:** apply-to-job workflow, list, remove. Manual application
  creation is intentionally **not** exposed in the menu â€” applications are only
  created through `ApplicationService.Apply(jobId, candidateId)`, which runs the
  full H1 validation pipeline.
- **Practice demonstrations:** runnable examples of `List<T>`,
  `Dictionary<TKey,TValue>`, `HashSet<T>`, `Queue<T>`, `Stack<T>`, generic
  methods, generic classes, and generic constraints.

---

## 5. Collections used and why

### `List<T>`
Used as the underlying storage inside `DataStore<T>`. Ordered, indexable,
grows automatically. Good default when we need a simple sequence we will scan
or iterate over.

### `Dictionary<int, Job>` (E5)
A parallel index maintained by `JobService` for fast **Id-based** job lookup.
Average lookup complexity is **O(1)** because the dictionary hashes the key
(Jon Id) into a bucket. A `List<T>` lookup by Id would be O(n). When we have
many jobs and look them up by Id frequently (as the application workflow does),
the dictionary is the right tool.

The dictionary is kept strictly in sync with the `DataStore<Job>`:
- `Add`     -> adds to both
- `Update`  -> replaces in both
- `Remove`  -> removes from both
- `JobService` constructor also re-syncs the dictionary from the store so any
  data that was seeded directly into the store (e.g. legacy Pending-company
  job) is also indexed.

### `HashSet<(int JobId, int CandidateId)>` (H1)
Tracks every accepted (job, candidate) application pair.
`HashSet<T>.Add` returns `false` if the element already exists, so duplicate
detection is a single O(1) call â€” no loop, no scan of the application list.
The set is rolled back if saving the application fails after reserving the
pair, and is also updated when an application is removed.

### `Stack<(int CompanyId, CompanyStatus PreviousStatus)>` (H2)
Records each company status change so the most recent one can be undone first
(LIFO). Each entry snapshots the **CompanyId** and the **previous** status so
the undo can target the correct company even when multiple companies have been
changed. The entity itself is **not** stored in the stack â€” only the snapshot
of its previous status â€” which avoids the "stored a mutable reference" trap.

### `Queue<T>`
Demonstrated in `CollectionsPractice` as the natural fit for "review the next
application in the order it was submitted" (FIFO). Not used in the main app
because the matching workflow (recruiter review pipeline) belongs to a later
sprint.

---

## 6. Explanation of `DataStore<T>`

```csharp
public class DataStore<T> where T : class, IEntity
{
    public void Add(T entity);
    public bool Update(T entity);
    public bool Remove(int id);
    public T? GetById(int id);
    public List<T> GetAll();
}
```

- **Constraint `where T : class, IEntity`:** ensures only reference types that
  expose an `Id` can be stored. This lets the generic class read `entity.Id`
  safely.
- **Internal storage:** a private `List<T>` that is never returned directly;
  `GetAll()` returns a defensive copy so callers cannot mutate the store.
- **Duplicate Id prevention:** `Add` throws `InvalidOperationException` if an
  entity with the same Id already exists.
- **Auto-id assignment:** if a caller passes an entity with `Id <= 0`,
  `DataStore` assigns the next free Id automatically.
- **Null safety:** `Add` throws `ArgumentNullException`; `Update` returns
  `false`; `GetById` returns `null` for invalid or unknown Ids.

Each service wraps a `DataStore<T>` for its entity type and translates its
results into friendly `(bool Success, string Message)` tuples for the UI.

---

## 7. Business rules

### Companies (E3)
- New companies default to `Pending`.
- A company's status can be moved to `Approved` or `Rejected`.
- **Only `Approved` companies can publish new jobs.** This rule is enforced in
  `JobService.Add`, not just in the UI.
- Undo restores the previous status, even after multiple changes.

### Jobs (E5)
- Job lookups by Id go through `JobService.GetById`, which uses the dictionary.
- The dictionary and the `DataStore<Job>` must always agree; `JobService`
  exposes `IsDictionaryConsistent()` for verification.

### Applications (H1)
Before creating an application, `ApplicationService.Apply` validates **all**
of:

1. The candidate exists.
2. The job exists.
3. The job is open.
4. The job's company exists.
5. The company's status is `Approved`.
6. The candidate has **not** already applied to the same job (using
   `HashSet<(int, int)>.Add`).

If any check fails, no state is mutated. On success:
- `AppliedDate = DateTime.Now`
- `Status = ApplicationStatus.Submitted`
- The pair is reserved in the `HashSet`.
- The application is stored in `DataStore<Application>`.

If saving the application fails **after** reserving the pair, the pair is
removed again (rollback) so the `HashSet` and the store stay consistent.

### Candidates
- Email must be unique (case-insensitive) across candidates.

### Recruiters
- Must reference an existing company.

---

## 8. How to build and run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### From the command line
```bash
cd JobApplicationTracker
dotnet build
dotnet run
```

On Windows PowerShell:
```powershell
cd JobApplicationTracker
dotnet build
dotnet run
```

---

## 9. Sample menu usage

```
---------- MAIN MENU ----------
 1. Companies
 2. Recruiters
 3. Jobs
 4. Candidates
 5. Applications
 6. Practice demonstrations
 0. Exit
------------------------------
Choice: 1

------- COMPANIES -------
 1. List all companies
 ...
 6. Approve company (E3)
 8. Undo last status change (H2)
 0. Back
-------------------------
Choice: 1

Companies (3):
+----+------------+-----------+---------+---------+
| Id | Name       | Industry  | City    | Status  |
+----+------------+-----------+---------+---------+
| 1  | Microsoft  | Software  | Redmond | Approved|
| 2  | FutureSoft | Software  | Dubai   | Pending |
| 3  | OldCorp    | Logistics | Cairo   | Rejected|
+----+------------+-----------+---------+---------+
```

### Trying the application workflow (H1)

1. From the main menu choose `5. Applications` -> `2. Apply to a job`.
2. Enter a Job Id and a Candidate Id.
3. Try combinations to see the rejection rules in action:
   - Job belonging to a `Pending` or `Rejected` company -> rejection.
   - Closed job (Job Id 4) -> rejection.
   - Non-existent candidate or job -> rejection.
   - Apply the same candidate to the same job twice -> second attempt rejected.
4. Apply candidate 1 to job 1 (Microsoft, Approved, open) -> accepted.
5. Choose `4. Show HashSet consistency check` -> reports `CONSISTENT`.

### Trying Undo (H2)

1. `1. Companies` -> `6. Approve company (E3)` -> enter `2` (FutureSoft).
2. `8. Undo last status change` -> FutureSoft returns to `Pending`.
3. `9. Show undo stack size` -> back to 0 after the undo.

---

## 10. Git workflow

The repository is intended to be developed with small, focused commits that
reference the requirement IDs. Suggested commit sequence:

```
git init
git add .
git commit -m "feat(E1): add entities and sample data"
git commit -m "feat(E2): implement console menus and CRUD"
git commit -m "feat(E3): implement company approval"
git commit -m "feat(E5): add dictionary-based job lookup"
git commit -m "feat(M1): implement generic DataStore"
git commit -m "feat(H1): implement application workflow and duplicate protection"
git commit -m "feat(H2): implement undo with stack"
git commit -m "docs: add practice examples and README"
git tag sprint-1
```

> Commit messages and the `sprint-1` tag should be created only when the
> related work has actually been performed locally.

---

## 11. Sprint 2 hand-off checklist

Before starting Sprint 2, the following should be true:

- [ ] All Sprint 1 features (E1, E2, E3, E5, M1, H1, H2) are implemented and
      manually verified.
- [ ] The application builds cleanly with `dotnet build` (zero warnings on
      the seven required files).
- [ ] No LINQ is used anywhere in the project.
- [ ] No TODOs or placeholder implementations remain in required files.
- [ ] `JobService.IsDictionaryConsistent()` returns `true` after Add/Update/Remove.
- [ ] `ApplicationService.IsHashSetConsistent()` returns `true` after Add/Remove.
- [ ] Undo on an empty stack returns a friendly message rather than crashing.
- [ ] Invalid console input ("abc") returns a friendly message and re-shows
      the menu.
- [ ] README lists which Sprint 2 features are **not** yet implemented.
- [ ] Tag `sprint-1` points to the final completed commit.

Sprint 2 candidates (do **not** implement yet):
- Events and `EventHandler` for status transitions.
- Delegates and advanced lambda usage.
- Job searching and sorting.
- Recruiter-specific job ownership workflows.
- File persistence (CSV or JSON).
- Custom application exceptions (M7).
- Advanced file I/O handling.

---

## 12. Design decisions (summary)

- **`AppServices.cs`** is the composition root. It builds the stores, seeds
  them, and wires the services in the correct order. Keeps `Program.cs`
  focused on UI.
- **`ConsoleHelper.cs`** centralises safe input parsing (`int.TryParse`,
  `decimal.TryParse`, email format check, "blank keeps current value"). Keeps
  UI code short and avoids crashes from "abc" input.
- **Services return `(bool Success, string Message)` tuples** instead of
  throwing for normal business errors. This keeps the UI loop simple and
  matches the "no custom exceptions in Sprint 1" rule.
- **`DataStore<T>` throws `InvalidOperationException` for duplicate Ids**;
  each service catches it and converts it into a friendly tuple message.
- **`JobService` synchronises its dictionary in the constructor** so legacy
  seed data (the Pending-company job) is indexed even though it bypassed the
  service's approval check.
- **`HashSet<(int, int)>.Add` is the only duplicate check** in
  `ApplicationService` â€” no manual loop, exactly as required by H1.
- **No LINQ anywhere**, including in `PrintTable` (uses manual loops and
  `string.Join`).
- **No Repository / UoW / DI framework / Clean Architecture patterns** â€” this
  is a small console app and does not need them.
