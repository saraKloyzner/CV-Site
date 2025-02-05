using Microsoft.AspNetCore.Mvc;
using Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GitHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;
        public GitHubController(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }
        // GET: api/<GitHubController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<GitHubController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<GitHubController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<GitHubController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GitHubController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        /// <summary>
        /// מחזירה את הפורטפוליו – רשימת ה-repositories עם המידע הרלוונטי
        /// </summary>
        [HttpGet("portfolio")]
        public async Task<IActionResult> GetPortfolio()
        {
            var portfolio = await _gitHubService.GetPortfolioAsync();
            return Ok(portfolio);
        }

        /// <summary>
        /// חיפוש ב-public repositories לפי פרמטרים אופציונליים
        /// </summary>
        /// <param name="name">שם repository (אופציונלי)</param>
        /// <param name="language">שפת תכנות (אופציונלי)</param>
        /// <param name="userName">שם משתמש (אופציונלי)</param>
        [HttpGet("search")]
        public async Task<IActionResult> SearchRepositories([FromQuery] string name = null, [FromQuery] string language = null, [FromQuery] string userName = null)
        {
            var results = await _gitHubService.SearchRepositoriesAsync(name, language, userName);
            return Ok(results);
        }
    }
}
