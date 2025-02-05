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
        private readonly GitHubSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);



        public Task<string> GetRepositoriesAsync()
        {
            throw new NotImplementedException();
        }


        public GitHubService(IOptions<GitHubSettings> options, IMemoryCache cache)
        {
            _settings = options.Value;
            _cache = cache;

            // אתחול ה-GitHubClient עם User-Agent ייחודי
            _client = new GitHubClient(new ProductHeaderValue("CVSiteApp"));

            // אם יש טוקן, נבצע הזדהות
            if (!string.IsNullOrWhiteSpace(_settings.Token))
            {
                _client.Credentials = new Credentials(_settings.Token);
            }
        }

        public async Task<IEnumerable<RepositoryInfo>> GetPortfolioAsync()
        {
            // נסה לבדוק אם יש תוצאות בקאש
            if (_cache.TryGetValue("Portfolio", out IEnumerable<RepositoryInfo> cachedPortfolio))
            {
                // ניתן להוסיף כאן לוגיקה לבדיקה אם יש עדכונים ב-GitHub – אתגר הפרויקט
                return cachedPortfolio;
            }

            // שליפת רשימת ה-repositories מהמשתמש
            var repositories = await _client.Repository.GetAllForUser(_settings.UserName);

            // יצירת רשימת מידע מותאם לכל repository
            var portfolio = new List<RepositoryInfo>();
            foreach (var repo in repositories)
            {
                // שליפת שפת הפיתוח העיקרית
                string language = repo.Language;

                // שליפת commit אחרון – לדוגמה, נשלוף את ה-commit מהבראנץ' הראשי (master/main)
                var commits = await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name);
                var lastCommit = commits?.FirstOrDefault()?.Commit.Author.Date.UtcDateTime ?? DateTime.MinValue;

                // שליפת כמות ה-star
                int starCount = repo.StargazersCount;

                // שליפת מספר pull requests – ניתן לבצע שאילתה פשוטה (כאן נעשה קריאה לא סימטרית, ניתן לייעל לפי הצורך)
                var pullRequests = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
                int prCount = pullRequests.Count();

                portfolio.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    Language = language,
                    LastCommit = lastCommit,
                    StarCount = starCount,
                    PullRequestCount = prCount,
                    HtmlUrl = repo.HtmlUrl
                });
            }

            // שמירת התוצאה ב-In-Memory Cache
            _cache.Set("Portfolio", portfolio, _cacheExpiration);

            return portfolio;
        }

        public async Task<IEnumerable<RepositoryInfo>> SearchRepositoriesAsync(string name = null, string language = null, string userName = null)
        {
            // הכנה של מחרוזת החיפוש לפי הפרמטרים – יש להתאים לפי התיעוד של Octokit
            string query = string.Empty;
            if (!string.IsNullOrWhiteSpace(name))
                query += name + " ";
            if (!string.IsNullOrWhiteSpace(language))
                query += "language:" + language + " ";
            if (!string.IsNullOrWhiteSpace(userName))
                query += "user:" + userName + " ";

            var request = new SearchRepositoriesRequest(query)
            {
                // ניתן להוסיף הגדרות נוספות לפי הצורך
            };

            var result = await _client.Search.SearchRepo(request);

            var repositories = result.Items.Select(repo => new RepositoryInfo
            {
                Name = repo.Name,
                Language = repo.Language,
                LastCommit = DateTime.MinValue, // כאן צריך להוסיף לוגיקה לשליפת commit אחרון, אם נדרש
                StarCount = repo.StargazersCount,
                PullRequestCount = 0, // ניתן להוסיף לוגיקה לשליפת PR count
                HtmlUrl = repo.HtmlUrl
            });

            return repositories;
        }
    }
}
