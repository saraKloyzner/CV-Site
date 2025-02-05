using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly IMemoryCache _cache;
        private readonly string _userName; // שמתקבל מה-UserSecrets/Configuration

        public GitHubService(IMemoryCache memoryCache, IOptions<GitHubSettings> options)
        {
            _cache = memoryCache;
            var settings = options.Value;
            _userName = settings.UserName;

            _client = new GitHubClient(new ProductHeaderValue("CVSiteApp"));
            if (!string.IsNullOrWhiteSpace(settings.Token))
            {
                _client.Credentials = new Credentials(settings.Token);
            }
        }

        public async Task<IEnumerable<RepositoryInfo>> GetPortfolioAsync()
        {
            // בדיקה אם הנתונים בקאש
            if (_cache.TryGetValue("Portfolio", out IEnumerable<RepositoryInfo> portfolio))
            {
                return portfolio;
            }

            var repositories = await _client.Repository.GetAllForUser(_userName);
            var repoInfos = new List<RepositoryInfo>();

            foreach (var repo in repositories)
            {
                // שליפת commit אחרון – לדוגמה מהבראנצ' הראשי (main/master)
                var commits = await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name);
                DateTime lastCommit = commits.FirstOrDefault()?.Commit.Author.Date.UtcDateTime ?? DateTime.MinValue;

                // שליפת pull requests
                var pullRequests = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);

                repoInfos.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    Language = repo.Language,
                    LastCommit = lastCommit,
                    StarCount = repo.StargazersCount,
                    PullRequestCount = pullRequests.Count,
                    HtmlUrl = repo.HtmlUrl
                });
            }

            // שמירת התוצאה בקאש למשך 10 דקות
            _cache.Set("Portfolio", repoInfos, TimeSpan.FromMinutes(10));
            return repoInfos;
        }

        public async Task<RepositoryInfo> GetRepositoryDetailsAsync(string repoName)
        {
            // אפשר להשתמש במידע מהפורטפוליו הקיים ולסנן את הפריט המתאים
            var portfolio = await GetPortfolioAsync();
            return portfolio.FirstOrDefault(r => r.Name.Equals(repoName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<RepositoryInfo>> SearchRepositoriesAsync(string name = null, string language = null, string userName = null)
        {
            // בניית שאילתה לפי הפרמטרים
            string query = $"{(name ?? "")} {(language != null ? "language:" + language : "")} {(userName != null ? "user:" + userName : "")}".Trim();

            var request = new SearchRepositoriesRequest(query);
            var result = await _client.Search.SearchRepo(request);

            // המרה למודל שלנו
            return result.Items.Select(repo => new RepositoryInfo
            {
                Name = repo.Name,
                Language = repo.Language,
                LastCommit = DateTime.MinValue, // במידה ונדרש – ניתן להוסיף לוגיקה לשליפת commit אחרון
                StarCount = repo.StargazersCount,
                PullRequestCount = 0,          // ניתן להוסיף לוגיקה נוספת לשליפת PR count
                HtmlUrl = repo.HtmlUrl
            });
        }

        public async Task<IEnumerable<RepositoryInfo>> RefreshPortfolioAsync()
        {
            // הסרת הקאש וחזרה על השליפה
            _cache.Remove("Portfolio");
            return await GetPortfolioAsync();
        }
    }
}
