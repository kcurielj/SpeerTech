using HtmlAgilityPack;
using CsvHelper;
using System.Globalization;
using static SimpleWebScraper.Program;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System;
using CsvHelper.Configuration.Attributes;

namespace SimpleWebScraper
{
    class Program
    {
        public class HtmlLink
        {
            public string? Url { get; set; }
            public int? Duplicates { get; set; }
        }


        static void AddLinks(List<HtmlLink> htmlLinks, HtmlWeb web, string link)
        {
            var currentDocument = web.Load(link);

            //looks for imbeded links using xpath of wikipedia pages
            HtmlNodeCollection pHTMLElements;
            pHTMLElements = currentDocument.DocumentNode.SelectNodes("//*[@id=\"mw-content-text\"]//p/a");

            if (pHTMLElements == null)
            {
                return;
            }

            int count = 10;
            //Condition if there are less than 10 links on the page
            if (pHTMLElements.Count < 10)
            {
                count = pHTMLElements.Count;
            }

            int x = 0;
            int loops = 1;
            
            //loop to create list of imbeded links
            while (loops <= count)
            {

                var hrefElement = HtmlEntity.DeEntitize(pHTMLElements[x].QuerySelector("a").Attributes["href"].Value.ToString());
                string hrefURL = "https://en.wikipedia.org" + hrefElement;

                bool unique = true;

                //loop to verrify duplicates links
                for (int j = 0; j <= htmlLinks.Count - 1; j++)
                {
                    if (htmlLinks[j].Url == hrefURL)
                    {
                        htmlLinks[j].Duplicates += 1;
                        unique = false;
                    }
                }

                if (unique)
                {
                    var htmlLink = new HtmlLink() { Url = hrefURL, Duplicates = 0 };
                    htmlLinks.Add(htmlLink);
                    loops++;
                }

                x++;
                if (x >= pHTMLElements.Count)
                {
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            //variables for web scraping
            var htmlLinks = new List<HtmlLink>();
            string link;

            //Request and Verification of link
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter Link:");

                    link = Console.ReadLine();

                    Uri uriResult;
                    bool result = Uri.TryCreate(link, UriKind.Absolute, out uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                    if (!result)
                    {
                        throw new ArgumentException("Invalid Link Provided");
                    }
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Link provided not in URL format");
                }

            }

            //Variables used to start web scraping
            var web = new HtmlWeb();

            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";

            //n is variable for loops of imbeded link searches
            int n = 0;
            
            //Request and Verification of n
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter number of loops between 1 and 3 (n):");

                    string nString = Console.ReadLine();
                    n = Convert.ToInt32(nString);

                    if (n > 3 || n < 1)
                    {
                        throw new ArgumentException("Invalid Number of Loops Provided");
                    }
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Number provided beyond scope");
                }
            }

            //loads the link provided to scrape for links

            AddLinks(htmlLinks, web, link);

            int j = 0;
            //Outer loop for link scraping using n provided
            for (int i = 0; i < n; i++) {

                int count = htmlLinks.Count;

                while (j < count-1)
                {
                    link = htmlLinks[j].Url;

                    AddLinks(htmlLinks, web, link);
                    j++;
                }
            }
            // initializing the CSV output file 
            using (var writer = new StreamWriter("html-links.csv"))
            // initializing the CSV writer 
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // populating the CSV file 
                csv.WriteRecords(htmlLinks);
            }
        }
    }
}