using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace scraper_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet("/GetUrl")]
        public IActionResult getUrl()
        {
            string url = "https://en.wikipedia.org/wiki/List_of_programmers";
            string response = CallUrl(url).Result;

            var linkList = ParseHtml(response);

            WriteToCsv(linkList);

            return Ok(linkList);
        }


        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private List<string> ParseHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var programmerLinks = htmlDoc.DocumentNode.Descendants("li")
                .Where(node => !node.GetAttributeValue("href", "*").Contains("/wiki/"))
                .ToList();

            List<string> wikiLink = new List<string>();

            foreach (var link in programmerLinks)
            {
                if (link.FirstChild.Attributes.Count > 0)
                    wikiLink.Add("https://en.wikipedia.org" + link.FirstChild.Attributes[0].Value);
            }

            return wikiLink;

        }

        private void WriteToCsv(List<string> links)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var link in links)
            {
                sb.AppendLine(link);
            }

            System.IO.File.WriteAllText("links.csv", sb.ToString());
        }
    }

}
