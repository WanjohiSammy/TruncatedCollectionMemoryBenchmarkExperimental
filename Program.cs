using BenchmarkDotNet.Running;
using TruncatedCollectionMemoryBenchmark;

// Where source is an IQueryable
BenchmarkRunner.Run<TruncatedCollectionBenchmarksForIQueryable>();

//// Where source is an IEnumerable
//BenchmarkRunner.Run<TruncatedCollectionBenchmarksForEnumerable>();
