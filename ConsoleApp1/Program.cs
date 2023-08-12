//****************************************//
//      Kevin Eduardo Curiel Justo        //
//             11-08-2023                 //
//****************************************//

using HtmlAgilityPack;
using CsvHelper;
using System.Globalization;

namespace SimpleWebScraper
{
    class Program
    {
        //class for the List data structure to create the .csv
        public class HtmlLink
        {
            public string? Url { get; set; }
            public int? Duplicates { get; set; }
        }

        //Function that looks for the first 10 unique imbeded links on page
        static void AddLinks(List<HtmlLink> htmlLinks, HtmlWeb web, string link)
        {
            //Loads the page information
            var currentDocument = web.Load(link);

            //looks for imbeded links using xpath of wikipedia pages
            HtmlNodeCollection pHTMLElements;
            pHTMLElements = currentDocument.DocumentNode.SelectNodes("//*[@id=\"mw-content-text\"]//p/a[@class=\"mw-redirect\"]");

            //Returns early if no imbeded links are found
            if (pHTMLElements == null)
            {
                return;
            }

            //Condition if there are less than 10 links on the page
            int count = 10;
            if (pHTMLElements.Count < 10)
            {
                count = pHTMLElements.Count;
            }

            //loop to create list of imbeded links
            int x = 0;
            int loops = 1;
            while (loops <= count)
            {
                //Looks for the href value in the x element of the pHTMLElements found
                var hrefElement = HtmlEntity.DeEntitize(pHTMLElements[x].QuerySelector("a").Attributes["href"].Value.ToString());
                string hrefURL = "https://en.wikipedia.org" + hrefElement;

                //loop to verrify duplicates links
                bool unique = true;
                for (int j = 0; j <= htmlLinks.Count - 1; j++)
                {
                    if (htmlLinks[j].Url == hrefURL)
                    {
                        //If not unique, adds 1 to the duplicate counter of the found link and makes the unique bool false
                        htmlLinks[j].Duplicates += 1;
                        unique = false;
                    }
                }

                if (unique)
                {
                    //inserts the unique link found on the htmlLinks List of htmlLink objects
                    var htmlLink = new HtmlLink() { Url = hrefURL, Duplicates = 0 };
                    htmlLinks.Add(htmlLink);
                    loops++;
                }

                x++;

                //Condition to end early if there are no more unique links on the page
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

            //loads the link provided to scrape for imbeded links
            AddLinks(htmlLinks, web, link);

            int j = 0;
            //Outer loop for link scraping using n provided
            for (int i = 0; i < n; i++)
            {

                //Variable to determine loop; this updates after finishing with n cycle of loops, as the list will update automatically in the AddLinks function 
                int count = htmlLinks.Count;

                while (j <= count - 1)
                {
                    //updates the variable link with the next link on list
                    link = htmlLinks[j].Url;

                    //Calls the AddLinks function with the new link
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