using System.Collections;

namespace TruncatedCollectionMemoryBenchmark.Interfaces;

/// <summary>
/// Represents a collection that has total count.
/// </summary>
public interface ICountOptionCollection : IEnumerable
{
    /// <summary>
    /// Gets a value representing the total count of the collection.
    /// </summary>
    long? TotalCount { get; }
}
