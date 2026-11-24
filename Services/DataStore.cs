using JobApplicationTracker.Models;

namespace JobApplicationTracker.Services;

/// <summary>
/// A small generic in-memory repository for any entity that implements <see cref="IEntity"/>.
/// Uses a <see cref="List{T}"/> internally and never exposes it directly so that callers
/// cannot mutate storage without going through the documented methods.
/// </summary>
/// <typeparam name="T">
/// Must be a reference type that implements <see cref="IEntity"/>. The constraint
/// <c>where T : class, IEntity</c> lets us read <c>entity.Id</c> generically.
/// </typeparam>
public class DataStore<T> where T : class, IEntity
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    /// <summary>
    /// Adds an entity. If <paramref name="entity"/>.Id is &lt;= 0 a new id is assigned
    /// automatically. If a positive id is provided it is honoured, and the next-id
    /// counter is moved past it to avoid future collisions.
    /// </summary>
    /// <exception cref="ArgumentNullException">If the entity is null.</exception>
    /// <exception cref="InvalidOperationException">If an entity with the same Id already exists.</exception>
    public void Add(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

        if (entity.Id <= 0)
        {
            // Auto-assign: take the next free Id and advance the counter so
            // the next auto-assign does not collide with this one.
            entity.Id = _nextId;
            _nextId++;
        }
        else
        {
            if (ContainsId(entity.Id))
                throw new InvalidOperationException($"An entity with Id {entity.Id} already exists.");
            if (entity.Id >= _nextId)
                _nextId = entity.Id + 1;
        }

        _items.Add(entity);
    }

    /// <summary>
    /// Replaces the stored entity that matches <paramref name="entity"/>.Id.
    /// Returns false if no entity with that Id exists.
    /// </summary>
    public bool Update(T entity)
    {
        if (entity == null)
            return false;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Id == entity.Id)
            {
                _items[i] = entity;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes the entity with the given Id. Returns false if no such entity exists.
    /// </summary>
    public bool Remove(int id)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Id == id)
            {
                _items.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the entity with the given Id, or null if not found.
    /// </summary>
    public T? GetById(int id)
    {
        if (id <= 0)
            return null;

        foreach (var item in _items)
        {
            if (item.Id == id)
                return item;
        }
        return null;
    }

    /// <summary>
    /// Returns a defensive copy of all entities. Mutating the returned list
    /// does not affect the internal storage.
    /// </summary>
    public List<T> GetAll()
    {
        // Returns a new list so callers cannot mutate the internal storage.
        return new List<T>(_items);
    }

    /// <summary>
    /// Number of entities currently stored.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Returns the next Id that will be assigned if Add() is called with Id &lt;= 0.
    /// Exposed so services can show users what Id a new entity will receive
    /// and so ApplicationService can reserve an Id atomically before saving.
    /// </summary>
    public int PeekNextId() => _nextId;

    /// <summary>
    /// Not part of the public five-method contract, but a tiny helper used by
    /// Add() to check duplicates. Kept private on purpose.
    /// </summary>
    private bool ContainsId(int id)
    {
        foreach (var item in _items)
        {
            if (item.Id == id)
                return true;
        }
        return false;
    }
    /// <summary>
    /// Returns all elements matching the supplied predicate (Part 2 - M3).
    /// Uses a regular loop; LINQ is not allowed.
    /// </summary>
    public List<T> Find(Predicate<T> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var results = new List<T>();
        foreach (T item in _items)
        {
            if (predicate(item))
                results.Add(item);
        }
        return results;
    }

    /// <summary>
    /// Returns a sorted copy of the stored elements (Part 2 - M3).
    /// The original collection is never modified.
    /// </summary>
    public List<T> Sort(Comparison<T> comparison)
    {
        if (comparison == null)
            throw new ArgumentNullException(nameof(comparison));

        var sortedItems = new List<T>(_items);
        sortedItems.Sort(comparison);
        return sortedItems;
    }
}
