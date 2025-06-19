using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace TruncatedCollectionMemoryBenchmark;

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class TruncatedCollectionBenchmarksForEnumerable
{
    private IEnumerable<int> _data;

    [Params(10, 1000, 10000)]
    public int PageSize;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(1, PageSize + 20); // Always more than pageSize to test truncation
    }

    [Benchmark(Baseline = true)]
    public object Original_TruncatedCollection_IEnumerable()
    {
        var collection = new TruncatedCollection<int>(_data, PageSize);
        Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
        return collection;
    }

    [Benchmark]
    public object Revised_TruncatedCollection_IEnumerable()
    {
        var collection = TruncatedCollectionOfTOpt<int>.Create(_data, PageSize);
        Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
        return collection;
    }

    //[Benchmark]
    //public object Revised_TruncatedCollection_IEnumerable_BackCompatibility()
    //{
    //    var collection = new TruncatedCollectionOfTOpt<int>(_data, PageSize);
    //    Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
    //    return collection;
    //}
}

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class TruncatedCollectionBenchmarksForIQueryable
{
    private IQueryable<int> _data;

    [Params(10, 1000, 10000)]
    public int PageSize;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(1, PageSize + 20).AsQueryable(); // Always more than pageSize to test truncation
    }

    [Benchmark(Baseline = true)]
    public object Original_TruncatedCollection_IQueryable()
    {
        var collection = new TruncatedCollection<int>(_data, PageSize);
        Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
        return collection;
    }

    [Benchmark]
    public object Revised_TruncatedCollection_IQueryable()
    {
        var collection = TruncatedCollectionOfTOpt<int>.Create(_data, PageSize);
        Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
        return collection;
    }

    //[Benchmark]
    //public object Revised_TruncatedCollection_IQueryable_BackCompatibility()
    //{
    //    var collection = new TruncatedCollectionOfTOpt<int>(_data, PageSize);
    //    Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
    //    return collection;
    //}
}

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class TruncatedCollectionBenchmarksForAsync
{
    private IAsyncEnumerable<int> _data;
    private IQueryable<int> _query;

    [Params(10, 1000, 10000)]
    public int PageSize;

    [GlobalSetup]
    public void Setup()
    {
        _data = AsyncEnumerable.Range(1, PageSize + 20); // Always more than pageSize to test truncation
        _query = Enumerable.Range(1, PageSize + 20).AsQueryable(); // Always more than pageSize to test truncation
    }

    [Benchmark(Baseline = true)]
    public async Task<object> Revised_TruncatedCollection_IQueryable()
    {
        var collection = await TruncatedCollectionOfTOpt<int>.CreateAsync(_query, PageSize, false);
        Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
        return collection;
    }

    [Benchmark]
    public async Task<object> Revised_TruncatedCollection_IAsyncEnumerable()
    {
        var collection = await TruncatedCollectionOfTOpt<int>.CreateAsync(_data, PageSize);
        Debug.Assert(collection.IsTruncated, "IsTruncated should be 'True'");
        return collection;
    }
}

#region Experimental Benchmarks

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class IEnumerableAddRangeVsEnumeratorsBenchmarks
{
    private IEnumerable<int> _data;

    [Params(10, 1000, 10000)]
    public int PageSize;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(1, PageSize + 20); // Always more than pageSize to test truncation
    }

    [Benchmark(Baseline = true)]
    public object AddRange_List()
    {
        var capacity = checked(PageSize + 1);

        var list = new List<int>(capacity);
        list.AddRange(_data.Take(capacity));

        if (list.Count > PageSize)
        {
            list.RemoveAt(PageSize);
        }

        return list;
    }

    [Benchmark]
    public object CustomInsertRange_List()
    {
        var capacity = checked(PageSize + 1);
        var items = _data.Take(capacity);
        var isTruncated = false;
        ICollection<int>? collection = items as ICollection<int>;
        if (collection is not null && collection.Count > PageSize)
        {
            isTruncated = true;
            return new List<int>(items.Take(PageSize));
        }

        int index = 0;
        var list = new List<int>(PageSize);
        using (IEnumerator<int> en = items.GetEnumerator())
        {
            while (en.MoveNext() && index < PageSize)
            {
                list.Insert(index++, en.Current);
            }
        }

        return list;
    }

    [Benchmark]
    public object UseEnumerator_List()
    {
        var capacity = checked(PageSize + 1);

        var list = new List<int>(PageSize);
        int index = 0;
        using (IEnumerator<int> en = _data.Take(capacity).GetEnumerator())
        {
            while (en.MoveNext() && index < PageSize)
            {
                list.Insert(index++, en.Current);
            }
        }

        return list;
    }
}

[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class IQueryableAddRangeVsEnumeratorsBenchmarks
{
    private IQueryable<int> _data; // changing to IQueryable

    [Params(10, 1000, 10000)]
    public int PageSize;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(1, PageSize + 1000).AsQueryable(); // changed to IQueryable
    }

    [Benchmark(Baseline = true)]
    public object AddRange_List()
    {
        var capacity = checked(PageSize + 1);

        var list = new List<int>(capacity);
        list.AddRange(Take(_data, PageSize, false));

        if (list.Count > PageSize)
        {
            list.RemoveAt(PageSize);
        }

        return list;
    }

    [Benchmark]
    public object UseEnumerator_List()
    {
        var list = new List<int>(PageSize);
        int index = 0;
        using (IEnumerator<int> en = Take(_data, PageSize, false).GetEnumerator())
        {
            while (en.MoveNext() && index < PageSize)
            {
                list.Insert(index++, en.Current);
            }
        }

        return list;
    }

    private static IQueryable<int> Take(IQueryable<int> source, int pageSize, bool parameterize)
    {
        // This uses existing ExpressionHelpers from OData to apply Take(pageSize + 1)
        return (IQueryable<int>)ExpressionHelpers.Take(source, checked(pageSize + 1), typeof(int), parameterize);
    }
}

#endregion
