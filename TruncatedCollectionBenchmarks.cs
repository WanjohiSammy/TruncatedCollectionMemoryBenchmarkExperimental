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
        return new TruncatedCollection<int>(_data, PageSize);
    }

    [Benchmark]
    public object Revised_TruncatedCollection_IEnumerable()
    {
        return TruncatedCollectionOfTOpt<int>.Create(_data, PageSize);
    }
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
        return new TruncatedCollection<int>(_data, PageSize);
    }

    [Benchmark]
    public object Revised_TruncatedCollection_IQueryable()
    {
        return TruncatedCollectionOfTOpt<int>.Create(_data, PageSize);
    }
}
