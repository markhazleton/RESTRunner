using RESTRunner.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTRunner.Domain.Interfaces
{
    public interface IExecuteRunner
    {
        public Task<IEnumerable<CompareResult>> ExecuteRunnerAsync();
    }
}
