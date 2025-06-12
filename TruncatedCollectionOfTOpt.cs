using System.Collections;
using TruncatedCollectionMemoryBenchmark.Interfaces;

namespace TruncatedCollectionMemoryBenchmark;

public class TruncatedCollectionOfTOpt<T> : IReadOnlyList<T>, ITruncatedCollection, ICountOptionCollection
{
    private const int MinPageSize = 1;
    private const int DefaultCapacity = 4;

    private readonly List<T> _items;
    private readonly bool _isTruncated;

    /// <summary>
    /// Private constructor used by static Create methods and public constructors.
    /// </summary>
    /// <param name="items">The list of items in the collection.</param>
    /// <param name="pageSize">The maximum number of items per page.</param>
    /// <param name="totalCount">The total number of items in the source collection, if known.</param>
    /// <param name="isTruncated">Indicates whether the collection is truncated.</param>
    private TruncatedCollectionOfTOpt(List<T> items, int pageSize, long? totalCount, bool isTruncated)
    {
        _items = items;
        PageSize = pageSize;
        TotalCount = totalCount;
        _isTruncated = isTruncated;
    }

    #region Constructors for Backward Compatibility

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    public TruncatedCollectionOfTOpt(IEnumerable<T> source, int pageSize)
        : this(CreateInternal(source, pageSize, totalCount: null)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    public TruncatedCollectionOfTOpt(IEnumerable<T> source, int pageSize, long? totalCount)
        : this(CreateInternal(source, pageSize, totalCount)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    public TruncatedCollectionOfTOpt(IQueryable<T> source, int pageSize)
        : this(CreateInternal(source, pageSize, false)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    public TruncatedCollectionOfTOpt(IQueryable<T> source, int pageSize, bool parameterize)
        : this(CreateInternal(source, pageSize, parameterize)) { }

    /// <summary>
    /// Wrapper used internally by the backward-compatible constructors.
    /// </summary>
    /// <param name="other">An instance of <see cref="TruncatedCollectionOfTOpt{T}"/>.</param>
    private TruncatedCollectionOfTOpt(TruncatedCollectionOfTOpt<T> other)
        : this(other._items, other.PageSize, other.TotalCount, other._isTruncated)
    {
    }

    #endregion

    /// <summary>
    /// Create a truncated collection from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count. Default is null.</param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"</returns>
    public static TruncatedCollectionOfTOpt<T> Create(IEnumerable<T> source, int pageSize, long? totalCount = null)
    {
        return CreateInternal(source, pageSize, totalCount);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count. Default is null.</param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"</returns>
    public static TruncatedCollectionOfTOpt<T> Create(IQueryable<T> source, int pageSize, long? totalCount = null)
    {
        return CreateInternal(source, pageSize, false, totalCount);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"</returns>
    public static TruncatedCollectionOfTOpt<T> Create(IQueryable<T> source, int pageSize, bool parameterize)
    {
        return CreateInternal(source, pageSize, parameterize);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count. Default is null.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"</returns>
    public static TruncatedCollectionOfTOpt<T> Create(IQueryable<T> source, int pageSize, long? totalCount, bool parameterize)
    {
        return CreateInternal(source, pageSize, parameterize, totalCount);
    }

    /// <summary>
    /// Create an async truncated collection from an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="source">The AsyncEnumerable to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count. Default is null.</param>
    /// <param name="cancellationToken">Cancellation token for async operations. Default is <see cref="default"/></param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"/></returns>
    public static async Task<TruncatedCollectionOfTOpt<T>> CreateAsync(IAsyncEnumerable<T> source, int pageSize, long? totalCount = null, CancellationToken cancellationToken = default)
    {
        return await CreateInternalAsync(source, pageSize, totalCount, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Create an async truncated collection from an <see cref="IQueryable{T}"/>.
    /// Uses Take(pageSize + 1) to detect truncation.
    /// </summary>
    /// <param name="source">The IQueryable to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    /// <param name="cancellationToken">Cancellation token for async operations. Default is <see cref="default"/></param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"/></returns>
    public static Task<TruncatedCollectionOfTOpt<T>> CreateAsync(IQueryable<T> source, int pageSize, bool parameterize = false, CancellationToken cancellationToken = default)
    {
        return CreateAsync(source, pageSize, null, parameterize, cancellationToken);
    }

    /// <summary>
    /// Create an async truncated collection from an <see cref="IQueryable{T}"/>.
    /// Uses Take(pageSize + 1) to detect truncation.
    /// </summary>
    /// <param name="source">The IQueryable to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    /// <param name="cancellationToken">Cancellation token for async operations. Default is <see cref="default"/></param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"/></returns>
    public static Task<TruncatedCollectionOfTOpt<T>> CreateAsync(IQueryable<T> source, int pageSize, long? totalCount, CancellationToken cancellationToken = default)
    {
        return CreateAsync(source, pageSize, totalCount, false, cancellationToken);
    }

    /// <summary>
    /// Create an async truncated collection from an <see cref="IQueryable{T}"/>.
    /// Uses Take(pageSize + 1) to detect truncation.
    /// </summary>
    /// <param name="source">The IQueryable to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    /// <param name="cancellationToken">Cancellation token for async operations. Default is <see cref="default"/></param>
    /// <returns>An instance of the <see cref="TruncatedCollectionOfTOpt{T}"/></returns>
    public static async Task<TruncatedCollectionOfTOpt<T>> CreateAsync(IQueryable<T> source, int pageSize, long? totalCount = null, bool parameterize = false, CancellationToken cancellationToken = default)
    {
        ValidateArgs(source, pageSize);

        var query = Take(source, pageSize, parameterize);
        var items = await ToListAsync(query, cancellationToken).ConfigureAwait(false);

        bool isTruncated = items.Count > pageSize;
        if (isTruncated)
        {
            items.RemoveAt(items.Count - 1);
        }

        return new TruncatedCollectionOfTOpt<T>(items, pageSize, totalCount, isTruncated);
    }

    #region Core Internal Creation (Sync/Async)

    private static TruncatedCollectionOfTOpt<T> CreateInternal(IEnumerable<T> source, int pageSize, long? totalCount)
    {
        ValidateArgs(source, pageSize);

        int capacity = pageSize > 0 ? pageSize : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity);

        // Use a Span-like approach to minimize allocations if the source is an array or list
        if (source is IList<T> list)
        {
            capacity = capacity > list.Count ? list.Count : capacity;
            var buffer = new List<T>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                buffer.Add(list[i]);
            }

            bool isTruncated = list.Count > pageSize;
            return new TruncatedCollectionOfTOpt<T>(buffer, pageSize, totalCount, isTruncated);
        }

        // Fallback for other IEnumerable<T> types
        var bufferFallback = new List<T>(capacity);
        using var enumerator = source.GetEnumerator();
        int count = 0;

        while (enumerator.MoveNext())
        {
            if (count < capacity)
            {
                bufferFallback.Add(enumerator.Current);
                count++;
            }
            else
            {
                return new TruncatedCollectionOfTOpt<T>(bufferFallback, pageSize, totalCount, isTruncated: true);
            }
        }

        return new TruncatedCollectionOfTOpt<T>(bufferFallback, pageSize, totalCount, isTruncated: false);
    }

    private static TruncatedCollectionOfTOpt<T> CreateInternal(IQueryable<T> source, int pageSize, bool parameterize = false, long? totalCount = null)
    {
        ValidateArgs(source, pageSize);

        int capacity = pageSize > 0 ? pageSize : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity);

        var buffer = new List<T>(capacity);
        using var enumerator = source.GetEnumerator();
        int count = 0;

        while (enumerator.MoveNext())
        {
            if (count < capacity)
            {
                buffer.Add(enumerator.Current);
                count++;
            }
            else
            {
                return new TruncatedCollectionOfTOpt<T>(buffer, pageSize, totalCount, isTruncated: true);
            }
        }

        return new TruncatedCollectionOfTOpt<T>(buffer, pageSize, totalCount, isTruncated: false);
    }

    private static async Task<TruncatedCollectionOfTOpt<T>> CreateInternalAsync(IAsyncEnumerable<T> source, int pageSize, long? totalCount, CancellationToken cancellationToken)
    {
        ValidateArgs(source, pageSize);

        int capacity = pageSize > 0 ? pageSize : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity);
        var buffer = new List<T>(capacity);

        int count = 0;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (count < pageSize)
            {
                buffer.Add(item);
                count++;
            }
            else
            {
                return new TruncatedCollectionOfTOpt<T>(buffer, pageSize, totalCount, isTruncated: true);
            }
        }

        return new TruncatedCollectionOfTOpt<T>(buffer, pageSize, totalCount, isTruncated: false);
    }

    private static IQueryable<T> Take(IQueryable<T> source, int pageSize, bool parameterize)
    {
        // This uses existing ExpressionHelpers from OData to apply Take(pageSize + 1)
        return ExpressionHelpers.Take(source, checked(pageSize + 1), typeof(T), parameterize) as IQueryable<T>;
    }

    private static ValueTask<List<T>> ToListAsync(IQueryable<T> query, CancellationToken cancellationToken)
    {
        if (query is IAsyncEnumerable<T> asyncSource)
        {
            return asyncSource.ToListAsync(cancellationToken);
        }

        // Fallback for in-memory or testing
        return ValueTask.FromResult(query.ToList());
    }

    private static void ValidateArgs(object source, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (pageSize < MinPageSize)
            throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size must be >= {MinPageSize}.");
    }

    #endregion

    public int PageSize { get; }
    public long? TotalCount { get; }
    public bool IsTruncated => _isTruncated;

    public int Count => _items.Count;
    public T this[int index] => _items[index];
    //public List<T> AsList() => new List<T>(_items);

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
