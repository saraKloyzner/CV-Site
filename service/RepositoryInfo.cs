using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class RepositoryInfo
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public DateTime LastCommit { get; set; }
        public int StarCount { get; set; }
        public int PullRequestCount { get; set; }
        public string HtmlUrl { get; set; }
       
    }
}
