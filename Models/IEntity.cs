namespace JobApplicationTracker.Models;

/// <summary>
/// Common contract for any entity that can be stored in DataStore&lt;T&gt;.
/// Every entity exposes a unique integer identifier.
/// </summary>
public interface IEntity
{
    int Id { get; set; }
}
