// Configure MSTest to run tests in parallel for better performance
// Workers: Specifies the maximum number of threads to use for parallel execution
//          0 is the default value and means that number of logical processors on the machine will be used
//   Scope: Determines the level of parallelization
//          ExecutionScope.ClassLevel runs test classes in parallel (but tests within a single class run sequentially)
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]
