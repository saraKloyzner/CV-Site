using Microsoft.AspNetCore.Mvc;
using Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GitHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;

        public GitHubController(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        /// <summary>
        /// מחזיר את הפורטפוליו (רשימת ה־repositories) עם המידע הרלוונטי:
        /// שפות, commit אחרון, מספר כוכבים, pull requests, וקישור לאתר ה־repo.
        /// </summary>
        [HttpGet("portfolio")]
        public async Task<IActionResult> GetPortfolio()
        {
            try
            {
                var portfolio = await _gitHubService.GetPortfolioAsync();
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving portfolio: {ex.Message}");
            }
        }

        /// <summary>
        /// מחזיר פרטי repository בודד לפי שם.
        /// </summary>
        [HttpGet("portfolio/{repoName}")]
        public async Task<IActionResult> GetRepositoryDetails(string repoName)
        {
            try
            {
                var repository = await _gitHubService.GetRepositoryDetailsAsync(repoName);
                if (repository == null)
                {
                    return NotFound($"Repository '{repoName}' not found.");
                }
                return Ok(repository);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving repository details: {ex.Message}");
            }
        }

        /// <summary>
        /// מבצע חיפוש ב־public repositories לפי קריטריונים אופציונליים:
        /// שם repository, שפת פיתוח ושם משתמש.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchRepositories(
            [FromQuery] string name = null,
            [FromQuery] string language = null,
            [FromQuery] string userName = null)
        {
            try
            {
                var results = await _gitHubService.SearchRepositoriesAsync(name, language, userName);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching repositories: {ex.Message}");
            }
        }

        /// <summary>
        /// מרענן את הפורטפוליו – מסיר את הקאש וכך מחלץ נתונים עדכניים מה־GitHub.
        /// </summary>
        [HttpPost("portfolio/refresh")]
        public async Task<IActionResult> RefreshPortfolio()
        {
            try
            {
                var refreshedPortfolio = await _gitHubService.RefreshPortfolioAsync();
                return Ok(refreshedPortfolio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error refreshing portfolio: {ex.Message}");
            }
        }
    }
}
