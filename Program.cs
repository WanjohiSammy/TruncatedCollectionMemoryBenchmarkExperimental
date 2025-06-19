using BenchmarkDotNet.Running;
using TruncatedCollectionMemoryBenchmark;

// Where source is an IQueryable
BenchmarkRunner.Run<TruncatedCollectionBenchmarksForIQueryable>();

//// Where source is an IEnumerable
//BenchmarkRunner.Run<TruncatedCollectionBenchmarksForEnumerable>();

// Asynchronous benchmarks for TruncatedCollection
//BenchmarkRunner.Run<TruncatedCollectionBenchmarksForAsync>();

//BenchmarkRunner.Run<IEnumerableAddRangeVsEnumeratorsBenchmarks>();

//var source = Enumerable.Range(1, 100).AsQueryable(); 
//var parameterize = false;
//var query = (IQueryable<int>)ExpressionHelpers.Take(source, checked(50 + 1), typeof(int), parameterize);
//Console.WriteLine($"Query: {query.Expression}");

//var pageSize = 50;
//var source = Enumerable.Range(1, 1023);
//for (int i = 0; i < 25; i++)
//{
//    var src = source.Skip(i * 50);
//    var truncatedCollection = await TruncatedCollectionOfTOpt<int>.CreateAsync(src.ToAsyncEnumerable(), pageSize);
//    Console.WriteLine($"i: {i}, Source Count: {src.Count()}, Truncated Collection Count: {truncatedCollection.Count}, IsTruncated: {truncatedCollection.IsTruncated}");
//}

//var pageSize = 5000;
//var source = Enumerable.Range(1, 1023).AsQueryable();
//var truncatedCollection = TruncatedCollectionOfTOpt<int>.Create(source, pageSize);
