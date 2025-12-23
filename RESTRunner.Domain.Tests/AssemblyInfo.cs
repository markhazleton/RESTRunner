using Microsoft.VisualStudio.TestTools.UnitTesting;

// Configure MSTest to run tests in parallel for better performance
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]
