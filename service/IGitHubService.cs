using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IGitHubService
    {
        Task<IEnumerable<RepositoryInfo>> GetPortfolioAsync();
        Task<IEnumerable<RepositoryInfo>> SearchRepositoriesAsync(string name = null, string language = null, string userName = null);
    }
}
