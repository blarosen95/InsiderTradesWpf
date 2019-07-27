using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;

namespace InsiderTradesWpf
{
    class Edgar
    {
        private static async Task<string> GetCikNumberAsync(string ticker)
        {
            var url = $"https://www.sec.gov/cgi-bin/browse-edgar?CIK={ticker}";

            var client = new HttpClient();

            using (var response = await client.GetAsync(@url))
            {
                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();

                    document.LoadHtml(result);

                    var pattern = @"((?<=CIK=)\d{10})";
                    Match match = Regex.Match(document.Text, pattern);

                    return match.Success ? match.Value : "Error on matching for a CIK number!";
                }
            }
        }

        public async Task<List<string>> GetInfo(string ticker)
        {
            var cik = await GetCikNumberAsync(ticker);
            if (cik.Equals("Error on matching for a CIK number!"))
            {
                //TODO: Can we find an async approach?
                MessageBox.Show(cik);
            }

            var url = $"https://www.sec.gov/cgi-bin/own-disp?action=getissuer&CIK={cik}";

            var client = new HttpClient();

            var cells = new List<string>();
            using (var response = await client.GetAsync(@url))
            {
                using (var content = response.Content)
                {
                    //TODO: I vaguely recall needing the following line because the document was too long to load from the result (like it's done in the previous method). However, why load page twice?
                    var document = new HtmlWeb().Load(@url);

                    var tableNodes = document.DocumentNode.SelectSingleNode("(//table)[8]");
                    cells.AddRange(from row in tableNodes.SelectNodes("tr")
                        from cell in row.SelectNodes("th|td")
                        select cell.InnerText);
                }
            }

            return cells;
        }
    }
}